using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Audit
{
    [Table("auditing")]
    public class Auditing
    {
        [Key]
        [Column("audt_id")]
        public int AuditId { get; set; }

        [Column("scr_nm")]
        public string ScreenName { get; set; }

        [Column("obj_nm")]
        public string ObjectName { get; set; }
        [Column("scr_pk")]
        public Guid ScreenPk { get; set; }
        [Column("audt_json")]
        public string AuditJson { get; set; }
    }
}
