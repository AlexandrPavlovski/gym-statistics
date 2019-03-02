using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using GymStatistics.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics
{
    class SheetDataProcessor
    {
        public TrainingDay[] TrainingDays;
        public ExerciseCombo[] AllCombos;
        public Dictionary<string, string[]> AllExercises;

        private SheetsService _sheetsService;
        private string _spreadsheetId;

        public static SheetDataProcessor Build(SheetsService sheetsService, string spreadsheetId)
        {
            var processor = new SheetDataProcessor();

            processor._sheetsService = sheetsService;
            processor._spreadsheetId = spreadsheetId;

            processor.BuildTrainingDays();
            processor.BuildAllCombos();
            processor.BuildAllExercises();

            return processor;
        }

        private void BuildTrainingDays()
        {
            var sheetsMetadata = GetSheetsMetadata();

            var valuesRequest = _sheetsService.Spreadsheets.Values.BatchGet(_spreadsheetId);
            valuesRequest.Ranges = new Repeatable<string>(sheetsMetadata.Select(x => x.Title));
            var values = valuesRequest.Execute();

            var trainingDays = new List<TrainingDay>();
            foreach (var sheet in sheetsMetadata)
            {
                TrainingDay day = null;
                ExerciseCombo combo = null;
                var rows = values.ValueRanges.First(x => x.Range.Contains(sheet.Title)).Values;
                for (int i = 0; i < rows.Count; i++)
                {
                    if (sheet.TrainingDayNumberRowIndices.Contains(i))
                    {
                        day = new TrainingDay
                        {
                            Number = rows[i][0].ToString()
                        };
                        trainingDays.Add(day);
                        continue;
                    }

                    if (sheet.ComboNameRowIndices.Contains(i))
                    {
                        if (combo != null)
                        {
                            combo.Muscles = combo.Muscles.GroupBy(x => x).Select(x => x.Key).ToList();
                        }

                        combo = new ExerciseCombo
                        {
                            Name = rows[i][0].ToString()
                        };
                        day.Combos.Add(combo);
                        continue;
                    }

                    combo.Exercises.Add(new Exercise
                    {
                        Name = rows[i][0].ToString(),
                        Muscle = rows[i][1].ToString()
                    });
                    combo.Muscles.Add(rows[i][1].ToString());
                }
            }

            TrainingDays = trainingDays.ToArray();
        }

        private IEnumerable<SheetMetadata> GetSheetsMetadata()
        {
            var spreadsheetsRequest = _sheetsService.Spreadsheets.Get(_spreadsheetId);
            spreadsheetsRequest.IncludeGridData = true;
            var spreadsheetsData = spreadsheetsRequest.Execute();

            return spreadsheetsData.Sheets
                .Select(x => new SheetMetadata
                {
                    Title = x.Properties.Title,
                    TrainingDayNumberRowIndices = GetTrainingDayNumberRowIndices(x),
                    ComboNameRowIndices = GetComboNameRowIndices(x)
                });
        }

        private int[] GetTrainingDayNumberRowIndices(Sheet sheet)
        {
            return sheet.Data[0].RowData
                .Select((y, index) => new
                {
                    index,
                    color = y.Values[0].EffectiveFormat.TextFormat.ForegroundColor,
                    isBold = y.Values[0].EffectiveFormat.TextFormat.Bold ?? false
                })
                .Where(y => y.color.Red == null && y.color.Green == null && y.color.Blue != null && y.isBold)
                .Select(y => y.index)
                .ToArray();
        }

        private int[] GetComboNameRowIndices(Sheet sheet)
        {
            return sheet.Data[0].RowData
                .Select((y, index) => new
                {
                    index,
                    color = y.Values[0].EffectiveFormat.BackgroundColor,
                    isBold = y.Values[0].EffectiveFormat.TextFormat.Bold ?? false
                })
                .Where(y => y.color.Red >= 0.8 && y.color.Red <= 0.9 && y.isBold)
                .Select(y => y.index)
                .ToArray();
        }

        private void BuildAllCombos()
        {
            var groupsOfCombos = TrainingDays
                .SelectMany(x => x.Combos)
                .GroupBy(x => x.Name);

            var allCombos = new List<ExerciseCombo>();
            foreach (var group in groupsOfCombos)
            {
                var comboCount = group.GroupBy(x => x, new ComboEqualityComparer())
                    .Select(x => new
                    {
                        Combo = x.Key,
                        Count = x.Count()
                    });

                ExerciseCombo trueCombo;
                if (comboCount.Count() == 1)
                {
                    trueCombo = comboCount.First().Combo;
                }
                else
                {
                    trueCombo = comboCount.Aggregate((curMax, x) => curMax == null || x.Count > curMax.Count ? x : curMax).Combo;
                }

                allCombos.Add(trueCombo);
            }

            AllCombos = allCombos.ToArray();
        }

        private void BuildAllExercises()
        {
            AllExercises = TrainingDays
                .SelectMany(x => x.Combos)
                .SelectMany(x => x.Exercises)
                .GroupBy(x => x.Muscle)
                .ToDictionary(x => x.Key, x => x.GroupBy(y => y.Name).Select(y => y.Key).ToArray());
        }

        private class ComboEqualityComparer : IEqualityComparer<ExerciseCombo>
        {
            public bool Equals(ExerciseCombo x, ExerciseCombo y)
            {
                return x.Exercises.Count == y.Exercises.Count && x.Exercises.All(y.Exercises.Contains);
            }

            public int GetHashCode(ExerciseCombo obj)
            {
                int hash = obj.Name.GetHashCode();
                foreach (var s in obj.Exercises)
                {
                    hash = hash ^ s.GetHashCode();
                }
                return hash;
            }
        }
    }
}
