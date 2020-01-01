using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ForCuteIzmChan
{
    public static class DefineScriptableObjectInheritedClassHelper
    {
        public static TypeDefinition Define(ModuleDefinition moduleDefinition, string nameSpace, string name)
        {
            var typeDefinition = moduleDefinition.GetType(nameSpace, name);
            if (!(typeDefinition is null)) return typeDefinition;

            var scriptableObject = moduleDefinition.GetType("UnityEngine", "ScriptableObject");

            typeDefinition = new TypeDefinition(nameSpace, name,
                TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                scriptableObject);
            moduleDefinition.Types.Add(typeDefinition);

            var constructor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, moduleDefinition.TypeSystem.Void)
            {
                HasThis = true,
            };
            typeDefinition.Methods.Add(constructor);

            var processor = constructor.Body.GetILProcessor();

            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Call, new MethodReference(".ctor", moduleDefinition.TypeSystem.Void, scriptableObject)));
            processor.Append(Instruction.Create(OpCodes.Ret));

            return typeDefinition;
        }
    }
}
