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
using System.Windows.Shapes;
using System.Diagnostics;
namespace Knife
{
    /// <summary>
    /// Логика взаимодействия для Stats.xaml
    /// </summary>
    public partial class Stats : MetroWindow
    {
        public Stats(List<SKnife> stats)
        {
            InitializeComponent();
            foreach (SKnife x in stats)
                DG.Items.Insert(0, x);
        }

        private void DG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(DG.SelectedIndex != -1)
            {
                Process.Start((DG.Items[DG.SelectedIndex] as SKnife).sender);
            }
        }
    }
}
