using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gymer.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; }
        public DateTime ReminderDate { get; set; }
    }
}
