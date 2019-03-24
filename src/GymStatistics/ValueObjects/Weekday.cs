using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics.ValueObjects
{
    public class Weekday
    {
        public string DisplayText { get; set; }
        public DayOfWeek Value { get; set; }
    }
}
