using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Remote.Protocol.Input;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TDSAot.State;
using TDSContentApp;
using TDSContentCore.Engine;
using Tmds.DBus.Protocol;

namespace TDSAot
{
    public partial class MainWindow : Window
    {
        static internal string keyword = string.Empty;
        static internal string[] words = [];

        readonly private RunningState runningState=new RunningState();
        int resultNumGlobal= 0;

        private async void SearchFilesThreadLoop(CancellationToken cancellationToken)
        {
            runningState.Threadrunning = true;

            while (runningState.Threadrunning == true && !cancellationToken.IsCancellationRequested)
            {   
                try
                {
                    await runningState.gOs.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    break;
                }

                words=[];

                runningState.Threadrest = false;  //÷ÿ∆Ù±Í«©

                string threadKeyword = keyword;
                vlist.Clear();

                if (string.IsNullOrEmpty(threadKeyword))
                {
                    continue;
                }

                IList<Lucene.Net.Documents.Document> results=[];

                if (threadKeyword.Contains("/a"))
                {
                    results = TDSContentApplication.Instance.ShowAll();
                }
                else
                {
                    try
                    {
                        results = TDSContentApplication.Instance.SearchFile(keyword, SearchMode.Pharse, Option.Findmax);
                      
                    }
                    catch (Exception ex)
                    {
                    }
                }

                try
                {
                    foreach (var result in results)
                    {
                        vlist.Add(new TDSContentCore.FrnResult(result));
                    }
                    resultNumGlobal = results.Count();
                }
                catch (Exception ex)
                {
                }

                if (!runningState.Threadrest)
                {
                    UpdateList();  //“Ï≤ΩBeginInvoke
                }
            }
        }

        private void UpdateList()
        {
            UpdateData(vlist, resultNumGlobal);
        }

        IInputElement? lastFocused;
        private void RefreshFileData()
        {
            TextChanged(null, null!);
            lastFocused?.Focus();
        }

    }
}