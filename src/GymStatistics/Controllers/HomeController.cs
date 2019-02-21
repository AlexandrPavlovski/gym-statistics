using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;

using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using Newtonsoft.Json;

namespace GymStatistics.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                var service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "gym-statistics"
                });

                string id = "18JWctnEO-3gTZ68-WLUkxrQyulycOPxgwvqmHUrGCsI";
                string range = "Sheet1!A2:E";
                ValueRange valueRange = new ValueRange();
                valueRange.MajorDimension = "COLUMNS";//"ROWS";//COLUMNS

                var request = service.Spreadsheets.Values.Get(id, range);
                var vals = request.Execute();

                return View(model: JsonConvert.SerializeObject(vals));
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<ActionResult> Goo(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);

            if (result.Credential != null)
            {
                var service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = result.Credential,
                    ApplicationName = "gym-statistics"
                });

                string id = "18JWctnEO-3gTZ68-WLUkxrQyulycOPxgwvqmHUrGCsI";
                string range = "Sheet1!A2:E";
                ValueRange valueRange = new ValueRange();
                valueRange.MajorDimension = "COLUMNS";//"ROWS";//COLUMNS

                var request = service.Spreadsheets.Values.Get(id, range);
                var vals = request.Execute();

                return View();
            }
            else
            {
                return new RedirectResult(result.RedirectUri);
            }
        }
    }
}