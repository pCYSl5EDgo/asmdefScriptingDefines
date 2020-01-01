using Mono.Cecil;

namespace ForCuteIzmChan
{
    public static class DefineEnumHelper
    {
        public static TypeDefinition Define(ModuleDefinition moduleDefinition, string nameSpace, string name, params (string name, int value)[] tuples)
        {
            var typeDefinition = moduleDefinition.GetType(nameSpace, name);
            if (!(typeDefinition is null)) return typeDefinition;
            var enumType = moduleDefinition.FindType("System", "Enum");
            typeDefinition = new TypeDefinition(nameSpace, name, TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.Sealed, enumType);
            moduleDefinition.Types.Add(typeDefinition);

            var fieldType = moduleDefinition.TypeSystem.Int32;
            var basement = new FieldDefinition("value__", FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName, fieldType);
            typeDefinition.Fields.Add(basement);

            foreach (var (field, value) in tuples)
            {
                var fieldDefinition = new FieldDefinition(field, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal, typeDefinition)
                {
                    HasDefault = true,
                    HasConstant = true,
                    Constant = value,
                };
                typeDefinition.Fields.Add(fieldDefinition);
            }

            return typeDefinition;
        }
    }
}