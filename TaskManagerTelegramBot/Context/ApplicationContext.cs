using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerTelegramBot.Classes;

namespace TaskManagerTelegramBot.Context
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Database=PR9;Server=localhost;Trusted_Connection=true;Encrypt=false;");
        }
        public DbSet<User> Users { get; set; }
        public DbSet<RepeatableEvent> RepeatableEvents { get; set; }
        public DbSet<Event> Events { get; set; }
    }
}
