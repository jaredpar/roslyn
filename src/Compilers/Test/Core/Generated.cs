// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This is a generated file, please edit Generate.ps1 to change the contents

#nullable disable

using Microsoft.CodeAnalysis;

namespace Roslyn.Test.Utilities
{
    public static class TestMetadata
    {
        public readonly struct ReferenceInfo
        {
            public string FileName { get; }
            public byte[] ImageBytes { get; }
            public ReferenceInfo(string fileName, byte[] imageBytes)
            {
                FileName = fileName;
                ImageBytes = imageBytes;
            }
        }
        public static class ResourcesNet20
        {
            private static byte[] _mscorlib;
            public static ReferenceInfo mscorlib => new ReferenceInfo("mscorlib.dll", ResourceLoader.GetOrCreateResource(ref _mscorlib, "net20.mscorlib.dll"));
            private static byte[] _System;
            public static ReferenceInfo System => new ReferenceInfo("System.dll", ResourceLoader.GetOrCreateResource(ref _System, "net20.System.dll"));
            private static byte[] _MicrosoftVisualBasic;
            public static ReferenceInfo MicrosoftVisualBasic => new ReferenceInfo("Microsoft.VisualBasic.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftVisualBasic, "net20.Microsoft.VisualBasic.dll"));
            public static ReferenceInfo[] All => new[]
            {
                mscorlib,
                System,
                MicrosoftVisualBasic,
            };
        }
        public static class Net20
        {
            public static PortableExecutableReference mscorlib { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet20.mscorlib.ImageBytes).GetReference(display: "mscorlib.dll (net20)", filePath: "mscorlib.dll");
            public static PortableExecutableReference System { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet20.System.ImageBytes).GetReference(display: "System.dll (net20)", filePath: "System.dll");
            public static PortableExecutableReference MicrosoftVisualBasic { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet20.MicrosoftVisualBasic.ImageBytes).GetReference(display: "Microsoft.VisualBasic.dll (net20)", filePath: "Microsoft.VisualBasic.dll");
        }
        public static class ResourcesNet35
        {
            private static byte[] _SystemCore;
            public static ReferenceInfo SystemCore => new ReferenceInfo("System.Core.dll", ResourceLoader.GetOrCreateResource(ref _SystemCore, "net35.System.Core.dll"));
            public static ReferenceInfo[] All => new[]
            {
                SystemCore,
            };
        }
        public static class Net35
        {
            public static PortableExecutableReference SystemCore { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet35.SystemCore.ImageBytes).GetReference(display: "System.Core.dll (net35)", filePath: "System.Core.dll");
        }
        public static class ResourcesNet40
        {
            private static byte[] _mscorlib;
            public static ReferenceInfo mscorlib => new ReferenceInfo("mscorlib.dll", ResourceLoader.GetOrCreateResource(ref _mscorlib, "net40.mscorlib.dll"));
            private static byte[] _System;
            public static ReferenceInfo System => new ReferenceInfo("System.dll", ResourceLoader.GetOrCreateResource(ref _System, "net40.System.dll"));
            private static byte[] _SystemCore;
            public static ReferenceInfo SystemCore => new ReferenceInfo("System.Core.dll", ResourceLoader.GetOrCreateResource(ref _SystemCore, "net40.System.Core.dll"));
            private static byte[] _SystemData;
            public static ReferenceInfo SystemData => new ReferenceInfo("System.Data.dll", ResourceLoader.GetOrCreateResource(ref _SystemData, "net40.System.Data.dll"));
            private static byte[] _SystemXml;
            public static ReferenceInfo SystemXml => new ReferenceInfo("System.Xml.dll", ResourceLoader.GetOrCreateResource(ref _SystemXml, "net40.System.Xml.dll"));
            private static byte[] _SystemXmlLinq;
            public static ReferenceInfo SystemXmlLinq => new ReferenceInfo("System.Xml.Linq.dll", ResourceLoader.GetOrCreateResource(ref _SystemXmlLinq, "net40.System.Xml.Linq.dll"));
            private static byte[] _MicrosoftVisualBasic;
            public static ReferenceInfo MicrosoftVisualBasic => new ReferenceInfo("Microsoft.VisualBasic.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftVisualBasic, "net40.Microsoft.VisualBasic.dll"));
            private static byte[] _MicrosoftCSharp;
            public static ReferenceInfo MicrosoftCSharp => new ReferenceInfo("Microsoft.CSharp.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftCSharp, "net40.Microsoft.CSharp.dll"));
            public static ReferenceInfo[] All => new[]
            {
                mscorlib,
                System,
                SystemCore,
                SystemData,
                SystemXml,
                SystemXmlLinq,
                MicrosoftVisualBasic,
                MicrosoftCSharp,
            };
        }
        public static class Net40
        {
            public static PortableExecutableReference mscorlib { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet40.mscorlib.ImageBytes).GetReference(display: "mscorlib.dll (net40)", filePath: "mscorlib.dll");
            public static PortableExecutableReference System { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet40.System.ImageBytes).GetReference(display: "System.dll (net40)", filePath: "System.dll");
            public static PortableExecutableReference SystemCore { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet40.SystemCore.ImageBytes).GetReference(display: "System.Core.dll (net40)", filePath: "System.Core.dll");
            public static PortableExecutableReference SystemData { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet40.SystemData.ImageBytes).GetReference(display: "System.Data.dll (net40)", filePath: "System.Data.dll");
            public static PortableExecutableReference SystemXml { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet40.SystemXml.ImageBytes).GetReference(display: "System.Xml.dll (net40)", filePath: "System.Xml.dll");
            public static PortableExecutableReference SystemXmlLinq { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet40.SystemXmlLinq.ImageBytes).GetReference(display: "System.Xml.Linq.dll (net40)", filePath: "System.Xml.Linq.dll");
            public static PortableExecutableReference MicrosoftVisualBasic { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet40.MicrosoftVisualBasic.ImageBytes).GetReference(display: "Microsoft.VisualBasic.dll (net40)", filePath: "Microsoft.VisualBasic.dll");
            public static PortableExecutableReference MicrosoftCSharp { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet40.MicrosoftCSharp.ImageBytes).GetReference(display: "Microsoft.CSharp.dll (net40)", filePath: "Microsoft.CSharp.dll");
        }
        public static class ResourcesNet451
        {
            private static byte[] _mscorlib;
            public static ReferenceInfo mscorlib => new ReferenceInfo("mscorlib.dll", ResourceLoader.GetOrCreateResource(ref _mscorlib, "net451.mscorlib.dll"));
            private static byte[] _System;
            public static ReferenceInfo System => new ReferenceInfo("System.dll", ResourceLoader.GetOrCreateResource(ref _System, "net451.System.dll"));
            private static byte[] _SystemConfiguration;
            public static ReferenceInfo SystemConfiguration => new ReferenceInfo("System.Configuration.dll", ResourceLoader.GetOrCreateResource(ref _SystemConfiguration, "net451.System.Configuration.dll"));
            private static byte[] _SystemCore;
            public static ReferenceInfo SystemCore => new ReferenceInfo("System.Core.dll", ResourceLoader.GetOrCreateResource(ref _SystemCore, "net451.System.Core.dll"));
            private static byte[] _SystemData;
            public static ReferenceInfo SystemData => new ReferenceInfo("System.Data.dll", ResourceLoader.GetOrCreateResource(ref _SystemData, "net451.System.Data.dll"));
            private static byte[] _SystemDrawing;
            public static ReferenceInfo SystemDrawing => new ReferenceInfo("System.Drawing.dll", ResourceLoader.GetOrCreateResource(ref _SystemDrawing, "net451.System.Drawing.dll"));
            private static byte[] _SystemEnterpriseServices;
            public static ReferenceInfo SystemEnterpriseServices => new ReferenceInfo("System.EnterpriseServices.dll", ResourceLoader.GetOrCreateResource(ref _SystemEnterpriseServices, "net451.System.EnterpriseServices.dll"));
            private static byte[] _SystemRuntimeSerialization;
            public static ReferenceInfo SystemRuntimeSerialization => new ReferenceInfo("System.Runtime.Serialization.dll", ResourceLoader.GetOrCreateResource(ref _SystemRuntimeSerialization, "net451.System.Runtime.Serialization.dll"));
            private static byte[] _SystemWindowsForms;
            public static ReferenceInfo SystemWindowsForms => new ReferenceInfo("System.Windows.Forms.dll", ResourceLoader.GetOrCreateResource(ref _SystemWindowsForms, "net451.System.Windows.Forms.dll"));
            private static byte[] _SystemWebServices;
            public static ReferenceInfo SystemWebServices => new ReferenceInfo("System.Web.Services.dll", ResourceLoader.GetOrCreateResource(ref _SystemWebServices, "net451.System.Web.Services.dll"));
            private static byte[] _SystemXml;
            public static ReferenceInfo SystemXml => new ReferenceInfo("System.Xml.dll", ResourceLoader.GetOrCreateResource(ref _SystemXml, "net451.System.Xml.dll"));
            private static byte[] _SystemXmlLinq;
            public static ReferenceInfo SystemXmlLinq => new ReferenceInfo("System.Xml.Linq.dll", ResourceLoader.GetOrCreateResource(ref _SystemXmlLinq, "net451.System.Xml.Linq.dll"));
            private static byte[] _MicrosoftCSharp;
            public static ReferenceInfo MicrosoftCSharp => new ReferenceInfo("Microsoft.CSharp.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftCSharp, "net451.Microsoft.CSharp.dll"));
            private static byte[] _MicrosoftVisualBasic;
            public static ReferenceInfo MicrosoftVisualBasic => new ReferenceInfo("Microsoft.VisualBasic.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftVisualBasic, "net451.Microsoft.VisualBasic.dll"));
            private static byte[] _SystemObjectModel;
            public static ReferenceInfo SystemObjectModel => new ReferenceInfo("System.ObjectModel.dll", ResourceLoader.GetOrCreateResource(ref _SystemObjectModel, "net451.System.ObjectModel.dll"));
            private static byte[] _SystemRuntime;
            public static ReferenceInfo SystemRuntime => new ReferenceInfo("System.Runtime.dll", ResourceLoader.GetOrCreateResource(ref _SystemRuntime, "net451.System.Runtime.dll"));
            private static byte[] _SystemRuntimeInteropServicesWindowsRuntime;
            public static ReferenceInfo SystemRuntimeInteropServicesWindowsRuntime => new ReferenceInfo("System.Runtime.InteropServices.WindowsRuntime.dll", ResourceLoader.GetOrCreateResource(ref _SystemRuntimeInteropServicesWindowsRuntime, "net451.System.Runtime.InteropServices.WindowsRuntime.dll"));
            private static byte[] _SystemThreading;
            public static ReferenceInfo SystemThreading => new ReferenceInfo("System.Threading.dll", ResourceLoader.GetOrCreateResource(ref _SystemThreading, "net451.System.Threading.dll"));
            private static byte[] _SystemThreadingTasks;
            public static ReferenceInfo SystemThreadingTasks => new ReferenceInfo("System.Threading.Tasks.dll", ResourceLoader.GetOrCreateResource(ref _SystemThreadingTasks, "net451.System.Threading.Tasks.dll"));
            public static ReferenceInfo[] All => new[]
            {
                mscorlib,
                System,
                SystemConfiguration,
                SystemCore,
                SystemData,
                SystemDrawing,
                SystemEnterpriseServices,
                SystemRuntimeSerialization,
                SystemWindowsForms,
                SystemWebServices,
                SystemXml,
                SystemXmlLinq,
                MicrosoftCSharp,
                MicrosoftVisualBasic,
                SystemObjectModel,
                SystemRuntime,
                SystemRuntimeInteropServicesWindowsRuntime,
                SystemThreading,
                SystemThreadingTasks,
            };
        }
        public static class Net451
        {
            public static PortableExecutableReference mscorlib { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.mscorlib.ImageBytes).GetReference(display: "mscorlib.dll (net451)", filePath: "mscorlib.dll");
            public static PortableExecutableReference System { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.System.ImageBytes).GetReference(display: "System.dll (net451)", filePath: "System.dll");
            public static PortableExecutableReference SystemConfiguration { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemConfiguration.ImageBytes).GetReference(display: "System.Configuration.dll (net451)", filePath: "System.Configuration.dll");
            public static PortableExecutableReference SystemCore { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemCore.ImageBytes).GetReference(display: "System.Core.dll (net451)", filePath: "System.Core.dll");
            public static PortableExecutableReference SystemData { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemData.ImageBytes).GetReference(display: "System.Data.dll (net451)", filePath: "System.Data.dll");
            public static PortableExecutableReference SystemDrawing { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemDrawing.ImageBytes).GetReference(display: "System.Drawing.dll (net451)", filePath: "System.Drawing.dll");
            public static PortableExecutableReference SystemEnterpriseServices { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemEnterpriseServices.ImageBytes).GetReference(display: "System.EnterpriseServices.dll (net451)", filePath: "System.EnterpriseServices.dll");
            public static PortableExecutableReference SystemRuntimeSerialization { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemRuntimeSerialization.ImageBytes).GetReference(display: "System.Runtime.Serialization.dll (net451)", filePath: "System.Runtime.Serialization.dll");
            public static PortableExecutableReference SystemWindowsForms { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemWindowsForms.ImageBytes).GetReference(display: "System.Windows.Forms.dll (net451)", filePath: "System.Windows.Forms.dll");
            public static PortableExecutableReference SystemWebServices { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemWebServices.ImageBytes).GetReference(display: "System.Web.Services.dll (net451)", filePath: "System.Web.Services.dll");
            public static PortableExecutableReference SystemXml { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemXml.ImageBytes).GetReference(display: "System.Xml.dll (net451)", filePath: "System.Xml.dll");
            public static PortableExecutableReference SystemXmlLinq { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemXmlLinq.ImageBytes).GetReference(display: "System.Xml.Linq.dll (net451)", filePath: "System.Xml.Linq.dll");
            public static PortableExecutableReference MicrosoftCSharp { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.MicrosoftCSharp.ImageBytes).GetReference(display: "Microsoft.CSharp.dll (net451)", filePath: "Microsoft.CSharp.dll");
            public static PortableExecutableReference MicrosoftVisualBasic { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.MicrosoftVisualBasic.ImageBytes).GetReference(display: "Microsoft.VisualBasic.dll (net451)", filePath: "Microsoft.VisualBasic.dll");
            public static PortableExecutableReference SystemObjectModel { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemObjectModel.ImageBytes).GetReference(display: "System.ObjectModel.dll (net451)", filePath: "System.ObjectModel.dll");
            public static PortableExecutableReference SystemRuntime { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemRuntime.ImageBytes).GetReference(display: "System.Runtime.dll (net451)", filePath: "System.Runtime.dll");
            public static PortableExecutableReference SystemRuntimeInteropServicesWindowsRuntime { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemRuntimeInteropServicesWindowsRuntime.ImageBytes).GetReference(display: "System.Runtime.InteropServices.WindowsRuntime.dll (net451)", filePath: "System.Runtime.InteropServices.WindowsRuntime.dll");
            public static PortableExecutableReference SystemThreading { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemThreading.ImageBytes).GetReference(display: "System.Threading.dll (net451)", filePath: "System.Threading.dll");
            public static PortableExecutableReference SystemThreadingTasks { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet451.SystemThreadingTasks.ImageBytes).GetReference(display: "System.Threading.Tasks.dll (net451)", filePath: "System.Threading.Tasks.dll");
        }
        public static class ResourcesNet461
        {
            private static byte[] _mscorlib;
            public static ReferenceInfo mscorlib => new ReferenceInfo("mscorlib.dll", ResourceLoader.GetOrCreateResource(ref _mscorlib, "net461.mscorlib.dll"));
            private static byte[] _System;
            public static ReferenceInfo System => new ReferenceInfo("System.dll", ResourceLoader.GetOrCreateResource(ref _System, "net461.System.dll"));
            private static byte[] _SystemCore;
            public static ReferenceInfo SystemCore => new ReferenceInfo("System.Core.dll", ResourceLoader.GetOrCreateResource(ref _SystemCore, "net461.System.Core.dll"));
            private static byte[] _SystemRuntime;
            public static ReferenceInfo SystemRuntime => new ReferenceInfo("System.Runtime.dll", ResourceLoader.GetOrCreateResource(ref _SystemRuntime, "net461.System.Runtime.dll"));
            private static byte[] _SystemThreadingTasks;
            public static ReferenceInfo SystemThreadingTasks => new ReferenceInfo("System.Threading.Tasks.dll", ResourceLoader.GetOrCreateResource(ref _SystemThreadingTasks, "net461.System.Threading.Tasks.dll"));
            private static byte[] _MicrosoftCSharp;
            public static ReferenceInfo MicrosoftCSharp => new ReferenceInfo("Microsoft.CSharp.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftCSharp, "net461.Microsoft.CSharp.dll"));
            private static byte[] _MicrosoftVisualBasic;
            public static ReferenceInfo MicrosoftVisualBasic => new ReferenceInfo("Microsoft.VisualBasic.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftVisualBasic, "net461.Microsoft.VisualBasic.dll"));
            public static ReferenceInfo[] All => new[]
            {
                mscorlib,
                System,
                SystemCore,
                SystemRuntime,
                SystemThreadingTasks,
                MicrosoftCSharp,
                MicrosoftVisualBasic,
            };
        }
        public static class Net461
        {
            public static PortableExecutableReference mscorlib { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet461.mscorlib.ImageBytes).GetReference(display: "mscorlib.dll (net461)", filePath: "mscorlib.dll");
            public static PortableExecutableReference System { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet461.System.ImageBytes).GetReference(display: "System.dll (net461)", filePath: "System.dll");
            public static PortableExecutableReference SystemCore { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet461.SystemCore.ImageBytes).GetReference(display: "System.Core.dll (net461)", filePath: "System.Core.dll");
            public static PortableExecutableReference SystemRuntime { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet461.SystemRuntime.ImageBytes).GetReference(display: "System.Runtime.dll (net461)", filePath: "System.Runtime.dll");
            public static PortableExecutableReference SystemThreadingTasks { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet461.SystemThreadingTasks.ImageBytes).GetReference(display: "System.Threading.Tasks.dll (net461)", filePath: "System.Threading.Tasks.dll");
            public static PortableExecutableReference MicrosoftCSharp { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet461.MicrosoftCSharp.ImageBytes).GetReference(display: "Microsoft.CSharp.dll (net461)", filePath: "Microsoft.CSharp.dll");
            public static PortableExecutableReference MicrosoftVisualBasic { get; } = AssemblyMetadata.CreateFromImage(ResourcesNet461.MicrosoftVisualBasic.ImageBytes).GetReference(display: "Microsoft.VisualBasic.dll (net461)", filePath: "Microsoft.VisualBasic.dll");
        }
        public static class ResourcesNetCoreApp
        {
            private static byte[] _mscorlib;
            public static ReferenceInfo mscorlib => new ReferenceInfo("mscorlib.dll", ResourceLoader.GetOrCreateResource(ref _mscorlib, "netcoreapp.mscorlib.dll"));
            private static byte[] _System;
            public static ReferenceInfo System => new ReferenceInfo("System.dll", ResourceLoader.GetOrCreateResource(ref _System, "netcoreapp.System.dll"));
            private static byte[] _SystemCore;
            public static ReferenceInfo SystemCore => new ReferenceInfo("System.Core.dll", ResourceLoader.GetOrCreateResource(ref _SystemCore, "netcoreapp.System.Core.dll"));
            private static byte[] _SystemCollections;
            public static ReferenceInfo SystemCollections => new ReferenceInfo("System.Collections.dll", ResourceLoader.GetOrCreateResource(ref _SystemCollections, "netcoreapp.System.Collections.dll"));
            private static byte[] _SystemConsole;
            public static ReferenceInfo SystemConsole => new ReferenceInfo("System.Console.dll", ResourceLoader.GetOrCreateResource(ref _SystemConsole, "netcoreapp.System.Console.dll"));
            private static byte[] _SystemLinq;
            public static ReferenceInfo SystemLinq => new ReferenceInfo("System.Linq.dll", ResourceLoader.GetOrCreateResource(ref _SystemLinq, "netcoreapp.System.Linq.dll"));
            private static byte[] _SystemLinqExpressions;
            public static ReferenceInfo SystemLinqExpressions => new ReferenceInfo("System.Linq.Expressions.dll", ResourceLoader.GetOrCreateResource(ref _SystemLinqExpressions, "netcoreapp.System.Linq.Expressions.dll"));
            private static byte[] _SystemRuntime;
            public static ReferenceInfo SystemRuntime => new ReferenceInfo("System.Runtime.dll", ResourceLoader.GetOrCreateResource(ref _SystemRuntime, "netcoreapp.System.Runtime.dll"));
            private static byte[] _SystemRuntimeInteropServices;
            public static ReferenceInfo SystemRuntimeInteropServices => new ReferenceInfo("System.Runtime.InteropServices.dll", ResourceLoader.GetOrCreateResource(ref _SystemRuntimeInteropServices, "netcoreapp.System.Runtime.InteropServices.dll"));
            private static byte[] _SystemThreadingTasks;
            public static ReferenceInfo SystemThreadingTasks => new ReferenceInfo("System.Threading.Tasks.dll", ResourceLoader.GetOrCreateResource(ref _SystemThreadingTasks, "netcoreapp.System.Threading.Tasks.dll"));
            private static byte[] _netstandard;
            public static ReferenceInfo netstandard => new ReferenceInfo("netstandard.dll", ResourceLoader.GetOrCreateResource(ref _netstandard, "netcoreapp.netstandard.dll"));
            private static byte[] _MicrosoftCSharp;
            public static ReferenceInfo MicrosoftCSharp => new ReferenceInfo("Microsoft.CSharp.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftCSharp, "netcoreapp.Microsoft.CSharp.dll"));
            private static byte[] _MicrosoftVisualBasic;
            public static ReferenceInfo MicrosoftVisualBasic => new ReferenceInfo("Microsoft.VisualBasic.dll", ResourceLoader.GetOrCreateResource(ref _MicrosoftVisualBasic, "netcoreapp.Microsoft.VisualBasic.dll"));
            public static ReferenceInfo[] All => new[]
            {
                mscorlib,
                System,
                SystemCore,
                SystemCollections,
                SystemConsole,
                SystemLinq,
                SystemLinqExpressions,
                SystemRuntime,
                SystemRuntimeInteropServices,
                SystemThreadingTasks,
                netstandard,
                MicrosoftCSharp,
                MicrosoftVisualBasic,
            };
        }
        public static class NetCoreApp
        {
            public static PortableExecutableReference mscorlib { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.mscorlib.ImageBytes).GetReference(display: "mscorlib.dll (netcoreapp)", filePath: "mscorlib.dll");
            public static PortableExecutableReference System { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.System.ImageBytes).GetReference(display: "System.dll (netcoreapp)", filePath: "System.dll");
            public static PortableExecutableReference SystemCore { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.SystemCore.ImageBytes).GetReference(display: "System.Core.dll (netcoreapp)", filePath: "System.Core.dll");
            public static PortableExecutableReference SystemCollections { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.SystemCollections.ImageBytes).GetReference(display: "System.Collections.dll (netcoreapp)", filePath: "System.Collections.dll");
            public static PortableExecutableReference SystemConsole { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.SystemConsole.ImageBytes).GetReference(display: "System.Console.dll (netcoreapp)", filePath: "System.Console.dll");
            public static PortableExecutableReference SystemLinq { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.SystemLinq.ImageBytes).GetReference(display: "System.Linq.dll (netcoreapp)", filePath: "System.Linq.dll");
            public static PortableExecutableReference SystemLinqExpressions { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.SystemLinqExpressions.ImageBytes).GetReference(display: "System.Linq.Expressions.dll (netcoreapp)", filePath: "System.Linq.Expressions.dll");
            public static PortableExecutableReference SystemRuntime { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.SystemRuntime.ImageBytes).GetReference(display: "System.Runtime.dll (netcoreapp)", filePath: "System.Runtime.dll");
            public static PortableExecutableReference SystemRuntimeInteropServices { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.SystemRuntimeInteropServices.ImageBytes).GetReference(display: "System.Runtime.InteropServices.dll (netcoreapp)", filePath: "System.Runtime.InteropServices.dll");
            public static PortableExecutableReference SystemThreadingTasks { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.SystemThreadingTasks.ImageBytes).GetReference(display: "System.Threading.Tasks.dll (netcoreapp)", filePath: "System.Threading.Tasks.dll");
            public static PortableExecutableReference netstandard { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.netstandard.ImageBytes).GetReference(display: "netstandard.dll (netcoreapp)", filePath: "netstandard.dll");
            public static PortableExecutableReference MicrosoftCSharp { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.MicrosoftCSharp.ImageBytes).GetReference(display: "Microsoft.CSharp.dll (netcoreapp)", filePath: "Microsoft.CSharp.dll");
            public static PortableExecutableReference MicrosoftVisualBasic { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetCoreApp.MicrosoftVisualBasic.ImageBytes).GetReference(display: "Microsoft.VisualBasic.dll (netcoreapp)", filePath: "Microsoft.VisualBasic.dll");
        }
        public static class ResourcesNetStandard20
        {
            private static byte[] _mscorlib;
            public static ReferenceInfo mscorlib => new ReferenceInfo("mscorlib.dll", ResourceLoader.GetOrCreateResource(ref _mscorlib, "netstandard20.mscorlib.dll"));
            private static byte[] _System;
            public static ReferenceInfo System => new ReferenceInfo("System.dll", ResourceLoader.GetOrCreateResource(ref _System, "netstandard20.System.dll"));
            private static byte[] _SystemCore;
            public static ReferenceInfo SystemCore => new ReferenceInfo("System.Core.dll", ResourceLoader.GetOrCreateResource(ref _SystemCore, "netstandard20.System.Core.dll"));
            private static byte[] _SystemDynamicRuntime;
            public static ReferenceInfo SystemDynamicRuntime => new ReferenceInfo("System.Dynamic.Runtime.dll", ResourceLoader.GetOrCreateResource(ref _SystemDynamicRuntime, "netstandard20.System.Dynamic.Runtime.dll"));
            private static byte[] _SystemLinq;
            public static ReferenceInfo SystemLinq => new ReferenceInfo("System.Linq.dll", ResourceLoader.GetOrCreateResource(ref _SystemLinq, "netstandard20.System.Linq.dll"));
            private static byte[] _SystemLinqExpressions;
            public static ReferenceInfo SystemLinqExpressions => new ReferenceInfo("System.Linq.Expressions.dll", ResourceLoader.GetOrCreateResource(ref _SystemLinqExpressions, "netstandard20.System.Linq.Expressions.dll"));
            private static byte[] _SystemRuntime;
            public static ReferenceInfo SystemRuntime => new ReferenceInfo("System.Runtime.dll", ResourceLoader.GetOrCreateResource(ref _SystemRuntime, "netstandard20.System.Runtime.dll"));
            private static byte[] _netstandard;
            public static ReferenceInfo netstandard => new ReferenceInfo("netstandard.dll", ResourceLoader.GetOrCreateResource(ref _netstandard, "netstandard20.netstandard.dll"));
            public static ReferenceInfo[] All => new[]
            {
                mscorlib,
                System,
                SystemCore,
                SystemDynamicRuntime,
                SystemLinq,
                SystemLinqExpressions,
                SystemRuntime,
                netstandard,
            };
        }
        public static class NetStandard20
        {
            public static PortableExecutableReference mscorlib { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetStandard20.mscorlib.ImageBytes).GetReference(display: "mscorlib.dll (netstandard20)", filePath: "mscorlib.dll");
            public static PortableExecutableReference System { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetStandard20.System.ImageBytes).GetReference(display: "System.dll (netstandard20)", filePath: "System.dll");
            public static PortableExecutableReference SystemCore { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetStandard20.SystemCore.ImageBytes).GetReference(display: "System.Core.dll (netstandard20)", filePath: "System.Core.dll");
            public static PortableExecutableReference SystemDynamicRuntime { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetStandard20.SystemDynamicRuntime.ImageBytes).GetReference(display: "System.Dynamic.Runtime.dll (netstandard20)", filePath: "System.Dynamic.Runtime.dll");
            public static PortableExecutableReference SystemLinq { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetStandard20.SystemLinq.ImageBytes).GetReference(display: "System.Linq.dll (netstandard20)", filePath: "System.Linq.dll");
            public static PortableExecutableReference SystemLinqExpressions { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetStandard20.SystemLinqExpressions.ImageBytes).GetReference(display: "System.Linq.Expressions.dll (netstandard20)", filePath: "System.Linq.Expressions.dll");
            public static PortableExecutableReference SystemRuntime { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetStandard20.SystemRuntime.ImageBytes).GetReference(display: "System.Runtime.dll (netstandard20)", filePath: "System.Runtime.dll");
            public static PortableExecutableReference netstandard { get; } = AssemblyMetadata.CreateFromImage(ResourcesNetStandard20.netstandard.ImageBytes).GetReference(display: "netstandard.dll (netstandard20)", filePath: "netstandard.dll");
        }
        public static class ResourcesMicrosoftCSharp
        {
            private static byte[] _Netstandard10;
            public static ReferenceInfo Netstandard10 => new ReferenceInfo("Netstandard10.dll", ResourceLoader.GetOrCreateResource(ref _Netstandard10, "netstandard10.microsoftcsharp.Microsoft.CSharp.dll"));
            private static byte[] _Netstandard13Lib;
            public static ReferenceInfo Netstandard13Lib => new ReferenceInfo("Netstandard13Lib.dll", ResourceLoader.GetOrCreateResource(ref _Netstandard13Lib, "netstandard13lib.microsoftcsharp.Microsoft.CSharp.dll"));
            public static ReferenceInfo[] All => new[]
            {
                Netstandard10,
                Netstandard13Lib,
            };
        }
        public static class MicrosoftCSharp
        {
            public static PortableExecutableReference Netstandard10 { get; } = AssemblyMetadata.CreateFromImage(ResourcesMicrosoftCSharp.Netstandard10.ImageBytes).GetReference(display: "Microsoft.CSharp.dll (microsoftcsharp)", filePath: "Netstandard10.dll");
            public static PortableExecutableReference Netstandard13Lib { get; } = AssemblyMetadata.CreateFromImage(ResourcesMicrosoftCSharp.Netstandard13Lib.ImageBytes).GetReference(display: "Microsoft.CSharp.dll (microsoftcsharp)", filePath: "Netstandard13Lib.dll");
        }
        public static class ResourcesMicrosoftVisualBasic
        {
            private static byte[] _Netstandard11;
            public static ReferenceInfo Netstandard11 => new ReferenceInfo("Netstandard11.dll", ResourceLoader.GetOrCreateResource(ref _Netstandard11, "netstandard11.microsoftvisualbasic.Microsoft.VisualBasic.dll"));
            public static ReferenceInfo[] All => new[]
            {
                Netstandard11,
            };
        }
        public static class MicrosoftVisualBasic
        {
            public static PortableExecutableReference Netstandard11 { get; } = AssemblyMetadata.CreateFromImage(ResourcesMicrosoftVisualBasic.Netstandard11.ImageBytes).GetReference(display: "Microsoft.VisualBasic.dll (microsoftvisualbasic)", filePath: "Netstandard11.dll");
        }
        public static class ResourcesSystemThreadingTasksExtensions
        {
            private static byte[] _PortableLib;
            public static ReferenceInfo PortableLib => new ReferenceInfo("PortableLib.dll", ResourceLoader.GetOrCreateResource(ref _PortableLib, "portablelib.systemthreadingtasksextensions.System.Threading.Tasks.Extensions.dll"));
            private static byte[] _NetStandard20Lib;
            public static ReferenceInfo NetStandard20Lib => new ReferenceInfo("NetStandard20Lib.dll", ResourceLoader.GetOrCreateResource(ref _NetStandard20Lib, "netstandard20lib.systemthreadingtasksextensions.System.Threading.Tasks.Extensions.dll"));
            public static ReferenceInfo[] All => new[]
            {
                PortableLib,
                NetStandard20Lib,
            };
        }
        public static class SystemThreadingTasksExtensions
        {
            public static PortableExecutableReference PortableLib { get; } = AssemblyMetadata.CreateFromImage(ResourcesSystemThreadingTasksExtensions.PortableLib.ImageBytes).GetReference(display: "System.Threading.Tasks.Extensions.dll (systemthreadingtasksextensions)", filePath: "PortableLib.dll");
            public static PortableExecutableReference NetStandard20Lib { get; } = AssemblyMetadata.CreateFromImage(ResourcesSystemThreadingTasksExtensions.NetStandard20Lib.ImageBytes).GetReference(display: "System.Threading.Tasks.Extensions.dll (systemthreadingtasksextensions)", filePath: "NetStandard20Lib.dll");
        }
        public static class ResourcesBuildExtensions
        {
            private static byte[] _NetStandardToNet461;
            public static ReferenceInfo NetStandardToNet461 => new ReferenceInfo("NetStandardToNet461.dll", ResourceLoader.GetOrCreateResource(ref _NetStandardToNet461, "netstandardtonet461.buildextensions.netstandard.dll"));
            public static ReferenceInfo[] All => new[]
            {
                NetStandardToNet461,
            };
        }
        public static class BuildExtensions
        {
            public static PortableExecutableReference NetStandardToNet461 { get; } = AssemblyMetadata.CreateFromImage(ResourcesBuildExtensions.NetStandardToNet461.ImageBytes).GetReference(display: "netstandard.dll (buildextensions)", filePath: "NetStandardToNet461.dll");
        }
    }
}
