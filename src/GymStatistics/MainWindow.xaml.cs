using GymStatistics.Model;
using GymStatistics.ValueObjects;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GoogleApiClient gac;
        private TrainingDay[] trainingDays;
        private SheetDataProcessor sdp;

        public MainWindow()
        {
            gac = new GoogleApiClient();

            InitializeComponent();

            var days = new[]
            {
                new Weekday
                {
                    DisplayText = "Понедельник",
                    Value = DayOfWeek.Monday
                },
                new Weekday
                {
                    DisplayText = "Среда",
                    Value = DayOfWeek.Wednesday
                },
                new Weekday
                {
                    DisplayText = "Пятница",
                    Value = DayOfWeek.Friday
                }
            };
            dayOfWeekCb.ItemsSource = days;
            dayOfWeekCb.DisplayMemberPath = "DisplayText";
            dayOfWeekCb.SelectedValuePath = "Value";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var sheetId = sheetIdInput.Text;
            if (string.IsNullOrWhiteSpace(sheetId))
            {
                sheetId = "1o9ecJKUtohwqReFGgHq5IeFwAwJuZAXrQNkmQ-Hn6Fc";
            }

            var sheetsService = await gac.GetSheetsServiceAsync();
            sdp = SheetDataProcessor.Build(sheetsService, sheetId);
            comboView.sdp = sdp;

            trainingDays = sdp.TrainingDays;

            var combos = trainingDays.SelectMany(x => x.Combos).GroupBy(x => x.Name);

            //combosComboBox.ItemsSource = combos.Select(x => x.Key).OrderBy(x => x);

        }

        private void DayOfWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDay = (DayOfWeek)dayOfWeekCb.SelectedValue;

            comboView.Reset();
            foreach (var c in sdp.GetCombos(selectedDay).OrderBy(x => x.Order))
            {
                comboView.AddCombo(c);
            }
        }
    }
}
