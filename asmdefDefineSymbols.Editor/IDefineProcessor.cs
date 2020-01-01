namespace UnityEditor.ForCuteIzmChan
{
    public interface IDefineProcessor
    {
        string Version { get; }
        void Process(ref string[] defines, string commands);
    }
}