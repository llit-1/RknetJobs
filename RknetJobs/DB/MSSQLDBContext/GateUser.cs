using System.ComponentModel.DataAnnotations;

namespace RknetJobs.DB.MSSQLDBContext
{
    public class GateUser
    {
        [Key]
        public int UserPtr { get; set; }
        public string Number { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string FatherName { get; set; }
        public int GroupPtr { get; set; }
        public bool Deleted { get; set; }
        public int? Status { get; set; }
        public bool? Photo { get; set; }
        public DateTime? Date { get; set; }
    }
}
