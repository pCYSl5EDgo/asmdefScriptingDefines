using Mono.Cecil;

namespace ForCuteIzmChan
{
    internal static class CoreLibraryUtility
    {
        public static TypeReference FindType(this ModuleDefinition moduleDefinition, string nameSpace, string type)
        {
            if (nameSpace == "System")
            {
                switch (type)
                {
                    case "Boolean": return moduleDefinition.TypeSystem.Boolean;
                    case "Char": return moduleDefinition.TypeSystem.Char;
                    case "IntPtr": return moduleDefinition.TypeSystem.IntPtr;
                    case "UIntPtr": return moduleDefinition.TypeSystem.UIntPtr;
                    case "Int16": return moduleDefinition.TypeSystem.Int16;
                    case "Int32": return moduleDefinition.TypeSystem.Int32;
                    case "Int64": return moduleDefinition.TypeSystem.Int64;
                    case "UInt16": return moduleDefinition.TypeSystem.UInt16;
                    case "UInt32": return moduleDefinition.TypeSystem.UInt32;
                    case "UInt64": return moduleDefinition.TypeSystem.UInt64;
                    case "Byte": return moduleDefinition.TypeSystem.Byte;
                    case "SByte": return moduleDefinition.TypeSystem.SByte;
                    case "String": return moduleDefinition.TypeSystem.String;
                    case "Single": return moduleDefinition.TypeSystem.Single;
                    case "Double": return moduleDefinition.TypeSystem.Double;
                }
            }
            return new TypeReference(nameSpace, type, moduleDefinition, moduleDefinition.TypeSystem.CoreLibrary);
        }

        public static MethodReference FindMethod(this TypeReference coreLibTypeReference, TypeReference returnTypeReference, string name, bool hasThis, params TypeReference[] parameterTypeReferences)
        {
            var method = new MethodReference(name, returnTypeReference, coreLibTypeReference)
            {
                HasThis = hasThis,
            };
            foreach (var reference in parameterTypeReferences)
            {
                method.Parameters.Add(new ParameterDefinition(reference));
            }
            return method;
        }

        public static MethodReference FindVoidMethod(this TypeReference coreLibTypeReference, string name, bool hasThis, params TypeReference[] parameterTypeReferences)
            => coreLibTypeReference.FindMethod(coreLibTypeReference.Module.TypeSystem.Void, name, hasThis, parameterTypeReferences);
    }
}
