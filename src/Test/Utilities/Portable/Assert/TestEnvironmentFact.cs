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
    public enum WindowsFactKind
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

    public sealed class WindowsFactAttribute : FactAttribute
    {
        public WindowsFactKind Kind { get; }

        public WindowsFactAttribute(WindowsFactKind kind)
        {
            Kind = kind;

            string reason = null;
            switch (kind)
            {
                case WindowsFactKind.Paths:
                    reason = "uses windows paths";
                    break;
                case WindowsFactKind.Unknown:
                    reason = "general";
                    break;
            }

            Skip = $"Test only supported on Windows: {reason}";
        }
    }

    public sealed class UnixLikeFactAttribute : FactAttribute
    {

    }
}
