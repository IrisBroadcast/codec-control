using System.Collections.Generic;

namespace CodecControl.Web.Models
{
    public class AvailableGposResponse 
    {
        public List<AvailableGpo> Gpos { get; set; } = new List<AvailableGpo>();
    }

    public class AvailableGpo
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }

}