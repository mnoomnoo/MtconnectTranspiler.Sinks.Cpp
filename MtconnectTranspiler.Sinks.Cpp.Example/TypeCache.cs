using ConsoulLibrary;

namespace MtconnectTranspiler.Sinks.Cpp.Example
{
    public static class TypeCache
    {

        private static List<TypeCacheItem> _types = new List<TypeCacheItem>();

        /// <summary>
        /// Key: <c>name</c> attribute
        /// Value: Indices to item in <see cref="_types"/>
        /// </summary>
        private static Dictionary<string, List<int>> _typeNameIndices = new Dictionary<string, List<int>>();
        /// <summary>
        /// Key: <c>xmi:id</c> attribute
        /// Value: Indices to item in <see cref="_types"/>
        /// </summary>
        private static Dictionary<string, int> _typeIdIndices = new Dictionary<string, int>();

        /// <summary>
        /// Registers information about a C++ type definition before code generation.
        /// </summary>
        /// <param name="referenceId">Reference to SysML ID</param>
        /// <param name="typeName">Name of the intended C++ type.</param>
        /// <param name="cppNamespace">Namespace of the intended C++ type.</param>
        public static void RegisterType(string referenceId, string typeName, string cppNamespace)
        {
            if (_typeIdIndices.ContainsKey(referenceId))
                return;
            int index = _types.Count;
            _types.Add(new TypeCacheItem
            {
                ReferenceId = referenceId,
                CppTypeName = typeName,
                CppNamespace = cppNamespace
            });

            if (_typeNameIndices.ContainsKey(typeName))
            {
                _typeNameIndices[typeName].Add(index);
            } else
            {
                _typeNameIndices.Add(typeName, new List<int> { index });
            }

            if (_typeIdIndices.ContainsKey(referenceId))
            {
                Consoul.Write("Type cache already contains ID '" + referenceId + "'!", ConsoleColor.Red);
                return;
            } else
            {
                _typeIdIndices.Add(referenceId, index);
            }
        }

        public static void ChangeTypeName(string referenceId, string newTypeName)
        {
            if (string.IsNullOrEmpty(referenceId))
                return;

            if (!_typeIdIndices.TryGetValue(referenceId, out int index))
                return;

            string oldTypeName = _types[index].CppTypeName;
            if (_typeNameIndices.ContainsKey(oldTypeName))
            {
                if (_typeNameIndices[oldTypeName].Count > 1)
                {
                    _typeNameIndices[oldTypeName].Remove(index);
                } else
                {
                    _typeNameIndices.Remove(oldTypeName);
                }
            }
            _types[index].CppTypeName = newTypeName;
        }

        public static string[]? GetTypeNamespaceFromName(string typeName)
            => !string.IsNullOrEmpty(typeName) && _typeNameIndices.TryGetValue(typeName, out List<int> indices)
                ? indices.Select(i => _types[i].CppNamespace).ToArray()
                : null;
        public static string? GetTypeNamespaceFromId(string referenceId)
            => !string.IsNullOrEmpty(referenceId) && _typeIdIndices.TryGetValue(referenceId, out int index)
                ? _types[index].CppNamespace
                : null;
    }
    internal class TypeCacheItem
    {
        /// <summary>
        /// Reference to SysML ID
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// Name of the intended C++ type.
        /// </summary>
        public string CppTypeName { get; set; }

        /// <summary>
        /// Namespace of the intended C++ type.
        /// </summary>
        public string CppNamespace { get; set; }
    }
}
