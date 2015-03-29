using MahApps.Metro.Controls;
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

namespace Knife
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void wb_InitializeView(object sender, Awesomium.Core.WebViewEventArgs e)
        {
            wb.WebSession = Awesomium.Core.WebCore.CreateWebSession(System.Environment.CurrentDirectory, new Awesomium.Core.WebPreferences());
        }

        private void wb_LoadingFrameComplete(object sender, Awesomium.Core.FrameEventArgs e)
        {

        }
    }
}
