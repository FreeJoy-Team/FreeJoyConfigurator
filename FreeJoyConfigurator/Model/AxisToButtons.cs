using InWit.Core.Collections;
using InWit.ViewModel.Utils;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class AxisToButtons : BindableBase
    {
        private ObservableContentCollection<RangeItem> m_rangeItems;
        private int _buttonCnt;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public ObservableContentCollection<RangeItem> RangeItems
        {
            get { return m_rangeItems; }
            set { SetProperty(ref m_rangeItems, value); }
        }

        public int ButtonCnt
        {
            get { return _buttonCnt; }
            set
            {
                SetProperty(ref _buttonCnt, value);
            }
        }

        

        public AxisToButtons ()
        {
            m_rangeItems = new ObservableContentCollection<RangeItem>
                            {
                                new RangeItem {From = 0, To = 127},
                                new RangeItem {From = 127, To = 255},
                            };

            _buttonCnt = m_rangeItems.Count;
            _isEnabled = false;
        }

    }

    public class RangeItem : BindableBase
    {
        private int m_from;
        private int m_to;

        public int From
        {
            get { return m_from; }
            set
            {
                SetProperty(ref m_from, value);
            }
        }

        public int To
        {
            get { return m_to; }
            set
            {
                SetProperty(ref m_to, value);
            }
        }
    }
}
