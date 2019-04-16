using GymStatistics.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ComboViewVM _vm;
        private SheetDataProcessor _sdp;

        public ComboView()
        {
            InitializeComponent();
        }

        public void Init(SheetDataProcessor sdp)
        {
            _sdp = sdp;

            _vm = new ComboViewVM
            {
                AllComboNames = _sdp.MostUsedCombos.Select(x => x.Name).ToArray(),
                AllExercises = _sdp.AllExercises.Select(x => x.Name).ToArray(),
            };

            itemsControl.DataContext = _vm;
        }

        public void SetDayOfWeek(DayOfWeek day)
        {
            Reset();
            foreach (var c in _sdp.GetCombos(day).OrderBy(x => x.OrderInDay))
            {
                AddCombo(c);
            }
        }

        public void AddCombo(Combo combo)
        {
            var comboVM = new ComboVM()
            {
                Name = combo.Name
            };
            _vm.Combos.Add(comboVM);

            foreach (var e in combo.Exercises)
            {
                var prevEx = _sdp.GetPrevExercise(e.Name);
                var ceVM = new CompareExerciseVM
                {
                    TodaysEx = new ExerciseVM
                    {
                        Name = e.Name,
                        Plan = e.Plan,
                        Repetitions = e.Repetitions,
                        Rest = e.Rest
                    },
                    PrevEx = new ExerciseVM
                    {
                        Plan = prevEx.Plan,
                        Repetitions = prevEx.Repetitions,
                        Feeling = prevEx.Feeling,
                        Date = prevEx.Date.ToString("yyyy-MM-dd"),
                        Work = prevEx.Work
                    }
                };

                comboVM.Exercises.Add(ceVM);
            }
        }

        public void Reset()
        {
            foreach (var c in _vm.Combos)
            {
                c.Exercises.Clear();
            }
            _vm.Combos.Clear();
        }
    }


    public class ComboViewVM
    {
        public string[] AllComboNames { get; set; }

        public string[] AllExercises { get; set; }

        public ObservableCollection<ComboVM> Combos { get; set; }

        public ComboViewVM()
        {
            Combos = new ObservableCollection<ComboVM>();
        }
    }

    public class ComboVM
    {
        public string Name { get; set; }

        public ObservableCollection<CompareExerciseVM> Exercises { get; set; }

        public ComboVM()
        {
            Exercises = new ObservableCollection<CompareExerciseVM>();
        }
    }

    public class CompareExerciseVM
    {
        public ExerciseVM TodaysEx { get; set; }

        public ExerciseVM PrevEx { get; set; }
    }

    public class ExerciseVM
    {
        public string Name { get; set; }
        public string Muscle { get; set; }
        public string Rest { get; set; }
        public string Repetitions { get; set; }
        public string Work { get; set; }
        public string Plan { get; set; }
        public string Best { get; set; }
        public string Date { get; set; }
        public string Feeling { get; set; }
    }
}
