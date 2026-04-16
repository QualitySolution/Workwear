using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.ViewModels.Extension;
using Workwear.Tools;

namespace workwear.Journal.ViewModels.Postomats {
	public class FullnessJournalViewModel : JournalViewModelBase, IDialogDocumentation {
		private readonly PostomatManagerService postomatService;
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("postomat.html#postamat-fullness");
		public string ButtonTooltip => DocHelper.GetDialogDocTooltip(Title);
		#endregion
		public FullnessJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			PostomatManagerService postomatService
			) : base(unitOfWorkFactory, interactiveService, navigation) {
			this.postomatService = postomatService ?? throw new ArgumentNullException(nameof(postomatService));
			Title = "Заполненность постаматов";
			SearchEnabled = false;
			
			DataLoader = new AnyDataLoader<FullnessInfo>(GetNodes);
		}

		/// <summary>
		/// Запоминаем время последнего запроса, чтобы при перерисовке журнала подсветка была корректная.
		/// </summary>
		public DateTime RequestTime { get; private set; }
		private IList<FullnessInfo> GetNodes(CancellationToken token) {
			RequestTime = DateTime.Now;
			var items = postomatService.GetFullness(token);
			return items;
		}

		#region Функции для табличного просмотра
		public string GetLongestPickupTooltip(FullnessInfo node) {
			var longest = node.Cells.OrderByDescending(x => x.CreateTime).FirstOrDefault();
			if(longest == null) {
				return null;
			}
			return "Дольше всего не забирают:" +
			       $"\nЯчейка {longest.CellTitle}" +
			       $"\n{longest.EmployeeFullName}" +
			       $"\n{longest.NomenclatureName}";
		}

		public string GetCellsTooltip(FullnessInfo node) {
			var cellsList = new List<string>();
			foreach(var cell in node.Cells.Where(c => !c.IsEmpty)) {
				cellsList.Add($"{cell.CellTitle} - {cell.CreateTime?.ToDateTime():g} - {cell.EmployeeFullName} - {cell.NomenclatureName}");
			}
			if (cellsList.Count == 0) 
				return null;
			
			return String.Join("\n", cellsList);
		}
		#endregion
	}
}
