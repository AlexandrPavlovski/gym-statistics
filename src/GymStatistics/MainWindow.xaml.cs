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
        private GoogleApiClient _gac;
        private SheetDataProcessor _sdp;
        private Weekday[] _days;

        private AppData _appData;

        public MainWindow()
        {
            _gac = new GoogleApiClient();
            _appData = AppData.Build();

            InitializeComponent();

            _days = new[]
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
            dayOfWeekCb.ItemsSource = _days;
            dayOfWeekCb.DisplayMemberPath = "DisplayText";
            dayOfWeekCb.SelectedValuePath = "Value";

            sheetIdInput.Text = _appData.LastOpenedSheetId;
            sheetNameInput.Text = _appData.LastOpenedSheetName;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var sheetId = sheetIdInput.Text;

            var sheetsService = await _gac.GetSheetsServiceAsync();
            _sdp = SheetDataProcessor.Build(sheetsService, sheetId);

            comboView.Init(_sdp);

            sheetNameInput.Text = _sdp.SheetTitle;

            _appData.LastOpenedSheetId = sheetId;
            _appData.LastOpenedSheetName = _sdp.SheetTitle;
            _appData.Save();

            dayOfWeekCb.IsEnabled = true;
            dateTexBox.IsEnabled = true;

            var today = DateTime.Now.Date;
            var todayDayOfWeekIndex = (int)today.DayOfWeek;
            var nextTrainingDaysToAdd = todayDayOfWeekIndex > 5 
                ? ((int)DayOfWeek.Monday - todayDayOfWeekIndex + 7) % 7
                : todayDayOfWeekIndex % 2 == 0 ? 1 : 0;
            var nextTrainingDay = today.AddDays(nextTrainingDaysToAdd);

            dateTexBox.Text = nextTrainingDay.ToString("yyyy-MM-dd");
            dayOfWeekCb.SelectedItem = _days.First(x => x.Value == nextTrainingDay.DayOfWeek);
        }

        private void DayOfWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboView.SetDayOfWeek((DayOfWeek)dayOfWeekCb.SelectedValue);
        }
    }
}
