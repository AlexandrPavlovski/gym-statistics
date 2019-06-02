using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics.Model
{
    public class Combo
    {
        public int OrderInDay;
        public string Name;
        public List<Exercise> Exercises = new List<Exercise>();
        public List<string> Muscles = new List<string>();
    }
}
