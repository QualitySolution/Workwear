using System;
using Gamma.Utilities;
using QS.Views.Dialog;
using workwear.Tools.IdentityCards;
using workwear.ViewModels.Stock;

namespace workwear.Views.Stock
{
	public partial class IssueByIdentifierView : DialogViewBase<IssueByIdentifierViewModel>
	{
		public IssueByIdentifierView(IssueByIdentifierViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ylabelStatus.Binding.AddSource(ViewModel)
				.AddBinding(v => v.CurentState, w => w.LabelProp)
				.InitializeFromSource();
			eventboxStatus.ModifyBg(Gtk.StateType.Normal, ColorUtil.Create(ViewModel.CurrentStateColor));

			comboDevice.SetRenderTextFunc<DeviceInfo>(x => x.Title);
			comboDevice.Binding.AddSource(viewModel)
				.AddBinding(v => v.Devices, w => w.ItemsList)
				.AddBinding(v => v.SelectedDevice, w => w.SelectedItem)
				.InitializeFromSource();
			checkSettings.Binding.AddBinding(viewModel, v => v.ShowSettings, w => w.Active).InitializeFromSource();
			tableSettings.Binding.AddBinding(viewModel, v => v.ShowSettings, w => w.Visible).InitializeFromSource();

			ytreeviewCardTypes.CreateFluentColumnsConfig<CardType>()
				.AddColumn("Вкл.").AddToggleRenderer(x => x.Active).Editing()
				.AddColumn("Тип карты").AddTextRenderer(x => x.Title)
				.Finish();
			ytreeviewCardTypes.ItemsDataSource = ViewModel.CardFamilies;

			viewModel.PropertyChanged += ViewModel_PropertyChanged;

		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof( ViewModel.CurrentStateColor))
				eventboxStatus.ModifyBg(Gtk.StateType.Normal, ColorUtil.Create(ViewModel.CurrentStateColor));
		}
	}
}
