using Google;
using GymStatistics.Model;
using GymStatistics.UserControls;
using GymStatistics.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private Key[] _cheatInput;

        private AppData _appData;

        public MainWindow()
        {
            _cheatInput = new Key[3];

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

        private async void LoadBnt_Click(object sender, RoutedEventArgs e)
        {
            ToggleLoading();

            string sheetId = sheetIdInput.Text;

            IProgress<int> progress = new Progress<int>(percent => progressBar.Value = percent);
            //SheetDataProcessor.Build(await _gac.GetSheetsServiceAsync(), sheetId.ToString(), progress);
            try
            {
                _sdp = await Task.Factory.StartNew(async () =>
                {
                    progress.Report(10);

                    var sheetsService = await _gac.GetSheetsServiceAsync();
                    progress.Report(20);

                    return SheetDataProcessor.Build(sheetsService, sheetId.ToString(), progress);
                })
                .Unwrap();
            }
            catch (GoogleApiException)
            {
                ToggleLoading();
                progressBar.Value = 0;

                MessageBox.Show("Таблица с указанным ID не найдена", "Еггог", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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
            dayOfWeekCb.SelectedItem = null;
            dayOfWeekCb.SelectedItem = _days.First(x => x.Value == nextTrainingDay.DayOfWeek);

            writeBtn.Visibility = Visibility.Visible;
            ToggleLoading();
            progressBar.Value = 0;
        }

        private async void WriteBnt_Click(object sender, RoutedEventArgs e)
        {
            ToggleWritting();

            IProgress<int> progress = new Progress<int>(percent => progressBar.Value = percent);
            await Task.Factory.StartNew(date =>
            {
                progress.Report(50);
                _sdp.WriteDataToSheet(comboView.ViewModel, date.ToString());
                progress.Report(100);
            }, dateTexBox.Text);

            ToggleWritting();
            progressBar.Value = 0;
        }

        private void DayOfWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dayOfWeekCb.SelectedValue != null)
            {
                comboView.SetDayOfWeek((DayOfWeek)dayOfWeekCb.SelectedValue);
            }
        }

        private void ToggleLoading()
        {
            progressBar.Visibility = progressBar.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            loadingVeiwbox.Visibility = progressBar.Visibility;
        }

        private void ToggleWritting()
        {
            if (loadingTextBlock.Tag == null)
            {
                loadingTextBlock.Tag = "Запись...";
            }

            var t = loadingTextBlock.Tag;
            loadingTextBlock.Tag = loadingTextBlock.Text;
            loadingTextBlock.Text = t.ToString();

            progressBar.Visibility = progressBar.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            loadingVeiwbox.Visibility = progressBar.Visibility;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _cheatInput[0] = _cheatInput[1];
            _cheatInput[1] = _cheatInput[2];
            _cheatInput[2] = e.Key;

            if (_cheatInput[0] == Key.G && _cheatInput[1] == Key.U && _cheatInput[2] == Key.D)
            {
                if (Tag == null)
                {
                    Tag = new YouDiedUserControl();
                }

                var t = Content;
                Content = Tag;
                Tag = t;
            }
        }
    }
}
