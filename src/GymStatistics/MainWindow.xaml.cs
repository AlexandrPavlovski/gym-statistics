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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GoogleApiClient gac;
        private TrainingDay[] trainingDays;
        private Dictionary<string, string[]> dayComboLookup = new Dictionary<string, string[]>
        {
            { "Понедельник", new[]
                {
                    ""
                }
            },
            { "Среда",new[]
                {
                    ""
                }
            },
            { "Пятница",new[]
                {
                    ""
                }
            }
        };

        public MainWindow()
        {
            gac = new GoogleApiClient();

            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var sheetId = sheetIdInput.Text;
            if (string.IsNullOrWhiteSpace(sheetId))
            {
                sheetId = "1o9ecJKUtohwqReFGgHq5IeFwAwJuZAXrQNkmQ-Hn6Fc";
            }

            var sheetsService = await gac.GetSheetsServiceAsync();
            var processor = SheetDataProcessor.Build(sheetsService, sheetId);
            trainingDays = processor.TrainingDays;

            var combos = trainingDays.SelectMany(x => x.Combos).GroupBy(x => x.Name);

            combosComboBox.ItemsSource = combos.Select(x => x.Key).OrderBy(x => x);
            dayOfWeekComboBox.ItemsSource = dayComboLookup.Keys;
            dayOfWeekComboBox.SelectedIndex = 0;
        }

        private void DayOfWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            combosComboBox.SelectedIndex = 0;
        }
    }
}
