using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Sinks.Cpp.Models;
using MtconnectTranspiler.Xmi;
using MtconnectTranspiler.Xmi.UML;

namespace MtconnectTranspiler.Sinks.Cpp.Example.Models
{
    /// <summary>
    /// Sentinel model that triggers generation of the MTConnect agent client example.
    /// No model properties are iterated — the template is a static C++ file with
    /// <c>{{ version }}</c> interpolated to record the transpiler version.
    /// </summary>
    [ScribanTemplate("Cpp.Example.scriban")]
    public class CppExample : CppType, IFileSource
    {
        public string Filename { get => "mtconnect_example_client.cpp"; set { } }
        public CppExample(XmiDocument doc, UmlModel source) : base(doc, source) { }
    }
}
