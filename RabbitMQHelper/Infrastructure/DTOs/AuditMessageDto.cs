using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQHelper.Infrastructure.DTOs
{
    public class AuditMessageDto
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
