using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace SignRoslyn
{
    internal sealed class SignTool : ISignTool
    {
        private readonly string _msbuildPath;
        private readonly string _binariesPath;
        private readonly string _sourcePath;
        private readonly string _buildFilePath;
        private bool _generatedBuildFile;

        internal SignTool(string runDir, string binariesPath, string sourcePath)
        {
            _binariesPath = binariesPath;
            _sourcePath = sourcePath;
            _buildFilePath = Path.Combine(runDir, "build.proj");

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            _msbuildPath = Path.Combine(path, @"MSBuild\14.0\Bin\MSBuild.exe");
            if (!File.Exists(_msbuildPath))
            {
                throw new Exception(@"Unable to locate MSBuild at the path {_msbuildPath}");
            }
        }

        private void Sign(IEnumerable<string> filePaths)
        {
            EnsureBuildFile();

            var commandLine = new StringBuilder();
            commandLine.Append(@"/v:m /target:RoslynSign ");
            commandLine.Append($@"/p:SignIntermediatesDir=""{Path.GetTempPath()}"" ");
            commandLine.Append($@"/p:SignOutDir=""{_binariesPath}"" ");

            commandLine.Append($@"/p:FilesToSign=""");
            var first = true;
            foreach (var filePath in filePaths)
            {
                if (!first)
                {
                    commandLine.Append(";");
                    first = false;
                }

                commandLine.Append(filePath);
            }
            commandLine.Append(@"""");

            var startInfo = new ProcessStartInfo()
            {
                FileName = _msbuildPath,
                Arguments = commandLine.ToString(),
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Console.WriteLine("MSBuild failed!!!");
                Console.WriteLine(process.StandardOutput.ReadToEnd());
                throw new Exception("Sign failed");
            }
        }

        private void EnsureBuildFile()
        {
            if (!_generatedBuildFile)
            {
                GenerateBuildFile();
                _generatedBuildFile = true;
            }
        }

        private void GenerateBuildFile()
        {
            File.WriteAllText(_buildFilePath, GenerateBuildFileContent());
        }

        private string GenerateBuildFileContent()
        {
            var builder = new StringBuilder();
            builder.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">");

            builder.AppendLine($@"    <Import Project=""{Path.Combine(_sourcePath, @"build\Targets\VSL.Settings.targets")}"" />");

            builder.AppendLine(@"
    <Import Project=""$(NuGetPackageRoot)\MicroBuild.Core\0.2.0\build\MicroBuild.Core.props"" />
    <Import Project=""$(NuGetPackageRoot)\MicroBuild.Core\0.2.0\build\MicroBuild.Core.targets"" />

    <Target Name=""RoslynSign"">

        <SignFiles Files=""@(FilesToSign)""
                   BinariesDirectory=""$(SignOutDir)""
                   IntermediatesDirectory=""$(SignIntermediatesDir)""
                   Type=""real"" />
    </Target>
</Project>");

            return builder.ToString();
        }

        void ISignTool.Sign(IEnumerable<string> filePaths)
        {
            Sign(filePaths);
        }
    }
}
