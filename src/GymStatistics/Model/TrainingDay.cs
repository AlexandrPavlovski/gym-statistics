using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics.Model
{
    public class TrainingDay
    {
        public int Oreder;
        public string Name;
        public List<Combo> Combos = new List<Combo>();
        public DateTime Date;
        public bool IsPlanned; // for days planned in advanced and not yet performed
    }
}
