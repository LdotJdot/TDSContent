using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using Avalonia.Threading;
using Avalonia.VisualTree;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TDSAot.Utils;
using TDSContentApp;
using TDSContentCore;

namespace TDSAot
{
    public partial class MainWindow : Window
    {
        string bufferedText_prev = string.Empty;
        string bufferedText = string.Empty;
        
        

        private void GoSearch()
        {

            if (string.IsNullOrWhiteSpace(bufferedText))
            {
                BindIndexProjects();
                //ChangeToRecord();
            }
            else if(!bufferedText.Equals(bufferedText_prev, StringComparison.OrdinalIgnoreCase))
            {
                TDSContentApplication.Instance.UpdateUSNAsync(MarkAsStarted, AddTaskTotalCount, IncrementRunningTaskCount, MarkAsCompleted);
                keyword = bufferedText;

                if(runningState.gOs.CurrentCount < 1)
                {
                    runningState.gOs.Release();
                }                
            }
            bufferedText_prev = bufferedText;

        }


        private void TextChanged(object? sender, RoutedEventArgs e)
        {
            bufferedText = inputBox.Text?.Trim() ?? string.Empty;
            runningState.Threadrest = true;
            Task.Run(GoSearch);
        }

        // 处理鼠标双击事件
        private void ListBox_MouseDoubleClick(object sender, TappedEventArgs e)
        {
            if (fileListBox.SelectedItem is IFrnFileOrigin file)
            {
                fileAction.Execute([file], FileActionType.Open);
            }
        }

        // 处理键盘按键事件
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyModifiers == KeyModifiers.None)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        Debug.WriteLine("enter");
                        var file = Items.DisplayedData.FirstOrDefault();
                        if (file != null)
                        {
                            Execute([file], FileActionType.Open);
                        }
                        break;

                    case Key.Down:
                        if (fileListBox.ItemCount > 0)
                        {
                            fileListBox.SelectedIndex = 0;
                            Dispatcher.UIThread.Invoke(
                            () => fileListBox.Focus());
                        }
                        break;
                }
            }
        }

        private void OnAppClosed(object? sender, EventArgs e)
        {
            TDSContentApplication.Instance.Dispose();
            _trayIcon?.Dispose();
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {


            if (e.Key == Key.Space)
            {
                Execute(GetSelectedItems(), FileActionType.OpenFolder);
            }
            else if (e.Key == Key.LeftCtrl && e.KeyModifiers==KeyModifiers.Control)
            {
                if (fileListBox.ContextMenu != null && fileListBox.SelectedItem != null)
                {
                    fileListBox.ContextMenu.Open();
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (e.KeyModifiers == KeyModifiers.Alt)
                {
                    ShowProperty(null, null);
                }
                else if (e.KeyModifiers == KeyModifiers.Shift)
                {
                    OpenFileWith(null, null);
                }
                else
                {
                    Execute(GetSelectedItems(), FileActionType.Open);
                }
            }
            else if (e.Key == Key.C && e.KeyModifiers == KeyModifiers.Control)
            {
                Copy(null, null);
            }
            else if (e.Key == Key.P && e.KeyModifiers == KeyModifiers.Control)
            {
                CopyPath(null, null);
            }
            else if (e.Key == Key.Delete)
            {
                Delete(null, null);
            }
        }

        List<IFrnFileOrigin> _selectItemsList = new List<IFrnFileOrigin>();

        private IFrnFileOrigin[] GetSelectedItems()
        {
            _selectItemsList.Clear();
            foreach (var item in fileListBox.SelectedItems ?? new object[] { })
            {
                if (item is IFrnFileOrigin frn)
                {
                    _selectItemsList.Add(frn);
                }
            }
            return _selectItemsList.ToArray();
        }
    }

}