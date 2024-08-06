namespace RknetJobs.DB.MSSQLDBContext
{
    public class GateEvent
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public DateTime? DateTime { get; set; }
        public int EventType { get; set; }
        public int EventCode { get; set; }
        public int RdrPtr { get; set; }
        public int UserPtr { get; set; }
        public int OperatorID { get; set; }
        public int AlarmStatus { get; set; }
        public string Unit { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
    }
}
