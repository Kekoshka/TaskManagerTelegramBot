using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot.Classes
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<RepeatableEvent> RepeatableEvents { get; set; }
    }
}
