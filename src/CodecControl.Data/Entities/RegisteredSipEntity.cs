using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodecControl.Data.Entities
{
    [Table("RegisteredSips")]
    public class RegisteredSipEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string SIP { get; set; }

        public string IP { get; set; }

        public DateTime Updated { get; set; }

        public Guid? UserAgentId { get; set; }

        [ForeignKey("UserAgentId")]
        public virtual UserAgentEntity UserAgent { get; set; }
    }
}
