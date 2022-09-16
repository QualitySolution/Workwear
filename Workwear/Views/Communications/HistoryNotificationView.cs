using System;
using QS.Cloud.WearLk.Manage;
using QS.Views.Dialog;
using Workwear.ViewModels.Communications;

namespace workwear.Views.Communications
{
    public partial class HistoryNotificationView : DialogViewBase<HistoryNotificationViewModel> 
    {
        public HistoryNotificationView(HistoryNotificationViewModel viewModel) : base(viewModel)
        {
            this.Build();
            ConfigureDlg();
        }

        private void ConfigureDlg() {
            ytreeviewMessange.CreateFluentColumnsConfig<MessageItem>()
                    .AddColumn("Заголовок").AddTextRenderer(x => TitleBuild(x))
                    .AddColumn("Содержание").AddTextRenderer(x => x.Text)
                    .RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Background = x.Read ? null : "White Smoke")
                    .Finish();
            ytreeviewMessange.Binding
                .AddBinding(ViewModel, vm => vm.MessageItems, w => w.ItemsDataSource)
                .InitializeFromSource();
        }
        private string TitleBuild(MessageItem item) => 
            $"{item.SendTime.ToDateTime():g} {item.SenderName} {(string.IsNullOrEmpty(item.SenderName)? String.Empty : ":")} {item.Title}";
    }
}
