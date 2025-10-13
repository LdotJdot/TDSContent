using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using TDSContent;

namespace TDSContent
{
    public class FileExtensionItem
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }

    public class IndexCreateModel : ReactiveObject
    {
        IndexCreateWindow indexWindow;
        public IndexCreateModel(IndexCreateWindow window,string[] supportedExt)
        {
            this.indexWindow = window;
            // 初始化命令
            SaveCommand = ReactiveCommand.Create(SaveSettings);
            CancelCommand = ReactiveCommand.Create(Cancel);
            BrowseFolderCommand = ReactiveCommand.Create(BrowseFolder);

            FileExtensions= new ObservableCollection<FileExtensionItem>(
                supportedExt.Select(o => new FileExtensionItem { Name = o, IsSelected = false })
                );
        }

        string category = "";
        public string Category
        {
            get => category;
            set => this.RaiseAndSetIfChanged(ref category, value);
        }


        string folderPath  = "";

        public string FolderPath
        {
            get => folderPath;
            set => this.RaiseAndSetIfChanged(ref folderPath, value);
        }

        // 是否启用单选生效的文本框
        private bool _isSingleEffectEnabled;
        public bool IsSingleEffectEnabled
        {
            get => _isSingleEffectEnabled;
            set => this.RaiseAndSetIfChanged(ref _isSingleEffectEnabled, value);
        }

        // 单选生效的文本框
        private string _painTextExtensions;
        public string PainTextExtensions
        {
            get => _painTextExtensions;
            set => this.RaiseAndSetIfChanged(ref _painTextExtensions, value);
        }

        // 文件扩展名列表
        public ObservableCollection<FileExtensionItem> FileExtensions { get; }

        // 选中的文件扩展名列表
        private ObservableCollection<string> _selectedFileExtensions;
        public ObservableCollection<string> SelectedFileExtensions
        {
            get => _selectedFileExtensions;
            set => this.RaiseAndSetIfChanged(ref _selectedFileExtensions, value);
        }

        // 命令

        public ReactiveCommand<Unit, Unit> BrowseFolderCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private async void BrowseFolder()
        {
            var fs= await DialogHelper.ShowFolderDialog(indexWindow);
            var path = fs.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(path))
            {
                FolderPath= path;
            }
        }
        private void SaveSettings()
        {
            indexWindow.SaveAndExit();
        }

        private void Cancel()
        {
            indexWindow.Exit();
        }
    }

    public static class DialogHelper
    {
        public static async Task<string[]> ShowFolderDialog(Window parent)
        {
            var storageProvider = parent.StorageProvider;

            var folderResult = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select folder",
                AllowMultiple = false
            });

            // 合并结果
            var folders = folderResult.Select(f => f.Path.LocalPath).ToArray();
            return folders;
        }

        public static async Task<string[]> ShowFileDialog(Window parent, IEnumerable<string> exts)
        {
            var storageProvider = parent.StorageProvider;

            // 选择文件
            var fileResult = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select files",
                AllowMultiple = true,
                FileTypeFilter = new[]
                {
                new FilePickerFileType("Support formats") { Patterns =exts.Select(o=>"*"+o).ToArray()}
            }
            });
            var files = fileResult.Select(f => f.Path.LocalPath).ToArray();
            return files;

        }
    }
}