using MtconnectTranspiler.Sinks.Cpp.Example;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Represents a single value in a C++ <c>enum class</c>.
    /// </summary>
    public class EnumItem : MtconnectVersionedObject
    {
        /// <summary>
        /// Documentation from the SysML model.
        /// </summary>
        public Summary Summary { get; protected set; }

        protected string _name { get; set; }
        /// <summary>
        /// The UPPER_SNAKE_CASE enumerator name used in C++.
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = CppHelperMethods.ToSnakeCase(base.SysML_Name)?.ToUpperInvariant();
                return _name;
            }
            set { _name = value; }
        }

        public string OriginalName => base.SysML_Name;

        private string _namespace;
        public string Namespace
        {
            get { return _namespace; }
            set
            {
                _namespace = value;
                TypeCache.RegisterType(this.SysML_ID, SysML_Name, _namespace);
            }
        }

        public EnumItem(XmiDocument model, XmiElement source) : base(model, source) { }

        public EnumItem(XmiDocument model, UmlEnumerationLiteral source) : this(model, source as XmiElement)
        {
            if (source?.Comments?.Length > 0)
                Summary = new Summary(source.Comments);
        }

        public EnumItem(XmiDocument model, UmlClass source) : this(model, source as XmiElement)
        {
            if (source?.Comments?.Length > 0)
                Summary = new Summary(source.Comments);
        }
    }
}
