using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Represents a C++ struct member derived from a UML property.
    /// </summary>
    public class Property : CppType
    {
        /// <summary>
        /// Reference to the <c>name</c> attribute.
        /// </summary>
        public string NormativeName { get; set; }

        /// <summary>
        /// Documentation from the SysML model.
        /// </summary>
        public Summary Summary { get; protected set; }

        /// <summary>
        /// The resolved C++ type for this property.
        /// </summary>
        public string Type { get; set; }

        public string OriginalPropertyType { get; set; }
        public string Aggregation { get; set; }
        public string Extension { get; set; }
        public string Association { get; set; }
        public string DefaultValue { get; set; }

        /// <summary>
        /// UML multiplicity, e.g. "0..*" or "1..1". Empty when unspecified.
        /// </summary>
        public string Multiplicity { get; set; }

        private XmiElement? _remoteType { get; set; }

        public Property(XmiDocument model, UmlProperty source) : base(model, source)
        {
            NormativeName = source.Name;

            if (source.Comments?.Length > 0)
                Summary = new Summary(source.Comments);

            AccessModifier = source.Visibility;
            Modifier = source.IsStatic ? "static" : source.IsReadOnly ? "const" : "";

            XmiElement? remoteType = null;
            Type = CppHelperMethods.ToPrimitiveType(model, source)
                ?? CppHelperMethods.TypeDeepSearch(model, source.PropertyType, out remoteType)
                ?? "std::string";

            OriginalPropertyType = source.PropertyType;
            Aggregation = source.Aggregation;
            Extension = source.Extension?.Extender;
            Association = CppHelperMethods.TypeDeepSearch(model, source.Association, out remoteType);
            if (source.DefaultValue is UmlInstanceValue instanceValue)
            {
                DefaultValue = CppHelperMethods.TypeDeepSearch(model, instanceValue.Instance, out XmiElement instanceType);
            }
            else
            {
                DefaultValue = source.DefaultValue?.Name;
            }

            string lower = source.LowerValue?.Value;
            Multiplicity = lower != null ? $"{lower}..*" : "";
        }
    }
}
