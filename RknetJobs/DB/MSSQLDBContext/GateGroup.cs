using System.ComponentModel.DataAnnotations;

namespace RknetJobs.DB.MSSQLDBContext
{
    public class GateGroup
    {
        [Key]
        public int GroupPtr { get; set; }
        public string Name { get; set; }
    }
}
