namespace SharedRepository.Audit
{
    public class AuditLogDto
    {
        public int OprtnTyp { get; set; }
        public string UsrNm { get; set; }
        public int UsrNo { get; set; }
        public List<string> LogDsc { get; set; }
        public int LogTyp { get; set; }
        public DateTime LogDate { get; set; }
    }
}
