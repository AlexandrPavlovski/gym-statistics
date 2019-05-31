using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using GymStatistics.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GymStatistics
{
    public partial class SheetDataProcessor
    {
        public TrainingDay[] TrainingDays { get; private set; }
        public Dictionary<string, Combo> AllCombos { get; private set; }
        public Combo[] MostUsedCombos { get; private set; }
        public Exercise[] AllExercises { get; private set; }
        public Dictionary<string, string[]> MuscleExercisesLookup { get; private set; }
        public Dictionary<DayOfWeek, string[]> DayMusclesLookup { get; private set; }

        public string SheetTitle { get; private set; }
        public int LastRowWithDataIndex { get; private set; }
        public List<string> ColumnNames { get; private set; }

        private string _spreadsheetId;
        private SheetsService _sheetsService;
        private IEnumerable<SheetMetadata> _sheetsMetadata;

        public static SheetDataProcessor Build(SheetsService sheetsService, string spreadsheetId, IProgress<int> progress)
        {
            var processor = new SheetDataProcessor();

            processor._sheetsService = sheetsService;
            processor._spreadsheetId = spreadsheetId;

            processor.BuildTrainingDays(progress);

            if (File.Exists(Constants.CombosFileName))
            {
                processor.ReadAllCombos();
            }
            else
            {
                processor.BuildAllCombos();
            }
            progress.Report(70);

            processor.BuildAllExercises();
            progress.Report(80);

            processor.BuildDayMusclesLookup();
            progress.Report(90);

            processor.BuildMuscleExercisesLookup();
            progress.Report(100);

            return processor;
        }

        private void BuildTrainingDays(IProgress<int> progress)
        {
            _sheetsMetadata = GetSheetsMetadata();

            var valuesRequest = _sheetsService.Spreadsheets.Values.BatchGet(_spreadsheetId);
            valuesRequest.Ranges = new Repeatable<string>(_sheetsMetadata.Select(x => x.Title));
            var values = valuesRequest.Execute();

            int prgrss = 20;
            int prgrssStep = 60 / _sheetsMetadata.Count();
            var trainingDays = new List<TrainingDay>();
            foreach (var sheet in _sheetsMetadata)
            {
                TrainingDay day = null;
                Combo combo = null;
                int trainingDayOrder = 1;
                int comboOrder = 0;
                int exerciseOrder = 0;

                var rows = values.ValueRanges.First(x => x.Range.Contains(sheet.Title)).Values;
                if (rows == null) continue;

                for (int i = 0; i < rows.Count; i++)
                {
                    if (sheet.TrainingDayNumberRowIndices.Contains(i))
                    {
                        comboOrder = 0;
                        if (day != null)
                        {
                            day.IsPlanned = day.Combos.All(x => x.Exercises.All(y => !string.IsNullOrWhiteSpace(y.Work)));
                        }

                        day = new TrainingDay
                        {
                            Oreder = trainingDayOrder++,
                            Name = rows[i][0].ToString().Trim(),
                            Date = DateTime.Parse(rows[i + 2][7].ToString())
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

                        exerciseOrder = 0;
                        combo = new Combo
                        {
                            Name = rows[i][0].ToString().Trim(),
                            OrderInDay = comboOrder++
                        };
                        day.Combos.Add(combo);
                        continue;
                    }

                    var exercise = new Exercise
                    {
                        Name = rows[i][0].ToString().Trim(),
                        Muscle = rows[i][1].ToString().Trim(),
                        Rest = rows[i][2].ToString().Trim(),
                        Repetitions = rows[i][3].ToString().Trim(),
                        Work = rows[i][4].ToString(),
                        Plan = rows[i][5].ToString().Trim(),
                        Best = rows[i][6].ToString().Trim(),
                        Date = day.Date,
                        Feeling = rows[i][8].ToString(),
                        Mode = rows[i][9].ToString(),
                        Gym = rows[i][10].ToString(),
                        Order = exerciseOrder++
                    };
                    combo.Exercises.Add(exercise);
                    combo.Muscles.Add(rows[i][1].ToString().Trim());
                }

                prgrss += prgrssStep;
                progress.Report(prgrss);
            }

            LastRowWithDataIndex = values.ValueRanges.First(x => x.Range.Contains(_sheetsMetadata.First().Title)).Values?.Count ?? -1;
            ColumnNames = values.ValueRanges.First(x => x.Range.Contains(_sheetsMetadata.Last().Title)).Values[1].Skip(1).Select(x => x.ToString()).ToList();

            TrainingDays = trainingDays.OrderByDescending(x => x.Date).ToArray();
        }

        private IEnumerable<SheetMetadata> GetSheetsMetadata()
        {
            var spreadsheetsRequest = _sheetsService.Spreadsheets.Get(_spreadsheetId);
            spreadsheetsRequest.IncludeGridData = true;
            var spreadsheetsData = spreadsheetsRequest.Execute();

            SheetTitle = spreadsheetsData.Properties.Title;

            return spreadsheetsData.Sheets
                .Where(x => Regex.IsMatch(x.Properties.Title, @"^TR\d+-\d*"))
                .Select(x => new SheetMetadata
                {
                    Title = x.Properties.Title,
                    Id = x.Properties.SheetId,
                    TrainingDayNumberRowIndices = GetTrainingDayNumberRowIndices(x),
                    ComboNameRowIndices = GetComboNameRowIndices(x)
                });
        }

        private int[] GetTrainingDayNumberRowIndices(Sheet sheet)
        {
            return sheet.Data[0].RowData == null
                ? new int[0]
                : sheet.Data[0].RowData
                    .Where(x => x.Values?[0]?.EffectiveFormat != null)
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
            return sheet.Data[0].RowData == null
                ? new int[0]
                : sheet.Data[0].RowData
                    .Where(x => x.Values?[0]?.EffectiveFormat != null)
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

            var allCombos = new List<Combo>();
            var mostUsedCombos = new List<Combo>();
            foreach (var group in groupsOfCombos)
            {
                var comboCount = group.GroupBy(x => x, new ComboEqualityComparer())
                    .Select(x => new
                    {
                        Combo = x.Key,
                        Count = x.Count()
                    });

                Combo trueCombo;
                if (comboCount.Count() == 1)
                {
                    trueCombo = comboCount.First().Combo;
                }
                else
                {
                    trueCombo = comboCount.Aggregate((curMax, x) => curMax == null || x.Count > curMax.Count ? x : curMax).Combo;
                    mostUsedCombos.Add(trueCombo);
                }

                allCombos.Add(trueCombo);
            }

            AllCombos = allCombos.ToDictionary(x => x.Name, x => x);
            MostUsedCombos = mostUsedCombos.OrderBy(x => x.Name).ToArray();
        }

        private void ReadAllCombos()
        {
            var json = File.ReadAllText(Constants.CombosFileName, Encoding.UTF8);
            var combos = JsonConvert.DeserializeObject<Combo[]>(json);

            foreach(var c in combos)
            {
                c.Muscles = c.Exercises.Select(x => x.Muscle).Distinct().ToList();
            }

            MostUsedCombos = combos;
            AllCombos = combos.ToDictionary(x => x.Name, x => x);
        }

        private void BuildAllExercises()
        {
            AllExercises = AllCombos.Values
                .SelectMany(x => x.Exercises)
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .OrderBy(x => x.Name)
                .ToArray();
        }

        private void BuildDayMusclesLookup()
        {
            var groupsOfDays = TrainingDays
                .Where(x => x.Date.DayOfWeek == DayOfWeek.Monday
                    || x.Date.DayOfWeek == DayOfWeek.Wednesday
                    || x.Date.DayOfWeek == DayOfWeek.Friday)
                .GroupBy(x => x.Date.DayOfWeek, x => x.Combos.SelectMany(y => y.Muscles));

            DayMusclesLookup = new Dictionary<DayOfWeek, string[]>();
            foreach (var group in groupsOfDays)
            {
                var musclesCount = group
                    .GroupBy(x => x, new EnumerableStringEqualityComparer())
                    .Select(x => new
                    {
                        Muscles = x.Key.ToArray(),
                        Count = x.Count()
                    });

                string[] trueMuscles;
                if (musclesCount.Count() == 1)
                {
                    trueMuscles = musclesCount.First().Muscles;
                }
                else
                {
                    trueMuscles = musclesCount.Aggregate((curMax, x) => curMax == null || x.Count > curMax.Count ? x : curMax).Muscles;
                }

                DayMusclesLookup.Add(group.Key, trueMuscles);
            }
        }

        private void BuildMuscleExercisesLookup()
        {
            MuscleExercisesLookup = TrainingDays
                .SelectMany(x => x.Combos)
                .SelectMany(x => x.Exercises)
                .GroupBy(x => x.Muscle)
                .ToDictionary(x => x.Key, x => x.GroupBy(y => y.Name).Select(y => y.Key).ToArray());
        }


        private class ComboEqualityComparer : IEqualityComparer<Combo>
        {
            public bool Equals(Combo x, Combo y)
            {
                return x.Exercises.Count == y.Exercises.Count && x.Exercises.All(y.Exercises.Contains);
            }

            public int GetHashCode(Combo obj)
            {
                int hash = obj.Name.GetHashCode();
                foreach (var s in obj.Exercises)
                {
                    hash = hash ^ s.GetHashCode();
                }
                return hash;
            }
        }

        private class EnumerableStringEqualityComparer : IEqualityComparer<IEnumerable<string>>
        {
            public bool Equals(IEnumerable<string> x, IEnumerable<string> y)
            {
                return x.Count() == y.Count() && x.All(y.Contains);
            }

            public int GetHashCode(IEnumerable<string> obj)
            {
                int hash = 19;
                foreach (var s in obj)
                {
                    hash = hash ^ s.GetHashCode();
                }
                return hash;
            }
        }
    }
}
