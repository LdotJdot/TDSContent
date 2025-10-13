using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using TDS;
using TDSAot.State;
using TDSAot.Utils;
using TDSContent;
using TDSContentApp;

namespace TDSAot
{
    public partial class MainWindow : Window
    {
        internal bool isOptionWinOpen = false;
        SettingWindow optionWindow;

        void ShowDialog_Option(object? sender, RoutedEventArgs e)
        {
            ShowDialog_Option();
        }
        void ShowDialog_Option()
        {
            if (!isOptionWinOpen)
            {             
                optionWindow?.Close();
                optionWindow = new SettingWindow(this);

                optionWindow.Show(); // 这将使新窗口作为模态对话框打开
                isOptionWinOpen = true;
            }
            else
            {
                optionWindow.WindowState = WindowState.Normal;
                optionWindow.Topmost = true;
                optionWindow.Activate();
            }
        }

        internal bool isCreateIndexWinOpen = false;
        IndexCreateWindow indexWindow;

        void ShowDialog_IndexCreate(object? sender, RoutedEventArgs e)
        {
            if (!isCreateIndexWinOpen)
            {
                indexWindow?.Close();
                indexWindow = new IndexCreateWindow(this,TDSContentApplication.Instance.Extensions);

                indexWindow.Show(); // 这将使新窗口作为模态对话框打开
                isCreateIndexWinOpen = true;
            }
            else
            {
                indexWindow.WindowState = WindowState.Normal;
                indexWindow.Topmost = true;
                indexWindow.Activate();
            }
        }

    }
}