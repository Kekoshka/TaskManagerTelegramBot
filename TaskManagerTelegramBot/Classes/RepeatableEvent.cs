using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot.Classes
{
    public class RepeatableEvent
    {
        public Guid Id { get; set; }
        public TimeOnly Time { get; set; }
        public string Message { get; set; }

    }
}
