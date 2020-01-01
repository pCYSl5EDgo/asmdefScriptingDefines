using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ForCuteIzmChan
{
    public static class ToStringHelper
    {
        public static MethodDefinition DefineToString(TypeDefinition typeDefinition)
        {
            var answer = typeDefinition.Methods.FirstOrDefault(x => x.Name == "ToString" && !x.HasParameters && x.IsVirtual);
            if (!(answer is null))
                return answer;
            var module = typeDefinition.Module;
            var stringType = module.TypeSystem.String;

            var stringBuilderType = module.FindType("System.Text", "StringBuilder");
            var stringBuilderCtor = stringBuilderType.FindVoidMethod(".ctor", true);
            var appendString = stringBuilderType.FindMethod(stringBuilderType, "Append", true, stringType);
            var appendObject = stringBuilderType.FindMethod(stringBuilderType, "Append", true, module.TypeSystem.Object);
            var appendInt32 = stringBuilderType.FindMethod(stringBuilderType, "Append", true, module.TypeSystem.Int32);
            var appendBoolean = stringBuilderType.FindMethod(stringBuilderType, "Append", true, module.TypeSystem.Boolean);

            var objectToString = module.TypeSystem.Object.FindMethod(stringType, "ToString", true);

            answer = new MethodDefinition("ToString", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, stringType)
            {
                HasThis = true,
                IsVirtual = true,
            };
            typeDefinition.Methods.Add(answer);

            var body = answer.Body;
            var processor = body.GetILProcessor();

            body.Variables.Add(new VariableDefinition(module.TypeSystem.Int32));
            body.Variables.Add(new VariableDefinition(stringBuilderType));
            body.InitLocals = true;

            processor.Append(Instruction.Create(OpCodes.Newobj, stringBuilderCtor));
            processor.Append(Instruction.Create(OpCodes.Stloc_1));
            processor.Append(Instruction.Create(OpCodes.Ldloc_1));
            processor.Append(Instruction.Create(OpCodes.Ldstr, "\r\n----Start----"));
            processor.Append(Instruction.Create(OpCodes.Callvirt, appendString));
            processor.Append(Instruction.Create(OpCodes.Pop));

            foreach (var field in typeDefinition.Fields)
            {
                if (field.IsStatic) continue;
                // Display Field Name
                processor.Append(Instruction.Create(OpCodes.Ldloc_1));
                var fieldNameLdStr = Instruction.Create(OpCodes.Ldstr, "\r\n" + field.Name);
                processor.Append(fieldNameLdStr);
                processor.Append(Instruction.Create(OpCodes.Callvirt, appendString));
                var ft = field.FieldType;
                if (ft.IsArray)
                {
                    ArrayToString(ft.GetElementType(), processor, appendString, field, appendInt32, appendObject, fieldNameLdStr);
                }
                else
                {
                    fieldNameLdStr.Operand = ((string)fieldNameLdStr.Operand) + "\r\n  ";
                    switch (ft.FullName)
                    {
                        case "System.String": 
                            LoadField(processor, field);
                            processor.Append(Instruction.Create(OpCodes.Callvirt, appendString));
                            break;
                        case "System.Int32":
                            LoadField(processor, field);
                            processor.Append(Instruction.Create(OpCodes.Callvirt, appendInt32));
                            break;
                        case "System.Boolean":
                            LoadField(processor, field);
                            processor.Append(Instruction.Create(OpCodes.Callvirt, appendBoolean));
                            break;
                        default:
                            LoadField(processor, field);
                            TryBoxing(processor, ft);
                            processor.Append(Instruction.Create(OpCodes.Callvirt, appendObject));
                            break;
                    } 
                    processor.Append(Instruction.Create(OpCodes.Pop));
                }
            }
            processor.Append(Instruction.Create(OpCodes.Ldloc_1));
            processor.Append(Instruction.Create(OpCodes.Ldstr, "\r\n----End----\r\n"));
            processor.Append(Instruction.Create(OpCodes.Callvirt, appendString));
            processor.Append(Instruction.Create(OpCodes.Callvirt, objectToString));
            processor.Append(Instruction.Create(OpCodes.Ret));

            return answer;
        }

        private static void ArrayToString(TypeReference elementType, ILProcessor processor, MethodReference appendString, FieldDefinition field, MethodReference appendInt32, MethodReference appendObject, Instruction fieldNameLdStr)
        {
            // Display Length
            fieldNameLdStr.Operand = (string)fieldNameLdStr.Operand + " -> length : ";
            LoadField(processor, field);
            var ifNotNull = Instruction.Create(OpCodes.Ldarg_0);
            var end = Instruction.Create(OpCodes.Nop);

            processor.Append(Instruction.Create(OpCodes.Brtrue_S, ifNotNull));

            processor.Append(Instruction.Create(OpCodes.Ldstr, "null"));
            processor.Append(Instruction.Create(OpCodes.Callvirt, appendString));
            processor.Append(Instruction.Create(OpCodes.Pop));
            processor.Append(Instruction.Create(OpCodes.Br, end));

            processor.Append(ifNotNull);
            processor.Append(Instruction.Create(OpCodes.Ldfld, field));
            processor.Append(Instruction.Create(OpCodes.Ldlen));
            processor.Append(Instruction.Create(OpCodes.Conv_I4));
            processor.Append(Instruction.Create(OpCodes.Callvirt, appendInt32));
            processor.Append(Instruction.Create(OpCodes.Pop));

            // Initialize loop index 0
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Stloc_0));

            var loopBody = Instruction.Create(OpCodes.Nop);
            var loopCondition = Instruction.Create(OpCodes.Ldloc_0);

            processor.Append(Instruction.Create(OpCodes.Br, loopCondition));

            // Loop Body
            // \r\n\s\s
            processor.Append(loopBody);
            processor.Append(Instruction.Create(OpCodes.Ldloc_1));
            processor.Append(Instruction.Create(OpCodes.Ldstr, "\r\n  "));
            processor.Append(Instruction.Create(OpCodes.Callvirt, appendString));

            // Load Element At Loop Index
            LoadField(processor, field);
            processor.Append(Instruction.Create(OpCodes.Ldloc_0));
            processor.Append(Instruction.Create(OpCodes.Ldelem_Any, elementType));

            // Element.ToString
            TryBoxing(processor, elementType);
            processor.Append(Instruction.Create(OpCodes.Callvirt, appendObject));
            processor.Append(Instruction.Create(OpCodes.Pop));

            // Increment Loop Index
            processor.Append(Instruction.Create(OpCodes.Ldloc_0));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            processor.Append(Instruction.Create(OpCodes.Add));
            processor.Append(Instruction.Create(OpCodes.Stloc_0));

            // Loop Condition
            processor.Append(loopCondition);
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldfld, field));
            processor.Append(Instruction.Create(OpCodes.Ldlen));
            processor.Append(Instruction.Create(OpCodes.Conv_I4));
            processor.Append(Instruction.Create(OpCodes.Blt, loopBody));
            processor.Append(end);
        }

        private static void TryBoxing(ILProcessor processor, TypeReference typeReference)
        {
            if (typeReference.IsValueType)
                processor.Append(Instruction.Create(OpCodes.Box, typeReference));
        }

        private static void LoadField(ILProcessor processor, FieldDefinition field)
        {
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldfld, field));
        }
    }
}