// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Roslyn.Test.Utilities
{
    /// <summary>
    /// The different kinds of test environments that affect whether tests can execute.
    /// </summary>
    public enum TestEnvironmentKind
    {
        // Runtimes
        CoreClr,
        DesktopClr,
        Mono,
        AnyClr,
        AnyDesktop,

        // OS
        Windows,
        Unix,

        // Locale
        EnglishLocale
    }

    public enum TestEnvironmentReason
    {
        /// <summary>
        /// Uses windows paths
        /// </summary>
        Paths,

        /// <summary>
        /// Uses signing
        /// </summary>
        Signing,

        /// <summary>
        /// Test uses Windows PDBs
        /// </summary>
        WindowsPdb,

        Unknown,
    }

    public sealed class TestEnvironmentUtil
    {
        public static bool IsRuntimeCoreClr => CoreClrShim.IsRunningOnCoreClr;
        public static bool IsRuntimeDesktopClr => CoreClrShim.AssemblyLoadContext.Type == null && !IsRuntimeMono;
        public static bool IsRuntimeMono => MonoHelpers.IsRunningOnMono();
        public static bool IsRuntimeAnyClr => IsRuntimeCoreClr || IsRuntimeDesktopClr;
        public static bool IsRuntimeAnyDesktop => IsRuntimeDesktopClr || IsRuntimeMono;

        public static bool IsWindows => Path.DirectorySeparatorChar == '\\';
        public static bool IsUnix => PathUtilities.IsUnixLikePlatform;

        public static bool IsEnglishLocale =>
            CultureInfo.CurrentUICulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
            CultureInfo.CurrentCulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        public static bool IsTestEnvironment(TestEnvironmentKind kind)
        {
            switch (kind)
            {
                case TestEnvironmentKind.CoreClr: return IsRuntimeCoreClr;
                case TestEnvironmentKind.DesktopClr: return IsRuntimeDesktopClr;
                case TestEnvironmentKind.Mono: return IsRuntimeMono;
                case TestEnvironmentKind.AnyClr: return IsRuntimeAnyClr;
                case TestEnvironmentKind.AnyDesktop: return IsRuntimeAnyDesktop;

                case TestEnvironmentKind.Windows: return IsWindows;
                case TestEnvironmentKind.Unix: return IsUnix;

                case TestEnvironmentKind.EnglishLocale: return IsEnglishLocale;

                default: throw new Exception($"Invalid value {kind}");
            }
        }

        public static string GetSkipMessage(TestEnvironmentKind kind, TestEnvironmentReason reason)
        {
            string getFriendlyName()
            {
                switch (kind)
                {
                    case TestEnvironmentKind.CoreClr: return "CoreClr";
                    case TestEnvironmentKind.DesktopClr: return "Desktop";
                    case TestEnvironmentKind.Mono: return "Mono";
                    case TestEnvironmentKind.AnyClr: return "any CLR";
                    case TestEnvironmentKind.AnyDesktop: return "any desktop";

                    case TestEnvironmentKind.Windows: return "Windows";
                    case TestEnvironmentKind.Unix: return "Unix";

                    case TestEnvironmentKind.EnglishLocale: return "English locale";

                    default: throw new Exception($"Invalid value {kind}");
                }
            }

            string getMessage()
            {
                switch (reason)
                {
                    case TestEnvironmentReason.Paths: return "uses OS specific paths";
                    case TestEnvironmentReason.Signing: return "signing";
                    case TestEnvironmentReason.WindowsPdb: return "uses windows pdbs";
                    case TestEnvironmentReason.Unknown: return "unknown";
                    default: throw new Exception($"Invalid value {reason}");
                }

            }

            string name = getFriendlyName();
            string message = getMessage();
            return $"Test only supported on {name}: {reason}";
        }
    }

    /// <summary>
    /// Represents a test that is executable only in certain environments.
    /// </summary>
    public class TestEnvironmentFactAttribute : FactAttribute
    {
        /// <summary>
        /// Test will only be executed in the environment defined by <paramref name="kind"/>
        /// </summary>
        public TestEnvironmentFactAttribute(TestEnvironmentReason reason, TestEnvironmentKind kind)
        {
            if (TestEnvironmentUtil.IsTestEnvironment(kind))
            {
                Skip = TestEnvironmentUtil.GetSkipMessage(kind, reason);
            }
        }

        /// <summary>
        /// Test will only be executed in the environments defined by <paramref name="kind"/>. All must 
        /// be true in order for the test to execute.
        /// </summary>
        public TestEnvironmentFactAttribute(TestEnvironmentReason reason, params TestEnvironmentKind[] kinds)
        {
            foreach (var kind in kinds)
            {
                if (!TestEnvironmentUtil.IsTestEnvironment(kind))
                {
                    Skip = "Test not supported in this environment";
                }
            }
        }
    }

    public sealed class WindowsFactAttribute : TestEnvironmentFactAttribute
    {
        public WindowsFactAttribute(TestEnvironmentReason reason) : base(reason, TestEnvironmentKind.Windows)
        {
        }
    }
}
