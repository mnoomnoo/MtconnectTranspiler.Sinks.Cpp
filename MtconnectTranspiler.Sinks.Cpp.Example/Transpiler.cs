using CaseExtensions;
using Microsoft.Extensions.Logging;
using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Contracts;
using MtconnectTranspiler.Sinks.Cpp.Example.Models;
using MtconnectTranspiler.Sinks.Cpp.Models;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;
using Scriban.Runtime;
using System.Text.RegularExpressions;

namespace MtconnectTranspiler.Sinks.Cpp.Example
{
    public class CppCategoryFunctions : ScriptObject
    {
        private static readonly HashSet<string> _cppKeywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "alignas", "alignof", "and", "and_eq", "asm", "auto",
            "bitand", "bitor", "bool", "break",
            "case", "catch", "char", "char8_t", "char16_t", "char32_t", "class", "compl", "concept", "const",
            "consteval", "constexpr", "constinit", "const_cast", "continue", "co_await", "co_return", "co_yield",
            "decltype", "default", "delete", "do", "double", "dynamic_cast",
            "else", "enum", "explicit", "export", "extern",
            "false", "float", "for", "friend",
            "goto",
            "if", "inline", "int",
            "long",
            "mutable",
            "namespace", "new", "noexcept", "not", "not_eq", "nullptr",
            "operator", "or", "or_eq",
            "private", "protected", "public",
            "register", "reinterpret_cast", "requires", "return",
            "short", "signed", "sizeof", "static", "static_assert", "static_cast", "struct", "switch",
            "template", "this", "thread_local", "throw", "true", "try", "typedef", "typeid", "typename",
            "union", "unsigned", "using",
            "virtual", "void", "volatile",
            "wchar_t", "while",
            "xor", "xor_eq"
        };

        public static string ToKeywordSafe(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return _cppKeywords.Contains(input) ? input + "_" : input;
        }

        public static string ToCodeSafe(string input, string replaceBy = "_")
        {
            if (string.IsNullOrEmpty(input))
                return input;
            if (input.Contains("^2"))
                input = input.Replace("^2", "_SQUARED");
            if (input.Contains("^3"))
                input = input.Replace("^3", "_CUBED");
            if (input.Contains("/"))
                input = input.Replace("/", "_PER_");
            if (input.Equals("float[]", StringComparison.OrdinalIgnoreCase))
                return "std::vector<float>";
            if (input.Equals("float[3]", StringComparison.OrdinalIgnoreCase))
                return "std::array<float,3>";
            char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            if (numbers.Any(c => input.StartsWith(c)))
                input = $"_{input}";

            var invalidChars = System.IO.Path
                .GetInvalidFileNameChars()
                .Concat(new char[] { ' ', '{', '}', '[', ']', '(', ')', '^', '`', '&', '+', '-', '!', '?', '%', '*', '<', '>', ',', '|', '\\', '/', '=', ':', ';' })
                .ToArray();
            var regex = new Regex(@"\" + String.Join(@"|\", invalidChars), RegexOptions.Compiled);
            return regex.Replace(input, replaceBy);
        }

        public static string ToPathSafe(string input, string replaceBy = "_")
        {
            if (string.IsNullOrEmpty(input))
                return input;
            var invalidChars = System.IO.Path
                .GetInvalidFileNameChars()
                .Concat(new char[] { ':', '.', '{', '}', '[', ']', '(', ')', '^', '`', '&', '+', '-', '!', '?', '%', '*', '<', '>', ',', '|', '=', ';', ' ' })
                .ToArray();
            var regex = new Regex(@"\" + String.Join(@"|\", invalidChars), RegexOptions.Compiled);
            return regex.Replace(input, replaceBy);
        }

        public static string? GetTypeNamespace(string referenceId)
            => TypeCache.GetTypeNamespaceFromId(referenceId);

        public static string[] GetClassIncludes(CppClass cppClass)
        {
            var result = new List<string>();
            foreach (var property in cppClass.Properties)
            {
                string[] namespaces = TypeCache.GetTypeNamespaceFromName(property.Type);
                if (namespaces?.Length > 0)
                    result.AddRange(namespaces);
            }
            return result.Distinct().Where(o => !string.IsNullOrEmpty(o)).ToArray();
        }

        public static string[] GetNamespaceIncludes(CppNamespace ns)
        {
            var namespaces = new List<string>();
            foreach (var cppClass in ns.Classes)
                namespaces.Add(cppClass.Namespace);
            foreach (var cppEnum in ns.Enums)
                namespaces.Add(cppEnum.Namespace);
            return namespaces.Distinct().Where(o => !string.IsNullOrEmpty(o)).ToArray();
        }
    }

    internal class Transpiler : ITranspilerSink
    {
        private readonly ILogger<ITranspilerSink>? _logger;
        private readonly IScribanTemplateGenerator _generator;

        public Transpiler(IScribanTemplateGenerator generator, ILogger<ITranspilerSink>? logger = default)
        {
            _logger = logger;
            _generator = generator;
        }

        public void Transpile(XmiDocument model, CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("Received MTConnectModel, beginning C++ transpilation");

            _generator.Model.SetValue("model", model, true);
            _generator.TemplateContext.PushGlobal(new CppCategoryFunctions());

            var allNamespaces = new List<CppNamespace>();
            var allClasses = new List<CppClass>();
            var allEnumerations = new List<CppEnum>();

            CppModel rootModel = new CppModel(model, model.Model);

            foreach (var package in model.Model.Packages)
            {
                allNamespaces.Add(new CppNamespace(model, package) { Namespace = "mtconnect" });
                allNamespaces.AddRange(getNamespaces(model, package));
                allClasses.AddRange(getClasses(model, package));
                allEnumerations.AddRange(getEnums(model, package));
            }
            foreach (var profile in model.Model.Profiles)
            {
                foreach (var package in profile.Packages)
                {
                    allNamespaces.Add(new CppNamespace(model, package) { Namespace = "mtconnect" });
                    allNamespaces.AddRange(getNamespaces(model, package));
                    allClasses.AddRange(getClasses(model, package));
                    allEnumerations.AddRange(getEnums(model, package));
                }
            }

            string includeRoot = Path.Combine(_generator.OutputPath, "include", "mtconnect");
            Directory.CreateDirectory(includeRoot);

            _logger?.LogInformation("Saving Enums...");
            _generator.ProcessTemplate(allEnumerations, Path.Combine(includeRoot, "Enums"), true);
            _logger?.LogInformation("Saving Classes...");
            _generator.ProcessTemplate(allClasses, Path.Combine(includeRoot, "Classes"), true);
            _logger?.LogInformation("Saving Namespaces...");
            _generator.ProcessTemplate(allNamespaces, Path.Combine(includeRoot, "Packages"), true);

            _logger?.LogInformation("Saving Root Model Header...");
            _generator.ProcessTemplate(rootModel, includeRoot, true);

            _logger?.LogInformation("Writing CMakeLists.txt...");
            var projectFile = new CppProject(model, model.Model);
            _generator.ProcessTemplate(projectFile, _generator.OutputPath, true);

            _logger?.LogInformation("Writing example agent client...");
            var examplePath = Path.Combine(_generator.OutputPath, "examples");
            Directory.CreateDirectory(examplePath);
            _generator.ProcessTemplate(new CppExample(model, model.Model), examplePath, true);
        }

        private IEnumerable<CppNamespace> getNamespaces(XmiDocument model, UmlPackage package, string namespacePrefix = "mtconnect")
        {
            namespacePrefix = $"{namespacePrefix}::{package.Name.ToPascalCase()}";
            var results = new List<CppNamespace>();
            foreach (var subpackage in package.Packages)
            {
                results.Add(new CppNamespace(model, subpackage) { Namespace = namespacePrefix });
                if (subpackage.Packages.Count > 0)
                    results.AddRange(getNamespaces(model, subpackage, namespacePrefix));
            }
            return results;
        }

        private IEnumerable<CppEnum> getEnums(XmiDocument model, UmlPackage package, string namespacePrefix = "mtconnect")
        {
            namespacePrefix = $"{namespacePrefix}::{package.Name.ToPascalCase()}";
            var results = new List<CppEnum>();
            if (package.Enumerations.Count > 0)
                foreach (var item in package.Enumerations)
                    results.Add(new CppEnum(model, item) { Namespace = namespacePrefix });
            if (package.Packages.Count > 0)
            {
                foreach (var item in package.Packages)
                {
                    var subEnums = getEnums(model, item, namespacePrefix);
                    if (subEnums.Any())
                        results.AddRange(subEnums);
                }
            }
            return results;
        }

        private IEnumerable<CppClass> getClasses(XmiDocument model, UmlPackage package, string namespacePrefix = "mtconnect")
        {
            namespacePrefix = $"{namespacePrefix}::{package.Name.ToPascalCase()}";
            var results = new List<CppClass>();
            if (package.Classes.Count > 0)
                foreach (var item in package.Classes)
                    results.Add(new CppClass(model, item) { Namespace = namespacePrefix });
            if (package.Packages.Count > 0)
            {
                foreach (var item in package.Packages)
                {
                    var subClasses = getClasses(model, item, namespacePrefix);
                    if (subClasses.Any())
                        results.AddRange(subClasses);
                }
            }
            return results;
        }
    }
}
