using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace TDSAot
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // ���������ڵ���������ʾ
                var mainWindow = new MainWindow();

                // ���ô��ڳ�ʼ״̬Ϊ����
                mainWindow.ShowInTaskbar = false;
                mainWindow.WindowState = WindowState.Minimized;

                desktop.MainWindow = mainWindow;

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}