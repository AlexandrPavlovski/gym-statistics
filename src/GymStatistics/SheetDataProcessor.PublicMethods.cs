using Google.Apis.Sheets.v4.Data;
using GymStatistics.Model;
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
            foreach(var ex in TrainingDays.SelectMany(x => x.Combos).SelectMany(x => x.Exercises))
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

        public void WriteDataToSheet()
        {
            IList<IList<object>> t = new List<IList<object>>
            {
                new List<object>
                {
                    1,2,3,4
                },
                new List<object>
                {
                    5,6,7,8
                }
            };

            var appendRequest = _sheetsService.Spreadsheets.Values.Append(new ValueRange { Values = t }, _spreadsheetId, "TEST");
            appendRequest.InsertDataOption = InsertDataOptionEnum.INSERTROWS;
            appendRequest.ValueInputOption = ValueInputOptionEnum.RAW;
            var response = appendRequest.Execute();

            var updatedRange = response.Updates.UpdatedRange.Split('!')[1];
            var rowToMerge = int.Parse(updatedRange.Split(':')[0].Substring(1));
            var id = _sheetsMetadata.First().Id;

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
                                SheetId = 1083751059,
                                StartColumnIndex = 0,
                                EndColumnIndex = 10,
                                StartRowIndex = 10,
                                EndRowIndex = 11
                            }
                        }
                    }
                }
            };
            var batchUpdateRequest = _sheetsService.Spreadsheets.BatchUpdate(updateRequest, _spreadsheetId);
            batchUpdateRequest.Execute();
        }
    }
}
