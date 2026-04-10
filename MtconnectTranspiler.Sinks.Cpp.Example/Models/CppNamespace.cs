using CaseExtensions;
using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Sinks.Cpp.Models;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Example.Models
{
    /// <summary>
    /// Represents a C++ namespace aggregation header generated from an MTConnect UML package.
    /// </summary>
    [ScribanTemplate("Cpp.Namespace.scriban")]
    public class CppNamespace : CppType, IFileSource
    {
        /// <summary>
        /// Reference to the xmi:id
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// Reference to the <c>name</c> attribute.
        /// </summary>
        public string NormativeName { get; set; }

        /// <summary>
        /// Internal reference to the filename.
        /// </summary>
        protected string _filename { get; set; }
        /// <inheritdoc />
        public string Filename
        {
            get
            {
                if (string.IsNullOrEmpty(_filename))
                    _filename = $"{CppCategoryFunctions.ToPathSafe(Name.ToPascalCase())}.hpp";
                return _filename;
            }
            set { _filename = value; }
        }

        /// <summary>
        /// Overridden to propagate the correct child namespace to sub-namespaces.
        /// </summary>
        public override string Namespace
        {
            get => base.Namespace;
            set
            {
                base.Namespace = value;
                string childNamespace = $"{value}::{Name}";
                foreach (var child in _packages)
                    child.Namespace = childNamespace;
                foreach (var cls in _classes)
                    cls.Namespace = childNamespace;
                foreach (var enm in _enums)
                    enm.Namespace = childNamespace;
            }
        }

        protected List<CppNamespace> _packages = new List<CppNamespace>();
        /// <summary>
        /// Collection of sub-namespaces.
        /// </summary>
        public new IEnumerable<CppNamespace> Packages => _packages;

        protected List<CppClass> _classes = new List<CppClass>();
        /// <summary>
        /// Collection of C++ classes in this namespace.
        /// </summary>
        public new IEnumerable<CppClass> Classes => _classes;

        protected List<CppEnum> _enums = new List<CppEnum>();
        /// <summary>
        /// Collection of C++ enums in this namespace.
        /// </summary>
        public IEnumerable<CppEnum> Enums => _enums;

        /// <summary>
        /// Documentation from the SysML model.
        /// </summary>
        public Summary Summary { get; protected set; }

        public CppNamespace(XmiDocument model, UmlPackage source) : base(model, source)
        {
            _name = CppHelperMethods.ToPascalCase(source.Name);
            NormativeName = source.Name;
            ReferenceId = source!.Id;

            if (source.Comments?.Length > 0)
                Summary = new Summary(source.Comments);

            _packages = source!.Packages
                ?.Select(o => new CppNamespace(model, o))
                ?.ToList()
                ?? new List<CppNamespace>();

            _classes = source!.Classes
                ?.Select(o => new CppClass(model, o))
                ?.ToList()
                ?? new List<CppClass>();

            _enums = source!.Enumerations
                ?.Select(o => new CppEnum(model, o))
                ?.ToList()
                ?? new List<CppEnum>();
        }
    }
}
