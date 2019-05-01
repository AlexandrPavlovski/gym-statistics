using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics
{
    public static class Extensions
    {
        public static ExtendedValue ToExtendedValue(this string source)
        {
            return new ExtendedValue { StringValue = source };
        }
    }
}
