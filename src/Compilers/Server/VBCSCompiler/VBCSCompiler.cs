// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.CommandLine;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Microsoft.CodeAnalysis.CompilerServer
{
    internal static class VBCSCompiler
    {
        public static int Main(string[] args)
        {
#if BOOTSTRAP
            ExitingTraceListener.Install(logger);
#endif
            string? pipeName;
            string? loggerFilePath;
            bool shutdown;

            if (!ParseCommandLine(args, out pipeName, out shutdown, out loggerFilePath))
            {
                return CommonCompiler.Failed;
            }

            using var logger = new CompilerServerLogger("VBCSCompiler", loggerFilePath);
            try
            {
                var appSettings = GetAppSettings(logger);
                var controller = new BuildServerController(appSettings, logger);

                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) => { cancellationTokenSource.Cancel(); };

                return shutdown
                    ? controller.RunShutdown(pipeName, cancellationToken: cancellationTokenSource.Token)
                    : controller.RunServer(pipeName, cancellationToken: cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                // Assume the exception was the result of a missing compiler assembly.
                logger.LogException(e, "Cannot start server");
            }

            return CommonCompiler.Failed;
        }

        private static NameValueCollection GetAppSettings(ICompilerServerLogger logger)
        {
            NameValueCollection appSettings;
            try
            {
#if NET472
                appSettings = System.Configuration.ConfigurationManager.AppSettings;
#else
                // Do not use AppSettings on non-desktop platforms
                appSettings = new NameValueCollection();
#endif
            }
            catch (Exception ex)
            {
                // It is possible for AppSettings to throw when the application or machine configuration 
                // is corrupted.  This should not prevent the server from starting, but instead just revert
                // to the default configuration.
                appSettings = new NameValueCollection();
                logger.LogException(ex, "Error loading application settings");
            }

            return appSettings;
        }

        private static bool ParseCommandLine(string[] args, [NotNullWhen(true)] out string? pipeName, out bool shutdown, out string? loggerFilePath)
        {
            pipeName = null;
            shutdown = false;
            loggerFilePath = null;

            foreach (var arg in args)
            {
                const string pipeArgPrefix = "-pipename:";
                const string loggerArgPrefix = "-logger:";
                if (arg.StartsWith(pipeArgPrefix, StringComparison.Ordinal))
                {
                    pipeName = arg.Substring(pipeArgPrefix.Length);
                }
                else if (arg.StartsWith(loggerArgPrefix, StringComparison.Ordinal))
                {
                    loggerFilePath = arg.Substring(loggerArgPrefix.Length);
                }
                else if (arg == "-shutdown")
                {
                    shutdown = true;
                }
                else
                {
                    Console.WriteLine($"Invalid arg: {arg}");
                    return false;
                }
            }

            pipeName = pipeName ?? BuildServerConnection.GetPipeName(BuildClient.GetClientDirectory());
            if (pipeName is null)
            {
                Console.WriteLine("Cannot calculate pipe name");
                return false;
            }

            return true;
        }
    }
}
