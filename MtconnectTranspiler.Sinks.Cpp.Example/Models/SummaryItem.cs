using MtconnectTranspiler.Xmi;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Represents a single SysML comment in a documentation block.
    /// </summary>
    public class SummaryItem
    {
        internal OwnedComment _source;

        public SummaryItem(OwnedComment source)
        {
            _source = source;
        }
    }
}
