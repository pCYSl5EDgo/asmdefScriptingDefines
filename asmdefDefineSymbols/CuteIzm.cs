using System;
using MicroBatchFramework;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

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
            var stringType = module.TypeSystem.String;

            var microsoftCSharpCompiler = module.GetType("UnityEditor.Scripting.Compilers", "ScriptCompilerBase");
            var ctor = microsoftCSharpCompiler.Methods.First(x => x.Name == ".ctor");

            var scriptAssembly = module.GetType("UnityEditor.Scripting.ScriptCompilation", "ScriptAssembly");
            var getFilename = scriptAssembly.Methods.First(x => x.Name == "get_Filename");
            var getDefines = scriptAssembly.Methods.First(x => x.Name == "get_Defines");
            var setDefines = scriptAssembly.Methods.First(x => x.Name == "set_Defines");

            var stringArray = new ArrayType(stringType, 1);

            var forCuteIzmChan = module.AssemblyReferences.First(x => x.Name == UtilityAssemblyName);
            var assemblyDefinitionFinder = new TypeReference(UtilityAssemblyName, "AssemblyDefinitionFinder", module, forCuteIzmChan, false);
            var modify = assemblyDefinitionFinder.FindVoidMethod("Modify", false, new ByReferenceType(stringArray), stringType);

            var body = ctor.Body;
            var tmpArrayVariable = new VariableDefinition(stringArray);
            body.Variables.Add(tmpArrayVariable);
            var instructions = body.Instructions;

            var end = Instruction.Create(OpCodes.Nop);

            var adds = new[]
            {
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Brfalse_S, end),

                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call, getDefines),
                Instruction.Create(OpCodes.Stloc, tmpArrayVariable),
                Instruction.Create(OpCodes.Ldloca, tmpArrayVariable),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call, getFilename),
                Instruction.Create(OpCodes.Call, modify),

                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Ldloc, tmpArrayVariable),
                Instruction.Create(OpCodes.Call, setDefines),

                end,
            };

            InsertBefore(body.GetILProcessor(), instructions[instructions.Count - 1], adds);
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

        private static void InsertBefore(ILProcessor processor, Instruction instruction, Instruction[] adds)
        {
            processor.InsertBefore(instruction, adds[0]);
            for (var j = 1; j < adds.Length; j++)
            {
                processor.InsertAfter(adds[j - 1], adds[j]);
            }
        }
    }
}