using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodecControl.Data.Entities
{
    // Kodarmodell
    [Table("UserAgents")]
    public class UserAgentEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string Api { get; set; }
        public string GpoNames { get; set; }
        public int Inputs { get; set; }
    }
}
