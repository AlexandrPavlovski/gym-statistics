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
using GymStatistics.Model;

namespace GymStatistics
{
    internal class GoogleApiClient
    {
        private const string applicationName = "gym-statistics";
        private const string clientSecret = "05jHhmew_R7ThqpQMb2UCUi-";
        private const string clientId = "188215911639-vfmu4svuiad08sn470uqb7h65mrtlevj.apps.googleusercontent.com";
        private static readonly ClientSecrets clientSecrets = new ClientSecrets() { ClientId = clientId, ClientSecret = clientSecret };
        private static readonly string[] scopes = { SheetsService.Scope.Spreadsheets };

        public async Task<SheetsService> GetSheetsServiceAsync()
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
}
