using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DocumentFormat.OpenXml.Bibliography;
using EngineCore.Engine.Actions.USN;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TDSAot.State;
using TDSAot.Utils;
using TDSContentApp;
using TDSContentApp.USN;
using TDSContentCore;
using TDSNET.Engine.Actions.USN;


namespace TDSAot
{
    public partial class MainWindow : Window
    {
        private static ActionState? state;

         private List<IFrnFileOrigin> vlist = new();

        private bool initialFinished = false;

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            SetWindowSizeByScreenRatio(0.4, 0.5);
     
            Reset();
        }

   
        private void Reset()
        {
            Option = new AppOption();

            if (!Option.HideAfterStarted) ShowWindow();
            this.Topmost = Option.AlwaysTop;
#if DEBUG
           // return;
#endif
            RegisterHotKeys();
            state?.Dispose();
            state = new ActionState();


            Task.Run(ListFilesThreadStart).ContinueWith((task) =>
            {
                Dispatcher.UIThread.InvokeAsync(AdjustToDefault); // when not adjust to default;
            });
        }


        private void ListFilesThreadStart()
        {
#if DEBUG
            var st = Stopwatch.StartNew();
#endif
            DisableController();


            InitializationApplication(Option.UsingCache);


            initialFinished = true;

            cts = new CancellationTokenSource();
            state?.Start(new Task(() => SearchFilesThreadLoop(cts.Token)), cts);            

            EnableController();
            
#if DEBUG
            Debug.WriteLine(st.Elapsed.ToString());
#endif
        }

        private void RefreshUSN()
        {
            TDSContentApplication.Instance.RefreshUSN();
        }

        private void InitializationApplication(bool cacheUsnDetails)
        {
            if (!TDSContentApplication.Instance.TryLoadUSNFromDickCache())
            {
                RefreshUSN();
                try
                {
                    TDSContentApplication.Instance.DumpUSNToDisk(cacheUsnDetails);  // Ö´ÐÐ»º´æ
                }
                catch (Exception ex)
                {
                    Message.ShowWaringOk("Error", $"Caching failed:{ex.Message}");
                }
            }
            TDSContentApplication.Instance.Initialize(true);
            TDSContentApplication.Instance.UpdateIndexAsync(MarkAsStarted, AddTaskTotalCount, IncrementRunningTaskCount, MarkAsCompleted);

        }

        CancellationTokenSource cts;

        private void EnableController()
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (_trayIcon.Menu?.Items[1] is NativeMenuItem nmi)
                {
                    nmi.IsEnabled = true;
                }
                if (_trayIcon.Menu?.Items[2] is NativeMenuItem res)
                {
                    res.IsEnabled = true;
                }
                MessageData.Watermark = "Please input keywords";
                inputBox.IsEnabled = true;
                fileListBox.IsEnabled = true;
                inputBox.Focus();
                lastFocused = inputBox;
            });
            BindIndexProjects();
        }

        private void DisableController(string msg= "Initialization pending...")
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (_trayIcon.Menu?.Items[1] is NativeMenuItem nmi)
                {
                    nmi.IsEnabled = false;
                }
                if (_trayIcon.Menu?.Items[2] is NativeMenuItem res)
                {
                    res.IsEnabled = false;
                }
                MessageData.Watermark = msg;
                inputBox.IsEnabled = false;
                fileListBox.IsEnabled = false;
            });
        }
    }
}