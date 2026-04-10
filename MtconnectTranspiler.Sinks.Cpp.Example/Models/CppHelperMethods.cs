using MtconnectTranspiler.Contracts;
using MtconnectTranspiler.Xmi.UML;
using MtconnectTranspiler.CodeGenerators.ScribanTemplates;

namespace MtconnectTranspiler.Sinks.Cpp.Models
{
    /// <summary>
    /// Helper methods to process content for scriban templates targeting C++ output.
    /// </summary>
    public class CppHelperMethods : ScribanHelperMethods
    {
        private static Dictionary<string, string> umlDataTypeToCpp = new Dictionary<string, string>()
        {
            { "boolean",         "bool" },
            { "ID",              "std::string" },
            { "string",          "std::string" },
            { "float",           "float" },
            { "datetime",        "std::string" },
            { "integer",         "int32_t" },
            { "int32",           "int32_t" },
            { "int64",           "int64_t" },
            { "xlinktype",       "std::string" },
            { "xslang",          "std::string" },
            { "SECOND",          "double" },
            { "IDREF",           "std::string" },
            { "xlinkhref",       "std::string" },
            { "MILLIMETER",      "double" },
            { "DEGREE",          "double" },
            { "x509",            "std::string" },
            { "CUBIC_MILLIMETER","double" },
            { "uint32",          "uint32_t" },
            { "uint64",          "uint64_t" },
            { "double",          "double" },
            { "version",         "std::string" },
        };

        /// <summary>
        /// Maps a UML primitive type name to its C++ equivalent.
        /// Returns null if the type is not a recognised primitive.
        /// </summary>
        public static string? ToPrimitiveType(string umlType)
        {
            if (umlDataTypeToCpp.TryGetValue(umlType, out var cppType))
                return cppType;
            return null;
        }

        /// <summary>
        /// Gets the C++ equivalent of the <see cref="UmlDataType"/>.
        /// </summary>
        public static string? ToPrimitiveType(UmlDataType source)
            => ToPrimitiveType(source.Name);

        /// <summary>
        /// Gets the C++ equivalent of the <see cref="UmlProperty"/>.
        /// </summary>
        public static string? ToPrimitiveType(Xmi.XmiDocument model, UmlProperty source)
        {
            var umlDataType = model.LookupDataType(source.PropertyType);
            if (umlDataType == null)
                return null;
            return ToPrimitiveType(umlDataType);
        }

        /// <summary>
        /// Resolves a property type reference from the XMI model, returning the C++ type name.
        /// </summary>
        public static string? TypeDeepSearch(Xmi.XmiDocument model, string propertyType, out Xmi.XmiElement? remoteType)
        {
            remoteType = null;
            if (string.IsNullOrEmpty(propertyType))
                return null;

            object _remote;
            if (model.IdCache.TryGetValue(propertyType, out _remote))
            {
                switch (_remote)
                {
                    case UmlEnumeration umlEnum:
                        remoteType = umlEnum;
                        return umlEnum.Name;
                    case UmlClass umlClass:
                        remoteType = umlClass;
                        return CppClass.GetClassName(model, umlClass);
                    case UmlDataType umlDataType:
                        remoteType = umlDataType;
                        switch (umlDataType.Name)
                        {
                            case "float3d":
                                return "std::array<float, 3>";
                            case "binary":
                                return "bool";
                            default:
                                break;
                        }
                        break;
                    case UmlAssociation umlAssociation:
                        remoteType = umlAssociation;
                        var ownedEnds = umlAssociation.OwnedEnds?.Select(o => TypeDeepSearch(model, o.TypeId, out _))?.ToList();
                        return umlAssociation.Name;
                    case UmlGeneralization umlGeneralization:
                        return TypeDeepSearch(model, umlGeneralization.General, out remoteType);
                    case UmlEnumerationLiteral umlEnumerationLiteral:
                        return umlEnumerationLiteral.Name;
                    default:
                        break;
                }
            }
            return null;
        }
    }
}
