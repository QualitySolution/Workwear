using System;
using Gamma.Utilities;
using QS.Views.Dialog;
using Workwear.Domain.ClothingService;
using Workwear.ViewModels.ClothingService;

namespace Workwear.Views.ClothingService {
	public partial class ServiceClaimView : EntityDialogViewBase<ServiceClaimViewModel, ServiceClaim> {
		public ServiceClaimView(ServiceClaimViewModel viewModel) : base(viewModel) {
			this.Build();
			CommonButtonSubscription();

			barcodeinfoview1.ViewModel = ViewModel.BarcodeInfoViewModel;
			checkNeedRepair.Binding
				.AddBinding(Entity, e => e.NeedForRepair, w => w.Active)
				.AddBinding(ViewModel, v => v.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			textDefect.Binding
				.AddBinding(Entity, e => e.Defect, w => w.Buffer.Text)
				.AddBinding(ViewModel, v => v.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			labelIsClosed.Binding
				.AddFuncBinding(Entity, e => "Закрыта: " + (e.IsClosed ? "Да" : "Нет"), w => w.LabelProp)
				.InitializeFromSource();
			
			treeOperations.CreateFluentColumnsConfig<StateOperation>()
				.AddColumn("Время").AddReadOnlyTextRenderer(x => x.OperationTime.ToString("g"))
				.AddColumn("Статус").AddReadOnlyTextRenderer(x => x.State.GetEnumTitle())
				.AddColumn("Пользователь").AddReadOnlyTextRenderer(x => x.User?.Name)
				.AddColumn("Комментарий").AddReadOnlyTextRenderer(x => x.Comment)
				.Finish();

			treeOperations.Binding
				.AddSource(Entity)
				.AddBinding(v => v.States, w => w.ItemsDataSource)
				.InitializeFromSource();
		}
	}
}
