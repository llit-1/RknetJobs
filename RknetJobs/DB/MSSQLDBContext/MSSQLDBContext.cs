using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RknetJobs.DB.MSSQLDBContext
{
    internal class MSSQLDBContext : DbContext
    { 
        public DbSet<GateEvent> GateEvents { get; set; }
        public DbSet<GateUser> GateUsers { get; set; }
        public DbSet<GateGroup> GateGroups { get; set; }
        public DbSet<GateReader> GateReaders { get; set; }
        public DbSet<GateSettings> GateSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=RKSQL.shzhleb.ru\\SQL2019; Initial Catalog=RKNET; User ID=rk7; Password=wZSbs6NKl2SF; TrustServerCertificate=True");
        }
    }
}
