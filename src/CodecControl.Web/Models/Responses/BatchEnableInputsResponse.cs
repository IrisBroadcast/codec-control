
using System.Collections.Generic;

namespace CodecControl.Web.Models.Responses
{
    public class BatchEnableInputsResponse
    {
        public IList<InputEnabledStatus> Inputs { get; set; }
    }
    public class InputEnabledStatus
    {
        public int Input { get; set; }
        public bool Enabled { get; set; }
    }
}