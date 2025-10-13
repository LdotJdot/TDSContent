using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using TDSContentCore;

namespace TDSAot.ViewModels
{
    // ViewModel
    public class DataViewModel : ReactiveObject
    {
        private IList<IFrnFileOrigin> _allData = [];
        private IEnumerable<IFrnFileOrigin> _displayedData = [];
        private int _displayCount = 100;
        private bool _isShowResults =  false;

        public bool IsShowResults
        {
            get => _isShowResults;
            private set => this.RaiseAndSetIfChanged(ref _isShowResults, value);
        }

        public IEnumerable<IFrnFileOrigin> DisplayedData
        {
            get => _displayedData;
            private set => this.RaiseAndSetIfChanged(ref _displayedData, value);
        }

        public int DisplayCount
        {
            get => _displayCount;
            private set
            {
                _displayCount = value;
                UpdateDisplayedData();
            }
        }

        public DataViewModel()
        {
        }

        public void Bind(IList<IFrnFileOrigin> _allData)
        {
            if (this._allData != _allData)
            {
                // 生成测试数据（实际中可能从文件或数据库加载）
                this._allData = _allData;
                //UpdateDisplayedData();
            }
        }

        private void UpdateDisplayedData()
        {
            // 使用 LINQ 的 Take()，这是惰性求值的，性能很好
            DisplayedData = _allData.Take(DisplayCount);
        }

        // 快速切换到不同数量级
        public void SetDisplayCount(int count)
        {
            DisplayCount = count;
        }
    }
}