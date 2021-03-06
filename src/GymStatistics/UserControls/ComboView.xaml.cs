﻿using GymStatistics.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace GymStatistics.UserControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ComboView : UserControl
    {
        public ComboViewVM ViewModel { get; private set; }

        private SheetDataProcessor _sdp;
        private DayOfWeek _selectedDay;

        public ComboView()
        {
            InitializeComponent();
        }

        public void Init(SheetDataProcessor sdp)
        {
            _sdp = sdp;

            ViewModel = new ComboViewVM
            {
                AllComboNames = _sdp.MostUsedCombos.Select(x => x.Name).ToArray(),
                AllExercises = _sdp.AllExercises.Select(x => x.Name).ToArray(),
            };

            itemsControl.DataContext = ViewModel;
        }

        public void SetDayOfWeek(DayOfWeek day)
        {
            _selectedDay = day;

            Reset();
            foreach (var c in _sdp.GetCombos(day).OrderBy(x => x.OrderInDay))
            {
                ViewModel.Combos.Add(new ComboVM()
                {
                    Name = c.Name
                });
            }
        }

        public void Reset()
        {
            foreach (var c in ViewModel.Combos)
            {
                c.Exercises.Clear();
            }
            ViewModel.Combos.Clear();
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboVM = (sender as ComboBox).DataContext as ComboVM;
            comboVM.Exercises.Clear();

            foreach (var exercise in _sdp.AllCombos[comboVM.Name].Exercises)
            {
                var compareExerciseVM = new CompareExerciseVM
                {
                    TodayEx = new ExerciseVM
                    {
                        Name = exercise.Name
                    },
                    PrevEx = new ExerciseVM()
                };

                comboVM.Exercises.Add(compareExerciseVM);
            }
        }

        private void Exercise_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var compareExerciseVM = (sender as ComboBox).DataContext as CompareExerciseVM;

            var prevEx = _sdp.GetPreviousExercise(compareExerciseVM.TodayEx.Name);

            compareExerciseVM.TodayEx.Plan = prevEx?.Plan;
            compareExerciseVM.TodayEx.Repetitions = prevEx?.Repetitions;
            compareExerciseVM.TodayEx.Rest = prevEx?.Rest;
            compareExerciseVM.TodayEx.Best = prevEx?.Best;

            compareExerciseVM.PrevEx.Plan = prevEx?.Plan ?? "N/A";
            compareExerciseVM.PrevEx.Repetitions = prevEx?.Repetitions ?? "N/A";
            compareExerciseVM.PrevEx.Feeling = prevEx?.Feeling ?? "N/A";
            compareExerciseVM.PrevEx.Date = prevEx?.Date.ToString("yyyy-MM-dd") ?? "N/A";
            compareExerciseVM.PrevEx.Work = prevEx?.Work ?? "N/A";
            compareExerciseVM.PrevEx.Best = prevEx?.Best ?? "N/A";

            // not visible in editor
            compareExerciseVM.TodayEx.Muscle = prevEx?.Muscle;
            compareExerciseVM.TodayEx.Mode = prevEx?.Mode;
            compareExerciseVM.TodayEx.Gym = prevEx?.Gym;
        }
    }


    public class ComboViewVM
    {
        public string[] AllComboNames { get; set; }

        public string[] AllExercises { get; set; }

        public ObservableCollection<ComboVM> Combos { get; set; } = new ObservableCollection<ComboVM>();
    }

    public class ComboVM
    {
        public string Name { get; set; }

        public ObservableCollection<CompareExerciseVM> Exercises { get; set; } = new ObservableCollection<CompareExerciseVM>();
    }

    public class CompareExerciseVM
    {
        public ExerciseVM TodayEx { get; set; }

        public ExerciseVM PrevEx { get; set; }
    }

    public class ExerciseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public string Rest { get; set; }
        public string Repetitions { get; set; }
        public string Work { get; set; }
        public string Plan { get; set; }
        public string Date { get; set; }
        public string Feeling { get; set; }
        public string Best { get; set; }

        public string Muscle;
        public string Mode;
        public string Gym;
    }
}
