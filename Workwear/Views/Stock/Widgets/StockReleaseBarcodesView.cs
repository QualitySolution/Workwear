using System;
using System.Text.RegularExpressions;
using Gtk;
using QS.Navigation;
using QS.Views.Dialog;
using QSOrmProject;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.Views.Stock.Widgets 
{
	public partial class StockReleaseBarcodesView : DialogViewBase<StockReleaseBarcodesViewModel>
	{
		public StockReleaseBarcodesView(StockReleaseBarcodesViewModel viewModel) : base(viewModel)
		{
			this.Build();

			labelDescription.Text = ViewModel.Description;
			amountSpin.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.WithoutBarcodesAmount, w => w.Adjustment.Upper)
				.AddBinding(vm => vm.SelectedAmount, w => w.ValueAsInt)
				.InitializeFromSource();
			createBarcodesButton.Binding
				.AddBinding(ViewModel, vm => vm.ConfirmButtonSensetive, w => w.Sensitive)
				.InitializeFromSource();
			
			labelAllNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.NomenclatureAmount, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();
			labelWithoutBarcodes.Binding
				.AddBinding(ViewModel, vm => vm.WithoutBarcodesAmount, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();
			labelWithBarcodesAmount.Binding
				.AddBinding(ViewModel, vm => vm.WithBarcodesAmount, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();
			labelBarcodesInStock.Binding
				.AddBinding(ViewModel, vm => vm.BarcodesInStockAmount, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();
			
			entryBarcodesLabel.Binding.AddBinding(ViewModel, vm => vm.Label, w => w.Text).InitializeFromSource();
			
			ConfigureAutocomplete();
		}

		private void ConfigureAutocomplete() 
		{
			ListStore store = new ListStore(typeof(string));
			foreach(string label in ViewModel.Labels) 
			{
				store.AppendValues(label);
			}
			
			entryBarcodesLabel.Completion = new EntryCompletion();
			entryBarcodesLabel.Completion.Model = store;
			entryBarcodesLabel.Completion.PopupCompletion = true;
			entryBarcodesLabel.Completion.TextColumn = 0;
			entryBarcodesLabel.Completion.MinimumKeyLength = 0;
			entryBarcodesLabel.Completion.MatchSelected += Completion_MatchSelected;
			entryBarcodesLabel.Completion.MatchFunc = Completion_MatchFunc;
			
			CellRenderer cell = entryBarcodesLabel.Completion.Cells[0];
			entryBarcodesLabel.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
		}
		
		private bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter) 
		{
			string val = (string)completion.Model.GetValue(iter, 0) ?? string.Empty;
			return Regex.IsMatch(val, Regex.Escape(key), RegexOptions.IgnoreCase);
		}

		[GLib.ConnectBefore]
		private void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			ViewModel.Label = (string)args.Model.GetValue(args.Iter, 0);
			args.RetVal = true;
		}
		
		private void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter) 
		{
			string value = (string)tree_model.GetValue(iter, 0) ?? String.Empty;
			string pattern = Regex.Escape(ViewModel.Label);
			value = Regex.Replace(value, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			(cell as CellRendererText).Markup = value;
		}
		
		protected void OnButtonCancel(object sender, System.EventArgs e)
		{
			ViewModel.Close(false, CloseSource.Self);
		}

		protected void OnCreateBarcodesButtonClicked(object sender, System.EventArgs e) 
		{
			ViewModel.CreateBarcodes();
		}
	}
}
