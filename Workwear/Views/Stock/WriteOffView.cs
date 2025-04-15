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
					.AddBinding(vm => vm.DocNumberText, w => w.Text)
					.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
					.InitializeFromSource();
				checkAuto.Binding
					.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active)
					.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource(); 
				ylabelCreatedBy.Binding
					.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource ();
				ydateDoc.Binding
					.AddBinding(ViewModel, vm => vm.DocumentDate, w => w.Date)
					.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextComment.Binding
					.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
					.AddBinding(ViewModel,vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
				labelSum.Binding
					.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp).InitializeFromSource();
				buttonDel.Binding
					.AddBinding(ViewModel, vm => vm.DelSensitive, w => w.Sensitive).InitializeFromSource();

				buttonAddStore.Sensitive = ViewModel.CanEdit && ViewModel.Employee == null && ViewModel.DutyNorm == null;
				buttonAddWorker.Sensitive = ViewModel.CanEdit && ViewModel.DutyNorm == null;
				buttonAddDutyNorm.Sensitive = ViewModel.CanEdit && ViewModel.Employee is null;
				
				buttonAddStore.Clicked += (s,e) => ViewModel.AddFromStock();
				buttonAddWorker.Clicked += (s,e) => ViewModel.AddFromEmployee();
				buttonAddDutyNorm.Clicked += (s,e) => ViewModel.AddFromDutyNorm();
				buttonDel.Clicked += (s,e) => ViewModel.DeleteItem(ytreeItems.GetSelectedObject<WriteoffItem>());

				ytreeMembers.Selection.Changed += YtreeItems_Selection_Changed;
				ybuttonAddMember.Sensitive = ViewModel.CanEdit;
				ybuttonAddMember.Clicked += (s, e) => ViewModel.AddMembers();
				ybuttonDelMember.Clicked += (s,e)  => ViewModel.DeleteMember(ytreeMembers.GetSelectedObject<Leader>());

				buttonPrint.Clicked += (s,e) => ViewModel.Print();
		}

		private void ConfigureItems()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<WriteoffItem> ()
					.AddColumn ("Наименование").Resizable().AddReadOnlyTextRenderer(e => e.Nomenclature?.Name ?? e.EmployeeWriteoffOperation.ProtectionTools?.Name)
						.AddSetter((w, item) => w.Foreground = item.Nomenclature != null ? "black" : "blue")
						.WrapWidth(700)
					.AddColumn("Размер").MinWidth(60)
						.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
						.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.SizeType, onlyUseInNomenclature:true).ToList())
						.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.SizeType != null)
					.AddColumn("Рост").MinWidth(70)
						.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
						.DynamicFillListFunc(x => ViewModel.SizeService.GetSize(ViewModel.UoW, x.Nomenclature?.Type?.HeightType, onlyUseInNomenclature:true).ToList())
						.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.Nomenclature?.Type?.SizeType != null)
					.AddColumn("Собственники")
						.Visible(ViewModel.FeaturesService.Available(WorkwearFeature.Owners))
						.AddComboRenderer(x => x.Owner)
						.SetDisplayFunc(x => x.Name)
						.FillItems(ViewModel.Owners, "Нет")
						.AddSetter((c, n) => c.Editable = ViewModel.CanEdit && n.CanSetOwner)
					.AddColumn ("Процент износа").AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
						.Editing(new Adjustment(0, 0, 999, 1, 10, 0), ViewModel.CanEdit).WidthChars(6).Digits(0)
						.AddTextRenderer(e => "%", expand: false)
					.AddColumn ("Списано из").AddTextRenderer (e => e.LastOwnText)
					.AddColumn ("Количество").AddNumericRenderer (e => e.Amount)
						.Editing (new Adjustment(0, 0, 100000, 1, 10, 1), ViewModel.CanEdit).WidthChars(7)
					.AddReadOnlyTextRenderer(e => e.Nomenclature?.Type?.Units?.Name ?? e.EmployeeWriteoffOperation.ProtectionTools?.Type?.Units?.Name)
					.AddColumn("Причина списания")
						.AddComboRenderer(x=>x.CausesWriteOff)
						.SetDisplayFunc(x=>x.Name)
						.FillItems(ViewModel.CausesWriteOffs, "Нет")
						.Editing(ViewModel.CanEdit)
					.AddColumn("Комментарий")
						.AddTextRenderer(e=>e.Comment)
						.Editable(ViewModel.CanEdit)
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
			ybuttonDelMember.Sensitive = ViewModel.CanEdit && ytreeMembers.Selection.CountSelectedRows() > 0;
		#endregion
	}
}
