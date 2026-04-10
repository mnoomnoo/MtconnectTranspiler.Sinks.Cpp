using CaseExtensions;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Represents a generic constraint from the SysML model.
    /// </summary>
    public class Constraint : MtconnectVersionedObject
    {
        protected string _name { get; set; } = string.Empty;
        /// <summary>
        /// The intended C++ method name (PascalCase).
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = base.SysML_Name.ToPascalCase();
                return _name;
            }
            set { _name = value; }
        }

        /// <summary>
        /// Language of the constraint specification (typically "OCL2.0").
        /// </summary>
        public string SourceLanguage { get; set; } = "Unspecified";

        /// <summary>
        /// Raw constraint body/script.
        /// </summary>
        public string RawScript { get; set; }

        public Constraint(XmiDocument model, UmlConstraint source) : base(model, source)
        {
            SourceLanguage = source.Specification?.Language ?? "Unspecified";
            RawScript = source.Specification?.Body ?? string.Empty;
        }
    }
}
