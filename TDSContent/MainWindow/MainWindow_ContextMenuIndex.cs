using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TDSAot.State;
using TDSAot.Utils;
using TDSContentApp;
using TDSContentCore;

namespace TDSAot
{

    public partial class MainWindow : Window
    {
        void MarkAsStarted()
        {
            Interlocked.Increment(ref MainWindow.taskCount);
            if (MessageData.ProgressVisible == false)
            {
                MessageData.ProgressVisible = true;
            }
            RefreshProgress();
        }
        void AddTaskTotalCount(long total)
        {
            Interlocked.Add(ref MainWindow.total, total);
            RefreshProgress();
        }

        void IncrementRunningTaskCount()
        {
            Interlocked.Increment(ref MainWindow.current);
            RefreshProgress();
        }
        
        void MarkAsCompleted()
        {
            if(Interlocked.Decrement(ref MainWindow.taskCount) == 0)
            {
                MessageData.ProgressVisible = false;
            }
            RefreshProgress();
        }

        public async void AddFolderToIndex(string folder, string[] exts)
        {
            try
            {
                await Task.Run(() =>
                {
                    TDSContentApplication.Instance.AddFolderToIndex(folder,  exts, MarkAsStarted, AddTaskTotalCount, IncrementRunningTaskCount, MarkAsCompleted);
                });
                BindIndexProjects();
            }
            catch (Exception ex)
            {
                Message.ShowWaringOk("Error", ex.Message);
            }
        }

            
      
        private void ReindexFile(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = GetSelectedItems();

                foreach (var item in items)
                {
                    if (item is FrnResult file)
                    {
                        TDSContentApplication.Instance.AddFileEntry(file.FilePath);
                    }
                }
            }
            finally
            {
                StaticState.CanBeHide = true;

            }
        }

        private void Reindex(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = GetSelectedItems();

                foreach (var item in items)
                {
                    if (item is IndexProjects index)
                    {
                        var id= index.Id;

                        TDSContentApplication.Instance.RemoveIndex(id);
                        AddFolderToIndex(index.FilePath,index.Ext);
                    }                    
                }
                BindIndexProjects();
            }
            finally
            {
                StaticState.CanBeHide = true;

            }
        }

        private void DeleteFolderFromIndex(object sender, RoutedEventArgs e)
        {
            StaticState.CanBeHide = false;
            try
            {
                var items = GetSelectedItems();

                foreach (var item in items)
                {
                    if (item is IndexProjects index)
                    {
                        if (Message.ShowYesNo("Remove", $"Are you sure to remmove folder [{item.FilePath}]?"))
                        {

                            TDSContentApplication.Instance.RemoveIndex(index.Id);
                        }
                    }
                    else
                    {
                        Message.ShowWaringOk("Error", "Please select a folder to remove.");
                        return;
                    }
                }
                BindIndexProjects();
            }
            finally
            {
                StaticState.CanBeHide = true;

            }

        }
    }   
}

