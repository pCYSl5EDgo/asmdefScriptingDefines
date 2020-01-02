using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UnityEditor.ForCuteIzmChan
{
    internal static class AssemblyDefinitionFinder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<string> Modify(IEnumerable<string> defines, string outputDllPath)
        {
            var dllName = Path.GetFileNameWithoutExtension(outputDllPath);
            var asmdefPath = FindByName(dllName);
            if (asmdefPath is null) return defines;
            var definesPath = asmdefPath + "_defines";
            if (!File.Exists(definesPath)) return defines;

            var contents = File.ReadAllText(definesPath);
            return ModifyDefines(defines, contents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FindByName(string nameWithoutExtension)
        {
            if (nameWithoutExtension == "Assembly-CSharp") return default;
            var searchPattern = nameWithoutExtension + ".asmdef";
            static bool Predicate(string file) => File.Exists(file + ".meta");
            return Directory.EnumerateFiles("./Assets", searchPattern, SearchOption.AllDirectories).FirstOrDefault(Predicate)
                   ?? Directory.EnumerateFiles("./Packages", searchPattern, SearchOption.AllDirectories).FirstOrDefault(Predicate);
        }
        private static readonly DefineProcessorV2 ProcessorV2 = new DefineProcessorV2();

        private static IEnumerable<string> ModifyDefines(IEnumerable<string> defines, string text)
        {
            if (defines is null)
                defines = Array.Empty<string>();
            return string.IsNullOrWhiteSpace(text) ? defines : ProcessorV2.Process(defines, text);
        }
    }
}
