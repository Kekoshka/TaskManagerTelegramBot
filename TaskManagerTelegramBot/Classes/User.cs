using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot.Classes
{
    public class User
    {
        public Guid Id { get; set; }
        public ICollection<Event> Events { get; set; }
        public User(Guid userId) 
        {
            Id = userId;
            Events = new ICollection<Event>;
        }
    }
}
