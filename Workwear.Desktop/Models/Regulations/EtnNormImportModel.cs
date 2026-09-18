using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Measurement.Repository;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

using EtnNormItem = QS.Cloud.WorkwearDictionary.Grpc.Contracts.NormItem;
using EtnGetNormResponse = QS.Cloud.WorkwearDictionary.Grpc.Contracts.GetNormResponse;
using EtnComplectType = QS.Cloud.WorkwearDictionary.Grpc.Contracts.ComplectType;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.Grpc.Contracts.ItemSIZ;
using EtnPeriodType = QS.Cloud.WorkwearDictionary.Grpc.Contracts.PeriodType;

namespace Workwear.Models.Regulations {
	/// <summary>
	/// Заполняет норму данными, полученными из справочника ЕТН: должность и строки нормы.
	/// Номенклатура, тип и условие нормы подбираются по совпадению названия иначе создаются.
	/// </summary>
	public class EtnNormImportModel {
		public const string CreatedComment = "Создано из справочника ЕТН";

		private readonly IUnitOfWork uow;
		private readonly Norm norm;
		private readonly IInteractiveService interactive;

		private readonly List<Post> autoCreatedPosts = new List<Post>();
		private readonly List<ItemsType> autoCreatedItemsTypes = new List<ItemsType>();
		private readonly List<ProtectionTools> autoCreatedProtectionTools = new List<ProtectionTools>();
		private readonly List<NormCondition> autoCreatedConditions = new List<NormCondition>();

		public EtnNormImportModel(IUnitOfWork uow, Norm norm, IInteractiveService interactive) {
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.norm = norm ?? throw new ArgumentNullException(nameof(norm));
			this.interactive = interactive;
		}

		public void FillFromEtn(EtnGetNormResponse etnNorm) {
			norm.Name = etnNorm.PostName;
			norm.Comment = $"{CreatedComment}, №{etnNorm.NumberNorm} в приложении №1 приказа Минтруда РФ от 29.10.2021 N 767Н ";

			FillPostFromEtn(etnNorm.PostName);

			var itemsTypesCache = new Dictionary<string, ItemsType>();
			var protectionToolsCache = new Dictionary<string, ProtectionTools>();
			var conditionsCache = new Dictionary<string, NormCondition>();

			foreach(var etnComplect in etnNorm.Items)
				ProcessEtnComplect(etnComplect, itemsTypesCache, protectionToolsCache, conditionsCache);

			norm.Posts.CollectionChanged += PostsCollectionChanged;
			norm.Items.CollectionChanged += ItemsCollectionChanged;
		}

		/// <summary>
		/// Если при заполнении из ЕТН были созданы новые записи справочников - предупреждаем и даём возможность отменить сохранение.
		/// </summary>
		public bool ConfirmAutoCreatedEntities() {
			if(!autoCreatedPosts.Any() && !autoCreatedProtectionTools.Any()
				&& !autoCreatedItemsTypes.Any() && !autoCreatedConditions.Any())
				return true;

			var lines = new List<string>();
			if(autoCreatedPosts.Any())
				lines.Add("Должности: " + String.Join(", ", autoCreatedPosts.Select(x => x.Name)));
			if(autoCreatedProtectionTools.Any())
				lines.Add("Номенклатура нормы: " + String.Join(", ", autoCreatedProtectionTools.Select(x => x.Name)));
			if(autoCreatedItemsTypes.Any())
				lines.Add("Типы номенклатуры: " + String.Join(", ", autoCreatedItemsTypes.Select(x => x.Name)));
			if(autoCreatedConditions.Any())
				lines.Add("Условия нормы: " + String.Join(", ", autoCreatedConditions.Select(x => x.Name)));

			var message = "При сохранении нормы в справочники будут добавлены новые записи, полученные из ЕТН:\n"
				+ String.Join("\n", lines) + "\n\nПродолжить сохранение?";
			return interactive.Question(message);
		}

		/// <summary>
		/// Вызывается после успешного сохранения нормы - учёт авто-созданных записей больше не нужен.
		/// </summary>
		public void ClearAutoCreated() {
			autoCreatedPosts.Clear();
			autoCreatedItemsTypes.Clear();
			autoCreatedProtectionTools.Clear();
			autoCreatedConditions.Clear();
		}

		/// <summary>
		/// Обрабатывает комплект указанный в норме.
		/// Комплект "или" - спрашиваем пользователя, какую строку добавить или добавить одной строкой все.
		/// Комплект "и" - спрашиваем, добавлять позиции по отдельности, одной строкой или не добавлять.
		/// Комплект из одной позиции считаем просто строкой.
		/// </summary>
		private void ProcessEtnComplect(
			EtnNormItem etnComplect,
			Dictionary<string, ItemsType> itemsTypesCache,
			Dictionary<string, ProtectionTools> protectionToolsCache,
			Dictionary<string, NormCondition> conditionsCache)
		{
			if(etnComplect.Items.Count > 1) {
				if(etnComplect.ComplectType == EtnComplectType.Or) {
					var chosen = ChooseOrVariant(etnComplect, itemsTypesCache);
					if(chosen != null)
						AddSizItem(chosen, itemsTypesCache, protectionToolsCache, conditionsCache);
					return;
				}

				if(etnComplect.ComplectType == EtnComplectType.And) {
					var answer = AskAndComplectAction(etnComplect, itemsTypesCache);
					if(answer == "Одной записью") {
						AddCombinedComplectItem(etnComplect, itemsTypesCache, protectionToolsCache, conditionsCache);
						return;
					}
					if(answer == "Не добавлять")
						return;
				}
			}

			foreach(var sizItem in etnComplect.Items)
				AddSizItem(sizItem, itemsTypesCache, protectionToolsCache, conditionsCache);
		}

		private EtnItemSIZ ChooseOrVariant(EtnNormItem etnComplect, Dictionary<string, ItemsType> itemsTypesCache) {
			var message = String.IsNullOrWhiteSpace(etnComplect.ComplectName)
				? "В комплекте несколько взаимозаменяемых вариантов. Выберите, какой из них добавить в норму:"
				: $"В комплекте «{etnComplect.ComplectName}» несколько взаимозаменяемых вариантов. Выберите, какой из них добавить в норму:";
			return interactive.ChooseOne(etnComplect.Items, x => FormatEtnSizOption(x, itemsTypesCache), message, "Выбор варианта СИЗ");
		}

		private string AskAndComplectAction(EtnNormItem etnComplect, Dictionary<string, ItemsType> itemsTypesCache) {
			var complectTitle = String.IsNullOrWhiteSpace(etnComplect.ComplectName) ? "без названия" : etnComplect.ComplectName;
			var itemsList = FormatEtnComplectItems(etnComplect, itemsTypesCache);
			var message = $"Комплект \"{complectTitle}\"\nвключает несколько позиций:\n{itemsList}\n\nКак их добавить?";
			return interactive.Question(new[] { "По отдельности", "Одной записью", "Не добавлять" }, message, "Комплект СИЗ");
		}

		private void AddCombinedComplectItem(
			EtnNormItem etnComplect,
			Dictionary<string, ItemsType> itemsTypesCache,
			Dictionary<string, ProtectionTools> protectionToolsCache,
			Dictionary<string, NormCondition> conditionsCache)
		{
			var firstItem = etnComplect.Items.First();
			var complectName = String.IsNullOrWhiteSpace(etnComplect.ComplectName) ? firstItem.SizName : etnComplect.ComplectName.Trim();

			var itemsType = FindOrCreateItemsType(firstItem.SizType, itemsTypesCache);
			var complectComment = $"{CreatedComment}. Комплект включает:\n{FormatEtnComplectItems(etnComplect, itemsTypesCache)}";
			var protectionTools = FindOrCreateProtectionTools(complectName, itemsType, protectionToolsCache, complectComment);
			//Запись уже существовала в справочнике до импорта - дописываем состав комплекта, только если у нее еще нет своего комментария.
			if(String.IsNullOrWhiteSpace(protectionTools.Comment))
				protectionTools.Comment = complectComment;

			var normItem = norm.AddItem(protectionTools);
			if(normItem == null)
				return;
			normItem.NormPeriod = MapPeriodType(firstItem.PeriodType);
			normItem.PeriodCount = firstItem.PeriodCount;
			normItem.Amount = firstItem.Amount > 0 ? firstItem.Amount : 1;
			normItem.NormCondition = FindOrCreateCondition(firstItem.Condition, conditionsCache);
		}

		private void AddSizItem(
			EtnItemSIZ sizItem,
			Dictionary<string, ItemsType> itemsTypesCache,
			Dictionary<string, ProtectionTools> protectionToolsCache,
			Dictionary<string, NormCondition> conditionsCache)
		{
			var itemsType = FindOrCreateItemsType(sizItem.SizType, itemsTypesCache);
			var protectionTools = FindOrCreateProtectionTools(sizItem.SizName, itemsType, protectionToolsCache);

			var normItem = norm.AddItem(protectionTools);
			if(normItem == null)
				return;

			normItem.NormPeriod = MapPeriodType(sizItem.PeriodType);
			normItem.PeriodCount = sizItem.PeriodCount;
			normItem.Amount = sizItem.Amount > 0 ? sizItem.Amount : 1;
			normItem.NormCondition = FindOrCreateCondition(sizItem.Condition, conditionsCache);
		}

		private string FormatEtnSizOption(EtnItemSIZ item, Dictionary<string, ItemsType> itemsTypesCache) {
			var period = MapPeriodType(item.PeriodType).GetEnumTitle();
			return $"{item.SizName} — {FormatEtnAmount(item, itemsTypesCache)}, {item.PeriodCount} × {period}";
		}

		/// <summary>
		/// Перечисляет позиции комплекта ЕТН по строкам, с количеством и единицей измерения.
		/// </summary>
		private string FormatEtnComplectItems(EtnNormItem etnComplect, Dictionary<string, ItemsType> itemsTypesCache) =>
			String.Join("\n", etnComplect.Items.Select(x => $"{x.SizName} - {FormatEtnAmount(x, itemsTypesCache)}"));

		private string FormatEtnAmount(EtnItemSIZ sizItem, Dictionary<string, ItemsType> itemsTypesCache) {
			var typeName = EtnItemsTypeName(sizItem.SizType);
			if(!itemsTypesCache.TryGetValue(typeName, out var itemsType))
				itemsType = uow.Session.QueryOver<ItemsType>().Where(x => x.Name == typeName).Take(1).SingleOrDefault();

			var units = itemsType?.Units ?? MeasurementUnitRepository.GetDefaultGoodsUnit(uow);
			return units?.MakeAmountShortStr(sizItem.Amount) ?? sizItem.Amount.ToString();
		}

		private static string EtnItemsTypeName(string sizType) =>
			String.IsNullOrWhiteSpace(sizType) ? "Не определено" : sizType.Trim();

		private void FillPostFromEtn(string postName) {
			postName = postName?.Trim();
			if(String.IsNullOrWhiteSpace(postName))
				return;

			var post = uow.Session.QueryOver<Post>().Where(x => x.Name == postName).Take(1).SingleOrDefault();
			if(post == null) {
				post = new Post { Name = postName, Comments = CreatedComment };
				uow.Save(post);
				autoCreatedPosts.Add(post);
			}
			norm.AddPost(post);
		}

		private ItemsType FindOrCreateItemsType(string typeName, Dictionary<string, ItemsType> cache) {
			typeName = EtnItemsTypeName(typeName);
			if(cache.TryGetValue(typeName, out var cachedType))
				return cachedType;

			var itemsType = uow.Session.QueryOver<ItemsType>().Where(x => x.Name == typeName).Take(1).SingleOrDefault();
			if(itemsType == null) {
				itemsType = new ItemsType {
					Name = typeName,
					Comment = CreatedComment,
					Units = MeasurementUnitRepository.GetDefaultGoodsUnit(uow)
				};
				uow.Save(itemsType);
				autoCreatedItemsTypes.Add(itemsType);
			}
			cache[typeName] = itemsType;
			return itemsType;
		}

		/// <param name="comment">Комментарий создаваемой номенклатуры. Если не указан - только отметка о создании из ЕТН.</param>
		private ProtectionTools FindOrCreateProtectionTools(
			string sizName,
			ItemsType itemsType,
			Dictionary<string, ProtectionTools> cache,
			string comment = null)
		{
			var name = String.IsNullOrWhiteSpace(sizName) ? "Без названия" : sizName.Trim();
			if(cache.TryGetValue(name, out var cachedTools))
				return cachedTools;

			var protectionTools = uow.Session.QueryOver<ProtectionTools>().Where(x => x.Name == name).Take(1).SingleOrDefault();
			if(protectionTools == null) {
				protectionTools = new ProtectionTools { Name = name, Type = itemsType, Comment = comment ?? CreatedComment };
				uow.Save(protectionTools);
				autoCreatedProtectionTools.Add(protectionTools);
			}
			cache[name] = protectionTools;
			return protectionTools;
		}

		private NormCondition FindOrCreateCondition(string conditionName, Dictionary<string, NormCondition> cache)
		{
			conditionName = conditionName?.Trim();
			if(String.IsNullOrWhiteSpace(conditionName))
				return null;
			if(cache.TryGetValue(conditionName, out var cachedCondition))
				return cachedCondition;

			var condition = uow.Session.QueryOver<NormCondition>().Where(x => x.Name == conditionName).Take(1).SingleOrDefault();
			if(condition == null) {
				condition = new NormCondition { Name = conditionName };
				uow.Save(condition);
				autoCreatedConditions.Add(condition);
			}
			cache[conditionName] = condition;
			return condition;
		}

		//TODO возможно прояснится после добавления прижения 2 приказа. Нужно проверить 
		//ЕТН не различает "разовое использование"/"по необходимости" пока приводим к "до износа".
		private NormPeriodType MapPeriodType(EtnPeriodType periodType) {
			switch(periodType) {
				case EtnPeriodType.Year: return NormPeriodType.Year;
				case EtnPeriodType.Month: return NormPeriodType.Month;
				case EtnPeriodType.Wearout:
				case EtnPeriodType.OneUse:
				case EtnPeriodType.NeedSet:
					return NormPeriodType.Wearout;
				default: return NormPeriodType.Year;
			}
		}

		private void PostsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if(e.Action != NotifyCollectionChangedAction.Remove || e.OldItems == null)
				return;
			foreach(Post removed in e.OldItems) {
				if(autoCreatedPosts.Remove(removed))
					uow.Delete(removed);
			}
		}

		private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if(e.Action != NotifyCollectionChangedAction.Remove || e.OldItems == null)
				return;
			foreach(NormItem removed in e.OldItems) {
				var tools = removed.ProtectionTools;
				if(autoCreatedProtectionTools.Contains(tools) && norm.Items.All(i => i.ProtectionTools != tools)) {
					autoCreatedProtectionTools.Remove(tools);
					uow.Delete(tools);

					var itemsType = tools.Type;
					if(autoCreatedItemsTypes.Contains(itemsType) && norm.Items.All(i => i.ProtectionTools.Type != itemsType)) {
						autoCreatedItemsTypes.Remove(itemsType);
						uow.Delete(itemsType);
					}
				}

				var condition = removed.NormCondition;
				if(condition != null && autoCreatedConditions.Contains(condition) && norm.Items.All(i => i.NormCondition != condition)) {
					autoCreatedConditions.Remove(condition);
					uow.Delete(condition);
				}
			}
		}
	}
}
