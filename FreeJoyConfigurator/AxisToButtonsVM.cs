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
    public class AxisToButtonsVM : BindableBase
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
                while (_buttonCnt > RangeItems.Count)
                {
                    for (int i=0; i<RangeItems.Count; i++)
                    {
                        RangeItems[i].From = i * (4095 / (RangeItems.Count + 1));
                        RangeItems[i].To = (i + 1) * (4095 / (RangeItems.Count + 1));
                    }
                    RangeItems.Add(new RangeItem { From = RangeItems.Last().To, To = 4095 });
                }
                while (_buttonCnt < RangeItems.Count)
                {
                    RangeItems.Remove(RangeItems.Last()) ;
                    for (int i = RangeItems.Count-1; i >=0; i--)
                    {                       
                        RangeItems[i].From = i * (4095 / (RangeItems.Count));
                        RangeItems[i].To = (i + 1) * (4095 / (RangeItems.Count));
                    }
                    RangeItems.Last().To = 4095;
                }
            }
        }

        

        public AxisToButtonsVM ()
        {
            //ranges = new ObservableCollection<RangeItem>();
            //for (int i = 0; i < 10; i++) ranges.Add(new RangeItem());
            m_rangeItems = new ObservableContentCollection<RangeItem>
                            {
                                new RangeItem {From = 0, To = 2048},
                                new RangeItem {From = 2048, To = 4095},
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
