using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace swClassTableHint
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            
            DateTime dt = DateTime.Now;
            //dt.DayOfWeek = DayOfWeek.Sunday;
            //dt.toS
            var _mainpage=new MainWindow();
            _mainpage.Show();
            // this.CheckForIllegalCrossThreadCalls = false;
        }
    }
}
