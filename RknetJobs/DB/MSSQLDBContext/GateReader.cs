using System.ComponentModel.DataAnnotations;

namespace RknetJobs.DB.MSSQLDBContext
{
    public class GateReader
    {
        [Key]
        public int RdrPtr { get; set; }
        public string Name { get; set; }
    }
}
