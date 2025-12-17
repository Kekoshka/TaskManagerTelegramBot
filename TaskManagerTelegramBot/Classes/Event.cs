using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot.Classes
{
    public class Event
    {
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public Event(DateTime time, string message)
        {
            Time = time;
            Message = message;
        }
    }
}
