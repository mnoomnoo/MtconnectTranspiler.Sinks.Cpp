using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Sinks.Cpp.Example;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Represents a C++ <c>enum class</c> generated from an MTConnect enumeration.
    /// </summary>
    [ScribanTemplate("Cpp.Enum.scriban")]
    public class CppEnum : CppType, IFileSource
    {
        /// <summary>
        /// Reference to the xmi:id
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// Reference to any Comments written in the SysML model.
        /// </summary>
        public Summary Summary { get; protected set; }

        /// <summary>
        /// Internal list of <see cref="EnumItem"/>.
        /// </summary>
        protected List<EnumItem> _items { get; set; } = new List<EnumItem>();
        /// <summary>
        /// Collection of enum values.
        /// </summary>
        public IEnumerable<EnumItem> Items => _items;

        /// <summary>
        /// Internal string for <see cref="Filename"/>.
        /// </summary>
        protected string _filename { get; set; }
        /// <inheritdoc />
        public virtual string Filename
        {
            get
            {
                if (string.IsNullOrEmpty(_filename))
                    _filename = $"{CppCategoryFunctions.ToPathSafe(Name)}.hpp";
                return _filename;
            }
            set { _filename = value; }
        }

        /// <summary>
        /// Constructs a <see cref="CppEnum"/> with an explicit name.
        /// </summary>
        public CppEnum(XmiDocument model, XmiElement source, string name) : base(model, source)
        {
            Name = name;
            ReferenceId = source.Id;
        }

        /// <summary>
        /// Constructs a <see cref="CppEnum"/> from a <see cref="UmlEnumeration"/>.
        /// </summary>
        public CppEnum(XmiDocument model, UmlEnumeration source) : this(model, source, source.Name)
        {
            AddRange(model, source.Items);
        }

        /// <summary>
        /// Constructs a <see cref="CppEnum"/> from a <see cref="UmlPackage"/>.
        /// </summary>
        public CppEnum(XmiDocument model, UmlPackage source) : this(model, source, source.Name)
        {
            AddRange(model, source.Classes.ToList());
            if (source.Comments?.Length > 0)
                Summary = new Summary(source.Comments);
        }

        /// <summary>
        /// Constructs a <see cref="CppEnum"/> from a <see cref="UmlClass"/>.
        /// </summary>
        public CppEnum(XmiDocument model, UmlClass source) : this(model, source, source.Name)
        {
            AddRange(model, source.Properties.ToList());
            if (source.Comments?.Length > 0)
                Summary = new Summary(source.Comments);
        }

        /// <summary>
        /// Adds a single <see cref="EnumItem"/>.
        /// </summary>
        public void Add(EnumItem item)
        {
            item.Namespace = $"{this.Namespace}::{this.Name}";
            _items.Add(item);
        }

        public void Add(XmiDocument model, XmiElement item) => Add(new EnumItem(model, item));
        public void Add(XmiDocument model, UmlClass item) => Add(new EnumItem(model, item));
        public void Add(XmiDocument model, UmlEnumerationLiteral item) => Add(new EnumItem(model, item));

        public void AddRange(XmiDocument model, IEnumerable<XmiElement> items)
        {
            if (items == null) return;
            foreach (var item in items.ToArray()) Add(model, item);
        }
        public void AddRange(XmiDocument model, IEnumerable<UmlClass> items)
        {
            if (items == null) return;
            foreach (var item in items.ToArray()) Add(model, item);
        }
        public void AddRange(XmiDocument model, IEnumerable<UmlEnumerationLiteral> items)
        {
            if (items == null) return;
            foreach (var item in items.ToArray()) Add(model, item);
        }
    }
}
