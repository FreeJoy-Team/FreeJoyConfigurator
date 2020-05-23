using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace FreeJoyConfigurator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Scroll to bottom when text is changed
        public void ActivityLogTextChangedHandler(object sender, EventArgs e)
        {
            ActivityLogScrollViewer.ScrollToBottom();
        }

        private void AxesCurvesView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        void Tab_AllTabOnStartup(object sender, RoutedEventArgs e)      //!!!!! КОСТЫЛЬ !!!!!!!!!!111111111111111111
        {
            var tabControl = sender as TabControl;

            //Thread thread = new Thread(() =>
            //{
            //    //tabControl.SelectedIndex = 1;
            //    //tabControl.UpdateLayout();
            //    //tabControl.SelectedIndex = 0;
            //    //tabControl.Loaded -= Tab_AllTabOnStartup;
            //    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            //    {
            //        //var tabIndex = tabControl.Items.Count;
            //        tabControl.SelectedIndex = 1;
            //        tabControl.UpdateLayout();
            //        tabControl.SelectedIndex = 0;
            //        //for (var tabIndex = tabControl.Items.Count - 1; tabIndex >= 0; tabIndex--)
            //        //{
            //        //    tabControl.SelectedIndex = tabIndex;
            //        //    tabControl.UpdateLayout();
            //        //}
            //        tabControl.Loaded -= Tab_AllTabOnStartup; // Do this on first time only
            //    }));
            //});
            //thread.Start();

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                //var tabIndex = tabControl.Items.Count;
                tabControl.SelectedIndex = 1;															// load selected tab on startup
                tabControl.UpdateLayout();																// update
                tabControl.SelectedIndex = 0;															// back to first tab
                //for (var tabIndex = tabControl.Items.Count - 1; tabIndex >= 0; tabIndex--)			// load all tabs on startup
                //{
                //    tabControl.SelectedIndex = tabIndex;
                //    tabControl.UpdateLayout();
                //}
                tabControl.Loaded -= Tab_AllTabOnStartup; // Do this on first time only
            }));
        }

        //private async void Tab_AllTabOnStartup(object sender, RoutedEventArgs e)
        //{

        //    // run a method in another thread
        //    await Task.Run(() => Dispatcher.Invoke(() =>
        //    {
        //        var tabControl = sender as TabControl;
        //        tabControl.SelectedIndex = 1;
        //        tabControl.UpdateLayout();
        //        tabControl.SelectedIndex = 0;
        //        tabControl.Loaded -= Tab_AllTabOnStartup;
        //    }));
        //}      
    }
}
