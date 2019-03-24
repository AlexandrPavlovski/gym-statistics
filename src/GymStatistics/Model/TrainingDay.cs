using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics.Model
{
    public class TrainingDay
    {
        public string Number;
        public List<Combo> Combos = new List<Combo>();
        public DateTime Date;
    }
}
