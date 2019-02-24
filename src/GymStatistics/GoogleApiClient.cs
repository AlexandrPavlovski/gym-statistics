using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Util;

namespace GymStatistics
{
    internal class GoogleApiClient
    {
        private const string applicationName = "gym-statistics";
        private const string clientSecret = "05jHhmew_R7ThqpQMb2UCUi-";
        private const string clientId = "188215911639-vfmu4svuiad08sn470uqb7h65mrtlevj.apps.googleusercontent.com";
        private static readonly ClientSecrets clientSecrets = new ClientSecrets() { ClientId = clientId, ClientSecret = clientSecret };
        private static readonly string[] scopes = { SheetsService.Scope.Spreadsheets };

        public async Task<List<TrainingDay>> GetSheetDataAsync(string spreadsheetId)
        {
            var sheetService = await AuthorizeGoogleAppAsync();

            var metadataRequest = sheetService.Spreadsheets.Get(spreadsheetId);
            metadataRequest.IncludeGridData = true;
            var sheetsMetadata = metadataRequest.Execute();

            var sheetsData = sheetsMetadata.Sheets
                .Select(x => new
                {
                    x.Properties.Title,
                    TrainingDayNumberRowIndices = x.Data[0].RowData
                        .Select((y, index) => new
                        {
                            index,
                            color = y.Values[0].EffectiveFormat.TextFormat.ForegroundColor,
                            isBold = y.Values[0].EffectiveFormat.TextFormat.Bold ?? false
                        })
                        .Where(y => y.color.Red == null && y.color.Green == null && y.color.Blue != null && y.isBold)
                        .Select(y => y.index)
                        .ToArray(),
                    ComboNameRowIndices = x.Data[0].RowData
                        .Select((y, index) => new
                        {
                            index,
                            color = y.Values[0].EffectiveFormat.BackgroundColor,
                            isBold = y.Values[0].EffectiveFormat.TextFormat.Bold ?? false
                        })
                        .Where(y => y.color.Red >= 0.8 && y.color.Red <= 0.9 && y.isBold)
                        .Select(y => y.index)
                        .ToArray()
                });

            var request = sheetService.Spreadsheets.Values.BatchGet(spreadsheetId);
            request.Ranges = new Repeatable<string>(sheetsData.Select(x => x.Title));

            var values = request.Execute();

            var trainingDays = new List<TrainingDay>();
            foreach (var sheet in sheetsData)
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
                        combo = new ExerciseCombo
                        {
                            Name = rows[i][0].ToString()
                        };
                        day.Combos.Add(combo);
                        continue;
                    }

                    combo.Exercises.Add(rows[i][0].ToString());
                }
            }

            return trainingDays;
        }

        private async Task<SheetsService> AuthorizeGoogleAppAsync()
        {
            string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");
            var fileStore = new FileDataStore(credPath, true);

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, scopes, "user", CancellationToken.None, fileStore);
            Console.WriteLine("Credential file saved to: " + credPath);

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });
            return service;
        }
    }

    class TrainingDay
    {
        public string Number;
        public List<ExerciseCombo> Combos = new List<ExerciseCombo>();
    }

    class ExerciseCombo
    {
        public string Name;
        public List<string> Exercises = new List<string>();
    }
}
