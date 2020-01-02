using System;
using System.Collections.Generic;

namespace UnityEditor.ForCuteIzmChan
{
    public sealed class DefineProcessorV2 : IDefineProcessor
    {
        public string Version => "1.0.0";
        public IEnumerable<string> Process(IEnumerable<string> defines, string commands)
        {
            var adds = new HashSet<string>();
            var removes = new HashSet<string>();

            var splits = commands.Split(new[]
            {
                '\r',
                '\n'
            }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var split in splits)
            {
                switch (split[0])
                {
                    case '+':
                        adds.Add(split.Substring(1));
                        break;
                    case '-':
                        removes.Add(split.Substring(1));
                        break;
                }
            }

            if (adds.Count == 0 && removes.Count == 0)
                return Array.Empty<string>();
            foreach (var s in defines)
            {
                adds.Add(s);
            }
            foreach (var s in removes)
            {
                adds.Remove(s);
            }
            return adds;
        }
    }
}