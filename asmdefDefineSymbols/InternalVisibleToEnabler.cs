using System.Text;
using System.Linq;
using Mono.Cecil;
namespace ForCuteIzmChan
{
    public static class InternalVisibleToEnabler
    {
        public static void Enable(ModuleDefinition moduleDefinition, string assemblyName)
        {
            var internalVisibleToType = moduleDefinition.FindType("System.Runtime.CompilerServices", "InternalsVisibleToAttribute");
            var stringType = moduleDefinition.TypeSystem.String;
            var ctor = internalVisibleToType.FindVoidMethod(".ctor", true, stringType);

            var blob = new byte[]{
                0x01, 0x00, (byte)assemblyName.Length, 
            }
            .Concat(Encoding.UTF8.GetBytes(assemblyName))
            .Concat(new byte[] {0x00, 0x00,})
            .ToArray();

            var customAttribute = new CustomAttribute(ctor, blob);
            moduleDefinition.Assembly.CustomAttributes.Add(customAttribute);
        }
    }
}