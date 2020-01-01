using System;
using System.Linq;
using Mono.Cecil;
namespace ForCuteIzmChan
{
    public static class AssemblyImporter
    {
        public static AssemblyNameReference Import(ModuleDefinition moduleDefinition, string assemblyName)
        {
            var answer = moduleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == assemblyName);
            if (!(answer is null))
                return answer;
            answer = new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0));
            moduleDefinition.AssemblyReferences.Add(answer);
            return answer;
        }
    }
}