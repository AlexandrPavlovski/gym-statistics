using GymStatistics.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymStatistics
{
    public partial class SheetDataProcessor
    {
        public IEnumerable<Combo> GetCombos(DayOfWeek day)
        {
            var muscles = DayMusclesLookup[day];
            var prevDay = TrainingDays
                    .Where(x => x.Date.DayOfWeek == day)
                    .First();

            foreach (var m in muscles)
            {
                var combos = MostUsedCombos
                    .Where(x => x.Muscles.Contains(m));

                var prevCombo = prevDay.Combos
                    .First(x => x.Muscles.Contains(m))
                    .Name;

                var combo = combos
                    .SkipWhile(x => x.Name != prevCombo)
                    .Skip(1)
                    .FirstOrDefault() ?? combos.First();

                yield return combo;
            }
        }

        public Exercise GetPrevExercise(string exerciseName, int reversedOrder = 1)
        {
            int foundCount = 0;
            foreach(var ex in TrainingDays.SelectMany(x => x.Combos).SelectMany(x => x.Exercises))
            {
                if (ex.Name == exerciseName)
                {
                    foundCount++;
                    if (foundCount == reversedOrder)
                    {
                        return ex;
                    }                    
                }
            }
            return null;
        }
    }
}
