using System;
using System.Globalization;
using Gamma.Utilities;
using Gtk;
using QS.Cloud.Postomat.Manage;
using QS.Views.Dialog;
using Workwear.Domain.ClothingService;
using Workwear.ViewModels.ClothingService;

namespace Workwear.Views.ClothingService {
	public partial class ServiceClaimView : EntityDialogViewBase<ServiceClaimViewModel, ServiceClaim> {
		public ServiceClaimView(ServiceClaimViewModel viewModel) : base(viewModel) {
			this.Build();
			CommonButtonSubscription();

			framePostamat.Visible = ViewModel.PostomatVisible;
			barcodeinfoview1.ViewModel = ViewModel.BarcodeInfoViewModel;
			checkNeedRepair.Binding
				.AddBinding(ViewModel, e => e.NeedForRepair, w => w.Active)
				.AddBinding(ViewModel, v => v.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			textDefect.Binding
				.AddBinding(ViewModel, e => e.Defect, w => w.Buffer.Text)
				.AddBinding(ViewModel, v => v.DefectCanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			labelIsClosed.Binding
				.AddFuncBinding(ViewModel, e => "Закрыта: " + (e.IsClosed ? "Да" : "Нет"), w => w.LabelProp)
				.InitializeFromSource();
			
			comboPostomat.SetRenderTextFunc<PostomatInfo>(p => $"{p.Name} ({p.Location})");
			comboPostomat.Binding.AddSource(ViewModel)
				.AddBinding(v => v.CanEdit, w => w.Sensitive)
				.AddBinding(v => v.Postomats, w => w.ItemsList)
				.AddBinding(v => v.Postomat, w => w.SelectedItem)
				.InitializeFromSource();
			
			treeOperations.CreateFluentColumnsConfig<StateOperation>()
				.AddColumn("Время").AddReadOnlyTextRenderer(x => x.OperationTime.ToString("g"))
				.AddColumn("Статус").AddReadOnlyTextRenderer(x => x.State.GetEnumTitle())
				.AddColumn("Пользователь").AddReadOnlyTextRenderer(x => x.User?.Name)
				.AddColumn("Комментарий").AddReadOnlyTextRenderer(x => x.Comment)
				.Finish();
			treeOperations.Binding.AddSource(ViewModel)
				.AddBinding(v => v.States, w => w.ItemsDataSource)
				.InitializeFromSource();
			
			treeServices.CreateFluentColumnsConfig<SelectableProvidedService>()
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Услуга").AddReadOnlyTextRenderer(x => x.Label)
				.AddColumn("Количество").AddNumericRenderer(x => x.Amount)
					.Digits(2).WidthChars(8)
					.Adjustment(new Adjustment(0, 0, 100000, 0.1, 1, 1))
					.Editing(x => x.Select)
					.AddSetter((c, n) => { if(!n.Select) c.Text = String.Empty; })
				.AddColumn("Стоимость").AddReadOnlyTextRenderer(x => x.Select ? x.Entity.TotalCost.ToString(CultureInfo.InvariantCulture) : String.Empty)
				.AddColumn("Дата оказания").AddReadOnlyTextRenderer(x => x.Select ? x.Entity.ServiceDate.ToShortDateString().ToString(CultureInfo.InvariantCulture) : String.Empty)
				.Finish();
			treeServices.Binding.AddSource(ViewModel)
				.AddBinding(v => v.ProvideServices, w => w.ItemsDataSource)
				.InitializeFromSource();

			ytextComment.Binding
				.AddBinding(ViewModel,v => v.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
		}
	}
}
