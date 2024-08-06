using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RknetJobs.DB.MSSQLDBContext
{    
    public class GateSettings
    {
        [Key]
        public int Id { get; set; }
        public string EventsPath { get; set; }
        public string PhotoPath { get; set; }
        public string WorkGroupFile { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string ConfigFile { get; set; }
        public DateTime LastUpdate { get; set; }
        
    }
}
