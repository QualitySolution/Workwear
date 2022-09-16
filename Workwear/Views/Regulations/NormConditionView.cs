using System;
using System.Globalization;
using QS.Views.Dialog;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Regulations;

namespace workwear.Views.Regulations
{
	public partial class NormConditionView : EntityDialogViewBase<NormConditionViewModel, NormCondition>
	{
		private NormConditionViewModel viewModel;
		public NormConditionView(NormConditionViewModel viewModel) : base(viewModel)
		{
			this.viewModel = viewModel;
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		public void ConfigureDlg() 
		{
			yenumSex.ItemsEnum = typeof(SexNormCondition);
			yenumSex.Binding.AddBinding(Entity, e => e.SexNormCondition, w => w.SelectedItem).InitializeFromSource();

			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yStartMonth.SetRenderTextFunc<DateTime>(m => m.ToString("MMM", new CultureInfo("ru-RU")));
			yStartMonth.Binding.AddSource(viewModel)
				.AddBinding(vm => vm.Months, v => v.ItemsList)
				.AddBinding(vm => vm.SelectedStartMonth, v => v.SelectedItem)
				.InitializeFromSource();
			if(Entity.IssuanceStart != null)
				yStartMonth.SelectedItem = new DateTime(2001, Entity.IssuanceStart.Value.Month, 1);
			
			yEndMonth.SetRenderTextFunc<DateTime>(m => m.ToString("MMM", new CultureInfo("ru-RU")));
			yEndMonth.Binding.AddSource(viewModel)
				.AddBinding(vm => vm.Months, v => v.ItemsList)
				.AddBinding(vm => vm.SelectedEndMonth, v => v.SelectedItem)
				.InitializeFromSource();
			if(Entity.IssuanceEnd != null)
				yEndMonth.SelectedItem = new DateTime(2001, Entity.IssuanceEnd.Value.Month, 1);
			
			yStartDay.SetRenderTextFunc<int>(m => m.ToString());
			yStartDay.Binding.AddSource(viewModel)
				.AddBinding(vm => vm.StartDays, v => v.ItemsList)
				.AddBinding(vm => vm.StartDay, v => v.SelectedItem)
				.InitializeFromSource();
			if (Entity.IssuanceStart != null)
				yStartDay.SelectedItem = Entity.IssuanceStart.Value.Day;
			
			yEndDay.SetRenderTextFunc<int>(m => m.ToString());
			yEndDay.Binding.AddSource(viewModel)
				.AddBinding(vm => vm.EndDays, v => v.ItemsList)
				.AddBinding(vm => vm.EndDay, v => v.SelectedItem)
				.InitializeFromSource();
			if (Entity.IssuanceEnd != null)
				yEndDay.SelectedItem = Entity.IssuanceEnd.Value.Day;
		}
	}
}
