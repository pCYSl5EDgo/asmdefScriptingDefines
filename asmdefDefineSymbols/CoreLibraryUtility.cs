using Mono.Cecil;

namespace ForCuteIzmChan
{
    internal static class CoreLibraryUtility
    {
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
