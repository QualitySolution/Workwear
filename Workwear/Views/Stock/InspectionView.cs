using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Utilities.Numeric;
using QS.Views.Dialog;
using QSOrmProject;
using QSWidgetLib;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	public partial class InspectionView : EntityDialogViewBase<InspectionViewModel, Inspection> {
		public InspectionView(InspectionViewModel viewModel) : base(viewModel) {
			Build();
			ConfigureDlg();
			ConfigureMembers();
			ConfigureItems();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() {
			entityentryDirectorPerson.ViewModel = ViewModel.ResponsibleDirectorPersonEntryViewModel;
			entityentryChairmanPerson.ViewModel = ViewModel.ResponsibleChairmanPersonEntryViewModel;
			entityentryOrganization.ViewModel = ViewModel.ResponsibleOrganizationEntryViewModel;
			
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
			ylabelUser.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelSum.Binding
				.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp).InitializeFromSource();
			ytreeMembers.Selection.Changed += Members_Selection_Changed;
			ybuttonAddMember.Clicked += OnButtonAddMembersClicked;
			ybuttonDelMember.Clicked += OnButtonDelMembersClicked;
			ytreeItems.Selection.Changed += Items_Selection_Changed;
			ybuttonDel.Clicked += OnButtonDelClicked;
			ybuttonAdd.Clicked += OnButtonAddClicked;
			buttonPrint.Clicked += OnButtonPrintClicked;
		}

		private void ConfigureMembers() {
			ytreeMembers.ColumnsConfig = FluentColumnsConfig<InspectionMember>.Create()
				.AddColumn("ФИО").AddTextRenderer(l => l.Member.Title)
				.AddColumn("Должность").AddTextRenderer(l => l.Member.Position)
				.Finish();
			ytreeMembers.ItemsDataSource = Entity.ObservableMembers;
		}
		
		private void ConfigureItems()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<InspectionItem> ()
					.AddColumn ("Сотрудник").AddReadOnlyTextRenderer(e => e.Employee.FullName)
					.AddColumn("Номер\nкарточки").AddTextRenderer(e => e.EmployeeNumber)
					.AddColumn ("Номенклатура").AddReadOnlyTextRenderer(e => e?.Nomenclature?.Name).WrapWidth(1000)
					.AddColumn ("Выдано").AddReadOnlyTextRenderer(e => e.Amount.ToString())
					.AddColumn ("Дата\nвыдачи").AddReadOnlyTextRenderer(e => e.IssueDate?.ToShortDateString() ?? "")
					.AddColumn ("Выдано до").AddReadOnlyTextRenderer(e => e.ExpiryByNormBefore?.ToShortDateString() ??  "до износа")
					.AddColumn ("% износа на\nдату выдачи").AddReadOnlyTextRenderer((e => ((int)(e.WearPercentBefore * 100))
						.Clamp(0, 100) + "%"))
					.AddColumn ("Установить\n% износа").AddNumericRenderer (e => e.WearPercentAfter, new MultiplierToPercentConverter())
						.Editing (new Adjustment(0,0,999,1,10,0)).WidthChars(6).Digits(0)
						.AddTextRenderer (e => "%", expand: false)
					.AddColumn("Списать").AddToggleRenderer(e => e.Writeoff).Editing()
					.AddColumn ("Продлить").AddDateRenderer(e => e.ExpiryByNormAfter).Editable()
					.AddColumn("Отметка об износе").AddTextRenderer(e => e.Cause).WrapWidth(800).Editable()
					.Finish ();
			ytreeItems.Binding
				.AddBinding(Entity, vm => vm.ObservableItems, w => w.ItemsDataSource)
				.InitializeFromSource();
		}

		private void OnButtonAddMembersClicked(object sender, EventArgs e) => ViewModel.AddMembers();
		private void OnButtonDelMembersClicked(object sender, EventArgs e) => ViewModel.DeleteMember(ytreeMembers.GetSelectedObject<InspectionMember>());
		private void Members_Selection_Changed(object sender, EventArgs e){
			ybuttonDelMember.Sensitive = ytreeMembers.Selection.CountSelectedRows() > 0;
		}
		
		private void OnButtonDelClicked(object sender, EventArgs e) => ViewModel.DeleteItem(ytreeItems.GetSelectedObject<InspectionItem>());
		private void OnButtonAddClicked(object sender, EventArgs e) => ViewModel.AddItems();
		private void OnButtonPrintClicked(object sender, EventArgs e) => ViewModel.Print();
		private void Items_Selection_Changed(object sender, EventArgs e){
			ybuttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}
		
		#region PopupMenu
		void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<InspectionItem>();
			
			var itemOpenEmployee = new MenuItemId<InspectionItem>("Открыть сотрудника");
			itemOpenEmployee.ID = selected;
			itemOpenEmployee.Sensitive = selected.Employee != null;
			itemOpenEmployee.Activated += ItemOpenEmployee_Activated;
			menu.Add(itemOpenEmployee);
			
			menu.ShowAll();
			menu.Popup();
		}
		
		void ItemOpenEmployee_Activated(object sender, EventArgs e) {
			ViewModel.OpenEmployee(((MenuItemId<InspectionItem>) sender).ID);
		}	
		#endregion
	}
}
