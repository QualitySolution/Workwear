using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.SqlCommand;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Repository.Regulations;

namespace Workwear.Models.Operations {
	public class DutyNormIssueModel {
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private readonly DutyNormRepository dutyNormRepository;

		public DutyNormIssueModel(
			DutyNormRepository dutyNormRepository,
			UnitOfWorkProvider unitOfWorkProvider = null) {
			this.unitOfWorkProvider = unitOfWorkProvider;
			this.dutyNormRepository = dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
		}
		
		#region Helpers
		private IUnitOfWork UoW => unitOfWorkProvider?.UoW;
		#endregion
		
		/// <summary>
		/// Заполняет графы
		/// </summary>
		/// <param name="progress">Можно предать начатый прогресс, количество шагов прогресса равно количеству элементов + 2</param>
		public void FillDutyNormItems(DutyNormItem[] items, DutyNormIssueOperation[] notSavedOperations = null, IProgressBarDisplayable progress = null) {
			bool needClose = false;
			if(progress != null && !progress.IsStarted) {
				progress.Start(items.Length + 2);
				needClose = true;
			}
			progress?.Add(text: "Подгружаем операции");
			if(items.All(i => i?.Id == 0))
				return;

			// Батчим с последующим запросом AllIssueOperationForItems через Future,
			// чтобы оба запроса ушли в БД одним round-trip.
			if(UoW != null) {
				var ptIds = items.Select(x => x.ProtectionTools.Id).Distinct().ToArray();
				if(ptIds.Any()) {
					ProtectionToolsNomenclature ptnAlias = null;
					UoW.Session.QueryOver<ProtectionTools>()
						.WhereRestrictionOn(p => p.Id).IsIn(ptIds)
						.JoinAlias(p => p.ProtectionToolsNomenclatures, () => ptnAlias, JoinType.LeftOuterJoin)
						.Fetch(SelectMode.Fetch, p => p.ProtectionToolsNomenclatures)
						.Fetch(SelectMode.Fetch, () => ptnAlias.Nomenclature)
						.Future();
				}
			}

			var ops1 = dutyNormRepository.AllIssueOperationForItems(items);
			var operations = ops1.ToList();
			progress?.Add(text: "Добавляем несохранённые");
			if(notSavedOperations != null)
				operations.AddRange(notSavedOperations);
			var groups = operations.GroupBy(x => x.DutyNormItem.Id)
				.ToDictionary(x => x.Key, x => x.ToList());
			foreach(var item in items) {
				progress?.Add(text: $"Заполняем норму №{item.DutyNorm.Id} - {item.ProtectionTools.Name}");
				if(groups.TryGetValue(item.Id, out var ops)) 
					item.Graph = new IssueGraph(ops.ToList<IGraphIssueOperation>());
				else 
					item.Graph = new IssueGraph(new List<IGraphIssueOperation>());
			}
			progress?.Update("Готово");
			if(needClose)
				progress.Close();
		}
	}
}
