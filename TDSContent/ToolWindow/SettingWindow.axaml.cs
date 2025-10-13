using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TDSAot;
using TDSAot.State;
using TDSAot.Utils;

namespace TDS;

public partial class SettingWindow : Window
{
    MainWindow mainWindow;
    public SettingWindow(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;
        InitializeComponent();
        this.Topmost = true;

        DataContext = LoadOption();
        this.Closed += SettingWindow_Closed;

    }

    private void SettingWindow_Closed(object? sender, System.EventArgs e)
    {
        mainWindow.isOptionWinOpen = false;
    }


    internal SettingsViewModel LoadOption()
    {
        var svm              = new SettingsViewModel(this);
        svm.Findmax          = MainWindow.Option.Findmax.ToString();
        svm.HotKey           = KeyTransfer.ReverseTransKey(MainWindow.Option.HotKey);
        svm.ModifierKey      = KeyTransfer.ReverseTransKey(MainWindow.Option.ModifierKey);
        svm.UsingCache       = MainWindow.Option.UsingCache;
        svm.HideAfterStarted = MainWindow.Option.HideAfterStarted;
        svm.Theme            = MainWindow.Option.Theme.ToString();
        svm.AutoHide         = MainWindow.Option.AutoHide;
        svm.AlwaysTop        = MainWindow.Option.AlwaysTop;
        return svm;
    }

    internal void SaveAndExit()
    {
        var svm=(SettingsViewModel)DataContext!;
        if (int.TryParse(svm.Findmax, out var value) && value>0 && value<1000)
        {
            MainWindow.Option.Findmax = value;
        }
        else
        {
            MainWindow.Option.Findmax = 100;
        }
        MainWindow.Option.HotKey           = KeyTransfer.TransKey(svm.HotKey);
        MainWindow.Option.ModifierKey      = KeyTransfer.TransKey(svm.ModifierKey);
        MainWindow.Option.UsingCache       = svm.UsingCache;
        MainWindow.Option.HideAfterStarted = svm.HideAfterStarted;
        MainWindow.Option.AutoHide         = svm.AutoHide;
        MainWindow.Option.AlwaysTop        = svm.AlwaysTop;
        MainWindow.Option.Theme            = Enum.TryParse<ThemeType>(svm.Theme, true, out var theme)?theme: ThemeType.Default;

        MainWindow.Option.Save();
        mainWindow.RegisterHotKeys();
        mainWindow.Topmost = MainWindow.Option.AlwaysTop;
        this.Exit();
    }
    internal void Exit()
    {
        this.Close();
    }
}


