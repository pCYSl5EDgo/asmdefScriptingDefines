using System.Buffers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ForCuteIzmChan
{
    public static class Beautify
    {
        public static void SimplifyBranch(MethodDefinition methodDefinition)
        {
            var instructions = methodDefinition.Body.Instructions;
            var processor=methodDefinition.Body.GetILProcessor();

            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                if (instruction.OpCode.Code != Code.Br && instruction.OpCode.Code != Code.Br_S) continue;
                var branch = (Instruction)instruction.Operand;
                if (instruction.Next is null) continue;
                if(instruction.Next != branch) continue;
                processor.Remove(instruction);
            }
        }
    }
}