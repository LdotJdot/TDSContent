using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using TDS.State;
using TDSAot.Utils;

namespace TDSAot
{
    public partial class MainWindow : Window
    {
        private TrayIcon _trayIcon;

        private void InitializeTrayIcon()
        {
            // 创建TrayIcon实例
            _trayIcon = new TrayIcon();

            var uri = new Uri(@"avares://TDSContent/Assets/tds32-32Green.ico");
            using var asset = AssetLoader.Open(uri);
            // 设置图标
            var icon = new WindowIcon(asset);
            _trayIcon.Icon = icon;
            this.Icon = icon;

            // 设置提示文本
            _trayIcon.ToolTipText = "TDSContent";

            // 创建上下文菜单
            var menu = new NativeMenu();

            var showItem = new NativeMenuItem("Show window");
            showItem.Click += (s, e) => ShowWindow();

            var option = new NativeMenuItem("Options...");
            option.Click += (s, e) => ShowDialog_Option();


            var reset = new NativeMenuItem("Refresh USN");
            reset.Click += (s, e) =>
            {
                DisableController("Refreshing USN...");
                Task.Run(RefreshUSN).ContinueWith((t) => EnableController());
            };

            var about = new NativeMenuItem("About...");
            about.Click += (s, e) => Message.ShowWaringOk(AppInfomation.AboutTitle, AppInfomation.AboutInfo);

            var exitItem = new NativeMenuItem("Exit");
            exitItem.Click += (s, e) => Exit();

            menu.Add(showItem);
            menu.Add(option);
            menu.Add(reset);
            menu.Add(about);
            menu.Add(exitItem);

            _trayIcon.Menu = menu;

            // 单击事件
            _trayIcon.Clicked += (s, e) => ShowWindow();

            // 确保TrayIcon可见
            _trayIcon.IsVisible = true;
        }

    }
}