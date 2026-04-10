using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Xmi;
using System.Text;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Represents a code documentation block extracted from SysML comments.
    /// </summary>
    [ScribanTemplate("UmlCommentsSummaryContent.scriban")]
    public class Summary
    {
        /// <summary>
        /// Collection of individual summary items.
        /// </summary>
        public SummaryItem[] Items { get; }

        public OwnedComment[] Comments => Items.Select(o => o._source).ToArray();

        public string OriginalValue { get; }

        /// <summary>
        /// Constructs a <see cref="Summary"/> from an array of SysML comments.
        /// </summary>
        public Summary(OwnedComment[] comments)
        {
            Items = comments?.Select(o => new SummaryItem(o))?.ToArray();

            StringBuilder sb = new StringBuilder();
            foreach (var item in Items)
            {
                sb.AppendLine(composeComment(item._source));
            }
            OriginalValue = sb.ToString();
        }

        private string composeComment(OwnedComment comment)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("&#10;" + comment.Name + "&#10;");
            sb.Append("&#10;" + comment.Body + "&#10;");
            if (comment.SubComment != null)
            {
                sb.Append(composeComment(comment.SubComment));
            }
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string ToString()
            => OriginalValue;
    }
}
