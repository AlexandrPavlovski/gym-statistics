using GymStatistics.Model;
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

namespace GymStatistics
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ComboView : UserControl
    {
        public ComboView()
        {
            InitializeComponent();
            //DockPanel dockPanel = new DockPanel();
            //dockPanel.Children.IndexOf();
        }

        private void AddExercise(Exercise exercise)
        {
            var dp = new DockPanel();
            DockPanel.SetDock(dp, Dock.Top);
            dp.Margin = new Thickness(0, 0, 5, 0);

            var l = new Label();
            DockPanel.SetDock(l, Dock.Left);
            l.Content = "Упражнение:";
            l.Width = 90;
            dp.Children.Add(l);

            var planTb = new TextBox();
            DockPanel.SetDock(planTb, Dock.Right);
            planTb.Margin = new Thickness(5, 0, 0, 0);
            planTb.Width = 100;
            planTb.Text = exercise.Plan;
            dp.Children.Add(planTb);

            var repetitionsTb = new TextBox();
            DockPanel.SetDock(repetitionsTb, Dock.Right);
            repetitionsTb.Margin = new Thickness(5, 0, 0, 0);
            repetitionsTb.Width = 100;
            repetitionsTb.Text = exercise.Repetitions;
            dp.Children.Add(repetitionsTb);

            var restTb = new TextBox();
            DockPanel.SetDock(restTb, Dock.Right);
            restTb.Margin = new Thickness(5, 0, 0, 0);
            restTb.Width = 100;
            restTb.Text = exercise.Rest;
            dp.Children.Add(restTb);

            var cb = new ComboBox();
            dp.Children.Add(cb);
        }
    }
}
