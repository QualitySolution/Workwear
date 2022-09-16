using Gamma.Binding.Converters;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views;
using Workwear.ViewModels.Import;

namespace workwear.Views.Import
{
	public partial class CountersView : ViewBase<CountersViewModel>
	{
		public CountersView(CountersViewModel viewModel) : base(viewModel)
		{
			this.Build();
			BuildWidget();
		}

		private void BuildWidget() {
			tableCounters.NRows = (uint)ViewModel.Counters.Count;
			uint nrow = 0;
			var converter = new NumbersToStringConverter();
			foreach(var counter in ViewModel.Counters.Values) {
				var label = new yLabel();
				label.Xalign = 1;
				label.Binding
					.AddFuncBinding(counter, c => c.Title + ":", w => w.LabelProp)
					.InitializeFromSource();
				tableCounters
					.Attach(label, 0, 1, nrow, nrow + 1, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);
				var labelCount = new yLabel();
				labelCount.WidthChars = 5;
				labelCount.Binding
					.AddBinding(counter, c => c.Count, w => w.LabelProp, converter)
					.InitializeFromSource();
				tableCounters
					.Attach(labelCount, 1, 2, nrow, nrow + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
				nrow++;
			}
			tableCounters.ShowAll();
		}
	}
}
