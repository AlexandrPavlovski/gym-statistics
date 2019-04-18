using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics
{
    public class AppData
    {
        public static AppData Build()
        {
            if (File.Exists(Constants.AppDataFileName))
            {
                var json = File.ReadAllText(Constants.AppDataFileName);
                return JsonConvert.DeserializeObject<AppData>(json);
            }
            else
            {
                return new AppData();
            }
        }

        public string LastOpenedSheetId { get; set; }
        public string LastOpenedSheetName { get; set; }


        private AppData() { }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(Constants.AppDataFileName, json);
        }
    }
}
