using System.Diagnostics;
using Xunit;

namespace CodecControl.IntegrationTests.Helpers
{
    public class RunnableInDebugOnlyFactAttribute : FactAttribute
    {
        public RunnableInDebugOnlyFactAttribute()
        {
            if (!Debugger.IsAttached)
            {
                Skip = "Only running in interactive mode.";
            }
        }
    }
}