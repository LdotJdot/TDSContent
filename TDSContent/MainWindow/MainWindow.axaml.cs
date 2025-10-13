using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using System;
using System.Security.Principal;
using System.Text;
using TDS.Utils;
using TDSAot.State;
using TDSAot.Utils;

namespace TDSAot
{
    public partial class MainWindow : Window
    {
        static internal AppOption Option;

        public static void CheckRunningAsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Utils.Message.ShowWaringOk("The program is about to exit", "Please run the program with administrator privileges.");
                Environment.Exit(-1);
            }
        }

        public MainWindow()
        {
            if (!ApplicationSingleton.Check())
            {
                Message.ShowWaringOk("Program already running", "Another instance of the program is already running. Check the right corner of your desktop.");
                Exit();
            }
            CheckRunningAsAdministrator();
            InitializeComponent();
            InitializeTrayIcon();
            InitializeEnvironment();
            InitializeFileAction();
            InitializeEvent();
            // ��ʼ��
        }

        double DefaultHeight;
       

        private void InitializeEvent()
        {
            this.Deactivated += OnWindowDeactivated;
            // ��ѡ�����Ĵ��ڻ�ý����¼�
            this.Activated += OnWindowActivated;

            this.KeyDown += OnAppKeyDown;
            Loaded += MainWindow_Loaded;
        }

    

        private void InitializeEnvironment()
        {
            this.Title = "TDS";
            // ���ݰ�ע��
            DataContext = this;
            // �ַ�ע��
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // UI�¼�ð��,�����ͻ
            inputBox.AddHandler(
             InputElement.PointerPressedEvent,
             Keywords_MouseDown!,
             RoutingStrategies.Bubble,
             true);

            // UI�¼�ð��,�����ͻ
            inputBox.AddHandler(
             InputElement.KeyDownEvent,
             Input_KeyDown!,
             RoutingStrategies.Bubble,
             true);

            // ���ע��
            hwnd = GetNativeHandle().Handle;    //��ȡ���ھ��


            InitializeHotKeys(hwnd);
            fileListBox.Focusable = true;
            // ��ʼ�������ļ�

        }

        private IPlatformHandle GetNativeHandle()
        {
            var topLevel = TopLevel.GetTopLevel(this)!;

            // ͨ�����ڻ�ȡ���������Ӽ򵥣�
            return topLevel.TryGetPlatformHandle()!;
        }

        private void Exit()
        {
            try
            {
                OnAppClosed(null,null);
                ApplicationSingleton.appMutex?.ReleaseMutex();
                ApplicationSingleton.appMutex?.Dispose();
            }
            catch
            {

            }
            _trayIcon?.Dispose();
            Environment.Exit(0);
        }



    }
}