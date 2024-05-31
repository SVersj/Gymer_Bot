using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gymer.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public int Weight { get; set; }
        public int Reps { get; set; }
        public DateTime Date { get; set; }
    }
}
