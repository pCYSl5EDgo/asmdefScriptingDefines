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

            InjectScriptingDefineSymbolsIntoMonoIsland(unityEditor.Module);
        }

        private static void InjectScriptingDefineSymbolsIntoMonoIsland(ModuleDefinition unityEditor)
        {
            var type = unityEditor.GetType("UnityEditor.Scripting", "MonoIsland");

            bool DoesAssignDefines(MethodDefinition definition)
            {
                foreach (var instruction in definition.Body.Instructions)
                {
                    if (instruction.OpCode.Code != Code.Stfld) continue;
                    var field = (FieldReference)instruction.Operand;
                    if (field.Name == "_defines")
                        return true;
                }
                return false;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = type.Methods.Count - 1; index >= 0; index--)
            {
                var constructor = type.Methods[index];
                if (constructor.Name != ".ctor") continue;
                if (DoesAssignDefines(constructor))
                    FindAsmDefDefinesAndInjectToConstructor(constructor);
            }
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

        private static void FindAsmDefDefinesAndInjectToConstructor(MethodDefinition method)
        {
            var module = method.Module;
            var stringType = module.TypeSystem.String;
            var stringTypeArray = new ArrayType(stringType, 1);

            var monoIsland = method.DeclaringType;

            var _output = monoIsland.Fields.First(x => x.Name == "_output");
            var _defines = monoIsland.Fields.First(x => x.Name == "_defines");

            var forCuteIzmChan = module.AssemblyReferences.First(x => x.Name == UtilityAssemblyName);
            var assemblyDefinitionFinder = new TypeReference(UtilityAssemblyName, "AssemblyDefinitionFinder", module, forCuteIzmChan, false);
            var modify = assemblyDefinitionFinder.FindVoidMethod("Modify", false, new ByReferenceType(stringTypeArray), stringType);

            var adds = new[]
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldflda, _defines),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, _output),

                Instruction.Create(OpCodes.Call, modify),
            };

            var processor = method.Body.GetILProcessor();
            var instructions = method.Body.Instructions;
            InsertBefore(processor, instructions[instructions.Count - 1], adds);
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