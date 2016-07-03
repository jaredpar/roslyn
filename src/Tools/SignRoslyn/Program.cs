using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignRoslyn
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            /*
            var binariesPath = @"e:\dd\roslyn\Binaries\Debug";
            var sourcePath = @"e:\dd\roslyn";
            */

            var binariesPath = Environment.GetEnvironmentVariable("BUILD_BINARIESDIRECTORY");
            var sourcePath = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");

            var filePath = Path.Combine(AppContext.BaseDirectory, "BinaryData.json");
            using (var file = File.OpenText(filePath))
            {
                var serializer = new JsonSerializer();
                var fileJson = (FileJson)serializer.Deserialize(file, typeof(FileJson));
                var tool = new SignTool(
                    AppContext.BaseDirectory,
                    binariesPath: binariesPath,
                    sourcePath: sourcePath);
                var util = new RunSignUtil(tool, binariesPath, fileJson.SignList, fileJson.ExcludeList);
                util.Go();
            }
        }
    }
}
