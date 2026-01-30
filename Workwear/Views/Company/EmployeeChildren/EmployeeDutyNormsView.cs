using System;
using Gamma.ColumnConfig;
using Gtk;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace Workwear.Views.Company.EmployeeChildren {
	public partial class EmployeeDutyNormsView : Gtk.Bin {
		public EmployeeDutyNormsView() {
			this.Build();
			ybuttonGiveWearByDutyNorm.Sensitive = ybuttonOpenDutyNorm.Sensitive = false;
		}
	
		private EmployeeDutyNormsViewModel viewModel;

		public EmployeeDutyNormsViewModel ViewModel {
			get => viewModel;
			set {
				viewModel = value;
				ConfigureTable();
			}
		}

		private void ConfigureTable() 
		{
			ytreeview.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DutyNormsItemsList, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeview.ColumnsConfig = FluentColumnsConfig<DutyNormItem>.Create()
				.AddColumn("Норма №")
					.AddTextRenderer(i => i.DutyNorm.Id.ToString())
				.AddColumn("Наименование").Resizable()
					.AddTextRenderer(i  => i.ProtectionTools != null ? i.ProtectionTools.Name : null).WrapWidth(700)
				.AddColumn("По норме")
					.AddNumericRenderer(i => i.Amount)
					.AddTextRenderer(i => i.AmountUnitText(i.Amount))
				.AddColumn("Срок службы")
					.AddNumericRenderer(i=>i.PeriodCount)
					.AddSetter((c, n) => c.Visible = n.NormPeriod != DutyNormPeriodType.Wearout)
					.AddEnumRenderer(i=>i.NormPeriod)
					.AddSetter((c,n) => c.Text = n.PeriodText )
				.AddColumn("Числится")
					.AddTextRenderer(i => i.Issued(DateTime.Now)!=0 ? i.Issued(DateTime.Now).ToString() : String.Empty)
					.AddSetter((w, i) => w.Foreground = i.DutyNorm.Archival ? "gray" : i.AmountColor)
					.AddTextRenderer(i => i.AmountUnitText(i.Issued(DateTime.Now)))
				.AddColumn("След. получение")
					.AddTextRenderer(i => i.NextIssueText)
					.AddSetter((w, i) => w.Foreground = i.DutyNorm.Archival ? "gray" : i.NextIssueColor)
				.AddColumn("Пункт норм").Resizable()
					.AddTextRenderer(x => x.NormParagraph)
				.AddColumn("Комментарий")
					.AddTextRenderer(x => x.Comment)
				.RowCells()
				.AddSetter<CellRendererText>((c,x) => c.Foreground = x.DutyNorm.Archival ? "gray" : "black")
				.Finish ();
			ytreeview.Selection.Changed += ytreeView_Selection_Changed;
		}
		
		void ytreeView_Selection_Changed(object sender, EventArgs e) {
			ybuttonOpenDutyNorm.Sensitive = ybuttonGiveWearByDutyNorm.Sensitive = ytreeview.Selection.CountSelectedRows() > 0;
		}
		
		#region Кнопки

		protected void OnButtonGiveWearByDutyNormClicked(object sender, EventArgs e) {
			ybuttonGiveWearByDutyNorm.Sensitive = false;
			ViewModel.GiveWearByDutyNorm(ytreeview.GetSelectedObject<DutyNormItem>());
			ybuttonGiveWearByDutyNorm.Sensitive = true;
		}

		protected void OnButtonOpenDutyNormClicked(object sender, EventArgs e) {
			ybuttonOpenDutyNorm.Sensitive = false;
			ViewModel.OpenDutyNorm(ytreeview.GetSelectedObject<DutyNormItem>());
			ybuttonOpenDutyNorm.Sensitive = true;

		}
		#endregion
	}
}
