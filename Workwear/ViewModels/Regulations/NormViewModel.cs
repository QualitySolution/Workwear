using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using workwear.Repository.Company;
using workwear.ViewModels.Company;

namespace workwear.ViewModels.Regulations
{
	public class NormViewModel : EntityDialogViewModelBase<Norm>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IInteractiveQuestion interactive;
		private readonly IProgressBarDisplayable progressBar;

		public NormViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation, IInteractiveQuestion interactive, IProgressBarDisplayable progressBar, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.interactive = interactive;
			this.progressBar = progressBar;
		}

		#region Sensetive

		private bool saveSensitive;
		public virtual bool SaveSensitive {
			get => saveSensitive;
			set => SetField(ref saveSensitive, value);
		}

		private bool cancelSensitive;
		public virtual bool CancelSensitive {
			get => cancelSensitive;
			set => SetField(ref cancelSensitive, value);
		}

		#endregion

		#region Действия View
		#region Профессии

		public void NewProfession()
		{
			var page = NavigationManager.OpenViewModel<PostViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);
			page.PageClosed += NewPost_PageClosed;
		}

		void NewPost_PageClosed(object sender, PageClosedEventArgs e)
		{
			if(e.CloseSource == CloseSource.Save) {
				var page = sender as IPage<PostViewModel>;
				var post = UoW.GetById<Post>(page.ViewModel.Entity.Id);
				Entity.AddProfession(post);
			}
		}

		public void AddProfession()
		{
			var page = NavigationManager.OpenViewModel<PostJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Post_OnSelectResult;
		}

		void Post_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var postNode in e.SelectedObjects) {
				var post = UoW.GetById<Post>(postNode.GetId());
				Entity.AddProfession(post);
			}
		}

		public void RemoveProfession(Post[] professions)
		{
			foreach(var prof in professions) {
				Entity.RemoveProfession(prof);
			}
		}
		#endregion
		#region Строки нормы
		public void AddItem()
		{
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Protection_OnSelectResult;
		}

		void Protection_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var protectionNode in e.SelectedObjects) {
				var protectionTools = UoW.GetById<ProtectionTools>(protectionNode.GetId());
				Entity.AddItem(protectionTools);
			}
		}

		public void RemoveItem(NormItem toRemove)
		{
			IList<EmployeeCard> worksEmployees = null;

			if(toRemove.Id > 0) {
				logger.Info("Поиск ссылок на удаляемую строку нормы...");
				worksEmployees = EmployeeRepository.GetEmployeesDependenceOnNormItem(UoW, toRemove);
				if(worksEmployees.Count > 0) {
					List<string> operations = new List<string>();
					foreach(var emp in worksEmployees) {
						bool canSwitch = emp.UsedNorms.SelectMany(x => x.Items)
							.Any(i => i.Id != toRemove.Id && i.ProtectionTools.Id == toRemove.ProtectionTools.Id);
						if(canSwitch)
							operations.Add(String.Format("* У сотрудника {0} требование спецодежды будет пререключено на другую норму.", emp.ShortName));
						else
							operations.Add(String.Format("* У сотрудника {0} будет удалено требование выдачи спецодежды.", emp.ShortName));
					}

					var mes = "При удалении строки нормы будут выполнены следующие операции:\n";
					mes += String.Join("\n", operations.Take(10));
					if(operations.Count > 10)
						mes += String.Format("\n... и еще {0}", operations.Count - 10);
					mes += "\nВы уверены что хотите выполнить удаление?";
					logger.Info("Ок");
					if(!interactive.Question(mes))
						return;
				}
			}
			Entity.RemoveItem(toRemove);

			if(worksEmployees != null) {
				SaveSensitive = CancelSensitive = false;
				progressBar.Start(worksEmployees.Count);

				foreach(var emp in worksEmployees) {
					emp.UoW = UoW;
					emp.UpdateWorkwearItems();
					UoW.Save(emp);
					progressBar.Add();
				}

				SaveSensitive = CancelSensitive = true;
				progressBar.Close();
			}
		}
		#endregion
		#endregion
	}
}
