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

        public async void AddCategoryToIndex(string folder, string categoryName,string[] exts)
        {
            try
            {
                await Task.Run(() =>
                {
                    TDSContentApplication.Instance.AddCategory(folder, categoryName, exts, MarkAsStarted, AddTaskTotalCount, IncrementRunningTaskCount, MarkAsCompleted);
                });
                BindIndexProjects();
            }
            catch (Exception ex)
            {
                Message.ShowWaringOk("Error", ex.Message);
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
                        var categoryName=item.FileName;
                        var info = TDSContentApplication.Instance.GetCategoryInfo(categoryName);

                        if (info != null)
                        {
                            TDSContentApplication.Instance.RemoveCategory(item.FileName);
                            AddCategoryToIndex(info.Value.path, categoryName, info.Value.exts);
                        }
                    }                    
                }
                BindIndexProjects();
            }
            finally
            {
                StaticState.CanBeHide = true;

            }
        }

        private void DeleteCategoryFromIndex(object sender, RoutedEventArgs e)
        {
            StaticState.CanBeHide = false;
            try
            {
                var items = GetSelectedItems();

                foreach (var item in items)
                {
                    if (item != null)
                    {
                        if (Message.ShowYesNo("Remove", $"Are you sure to remmove category [{item.FileName}]?"))
                        {

                            TDSContentApplication.Instance.RemoveCategory(item.FileName);
                        }
                    }
                    else
                    {
                        Message.ShowWaringOk("Error", "Please select a category to remove.");
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

