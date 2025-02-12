namespace RabbitMQHelper.Infrastructure.Entities
{
    public class AuditMessage
    {
        public int OprtnTyp { get; set; }
        public string UsrNm { get; set; }
        public int UsrNo { get; set; }
        public List<string> LogDsc { get; set; }
        public int LogTyp { get; set; }
        public DateTime LogDate { get; set; }
        public string ScreenName { get; set; }
        public string ObjectName { get; set; }
        public Guid ScreenPk { get; set; }
    }
}
