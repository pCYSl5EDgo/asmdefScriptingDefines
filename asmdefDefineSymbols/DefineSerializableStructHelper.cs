using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ForCuteIzmChan
{
    public static class DefineSerializableStructHelper
    {
        public static TypeDefinition Define(ModuleDefinition moduleDefinition, string nameSpace, string name, params (TypeReference type, string name)[] tuples)
        {
            var typeDefinition = moduleDefinition.GetType(nameSpace, name);
            if (!(typeDefinition is null)) return typeDefinition;
            typeDefinition = new TypeDefinition(nameSpace, name,
                TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.Serializable | TypeAttributes.BeforeFieldInit,
                moduleDefinition.FindType("System", "ValueType"));
            moduleDefinition.Types.Add(typeDefinition);

            var constructor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, moduleDefinition.TypeSystem.Void)
            {
                HasThis = true,
            };
            typeDefinition.Methods.Add(constructor);

            var processor = constructor.Body.GetILProcessor();

            foreach (var (type, fName) in tuples)
            {
                var field = new FieldDefinition(fName, FieldAttributes.Public, type);
                typeDefinition.Fields.Add(field);
                var parameter = new ParameterDefinition(fName.ToLowerInvariant(), ParameterAttributes.None, type);
                constructor.Parameters.Add(parameter);

                processor.Append(Instruction.Create(OpCodes.Ldarg_0));
                processor.Append(Instruction.Create(OpCodes.Ldarg, parameter));
                processor.Append(Instruction.Create(OpCodes.Stfld, field));
            }

            processor.Append(Instruction.Create(OpCodes.Ret));

            return typeDefinition;
        }
    }
}