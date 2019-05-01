using Google.Apis.Sheets.v4.Data;
using GymStatistics.Model;
using GymStatistics.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.AppendRequest;

namespace GymStatistics
{
    public partial class SheetDataProcessor
    {
        public IEnumerable<Combo> GetCombos(DayOfWeek day)
        {
            var muscles = DayMusclesLookup[day];
            var prevDay = TrainingDays
                    .Where(x => x.Date.DayOfWeek == day)
                    .First();

            foreach (var m in muscles)
            {
                var combos = MostUsedCombos
                    .Where(x => x.Muscles.Contains(m));

                var prevCombo = prevDay.Combos
                    .First(x => x.Muscles.Contains(m))
                    .Name;

                var combo = combos
                    .SkipWhile(x => x.Name != prevCombo)
                    .Skip(1)
                    .FirstOrDefault() ?? combos.First();

                yield return combo;
            }
        }

        public Exercise GetPrevExercise(string exerciseName, int reversedOrder = 1)
        {
            int foundCount = 0;
            foreach (var ex in TrainingDays.SelectMany(x => x.Combos).SelectMany(x => x.Exercises))
            {
                if (ex.Name == exerciseName)
                {
                    if (foundCount == reversedOrder)
                    {
                        return ex;
                    }
                    foundCount++;
                }
            }
            return null;
        }

        public void WriteDataToSheet(ComboViewVM comboViewVM, string date)
        {            
            var trainingDayNumberCellFormat = new CellFormat
            {
                TextFormat = new TextFormat { Bold = true, ForegroundColor = new Color { Blue = 1f } },
                Borders = new Borders
                {
                    Bottom = new Border { Width = 1, Style = "SOLID" },
                    Left = new Border { Width = 1, Style = "SOLID" },
                    Right = new Border { Width = 1, Style = "SOLID" },
                    Top = new Border { Width = 1, Style = "SOLID" }
                }
            };
            var comboHeaderCellFormat = new CellFormat
            {
                BackgroundColor = new Color { Blue = 0.8509804f, Green = 0.8509804f, Red = 0.8509804f },
                TextFormat = new TextFormat { Bold = true },
                Borders = new Borders
                {
                    Bottom = new Border { Width = 1, Style = "SOLID" },
                    Left = new Border { Width = 1, Style = "SOLID" },
                    Right = new Border { Width = 1, Style = "SOLID" },
                    Top = new Border { Width = 1, Style = "SOLID" }
                }
            };
            var regularCelFormat = new CellFormat
            {
                Borders = new Borders
                {
                    Bottom = new Border { Width = 1, Style = "SOLID" },
                    Left = new Border { Width = 1, Style = "SOLID" },
                    Right = new Border { Width = 1, Style = "SOLID" },
                    Top = new Border { Width = 1, Style = "SOLID" }
                }
            };

            List<RowData> rowsData = new List<RowData>(8);
            rowsData.Add(new RowData
            {
                Values = new List<CellData>
                {
                    GetSellData($"Тренировка {TrainingDays.Length + 1}", trainingDayNumberCellFormat)
                }
            });

            foreach (var combo in comboViewVM.Combos)
            {
                rowsData.Add(new RowData
                {
                    Values = new List<CellData>
                    {
                        GetSellData(combo.Name, comboHeaderCellFormat),
                        GetSellData("Мышцы", comboHeaderCellFormat),
                        GetSellData("Отдых", comboHeaderCellFormat),
                        GetSellData("Подходы", comboHeaderCellFormat),
                        GetSellData("Работа", comboHeaderCellFormat),
                        GetSellData("План", comboHeaderCellFormat),
                        GetSellData("Best", comboHeaderCellFormat),
                        GetSellData("Дата", comboHeaderCellFormat),
                        GetSellData("Чувство", comboHeaderCellFormat),
                        GetSellData("Режим", comboHeaderCellFormat),
                        GetSellData("Зал", comboHeaderCellFormat)
                    }
                });

                foreach (var exercise in combo.Exercises)
                {
                    rowsData.Add(new RowData
                    {
                        Values = new List<CellData>
                        {
                            GetSellData(exercise.TodayEx.Name),
                            GetSellData(exercise.TodayEx.Muscle),
                            GetSellData(exercise.TodayEx.Rest),
                            GetSellData(exercise.TodayEx.Repetitions),
                            GetSellData(string.Empty),
                            GetSellData(exercise.TodayEx.Plan),
                            GetSellData(exercise.TodayEx.Best),
                            GetSellData(date),
                            GetSellData(string.Empty),
                            GetSellData(exercise.TodayEx.Mode),
                            GetSellData(exercise.TodayEx.Gym)
                        }
                    });
                }
            }

            CellData GetSellData(string value, CellFormat format = null)
            {
                return new CellData
                {
                    UserEnteredValue = value.ToExtendedValue(),
                    UserEnteredFormat = format ?? regularCelFormat
                };
            }

            var sheetId = _sheetsMetadata.First().Id;
            var updateRequest = new BatchUpdateSpreadsheetRequest()
            {
                Requests = new List<Request>
                {
                    new Request
                    {
                        MergeCells = new MergeCellsRequest
                        {
                            MergeType = "MERGE_ALL",
                            Range = new GridRange
                            {
                                SheetId = sheetId,
                                StartColumnIndex = 0,
                                EndColumnIndex = 11,
                                StartRowIndex = LastRowWithDataIndex + 1,
                                EndRowIndex = LastRowWithDataIndex + 2
                            }
                        }
                    },
                    new Request
                    {
                        AppendCells = new AppendCellsRequest
                        {
                            SheetId = sheetId,
                            Fields = "*",
                            Rows = rowsData
                        }
                    }
                }
            };
            var batchUpdateRequest = _sheetsService.Spreadsheets.BatchUpdate(updateRequest, _spreadsheetId);
            batchUpdateRequest.Execute();
        }
    }
}
