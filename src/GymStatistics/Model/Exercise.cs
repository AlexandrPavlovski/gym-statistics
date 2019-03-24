using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics.Model
{
    public class Exercise
    {
        public int Order;
        public string Name;
        public string Muscle;
        public string Rest;
        public string Repetitions;
        public string Work;
        public string Plan;
        public string Best;
        public DateTime Date;
        public string Feeling;
        public string Mode = "Разминка";
        public string Gym = "Империя силы";
    }
}
