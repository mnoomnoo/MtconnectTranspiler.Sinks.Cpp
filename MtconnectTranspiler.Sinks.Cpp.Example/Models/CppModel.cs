using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Sinks.Cpp.Models;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Example.Models
{
    /// <summary>
    /// Represents the root MTConnect model header — includes all top-level namespaces.
    /// </summary>
    [ScribanTemplate("Cpp.Model.scriban")]
    public class CppModel : CppType, IFileSource
    {
        /// <summary>
        /// Reference to the xmi:id
        /// </summary>
        public string ReferenceId { get; set; }

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
                    _filename = "MtconnectModel.hpp";
                return _filename;
            }
            set { _filename = value; }
        }

        protected List<CppNamespace> _packages = new List<CppNamespace>();
        /// <summary>
        /// Collection of top-level namespaces.
        /// </summary>
        public IEnumerable<CppNamespace> Packages => _packages;

        public CppModel(XmiDocument model, UmlModel source) : base(model, source)
        {
            _name = CppHelperMethods.ToPascalCase(source.Name);
            ReferenceId = source!.Id;

            _packages = source!.Packages
                ?.Select(o => new CppNamespace(model, o))
                ?.ToList()
                ?? new List<CppNamespace>();
            foreach (var profile in source!.Profiles)
            {
                foreach (var package in profile.Packages)
                {
                    _packages.Add(new CppNamespace(model, package));
                }
            }
        }
    }
}
