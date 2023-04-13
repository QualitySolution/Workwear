﻿using System;
using Gtk;
using QS.Views.Dialog;
using QS.Widgets.GtkUI;
using QSOrmProject;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

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
					.AddColumn ("Номенклатура").AddReadOnlyTextRenderer(e => e?.Nomenclature?.Name).WrapWidth(800)
					.AddColumn ("Выдано").AddReadOnlyTextRenderer(e => e.Amount.ToString())
					.AddColumn("Износ").AddTextRenderer(e => e.WearPercentBefore.ToString("P0") ?? String.Empty)
					.AddColumn ("Дата списания").AddReadOnlyTextRenderer(e => e.WriteOffDateBefore?.ToShortDateString() ??  String.Empty)
					.AddColumn ("Установить износ").AddNumericRenderer (e => e.WearPercentAfter, new MultiplierToPercentConverter())
						.Editing (new Adjustment(0,0,999,1,10,0)).WidthChars(6).Digits(0)
						.AddTextRenderer (e => "%", expand: false)
					//.AddColumn ("Автосписание").AddToggleRenderer(e => e.UseAutoWriteoff).Editing()
					.AddColumn ("Установить дату списания").AddDateRenderer(e => e.WriteOffDateAfter).Editable()
					.AddColumn("Отметка об износе").AddTextRenderer(e => e.Cause).Editable()
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
		private void Items_Selection_Changed(object sender, EventArgs e){
			ybuttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}
	}
}
