using System.Collections.Generic;

namespace UnityEditor.ForCuteIzmChan
{
    public interface IDefineProcessor
    {
        string Version { get; }
        IEnumerable<string> Process(IEnumerable<string> defines, string commands);
    }
}