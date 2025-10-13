using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TDS;
using TDSAot;
using TDSAot.State;
using TDSAot.Utils;
using TDSContentApp;

namespace TDSContent;

public partial class IndexCreateWindow : Window
{
    MainWindow mainWindow;

    public IndexCreateWindow(MainWindow mainWindow, string[] supportedExts)
    {
        this.mainWindow = mainWindow;
        InitializeComponent();
        DataContext = new IndexCreateModel(this, supportedExts);
        this.Closed += IndexCreateWindow_Closed;

    }

    private void IndexCreateWindow_Closed(object? sender, System.EventArgs e)
    {
        mainWindow.isCreateIndexWinOpen = false;
    }


    internal void SaveAndExit()
    {
        var svm = (IndexCreateModel)DataContext!;
        try
        {
            TDSContentApplication.Instance.CheckCategoryNameExisted(svm.Category);

            if(Directory.Exists(svm.FolderPath)==false) throw new Exception($"Folder [{svm.FolderPath}] not exist!");

            var painExts = svm.PainTextExtensions?.Trim('"').Replace(" ", "").Split('|')??[];
            var exts = svm.FileExtensions
                .Where(o => o.IsSelected)
                .Select(o => o.Name).Concat(painExts.Where(o => !string.IsNullOrEmpty(o)).Select(o=> "."+o.TrimStart('.'))).ToArray();

            if (exts.Length == 0) throw new Exception("File extension did not selected.");

            if (Message.ShowYesNo("Create category confirm", 
                $"Are you sure to create category [{svm.Category}] for folder [{svm.FolderPath}] with [{string.Join(",", exts).ToLower()}] file extentions?")
                )
            {
                mainWindow.AddCategoryToIndex(
                    svm.FolderPath,
                    svm.Category,
                    exts
                    );
                mainWindow.Topmost = MainWindow.Option.AlwaysTop;
                this.Exit();
            }
        }
        catch(Exception ex)
        {
            Message.ShowWaringOk("Error", ex.Message);
        }
    }

    internal void Exit()
    {
        this.Close();
    }
}

