using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UnityEditor.ForCuteIzmChan
{
    internal static class AssemblyDefinitionFinder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Modify(ref string[] defines, string outputDllPath)
        {
            if(string.IsNullOrEmpty(outputDllPath)) return;
            var dllName = Path.GetFileNameWithoutExtension(outputDllPath);
            var asmdefPath = FindByName(dllName);
            if (asmdefPath is null) return;
            var definesPath = asmdefPath + "_defines";
            if (!File.Exists(definesPath)) return;

            var contents = File.ReadAllText(definesPath);
            ModifyDefines(ref defines, contents);
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

        private static void ModifyDefines(ref string[] defines, string text)
        {
            if (defines is null)
                defines = Array.Empty<string>();
            if(!string.IsNullOrWhiteSpace(text))
                ProcessorV2.Process(ref defines, text);
        }
    }
}
