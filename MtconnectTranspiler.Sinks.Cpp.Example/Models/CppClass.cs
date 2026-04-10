using CaseExtensions;
using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Sinks.Cpp.Example;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Represents a C++ struct/class generated from an MTConnect UML class.
    /// </summary>
    [ScribanTemplate("Cpp.Class.scriban")]
    public partial class CppClass : CppType, IFileSource
    {
        /// <summary>
        /// Reference to the xmi:id
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Reference to the <c>name</c> attribute.
        /// </summary>
        public string? NormativeName { get; set; }

        /// <summary>
        /// Documentation from the SysML model.
        /// </summary>
        public Summary? Summary { get; protected set; }

        /// <summary>
        /// Internal reference to the filename.
        /// </summary>
        protected string? _filename { get; set; }
        /// <inheritdoc />
        public virtual string Filename
        {
            get
            {
                if (string.IsNullOrEmpty(_filename))
                    _filename = $"{CppCategoryFunctions.ToPathSafe(Name.ToPascalCase())}.hpp";
                return _filename;
            }
            set { _filename = value; }
        }

        protected List<Property> _properties = new List<Property>();
        /// <summary>
        /// Collection of class properties.
        /// </summary>
        public IEnumerable<Property> Properties => _properties;

        protected List<Constraint> _constraints = new List<Constraint>();
        /// <summary>
        /// Collection of class constraints.
        /// </summary>
        public IEnumerable<Constraint> Constraints => _constraints;

        /// <summary>
        /// The resolved C++ base class name, if any.
        /// </summary>
        public string? Generalization { get; set; }

        /// <summary>
        /// The original xmi:id of the generalization.
        /// </summary>
        public string? GeneralizationId { get; set; }

        private XmiElement? _remoteType { get; set; }

        public CppClass(XmiDocument model, UmlClass source) : base(model, source)
        {
            ReferenceId = source.Id;

            if (source.Comments?.Length > 0)
                Summary = new Summary(source.Comments);

            if (source.IsAbstract)
                Modifier = "abstract";

            AccessModifier = "public";

            _properties = source.Properties
                ?.Where(o => !string.IsNullOrEmpty(o.Name))
                ?.Select(o => new Property(model, o))
                ?.ToList()
                ?? new List<Property>();
            var propertyGroupings = _properties.GroupBy(o => o.Name);
            foreach (var propertyGrouping in propertyGroupings)
            {
                if (propertyGrouping.Count() <= 1)
                    continue;
                var properties = _properties.Where(o => o.Name == propertyGrouping.Key).ToList();
                foreach (var property in properties)
                {
                    if (property.Type.EndsWith("Class"))
                    {
                        string remoteClassName = property.Type.Replace("Class", string.Empty);
                        if (!property.Name.EndsWith(remoteClassName))
                            property.Name += remoteClassName;
                    }
                }
            }

            _constraints = source.Constraints
                ?.Where(o => !string.IsNullOrEmpty(o.Name))
                ?.Select(o => new Constraint(model, o))
                ?.ToList()
                ?? new List<Constraint>();

            GeneralizationId = source.Generalization?.Name ?? source.Generalization?.General;
            if (!string.IsNullOrEmpty(GeneralizationId))
            {
                XmiElement? remoteType = null;
                Generalization = CppHelperMethods.TypeDeepSearch(model, GeneralizationId, out remoteType) ?? "";
            }

            Name = GetClassName(model, source);
            NormativeName = source.Name;
        }

        public void Add(Property property) => _properties.Add(property);

        public static string GetClassName(XmiDocument model, UmlClass umlClass)
        {
            string name = CppHelperMethods.ToPascalCase(umlClass.Name);

            string? generalization = umlClass.Generalization?.Name ?? umlClass.Generalization?.Id;
            if (!string.IsNullOrEmpty(generalization))
            {
                string? generalizedType = CppHelperMethods.TypeDeepSearch(model, generalization, out XmiElement? remoteType);
                if (!string.IsNullOrEmpty(generalizedType) && generalizedType.EndsWith("Class"))
                {
                    string remoteGeneralization = generalizedType.Replace("Class", string.Empty);
                    if (name.EndsWith(remoteGeneralization, StringComparison.OrdinalIgnoreCase))
                        name += "Generalization";
                    else
                        name += "Class";
                }
                else
                {
                    name += "Class";
                }
            }
            else
            {
                name += "Class";
            }
            return name;
        }
    }
}
