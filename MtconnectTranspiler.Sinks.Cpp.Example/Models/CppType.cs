using MtconnectTranspiler.Sinks.Cpp.Example;
using MtconnectTranspiler.Sinks.Cpp.Example.Models;
using MtconnectTranspiler.Xmi;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Represents the abstract base for many C++ types.
    /// </summary>
    public abstract class CppType : MtconnectVersionedObject
    {
        private string _namespace = "mtconnect";
        /// <summary>
        /// The intended C++ namespace for the type (e.g. "mtconnect::Device").
        /// </summary>
        public virtual string Namespace
        {
            get { return _namespace; }
            set
            {
                _namespace = value;
                switch (this)
                {
                    case CppClass cppClass:
                        TypeCache.RegisterType(cppClass.ReferenceId, cppClass.Name, cppClass.Namespace);
                        break;
                    case Property cppProperty:
                        TypeCache.RegisterType(cppProperty.SysML_ID, cppProperty.Name, cppProperty.Namespace);
                        break;
                    case CppNamespace cppNamespace:
                        TypeCache.RegisterType(cppNamespace.ReferenceId, cppNamespace.Name, cppNamespace.Namespace);
                        break;
                    case CppEnum cppEnum:
                        TypeCache.RegisterType(cppEnum.ReferenceId, cppEnum.Name, cppEnum.Namespace);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Internal string, used by <see cref="Name"/>.
        /// </summary>
        protected string _name { get; set; }
        /// <summary>
        /// The intended C++ identifier name (PascalCase).
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = CppHelperMethods.ToPascalCase(base.SysML_Name);
                return _name;
            }
            set
            {
                TypeCache.ChangeTypeName(base.SysML_ID, value);
                _name = value;
            }
        }

        /// <summary>
        /// The access modifier for this type.
        /// </summary>
        public string AccessModifier { get; set; } = "public";

        /// <summary>
        /// An optional modifier (e.g. "abstract", "static").
        /// </summary>
        public string Modifier { get; set; }

        /// <summary>
        /// Constructs a <see cref="CppType"/> generically.
        /// </summary>
        protected CppType(XmiDocument model, XmiElement source) : base(model, source) { }
    }
}
