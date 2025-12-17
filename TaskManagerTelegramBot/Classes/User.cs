using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot.Classes
{
    public class User
    {
        public long Id { get; set; }
        public ICollection<Event> Events { get; set; }
        public User(long userId) 
        {
            Id = userId;
            Events = new List<Event>();
        }
    }
}
