using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using QS.Project.Domain;
using QS.Views.Dialog;
using QSOrmProject;
using QSWidgetLib;
using Workwear.Domain.Company;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace Workwear.Views.Stock
{
	public partial class WriteOffView : EntityDialogViewBase<WriteOffViewModel, Writeoff>
	{
		public WriteOffView(WriteOffViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			ConfigureMembers();
			ConfigureItems();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			entityentryDirectorPerson.ViewModel = ViewModel.ResponsibleDirectorPersonEntryViewModel;
			entityentryChairmanPerson.ViewModel = ViewModel.ResponsibleChairmanPersonEntryViewModel;
			entityentryOrganization.ViewModel = ViewModel.ResponsibleOrganizationEntryViewModel;
			
				entryId.Binding.AddSource(ViewModel)
					.AddBinding(vm => vm.DocNumber, w => w.Text)
					.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
					.InitializeFromSource();
				checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource(); 
				ylabelCreatedBy.Binding
					.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
					.InitializeFromSource ();
				ydateDoc.Binding
					.AddBinding(Entity, e => e.Date, w => w.Date)
					.InitializeFromSource();
				ytextComment.Binding
					.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
					.InitializeFromSource();
				labelSum.Binding
					.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp)
					.InitializeFromSource();
				buttonDel.Binding
					.AddBinding(ViewModel, vm => vm.DelSensitive, w => w.Sensitive)
					.InitializeFromSource();

				buttonAddStore.Sensitive = ViewModel.Employee is null;
				buttonAddStore.Clicked += OnButtonAddStoreClicked;
				buttonAddWorker.Clicked += OnButtonAddFromEmployeeClicked;
				buttonDel.Clicked += OnButtonDelClicked;
				
				ytreeMembers.Selection.Changed += Members_Selection_Changed;
				ybuttonAddMember.Clicked += OnButtonAddMembersClicked;
				ybuttonDelMember.Clicked += OnButtonDelMembersClicked;
				buttonPrint.Clicked += OnButtonPrintClicked;
		}
		private void ConfigureItems()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<WriteoffItem> ()
					.AddColumn ("Наименование").Resizable().AddReadOnlyTextRenderer(e => e.Nomenclature?.Name ?? e.EmployeeWriteoffOperation.ProtectionTools?.Name)
						.AddSetter((w, item) => w.Foreground = item.Nomenclature != null ? "black" : "blue")
						.WrapWidth(700)
					.AddColumn("Размер").MinWidth(60)
						.AddReadOnlyTextRenderer(x => x.WearSize?.Name)
					.AddColumn("Рост").MinWidth(70)
						.AddReadOnlyTextRenderer(x => x.Height?.Name)
					.AddColumn("Собственники")
						.Visible(ViewModel.FeaturesService.Available(WorkwearFeature.Owners))
						.AddReadOnlyTextRenderer(x => x.Owner?.Name ?? "Нет")
					.AddColumn ("Процент износа").AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
						.Editing(new Adjustment(0, 0, 999, 1, 10, 0)).WidthChars(6).Digits(0)
						.AddTextRenderer(e => "%", expand: false)
					.AddColumn ("Списано из")
						.AddTextRenderer (e => e.LastOwnText)
					.AddColumn ("Количество")
						.AddNumericRenderer (e => e.Amount)
						.Editing (new Adjustment(0, 0, 100000, 1, 10, 1))
						.WidthChars(7)
					.AddColumn("Причина списания")
						.AddTextRenderer(e => e.Cause).WrapWidth(800).Editable()
					.Finish ();
			
			ytreeItems.Binding
				.AddBinding(Entity, vm => vm.Items, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeItems.Selection.Changed += YtreeItems_Selection_Changed;
			ytreeItems.ButtonReleaseEvent += YtreeItems_ButtonReleaseEvent;
		}
		
		private void ConfigureMembers() {
			ytreeMembers.ColumnsConfig = FluentColumnsConfig<Leader>.Create()
				.AddColumn("ФИО").AddTextRenderer(l => l.Title)
				.AddColumn("Должность").AddTextRenderer(l => l.Position)
				.Finish();
			ytreeMembers.ItemsDataSource = Entity.Members;
		}

		#region Methods
		private void YtreeItems_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if (args.Event.Button != 3) return;
			var menu = new Menu();
			var selected = ytreeItems.GetSelectedObject<WriteoffItem>();
			var item = new MenuItemId<WriteoffItem>("Открыть номенклатуру");
			item.ID = selected;
			if(selected?.Nomenclature != null)
				item.Activated += Item_Activated;
			else
				item.Sensitive = false;
			menu.Add(item);
			menu.ShowAll();
			menu.Popup();
		}
		private void Item_Activated(object sender, EventArgs e) {
			var item = ((MenuItemId<WriteoffItem>) sender).ID;
			ViewModel.NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(
				ViewModel, EntityUoWBuilder.ForOpen(item.Nomenclature.Id));
		}

		private void YtreeItems_Selection_Changed(object sender, EventArgs e) => 
			ViewModel.DelSensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		protected void OnButtonAddStoreClicked(object sender, EventArgs e) => ViewModel.AddFromStock();

		private void OnButtonDelClicked(object sender, EventArgs e) => 
			ViewModel.DeleteItem(ytreeItems.GetSelectedObject<WriteoffItem>());

		private void OnButtonPrintClicked(object sender, EventArgs e) => ViewModel.Print();
		private void OnButtonAddFromEmployeeClicked(object sender, EventArgs e) => ViewModel.AddFromEmployee();
		private void OnButtonAddMembersClicked(object sender, EventArgs e) => ViewModel.AddMembers();
		private void OnButtonDelMembersClicked(object sender, EventArgs e) => ViewModel.DeleteMember(ytreeMembers.GetSelectedObject<Leader>());
		private void Members_Selection_Changed(object sender, EventArgs e){
			ybuttonDelMember.Sensitive = ytreeMembers.Selection.CountSelectedRows() > 0;
		}
		#endregion
	}
}
