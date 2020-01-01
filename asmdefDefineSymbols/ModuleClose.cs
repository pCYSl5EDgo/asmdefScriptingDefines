using System;
using System.IO;
using Mono.Cecil;

namespace ForCuteIzmChan
{
    internal readonly struct ModuleClose : IDisposable
    {
        private const string CopySuffix = ".copy";
        private const string BytesSuffix = ".bytes";

        private readonly string path;
        public readonly ModuleDefinition Module;

        public ModuleClose(string path, ReaderParameters parameters)
        {
            var bytesFile = path + BytesSuffix;
            if(File.Exists(bytesFile))
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.Move(bytesFile, path);
            }
            this.path = path;
            Module = ModuleDefinition.ReadModule(path, parameters);
        }
        public void Dispose()
        {
            Module.Write(path + CopySuffix);
            Module.Dispose();
            Switch(path);
        }
        private static void Switch(string basePath)
        {
            var bytesFile = basePath + BytesSuffix;
            var copyPath = basePath + CopySuffix;
            if (File.Exists(bytesFile))
            {
                File.Delete(bytesFile);
            }
            if(File.Exists(basePath))
            {
                File.Move(basePath, bytesFile);
            }
            if(File.Exists(copyPath))
            {
                File.Move(copyPath, basePath);
            }
        }
    }
}