using System.Collections.Generic;
using CodecControl.Client.Models;

namespace CodecControl.Web.Models.Responses
{
    public class UnitStatusResponse 
    {
        public bool Available { get; set; }
        public string IpAddress { get; set; }
    }
}