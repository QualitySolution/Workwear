using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.Dialog;
using QS.Dialog.ViewModels;
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
using workwear.Repository.Operations;
using workwear.ViewModels.Company;

namespace workwear.ViewModels.Regulations
{
	public class NormViewModel : EntityDialogViewModelBase<Norm>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly IInteractiveQuestion interactive;

		public NormViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			EmployeeIssueRepository employeeIssueRepository,
			INavigationManager navigation, 
			IInteractiveQuestion interactive,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			employeeIssueRepository.RepoUow = UoW;
			this.interactive = interactive;

			NormConditions = UoW.GetAll<NormCondition>().ToList();
		}

		/// <summary>
		/// Копирует существующую в базе норму по id
		/// </summary>
		public void CopyNormFrom(int normId)
		{
			var norm = UoW.GetById<Norm>(normId);
			norm.CopyNorm(Entity);
		}

		#region Sensetive

		private bool saveSensitive = true;
		public virtual bool SaveSensitive {
			get => saveSensitive;
			set => SetField(ref saveSensitive, value);
		}

		private bool cancelSensitive = true;
		public virtual bool CancelSensitive {
			get => cancelSensitive;
			set => SetField(ref cancelSensitive, value);
		}

		#endregion

		#region Свойства
		public List<NormCondition> NormConditions { get; set; }
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
				Entity.AddPost(post);
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
				Entity.AddPost(post);
			}
		}

		public void RemoveProfession(Post[] professions)
		{
			foreach(var prof in professions) {
				Entity.RemovePost(prof);
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
					mes += "\nОткрытые диалоги этих сотрудников будут закрыты.\nВы уверены что хотите выполнить удаление?";
					logger.Info("Ок");
					if(!interactive.Question(mes))
						return;
				}
			}
			Entity.RemoveItem(toRemove);

			if(worksEmployees != null) {
				SaveSensitive = CancelSensitive = false;
				var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(this); 
				progressPage.ViewModel.Progress.Start(worksEmployees.Count, text: "Обработка сотрудников...");

				foreach(var emp in worksEmployees) {
					emp.UoW = UoW;
					emp.UpdateWorkwearItems();
					UoW.Save(emp);
					progressPage.ViewModel.Progress.Add();
				}

				SaveSensitive = CancelSensitive = true;
				NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
			}
		}

		public void ReplaceNomenclature(NormItem item)
		{
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.Tag = item;
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			page.ViewModel.OnSelectResult += ProtectionReplace_OnSelectResult;
		}

		void ProtectionReplace_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var page = NavigationManager.FindPage((DialogViewModelBase)sender);
			var item = (NormItem)page.Tag;
			var newProtectionTools = UoW.GetById<ProtectionTools>(e.GetSelectedObjects<ProtectionToolsJournalNode>().First().Id);
			item.ProtectionTools = newProtectionTools;

			if(item.Id > 0) {
				logger.Info("Поиск ссылок на заменяемую строку нормы...");
				IList<EmployeeCard> worksEmployees = EmployeeRepository.GetEmployeesDependenceOnNormItem(UoW, item);
				var operations = employeeIssueRepository.GetOperationsForNormItem(item, q => q.Fetch(SelectMode.Fetch, x => x.Employee));
				if(worksEmployees.Count > 0) {
					var names = worksEmployees.Union(operations.Select(x => x.Employee)).Distinct().Select(x => x.ShortName).ToList();
					var mes = "Замена номенклатуры нормы затронет потребности и прошлые выдачи следующих сотрудников:\n";
					mes += String.Join(", ", names.Take(50));
					if(names.Count > 50)
						mes += String.Format("\n... и еще {0}", names.Count - 50);
					mes += "\nОткрытые диалоги этих сотрудников будут закрыты. Норма будет сохранена.\nВы уверены что хотите выполнить замену?";
					logger.Info("Ок");
					if(!interactive.Question(mes))
						return;

					SaveSensitive = CancelSensitive = false;
					var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(this);
					progressPage.ViewModel.Progress.Start(worksEmployees.Count + operations.Count + 1, text: "Обработка сотрудников...");
					foreach(var emp in worksEmployees) {
						emp.UoW = UoW;
						foreach(var employeeItem in emp.WorkwearItems) {
							if(item.IsSame(employeeItem.ActiveNormItem))
								employeeItem.ProtectionTools = newProtectionTools;
						}
						UoW.Save(emp);
						progressPage.ViewModel.Progress.Add();
					}
					logger.Info($"Заменены потребности у {worksEmployees.Count} сотрудников");
					progressPage.ViewModel.Progress.Update("Обработка произведенных выдач...");
					foreach(var operation in operations) {
						operation.ProtectionTools = newProtectionTools;
						UoW.Save(operation);
						progressPage.ViewModel.Progress.Add();
					}
					logger.Info($"Заменены номенклатуры нормы в {operations.Count} операциях");
					progressPage.ViewModel.Progress.Update("Сохранение нормы...");
					Save();
					progressPage.ViewModel.Progress.Add();
					SaveSensitive = CancelSensitive = true;
					NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
				}
			}
		}
		#endregion
		#endregion
	}
}
