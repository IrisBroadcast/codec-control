using System.Collections.Generic;

namespace CodecControl.Web.Models
{
    public class AvailableGposViewModel 
    {
        public List<GpoViewModel> Gpos { get; set; }
        public AvailableGposViewModel()
        {
            Gpos = new List<GpoViewModel>();
        }
    }
}