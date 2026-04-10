using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Sinks.Cpp.Models;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Example.Models
{
    /// <summary>
    /// Generates a CMakeLists.txt for the header-only C++ interface library.
    /// </summary>
    [ScribanTemplate("Cpp.CMakeLists.scriban")]
    public class CppProject : CppType, IFileSource
    {
        public string Filename { get => "CMakeLists.txt"; set { } }
        public CppProject(XmiDocument doc, UmlModel source) : base(doc, source) { }
    }
}
