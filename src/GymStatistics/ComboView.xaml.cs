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
        public SheetDataProcessor sdp;

        public ComboView()
        {
            InitializeComponent();

            itemsControl.ItemsSource = new[] { new { exercises = new[] { 0, 0, 0} }, new { exercises = new[] { 0, 0, 0} }, };
        }

        public void AddCombo(Combo combo)
        {
            var comboDp = new DockPanel()
            {
                Height = 25,
                Margin = new Thickness(0, 5, 0, 0)
            };
            DockPanel.SetDock(comboDp, Dock.Top);

            var l = new Label()
            {
                Content = "Комбо:",
                Width = 90
            };
            DockPanel.SetDock(l, Dock.Left);
            comboDp.Children.Add(l);

            var combosCb = new ComboBox()
            {
                ItemsSource = sdp.AllCombos.Keys.OrderBy(x => x),
                SelectedValue = combo.Name
            };
            comboDp.Children.Add(combosCb);

            var exerciseDp = new DockPanel()
            {
                Margin = new Thickness(10, 0, 0, 0)
            };
            DockPanel.SetDock(exerciseDp, Dock.Top);

            foreach (var e in combo.Exercises)
            {
                AddExercise(e, exerciseDp);
            }

            //mainDockPanel.Children.Add(comboDp);
            //mainDockPanel.Children.Add(exerciseDp);
        }

        public void AddExercise(Exercise exercise, DockPanel dp)
        {
            var todaysExerciseDp = new DockPanel()
            {
                Height = 25,
                Margin = new Thickness(0, 5, 0, 0)
            };
            DockPanel.SetDock(todaysExerciseDp, Dock.Top);

            var l = new Label()
            {
                Content = "Упражнение:",
                Width = 90
            };
            DockPanel.SetDock(l, Dock.Left);
            todaysExerciseDp.Children.Add(l);

            AddTextBox(todaysExerciseDp, exercise.Plan);
            AddTextBox(todaysExerciseDp, exercise.Repetitions);
            AddTextBox(todaysExerciseDp, exercise.Rest);

            var exerciseCb = new ComboBox()
            {
                ItemsSource = sdp.AllExercises.Select(x => x.Name),
                SelectedValue = exercise.Name
            };
            todaysExerciseDp.Children.Add(exerciseCb);


            var prevExerciseDp = new DockPanel()
            {
                Height = 25,
                Margin = new Thickness(0, 5, 0, 0)
            };
            DockPanel.SetDock(prevExerciseDp, Dock.Top);

            var prevL = new Label()
            {
                Content = "Раньше:",
                Width = 90
            };
            DockPanel.SetDock(prevL, Dock.Left);
            prevExerciseDp.Children.Add(prevL);

            var prevExercise = sdp.GetPrevExercise(exercise.Name);

            AddTextBox(prevExerciseDp, prevExercise.Plan, false);
            AddTextBox(prevExerciseDp, prevExercise.Repetitions, false);
            AddTextBox(prevExerciseDp, prevExercise.Feeling, false);
            AddTextBox(prevExerciseDp, prevExercise.Date.ToString("yyyy-MM-dd"), false, Dock.Left, false);

            var prevWorkTb = new TextBox()
            {
                IsEnabled = false,
                Margin = new Thickness(5, 0, 0, 0),
                Text = prevExercise.Work
            };
            DockPanel.SetDock(prevWorkTb, Dock.Right);
            prevExerciseDp.Children.Add(prevWorkTb);

            dp.Children.Add(todaysExerciseDp);
            dp.Children.Add(prevExerciseDp);

            void AddTextBox(DockPanel parent, string text, bool isEnabled = true, Dock dock = Dock.Right, bool setMargin = true)
            {
                var tb = new TextBox()
                {
                    Text = text,
                    IsEnabled = isEnabled,
                    Margin = new Thickness(setMargin ? 5 : 0, 0, 0, 0),
                    Width = 100
                };
                DockPanel.SetDock(tb, dock);
                parent.Children.Add(tb);
            }
        }

        public void Reset()
        {
            //mainDockPanel.Children.Clear();
        }

        public class ComboViewModel
        {

        }
    }
}
