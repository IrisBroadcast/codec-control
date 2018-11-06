using System.Collections.Generic;

namespace CodecControl.Web.Models.Responses
{
    public class BatchEnableInputsResponse
    {
        public IList<InputEnabledStatus> Inputs { get; set; } = new List<InputEnabledStatus>();
    }

    public class InputEnabledStatus
    {
        public bool Enabled { get; set; }
    }

}