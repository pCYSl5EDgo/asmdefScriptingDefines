using System;
using MicroBatchFramework;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace ForCuteIzmChan
{
    internal class CuteIzm : BatchBase
    {
        private const string UnityEditorDll = "UnityEditor.dll";
        private const string UtilityAssemblyName = "UnityEditor.ForCuteIzmChan";

        [Command(new[]
        {
            "copy",
            "c",
        })]
        public void Copy(
            [Option(0, "Directory that includes " + UnityEditorDll + ".\r\nSuch as " + @"""C:\Program Files\Unity\Hub\Editor\2018.4.14f1\Editor\Data\Managed""")]string directory,
            [Option(1, UtilityAssemblyName + ".dll path")] string path
        )
        {
            try
            {
                var destination = Path.Combine(directory, UtilityAssemblyName + ".dll");
                File.Copy(path, destination, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Command(new[]{
            "execute",
            "e",
        }, "Once execute, you may have the ability to add/remove Scripting Define Symbols.")]
        public void Execute(
            [Option(0, "Directory that includes " + UnityEditorDll + ".\r\nSuch as " + @"""C:\Program Files\Unity\Hub\Editor\2018.4.14f1\Editor\Data\Managed""")]string directory
        )
        {
            try
            {
                var unityEditorPath = Path.Combine(directory, UnityEditorDll);

                var readerParameters = PrepareReaderParameters(directory);
                using (var unityEditor = new ModuleClose(unityEditorPath, readerParameters))
                {
                    SetUpUnityEditor(unityEditor);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void SetUpUnityEditor(ModuleClose unityEditor)
        {
            AssemblyImporter.Import(unityEditor.Module, UtilityAssemblyName);

            InjectScriptingDefineSymbolsIntoGenerateResponseFile(unityEditor.Module);
        }

        private static void InjectScriptingDefineSymbolsIntoGenerateResponseFile(ModuleDefinition module)
        {
            var microsoftCSharpCompiler = module.GetType("UnityEditor.Scripting.Compilers.MicrosoftCSharpCompiler");
            var generateResponseFile = microsoftCSharpCompiler.Methods.First(x => x.Name == "GenerateResponseFile");

            var scriptAssembly = module.GetType("UnityEditor.Scripting.ScriptCompilation", "ScriptAssembly");
            var getFileName = scriptAssembly.Methods.First(x => x.Name == "get_Filename");

            var body = generateResponseFile.Body;
            body.SimplifyMacros();

            var instructions = body.Instructions;

            var indexOfLoadFieldDefines = FindIndexOfLoadFieldDefines(instructions);

            var stringType = module.TypeSystem.String;
            var iEnumerable = module.FindType("System.Collections.Generic", "IEnumerable`1");
            iEnumerable.GenericParameters.Add(new GenericParameter("T", iEnumerable));

            var stringEnumerable = new GenericInstanceType(iEnumerable)
            {
                GenericArguments = { stringType }
            };

            var forCuteIzmChan = module.AssemblyReferences.First(x => x.Name == UtilityAssemblyName);
            var assemblyDefinitionFinder = new TypeReference(UtilityAssemblyName, "AssemblyDefinitionFinder", module, forCuteIzmChan, false);
            var modify = assemblyDefinitionFinder.FindMethod(stringEnumerable, "Modify", false, stringEnumerable, stringType);

            var adds = new Instruction[]
            {
                Instruction.Create(OpCodes.Ldarg_0), 
                Instruction.Create(OpCodes.Callvirt, getFileName),
                Instruction.Create(OpCodes.Call, modify), 
            };

            InsertAfter(body.GetILProcessor(), instructions[indexOfLoadFieldDefines], adds);

            body.Optimize();
        }

        private static int FindIndexOfLoadFieldDefines(Collection<Instruction> instructions)
        {
            for (var index = 0; index < instructions.Count; index++)
            {
                var instruction = instructions[index];
                if (instruction.OpCode.Code != Code.Ldarg) continue;
                var arg0 = (ParameterDefinition)instruction.Operand;
                if (arg0.Index != 0) continue;

                if (index + 1 >= instructions.Count) return -1;

                var callVirtual = instructions[index + 1];
                if (callVirtual.OpCode.Code != Code.Callvirt) continue;

                var getDefines = (MethodReference)callVirtual.Operand;
                if (getDefines.Name != "get_Defines") continue;
                return index + 1;
            }
            return -1;
        }

        private static ReaderParameters PrepareReaderParameters(string directory)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(directory);

            var readerParameters = new ReaderParameters()
            {
                AssemblyResolver = resolver,
            };
            return readerParameters;
        }

        private static void InsertAfter(ILProcessor processor, Instruction instruction, Instruction[] adds)
        {
            processor.InsertAfter(instruction, adds[0]);
            for (var j = 1; j < adds.Length; j++)
            {
                processor.InsertAfter(adds[j - 1], adds[j]);
            }
        }
    }
}