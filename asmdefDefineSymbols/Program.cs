using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MicroBatchFramework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Extensions.Hosting;

namespace ForCuteIzmChan
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder().RunBatchEngineAsync<CuteIzm>(args);
        }
    }
}
