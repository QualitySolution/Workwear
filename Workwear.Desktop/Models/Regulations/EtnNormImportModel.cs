using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Measurement.Repository;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Models.Import;
using Workwear.Tools.Sizes;

using EtnNormItem = QS.Cloud.WorkwearDictionary.Grpc.Contracts.NormItem;
using EtnGetNormResponse = QS.Cloud.WorkwearDictionary.Grpc.Contracts.GetNormResponse;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.Grpc.Contracts.ItemSIZ;
using EtnPeriodType = QS.Cloud.WorkwearDictionary.Grpc.Contracts.PeriodType;
using EtnComplectType = QS.Cloud.WorkwearDictionary.Grpc.Contracts.ComplectType;

namespace Workwear.Models.Regulations {
	/// <summary>
	/// Заполняет норму данными, полученными из справочника ЕТН: должность и строки нормы.
	/// Номенклатура, тип и условие нормы подбираются по совпадению названия иначе создаются.
	/// </summary>
	public class EtnNormImportModel {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public const string CreatedComment = "Создано из справочника ЕТН";

		//TODO сервис пока не отдаёт реальный текст особого периода (period_special). Добавить после реализации
		private const string UndefinedPeriodComment = "";

		private readonly IUnitOfWork uow;
		private readonly Norm norm;
		private readonly IInteractiveService interactive;
		private readonly IEtnComplectResolver complectResolver;
		private readonly SizeService sizeService;

		private readonly List<Post> autoCreatedPosts = new List<Post>();
		private readonly List<ItemsType> autoCreatedItemsTypes = new List<ItemsType>();
		private readonly List<ProtectionTools> autoCreatedProtectionTools = new List<ProtectionTools>();
		private readonly List<NormCondition> autoCreatedConditions = new List<NormCondition>();

		private string normParagraph;

		private NomenclatureTypes nomenclatureTypes;
		private bool nomenclatureTypesInitialized;

		public EtnNormImportModel(
			IUnitOfWork uow,
			Norm norm,
			IInteractiveService interactive,
			IEtnComplectResolver complectResolver = null,
			SizeService sizeService = null)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.norm = norm ?? throw new ArgumentNullException(nameof(norm));
			this.interactive = interactive;
			this.complectResolver = complectResolver;
			this.sizeService = sizeService;
		}

		public void FillFromEtn(EtnGetNormResponse etnNorm) {
			norm.Name = etnNorm.PostName;
			norm.Comment = $"{CreatedComment}, №{etnNorm.NumberNorm} в приложении №1 приказа Минтруда РФ от 29.10.2021 N 767Н ";
			normParagraph = $"п.{etnNorm.NumberNorm} Приложение 1 приказа №767Н от 29.10.2021";

			FillPostFromEtn(etnNorm.PostName);

			var itemsTypesCache = new Dictionary<string, ItemsType>();
			var protectionToolsCache = new Dictionary<string, ProtectionTools>();
			var conditionsCache = new Dictionary<string, NormCondition>();
			var needPeriodItems = new List<EtnItemSIZ>();

			foreach(var etnComplect in etnNorm.Items)
				ProcessEtnComplect(etnComplect, itemsTypesCache, protectionToolsCache, conditionsCache, needPeriodItems);

			if(needPeriodItems.Count > 0) {
				var resolved = complectResolver.ResolveNeedPeriodItems(needPeriodItems);
				if(resolved != null)
					foreach(var item in resolved)
						AddResolvedItem(item, itemsTypesCache, protectionToolsCache, conditionsCache);
			}

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
		/// Обрабатывает комплект в зависимости от типа:
		/// "и" - без диалога, добавляем все позиции отдельными строками;
		/// "или" - отдаём выбор варианта пользователю через <see cref="IEtnComplectResolver"/>;
		/// "один" (или одной позиции) - добавляем без вопросов, но позиции с неопределённым сроком выдачи откладываем в <paramref name="needPeriodItems"/>.
		/// </summary>
		private void ProcessEtnComplect(
			EtnNormItem etnComplect,
			Dictionary<string, ItemsType> itemsTypesCache,
			Dictionary<string, ProtectionTools> protectionToolsCache,
			Dictionary<string, NormCondition> conditionsCache,
			List<EtnItemSIZ> needPeriodItems)
		{
			if(etnComplect.ComplectType == EtnComplectType.And) {
				foreach(var sizItem in etnComplect.Items)
					AddSizItem(sizItem, itemsTypesCache, protectionToolsCache, conditionsCache, etnComplect.ComplectName);
				return;
			}

			if(etnComplect.ComplectType == EtnComplectType.Or && etnComplect.Items.Count > 1) {
				var resolved = complectResolver.ResolveOR(etnComplect);
				if(resolved == null)
					return; //Пользователь отказался добавлять комплект.

				foreach(var item in resolved)
					AddResolvedItem(item, itemsTypesCache, protectionToolsCache, conditionsCache);
				return;
			}

			foreach(var sizItem in etnComplect.Items) {
				if(sizItem.PeriodType == EtnPeriodType.OneUse || sizItem.PeriodType == EtnPeriodType.NeedSet)
					needPeriodItems.Add(sizItem);
				else
					AddSizItem(sizItem, itemsTypesCache, protectionToolsCache, conditionsCache);
			}
		}

		/// <summary>
		/// Добавляет позицию комплекта, выбранную и при необходимости отредактированную пользователем.
		/// </summary>
		private void AddResolvedItem(
			EtnComplectResolvedItem item,
			Dictionary<string, ItemsType> itemsTypesCache,
			Dictionary<string, ProtectionTools> protectionToolsCache,
			Dictionary<string, NormCondition> conditionsCache)
		{
			var itemsType = FindOrCreateItemsType(item.TypeName, item.Name, itemsTypesCache);
			var protectionTools = FindOrCreateProtectionTools(
				item.Name, itemsType, protectionToolsCache, BuildProtectionToolsComment(item.HasUndefinedPeriod, item.ProtectionToolsComment));

			var normItem = norm.AddItem(protectionTools);
			if(normItem == null)
				return;

			normItem.NormPeriod = item.PeriodType ?? NormPeriodType.Wearout;
			normItem.PeriodCount = item.PeriodCount ?? 1;
			normItem.Amount = item.Amount ?? 1;
			normItem.NormCondition = FindOrCreateCondition(item.Condition, conditionsCache);
			normItem.NormParagraph = normParagraph;
			if(!String.IsNullOrWhiteSpace(item.NormItemComment))
				normItem.Comment = item.NormItemComment;
		}

		/// <param name="rowComment">Комментарий к строке нормы (не к номенклатуре) - например название
		/// комплекта "и", частью которого была эта позиция.</param>
		private void AddSizItem(
			EtnItemSIZ sizItem,
			Dictionary<string, ItemsType> itemsTypesCache,
			Dictionary<string, ProtectionTools> protectionToolsCache,
			Dictionary<string, NormCondition> conditionsCache,
			string rowComment = null)
		{
			var itemsType = FindOrCreateItemsType(sizItem.SizType, sizItem.SizName, itemsTypesCache);
			var hasUndefinedPeriod = sizItem.PeriodType == EtnPeriodType.OneUse || sizItem.PeriodType == EtnPeriodType.NeedSet;
			var protectionTools = FindOrCreateProtectionTools(
				sizItem.SizName, itemsType, protectionToolsCache, BuildProtectionToolsComment(hasUndefinedPeriod));

			var normItem = norm.AddItem(protectionTools);
			if(normItem == null)
				return;

			normItem.NormPeriod = MapPeriodType(sizItem.PeriodType);
			normItem.PeriodCount = sizItem.PeriodCount;
			normItem.Amount = sizItem.Amount > 0 ? sizItem.Amount : 1;
			normItem.NormCondition = FindOrCreateCondition(sizItem.Condition, conditionsCache);
			normItem.NormParagraph = normParagraph;
			if(!String.IsNullOrWhiteSpace(rowComment))
				normItem.Comment = rowComment.Trim();
		}

		/// <summary>
		/// Комментарий для создаваемой номенклатуры нормы
		/// </summary>
		private static string BuildProtectionToolsComment(bool hasUndefinedPeriod, string nomenclatureNote = null) {
			var parts = new List<string> { CreatedComment };
			if(hasUndefinedPeriod && !String.IsNullOrWhiteSpace(UndefinedPeriodComment))
				parts.Add(UndefinedPeriodComment);
			if(!String.IsNullOrWhiteSpace(nomenclatureNote))
				parts.Add(nomenclatureNote);
			return String.Join(". ", parts);
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

		private ItemsType FindOrCreateItemsType(string typeName, string sizName, Dictionary<string, ItemsType> cache) {
			var guessed = GetNomenclatureTypes()?.ParseNomenclatureName(sizName ?? String.Empty);
			var effectiveName = guessed != null ? guessed.Name : EtnItemsTypeName(typeName);

			if(cache.TryGetValue(effectiveName, out var cachedType))
				return cachedType;

			var itemsType = guessed ?? uow.Session.QueryOver<ItemsType>().Where(x => x.Name == effectiveName).Take(1).SingleOrDefault();
			if(itemsType == null) {
				itemsType = new ItemsType {
					Name = effectiveName,
					Comment = CreatedComment,
					Units = MeasurementUnitRepository.GetDefaultGoodsUnit(uow)
				};
				uow.Save(itemsType);
				autoCreatedItemsTypes.Add(itemsType);
			} else if(itemsType.Id == 0) {
				//подобрали категорию, но в справочнике её ещё нет - сохраняем.
				uow.Save(itemsType);
				autoCreatedItemsTypes.Add(itemsType);
			}
			cache[effectiveName] = itemsType;
			return itemsType;
		}

		private NomenclatureTypes GetNomenclatureTypes() {
			if(sizeService == null)
				return null;
			if(nomenclatureTypesInitialized)
				return nomenclatureTypes;

			nomenclatureTypesInitialized = true;
			try {
				nomenclatureTypes = new NomenclatureTypes(uow, sizeService, tryLoad: true);
			} catch(Exception ex) {
				logger.Warn(ex, "Не удалось подготовить механизм определения типа номенклатуры - будут дефолтными.");
			}
			return nomenclatureTypes;
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
		public static NormPeriodType MapPeriodType(EtnPeriodType periodType) {
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
