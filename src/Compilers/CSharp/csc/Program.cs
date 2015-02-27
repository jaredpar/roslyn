// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.IO;

namespace Microsoft.CodeAnalysis.CSharp.CommandLine
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Console.Error.WriteLine("C# compiler");
            foreach (var arg in args) 
            {
                Console.Error.WriteLine(arg);
                if (arg[0] == '@') 
                {
                    var rspPath = arg.Substring(1);
                    var text = File.ReadAllText(rspPath);
                    var outPath = Path.Combine(@"c:\users\jaredpar\temp", Path.GetFileName(rspPath));
                    File.WriteAllText(outPath, text);
                }
            }

            return Csc.Run(args);
        }
    }
}
