using System.ComponentModel.DataAnnotations;

namespace SharedRepository.Audit
{
    public class AuditMessage
    {
        [Range(1, 3)]
        public int OprtnTyp { get; set; }
        [StringLength(100)]
        public string UsrNm { get; set; }
        public int UsrNo { get; set; }
        public List<string> LogDsc { get; set; }
        [Range(1, int.MaxValue)]
        public int LogTyp { get; set; }
        public DateTime LogDate { get; set; }
        [StringLength(50)]
        public string ScreenName { get; set; }
        [StringLength(50)]
        public string ObjectName { get; set; }
        public Guid ScreenPk { get; set; }
    }
}
