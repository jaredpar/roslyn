﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

#nullable disable

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class RefStructInterfacesTests : CSharpTestBase
    {
        // PROTOTYPE(RefStructInterfaces): Switch to supporting target framework once we have its ref assemblies.
        private static readonly TargetFramework s_targetFrameworkSupportingByRefLikeGenerics = TargetFramework.Net80;

        [Theory]
        [CombinatorialData]
        public void UnscopedRefInInterface_Method_01(bool isVirtual)
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    " + (isVirtual ? "virtual " : "") + @" ref int M()" + (isVirtual ? " => throw null" : "") + @";
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: ExecutionConditionUtil.IsMonoOrCoreClr || !isVirtual ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                Assert.True(m.GlobalNamespace.GetMember<MethodSymbol>("I.M").HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (6,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(6, 6)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Method_02()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    ref int M()
    {
        throw null;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                Assert.True(m.GlobalNamespace.GetMember<MethodSymbol>("I.M").HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (6,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(6, 6)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Method_03()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    sealed ref int M()
    {
        throw null;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (6,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(6, 6)
                );

            Assert.False(comp.GetMember<MethodSymbol>("I.M").HasUnscopedRefAttribute);
        }

        [Fact]
        public void UnscopedRefInInterface_Method_04()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    abstract static ref int M();
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (6,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(6, 6)
                );

            Assert.False(comp.GetMember<MethodSymbol>("I.M").HasUnscopedRefAttribute);
        }

        [Theory]
        [CombinatorialData]
        public void UnscopedRefInInterface_Property_01(bool isVirtual)
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    " + (isVirtual ? "virtual " : "") + @" ref int P { get" + (isVirtual ? " => throw null" : "") + @"; }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: ExecutionConditionUtil.IsMonoOrCoreClr || !isVirtual ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("I.P");
                Assert.True(propertySymbol.HasUnscopedRefAttribute);
                Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (6,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(6, 6)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Property_02()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    ref int P => throw null;
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("I.P");
                Assert.True(propertySymbol.HasUnscopedRefAttribute);
                Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (6,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(6, 6)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Property_03()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    sealed ref int P => throw null;
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (6,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(6, 6)
                );

            PropertySymbol propertySymbol = comp.GetMember<PropertySymbol>("I.P");
            Assert.False(propertySymbol.HasUnscopedRefAttribute);
            Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
        }

        [Fact]
        public void UnscopedRefInInterface_Property_04()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    abstract static ref int P { get; }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (6,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(6, 6)
                );

            PropertySymbol propertySymbol = comp.GetMember<PropertySymbol>("I.P");
            Assert.False(propertySymbol.HasUnscopedRefAttribute);
            Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
        }

        [Fact]
        public void UnscopedRefInInterface_Property_05()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    ref int P
    {
        [UnscopedRef]
        get;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("I.P");
                Assert.False(propertySymbol.HasUnscopedRefAttribute);
                Assert.True(propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (8,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //         [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(8, 10)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Property_06()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    ref int P
    {
        [UnscopedRef]
        get
        {
            throw null;
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("I.P");
                Assert.False(propertySymbol.HasUnscopedRefAttribute);
                Assert.True(propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (8,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //         [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(8, 10)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Property_07()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    sealed ref int P
    {
        [UnscopedRef]
        get
        {
            throw null;
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (8,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                //         [UnscopedRef]
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(8, 10)
                );

            PropertySymbol propertySymbol = comp.GetMember<PropertySymbol>("I.P");
            Assert.False(propertySymbol.HasUnscopedRefAttribute);
            Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
        }

        [Fact]
        public void UnscopedRefInInterface_Property_08()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    abstract static ref int P
    {
        [UnscopedRef]
        get;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (8,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                //         [UnscopedRef]
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(8, 10)
                );

            PropertySymbol propertySymbol = comp.GetMember<PropertySymbol>("I.P");
            Assert.False(propertySymbol.HasUnscopedRefAttribute);
            Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
        }

        [Theory]
        [CombinatorialData]
        public void UnscopedRefInInterface_Indexer_01(bool isVirtual)
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    " + (isVirtual ? "virtual " : "") + @" ref int this[int i]  { get" + (isVirtual ? " => throw null" : "") + @"; }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: ExecutionConditionUtil.IsMonoOrCoreClr || !isVirtual ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
                Assert.True(propertySymbol.HasUnscopedRefAttribute);
                Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (6,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(6, 6)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Indexer_02()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    ref int this[int i] => throw null;
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
                Assert.True(propertySymbol.HasUnscopedRefAttribute);
                Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (6,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(6, 6)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Indexer_03()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    sealed ref int this[int i] => throw null;
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (6,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(6, 6)
                );

            PropertySymbol propertySymbol = comp.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
            Assert.False(propertySymbol.HasUnscopedRefAttribute);
            Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
        }

        [Fact]
        public void UnscopedRefInInterface_Indexer_04()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    abstract static ref int this[int i] { get; }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (7,29): error CS0106: The modifier 'static' is not valid for this item
                //     abstract static ref int this[int i] { get; }
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "this").WithArguments("static").WithLocation(7, 29)
                );

            PropertySymbol propertySymbol = comp.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
            Assert.False(propertySymbol.IsStatic);
            Assert.True(propertySymbol.HasUnscopedRefAttribute);
            Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
        }

        [Fact]
        public void UnscopedRefInInterface_Indexer_05()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    ref int this[int i]
    {
        [UnscopedRef]
        get;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
                Assert.False(propertySymbol.HasUnscopedRefAttribute);
                Assert.True(propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (8,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //         [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(8, 10)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Indexer_06()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    ref int this[int i]
    {
        [UnscopedRef]
        get
        {
            throw null;
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
                Assert.False(propertySymbol.HasUnscopedRefAttribute);
                Assert.True(propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (8,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //         [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(8, 10)
                );
        }

        [Fact]
        public void UnscopedRefInInterface_Indexer_07()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    sealed ref int this[int i]
    {
        [UnscopedRef]
        get
        {
            throw null;
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (8,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                //         [UnscopedRef]
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(8, 10)
                );

            PropertySymbol propertySymbol = comp.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
            Assert.False(propertySymbol.HasUnscopedRefAttribute);
            Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
        }

        [Fact]
        public void UnscopedRefInInterface_Indexer_08()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    abstract static ref int this[int i]
    {
        [UnscopedRef]
        get;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net80);

            comp.VerifyDiagnostics(
                // (6,29): error CS0106: The modifier 'static' is not valid for this item
                //     abstract static ref int this[int i]
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "this").WithArguments("static").WithLocation(6, 29)
                );

            PropertySymbol propertySymbol = comp.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
            Assert.False(propertySymbol.IsStatic);
            Assert.False(propertySymbol.HasUnscopedRefAttribute);
            Assert.True(propertySymbol.GetMethod.HasUnscopedRefAttribute);
        }

        [Fact]
        public void UnscopedRefInImplementation_Method_01()
        {
            var src1 = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    [UnscopedRef]
    ref int M();
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.Net80);
            MetadataReference[] comp1Refs = [comp1.EmitToImageReference(), comp1.ToMetadataReference()];

            var src2 = @"
using System.Diagnostics.CodeAnalysis;

class C : I
{
    [UnscopedRef]
    public ref int M()
    {
        throw null;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp2 = CreateCompilation(src2, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp2.VerifyDiagnostics(
                    // (6,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                    //     [UnscopedRef]
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(6, 6)
                    );
                Assert.False(comp2.GetMember<MethodSymbol>("C.M").HasUnscopedRefAttribute);
            }

            var src3 = @"
using System.Diagnostics.CodeAnalysis;

class C : I
{
    [UnscopedRef]
    ref int I.M()
    {
        throw null;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp3 = CreateCompilation(src3, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp3.VerifyDiagnostics(
                    // (6,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                    //     [UnscopedRef]
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(6, 6)
                    );
                Assert.False(comp3.GetMember<MethodSymbol>("C.I.M").HasUnscopedRefAttribute);
            }

            var src4 = @"
class C1 : I
{
    int f = 0;
    public ref int M()
    {
        return ref f;
    }
}

class C2 : I
{
    int f = 0;
    ref int I.M()
    {
        return ref f;
    }
}

class C3
{
    int f = 0;
    public ref int M()
    {
        return ref f;
    }
}

class C4 : C3, I {}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp4 = CreateCompilation(src4, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                CompileAndVerify(comp4, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                void verify(ModuleSymbol m)
                {
                    Assert.False(m.GlobalNamespace.GetMember<MethodSymbol>("C1.M").HasUnscopedRefAttribute);
                    Assert.False(m.GlobalNamespace.GetMember<MethodSymbol>("C2.I.M").HasUnscopedRefAttribute);
                    Assert.False(m.GlobalNamespace.GetMember<MethodSymbol>("C3.M").HasUnscopedRefAttribute);
                }
            }

            var src5 = @"
using System.Diagnostics.CodeAnalysis;

interface C : I
{
    [UnscopedRef]
    ref int I.M()
    {
        throw null;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp5 = CreateCompilation(src5, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp5.VerifyDiagnostics(
                    // (6,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                    //     [UnscopedRef]
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(6, 6)
                    );
                Assert.False(comp5.GetMember<MethodSymbol>("C.I.M").HasUnscopedRefAttribute);
            }

            var src6 = @"
interface C : I
{
    ref int I.M()
    {
        throw null;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp6 = CreateCompilation(src6, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                CompileAndVerify(comp6, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                void verify(ModuleSymbol m)
                {
                    Assert.False(m.GlobalNamespace.GetMember<MethodSymbol>("C.I.M").HasUnscopedRefAttribute);
                }
            }

            var src7 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

    [UnscopedRef]
    public ref int M()
    {
        return ref f;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp7 = CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                CompileAndVerify(comp7, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                void verify(ModuleSymbol m)
                {
                    Assert.True(m.GlobalNamespace.GetMember<MethodSymbol>("C.M").HasUnscopedRefAttribute);
                }

                CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

                CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                    // (8,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                    //     [UnscopedRef]
                    Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(8, 6)
                    );
            }

            var src8 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

    [UnscopedRef]
    ref int I.M()
    {
        return ref f;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp8 = CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                CompileAndVerify(comp8, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                void verify(ModuleSymbol m)
                {
                    Assert.True(m.GlobalNamespace.GetMember<MethodSymbol>("C.I.M").HasUnscopedRefAttribute);
                }

                CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

                CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                    // (8,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                    //     [UnscopedRef]
                    Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(8, 6)
                    );
            }

            var src9 = @"
public struct C : I
{
    public ref int M()
    {
        throw null;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp9 = CreateCompilation(src9, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                CompileAndVerify(comp9, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                void verify(ModuleSymbol m)
                {
                    Assert.False(m.GlobalNamespace.GetMember<MethodSymbol>("C.M").HasUnscopedRefAttribute);
                }
            }

            var src10 = @"
public struct C : I
{
    ref int I.M()
    {
        throw null;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp10 = CreateCompilation(src10, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                CompileAndVerify(comp10, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                void verify(ModuleSymbol m)
                {
                    Assert.False(m.GlobalNamespace.GetMember<MethodSymbol>("C.I.M").HasUnscopedRefAttribute);
                }
            }

            var src11 = @"
public struct C : I
{
    public int f;

    public ref int M()
    {
        return ref f;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp11 = CreateCompilation(src11, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp11.VerifyDiagnostics(
                    // (8,20): error CS8170: Struct members cannot return 'this' or other instance members by reference
                    //         return ref f;
                    Diagnostic(ErrorCode.ERR_RefReturnStructThis, "f").WithLocation(8, 20)
                    );
            }

            var src12 = @"
public struct C : I
{
    public int f;

    ref int I.M()
    {
        return ref f;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp12 = CreateCompilation(src12, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp12.VerifyDiagnostics(
                    // (8,20): error CS8170: Struct members cannot return 'this' or other instance members by reference
                    //         return ref f;
                    Diagnostic(ErrorCode.ERR_RefReturnStructThis, "f").WithLocation(8, 20)
                    );
            }
        }

        [Fact]
        public void UnscopedRefInImplementation_Method_02()
        {
            var src1 = @"
public interface I
{
    ref int M();
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.Net80);
            MetadataReference[] comp1Refs = [comp1.EmitToImageReference(), comp1.ToMetadataReference()];

            var src7 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

    [UnscopedRef]
    public ref int M()
    {
        return ref f;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp7 = CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp7.VerifyDiagnostics(
                    // (9,20): error CS9102: UnscopedRefAttribute cannot be applied to an interface implementation because implemented member 'I.M()' doesn't have this attribute.
                    //     public ref int M()
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeInterfaceImplementation, "M").WithArguments("I.M()").WithLocation(9, 20)
                    );

                Assert.True(comp7.GetMember<MethodSymbol>("C.M").HasUnscopedRefAttribute);
            }

            var src8 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

    [UnscopedRef]
    ref int I.M()
    {
        return ref f;
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp8 = CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp8.VerifyDiagnostics(
                    // (9,15): error CS9102: UnscopedRefAttribute cannot be applied to an interface implementation because implemented member 'I.M()' doesn't have this attribute.
                    //     ref int I.M()
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeInterfaceImplementation, "M").WithArguments("I.M()").WithLocation(9, 15)
                    );

                Assert.True(comp8.GetMember<MethodSymbol>("C.I.M").HasUnscopedRefAttribute);
            }
        }

        [Fact]
        public void UnscopedRefInImplementation_Method_03()
        {
            var src = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
#line 100
    [UnscopedRef]
    ref int M();
}

public struct C : I
{
    public int f;

#line 200
    [UnscopedRef]
    public ref int M()
    {
        return ref f;
    }
}
";

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (100,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     [UnscopedRef]
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(100, 6)
                );
        }

        [Theory]
        [CombinatorialData]
        public void UnscopedRefInImplementation_Property_01(bool onInterfaceProperty, bool onInterfaceGet, bool onImplementationProperty, bool onImplementationGet)
        {
            if (!onInterfaceProperty && !onInterfaceGet)
            {
                return;
            }

            var src1 = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    " + (onInterfaceProperty ? "[UnscopedRef]" : "") + @"
    ref int P { " + (onInterfaceGet ? "[UnscopedRef] " : "") + @"get; }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.Net80);

            var p = comp1.GetMember<PropertySymbol>("I.P");
            Assert.Equal(onInterfaceProperty, p.HasUnscopedRefAttribute);
            Assert.Equal(onInterfaceGet, p.GetMethod.HasUnscopedRefAttribute);

            MetadataReference[] comp1Refs = [comp1.EmitToImageReference(), comp1.ToMetadataReference()];

            if (onImplementationProperty || onImplementationGet)
            {
                var src2 = @"
using System.Diagnostics.CodeAnalysis;

class C : I
{
#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    public ref int P
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
            => throw null;
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp2 = CreateCompilation(src2, references: [comp1Ref], targetFramework: TargetFramework.Net80);

                    if (onImplementationProperty)
                    {
                        if (onImplementationGet)
                        {
                            comp2.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6),
                                // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //         [UnscopedRef] 
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                                );
                        }
                        else
                        {
                            comp2.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6)
                                );
                        }
                    }
                    else
                    {
                        comp2.VerifyDiagnostics(
                            // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                            );
                    }

                    PropertySymbol propertySymbol = comp2.GetMember<PropertySymbol>("C.P");
                    Assert.False(propertySymbol.HasUnscopedRefAttribute);
                    Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                }

                var src3 = @"
using System.Diagnostics.CodeAnalysis;

class C : I
{
#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    ref int I. P
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
            => throw null;
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp3 = CreateCompilation(src3, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    if (onImplementationProperty)
                    {
                        if (onImplementationGet)
                        {
                            comp3.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6),
                                // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //         [UnscopedRef] 
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                                );
                        }
                        else
                        {
                            comp3.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6)
                                );
                        }
                    }
                    else
                    {
                        comp3.VerifyDiagnostics(
                            // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                            );
                    }

                    PropertySymbol propertySymbol = comp3.GetMember<PropertySymbol>("C.I.P");
                    Assert.False(propertySymbol.HasUnscopedRefAttribute);
                    Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                }
            }

            if (!onImplementationProperty && !onImplementationGet)
            {
                var src4 = @"
class C1 : I
{
    int f = 0;
    public ref int P 
    { get{
        return ref f;
    }}
}

class C2 : I
{
    int f = 0;
    ref int I.P 
    { get{
        return ref f;
    }}
}

class C3
{
    int f = 0;
    public ref int P 
    { get{
        return ref f;
    }}
}

class C4 : C3, I {}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp4 = CreateCompilation(src4, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp4, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol c1P = m.GlobalNamespace.GetMember<PropertySymbol>("C1.P");
                        Assert.False(c1P.HasUnscopedRefAttribute);
                        Assert.False(c1P.GetMethod.HasUnscopedRefAttribute);
                        PropertySymbol c2P = m.GlobalNamespace.GetMember<PropertySymbol>("C2.I.P");
                        Assert.False(c2P.HasUnscopedRefAttribute);
                        Assert.False(c2P.GetMethod.HasUnscopedRefAttribute);
                        PropertySymbol c3P = m.GlobalNamespace.GetMember<PropertySymbol>("C3.P");
                        Assert.False(c3P.HasUnscopedRefAttribute);
                        Assert.False(c3P.GetMethod.HasUnscopedRefAttribute);
                    }
                }
            }

            if (onImplementationProperty || onImplementationGet)
            {
                var src5 = @"
using System.Diagnostics.CodeAnalysis;

interface C : I
{
#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    ref int I.P
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
            => throw null;
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp5 = CreateCompilation(src5, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    if (onImplementationProperty)
                    {
                        if (onImplementationGet)
                        {
                            comp5.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6),
                                // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //         [UnscopedRef] 
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                                );
                        }
                        else
                        {
                            comp5.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6)
                                );
                        }
                    }
                    else
                    {
                        comp5.VerifyDiagnostics(
                            // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                            );
                    }

                    PropertySymbol propertySymbol = comp5.GetMember<PropertySymbol>("C.I.P");
                    Assert.False(propertySymbol.HasUnscopedRefAttribute);
                    Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                }
            }

            if (!onImplementationProperty && !onImplementationGet)
            {
                var src6 = @"
interface C : I
{
    ref int I.P => throw null;
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp6 = CreateCompilation(src6, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp6, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C.I.P");
                        Assert.False(propertySymbol.HasUnscopedRefAttribute);
                        Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }
                }
            }

            if (onImplementationProperty || onImplementationGet)
            {
                var src7 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    public ref int P 
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
        {
            return ref f;
        }
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp7 = CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp7, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C.P");
                        Assert.Equal(onImplementationProperty, propertySymbol.HasUnscopedRefAttribute);
                        Assert.Equal(onImplementationGet, propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }

                    CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

                    comp7 = CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12);
                    if (onImplementationGet)
                    {
                        comp7.VerifyDiagnostics(
                            // (200,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(200, 10)
                            );
                    }
                    else
                    {
                        comp7.VerifyDiagnostics(
                            // (100,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                            //     [UnscopedRef]
                            Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(100, 6)
                            );
                    }
                }

                var src8 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;
#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    ref int I.P 
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
        {
            return ref f;
        }
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp8 = CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp8, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C.I.P");
                        Assert.Equal(onImplementationProperty, propertySymbol.HasUnscopedRefAttribute);
                        Assert.Equal(onImplementationGet, propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }

                    CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

                    comp8 = CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12);
                    if (onImplementationProperty)
                    {
                        if (onImplementationGet)
                        {
                            comp8.VerifyDiagnostics(
                                // (100,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(100, 6),
                                // (200,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                                //         [UnscopedRef] 
                                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(200, 10)
                                );
                        }
                        else
                        {
                            comp8.VerifyDiagnostics(
                                // (100,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(100, 6)
                                );
                        }
                    }
                    else
                    {
                        comp8.VerifyDiagnostics(
                            // (200,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(200, 10)
                            );
                    }
                }
            }

            if (!onImplementationProperty && !onImplementationGet)
            {
                var src9 = @"
public struct C : I
{
    public ref int P => throw null;
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp9 = CreateCompilation(src9, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp9, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C.P");
                        Assert.False(propertySymbol.HasUnscopedRefAttribute);
                        Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }
                }

                var src10 = @"
public struct C : I
{
    ref int I.P => throw null;
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp10 = CreateCompilation(src10, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp10, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C.I.P");
                        Assert.False(propertySymbol.HasUnscopedRefAttribute);
                        Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }
                }

                var src11 = @"
public struct C : I
{
    public int f;

    public ref int P 
    { get{
        return ref f;
    }}
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp11 = CreateCompilation(src11, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    comp11.VerifyDiagnostics(
                        // (8,20): error CS8170: Struct members cannot return 'this' or other instance members by reference
                        //         return ref f;
                        Diagnostic(ErrorCode.ERR_RefReturnStructThis, "f").WithLocation(8, 20)
                        );
                }

                var src12 = @"
public struct C : I
{
    public int f;

    ref int I.P 
    { get{
        return ref f;
    }}
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp12 = CreateCompilation(src12, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    comp12.VerifyDiagnostics(
                        // (8,20): error CS8170: Struct members cannot return 'this' or other instance members by reference
                        //         return ref f;
                        Diagnostic(ErrorCode.ERR_RefReturnStructThis, "f").WithLocation(8, 20)
                        );
                }
            }
        }

        [Theory]
        [CombinatorialData]
        public void UnscopedRefInImplementation_Property_02(bool onProperty, bool onGet)
        {
            if (!onProperty && !onGet)
            {
                return;
            }

            var src1 = @"
public interface I
{
    ref int P { get; }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.Net80);
            MetadataReference[] comp1Refs = [comp1.EmitToImageReference(), comp1.ToMetadataReference()];

            var src7 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

    " + (onProperty ? "[UnscopedRef]" : "") + @"
    public ref int P 
    {
#line 200
        " + (onGet ? "[UnscopedRef] " : "") + @"
        get
        {
            return ref f;
        }
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp7 = CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp7.VerifyDiagnostics(
                    // (201,9): error CS9102: UnscopedRefAttribute cannot be applied to an interface implementation because implemented member 'I.P.get' doesn't have this attribute.
                    //         get
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeInterfaceImplementation, "get").WithArguments("I.P.get").WithLocation(201, 9)
                    );

                PropertySymbol propertySymbol = comp7.GetMember<PropertySymbol>("C.P");
                Assert.Equal(onProperty, propertySymbol.HasUnscopedRefAttribute);
                Assert.Equal(onGet, propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            var src8 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

    " + (onProperty ? "[UnscopedRef]" : "") + @"
    ref int I.P 
    {
#line 200
        " + (onGet ? "[UnscopedRef] " : "") + @"
        get
        {
            return ref f;
        }
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp8 = CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp8.VerifyDiagnostics(
                    // (201,9): error CS9102: UnscopedRefAttribute cannot be applied to an interface implementation because implemented member 'I.P.get' doesn't have this attribute.
                    //         get
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeInterfaceImplementation, "get").WithArguments("I.P.get").WithLocation(201, 9)
                    );

                PropertySymbol propertySymbol = comp8.GetMember<PropertySymbol>("C.I.P");
                Assert.Equal(onProperty, propertySymbol.HasUnscopedRefAttribute);
                Assert.Equal(onGet, propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }
        }

        [Theory]
        [CombinatorialData]
        public void UnscopedRefInImplementation_Indexer_01(bool onInterfaceProperty, bool onInterfaceGet, bool onImplementationProperty, bool onImplementationGet)
        {
            if (!onInterfaceProperty && !onInterfaceGet)
            {
                return;
            }

            var src1 = @"
using System.Diagnostics.CodeAnalysis;

public interface I
{
    " + (onInterfaceProperty ? "[UnscopedRef]" : "") + @"
    ref int this[int i] { " + (onInterfaceGet ? "[UnscopedRef] " : "") + @"get; }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.Net80);

            var p = comp1.GetMember<PropertySymbol>("I." + WellKnownMemberNames.Indexer);
            Assert.Equal(onInterfaceProperty, p.HasUnscopedRefAttribute);
            Assert.Equal(onInterfaceGet, p.GetMethod.HasUnscopedRefAttribute);

            MetadataReference[] comp1Refs = [comp1.EmitToImageReference(), comp1.ToMetadataReference()];

            if (onImplementationProperty || onImplementationGet)
            {
                var src2 = @"
using System.Diagnostics.CodeAnalysis;

class C : I
{
#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    public ref int this[int i]
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
            => throw null;
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp2 = CreateCompilation(src2, references: [comp1Ref], targetFramework: TargetFramework.Net80);

                    if (onImplementationProperty)
                    {
                        if (onImplementationGet)
                        {
                            comp2.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6),
                                // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //         [UnscopedRef] 
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                                );
                        }
                        else
                        {
                            comp2.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6)
                                );
                        }
                    }
                    else
                    {
                        comp2.VerifyDiagnostics(
                            // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                            );
                    }

                    PropertySymbol propertySymbol = comp2.GetMember<PropertySymbol>("C." + WellKnownMemberNames.Indexer);
                    Assert.False(propertySymbol.HasUnscopedRefAttribute);
                    Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                }

                var src3 = @"
using System.Diagnostics.CodeAnalysis;

class C : I
{
#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    ref int I. this[int i]
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
            => throw null;
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp3 = CreateCompilation(src3, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    if (onImplementationProperty)
                    {
                        if (onImplementationGet)
                        {
                            comp3.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6),
                                // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //         [UnscopedRef] 
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                                );
                        }
                        else
                        {
                            comp3.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6)
                                );
                        }
                    }
                    else
                    {
                        comp3.VerifyDiagnostics(
                            // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                            );
                    }

                    PropertySymbol propertySymbol = comp3.GetMember<PropertySymbol>("C.I." + WellKnownMemberNames.Indexer);
                    Assert.False(propertySymbol.HasUnscopedRefAttribute);
                    Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                }
            }

            if (!onImplementationProperty && !onImplementationGet)
            {
                var src4 = @"
class C1 : I
{
    int f = 0;
    public ref int this[int i] 
    { get{
        return ref f;
    }}
}

class C2 : I
{
    int f = 0;
    ref int I.this[int i] 
    { get{
        return ref f;
    }}
}

class C3
{
    int f = 0;
    public ref int this[int i] 
    { get{
        return ref f;
    }}
}

class C4 : C3, I {}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp4 = CreateCompilation(src4, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp4, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol c1P = m.GlobalNamespace.GetMember<PropertySymbol>("C1." + WellKnownMemberNames.Indexer);
                        Assert.False(c1P.HasUnscopedRefAttribute);
                        Assert.False(c1P.GetMethod.HasUnscopedRefAttribute);
                        PropertySymbol c2P = m.GlobalNamespace.GetMember<PropertySymbol>("C2.I." + (m is PEModuleSymbol ? "Item" : WellKnownMemberNames.Indexer));
                        Assert.False(c2P.HasUnscopedRefAttribute);
                        Assert.False(c2P.GetMethod.HasUnscopedRefAttribute);
                        PropertySymbol c3P = m.GlobalNamespace.GetMember<PropertySymbol>("C3." + WellKnownMemberNames.Indexer);
                        Assert.False(c3P.HasUnscopedRefAttribute);
                        Assert.False(c3P.GetMethod.HasUnscopedRefAttribute);
                    }
                }
            }

            if (onImplementationProperty || onImplementationGet)
            {
                var src5 = @"
using System.Diagnostics.CodeAnalysis;

interface C : I
{
#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    ref int I.this[int i]
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
            => throw null;
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp5 = CreateCompilation(src5, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    if (onImplementationProperty)
                    {
                        if (onImplementationGet)
                        {
                            comp5.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6),
                                // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //         [UnscopedRef] 
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                                );
                        }
                        else
                        {
                            comp5.VerifyDiagnostics(
                                // (100,6): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(100, 6)
                                );
                        }
                    }
                    else
                    {
                        comp5.VerifyDiagnostics(
                            // (200,10): error CS9101: UnscopedRefAttribute can only be applied to struct or virtual interface instance methods and properties, and cannot be applied to constructors or init-only members.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedMemberTarget, "UnscopedRef").WithLocation(200, 10)
                            );
                    }

                    PropertySymbol propertySymbol = comp5.GetMember<PropertySymbol>("C.I." + WellKnownMemberNames.Indexer);
                    Assert.False(propertySymbol.HasUnscopedRefAttribute);
                    Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                }
            }

            if (!onImplementationProperty && !onImplementationGet)
            {
                var src6 = @"
interface C : I
{
    ref int I.this[int i] => throw null;
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp6 = CreateCompilation(src6, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp6, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C.I." + (m is PEModuleSymbol ? "Item" : WellKnownMemberNames.Indexer));
                        Assert.False(propertySymbol.HasUnscopedRefAttribute);
                        Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }
                }
            }

            if (onImplementationProperty || onImplementationGet)
            {
                var src7 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    public ref int this[int i] 
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
        {
            return ref f;
        }
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp7 = CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp7, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C." + WellKnownMemberNames.Indexer);
                        Assert.Equal(onImplementationProperty, propertySymbol.HasUnscopedRefAttribute);
                        Assert.Equal(onImplementationGet, propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }

                    CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

                    comp7 = CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12);
                    if (onImplementationGet)
                    {
                        comp7.VerifyDiagnostics(
                            // (200,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(200, 10)
                            );
                    }
                    else
                    {
                        comp7.VerifyDiagnostics(
                            // (100,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                            //     [UnscopedRef]
                            Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(100, 6)
                            );
                    }
                }

                var src8 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;
#line 100
    " + (onImplementationProperty ? "[UnscopedRef]" : "") + @"
    ref int I.this[int i] 
    {
#line 200
        " + (onImplementationGet ? "[UnscopedRef] " : "") + @"
        get
        {
            return ref f;
        }
    }
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp8 = CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp8, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C.I." + (m is PEModuleSymbol ? "Item" : WellKnownMemberNames.Indexer));
                        Assert.Equal(onImplementationProperty, propertySymbol.HasUnscopedRefAttribute);
                        Assert.Equal(onImplementationGet, propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }

                    CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

                    comp8 = CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12);
                    if (onImplementationProperty)
                    {
                        if (onImplementationGet)
                        {
                            comp8.VerifyDiagnostics(
                                // (100,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(100, 6),
                                // (200,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                                //         [UnscopedRef] 
                                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(200, 10)
                                );
                        }
                        else
                        {
                            comp8.VerifyDiagnostics(
                                // (100,6): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                                //     [UnscopedRef]
                                Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(100, 6)
                                );
                        }
                    }
                    else
                    {
                        comp8.VerifyDiagnostics(
                            // (200,10): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                            //         [UnscopedRef] 
                            Diagnostic(ErrorCode.ERR_FeatureInPreview, "UnscopedRef").WithArguments("ref struct interfaces").WithLocation(200, 10)
                            );
                    }
                }
            }

            if (!onImplementationProperty && !onImplementationGet)
            {
                var src9 = @"
public struct C : I
{
    public ref int this[int i] => throw null;
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp9 = CreateCompilation(src9, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp9, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C." + WellKnownMemberNames.Indexer);
                        Assert.False(propertySymbol.HasUnscopedRefAttribute);
                        Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }
                }

                var src10 = @"
public struct C : I
{
    ref int I.this[int i] => throw null;
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp10 = CreateCompilation(src10, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    CompileAndVerify(comp10, sourceSymbolValidator: verify, symbolValidator: verify, verify: Verification.Skipped).VerifyDiagnostics();

                    void verify(ModuleSymbol m)
                    {
                        PropertySymbol propertySymbol = m.GlobalNamespace.GetMember<PropertySymbol>("C.I." + (m is PEModuleSymbol ? "Item" : WellKnownMemberNames.Indexer));
                        Assert.False(propertySymbol.HasUnscopedRefAttribute);
                        Assert.False(propertySymbol.GetMethod.HasUnscopedRefAttribute);
                    }
                }

                var src11 = @"
public struct C : I
{
    public int f;

    public ref int this[int i] 
    { get{
        return ref f;
    }}
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp11 = CreateCompilation(src11, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    comp11.VerifyDiagnostics(
                        // (8,20): error CS8170: Struct members cannot return 'this' or other instance members by reference
                        //         return ref f;
                        Diagnostic(ErrorCode.ERR_RefReturnStructThis, "f").WithLocation(8, 20)
                        );
                }

                var src12 = @"
public struct C : I
{
    public int f;

    ref int I.this[int i] 
    { get{
        return ref f;
    }}
}
";

                foreach (var comp1Ref in comp1Refs)
                {
                    var comp12 = CreateCompilation(src12, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                    comp12.VerifyDiagnostics(
                        // (8,20): error CS8170: Struct members cannot return 'this' or other instance members by reference
                        //         return ref f;
                        Diagnostic(ErrorCode.ERR_RefReturnStructThis, "f").WithLocation(8, 20)
                        );
                }
            }
        }

        [Theory]
        [CombinatorialData]
        public void UnscopedRefInImplementation_Indexer_02(bool onProperty, bool onGet)
        {
            if (!onProperty && !onGet)
            {
                return;
            }

            var src1 = @"
public interface I
{
    ref int this[int i] { get; }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.Net80);
            MetadataReference[] comp1Refs = [comp1.EmitToImageReference(), comp1.ToMetadataReference()];

            var src7 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

    " + (onProperty ? "[UnscopedRef]" : "") + @"
    public ref int this[int i] 
    {
#line 200
        " + (onGet ? "[UnscopedRef] " : "") + @"
        get
        {
            return ref f;
        }
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp7 = CreateCompilation(src7, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp7.VerifyDiagnostics(
                    // (201,9): error CS9102: UnscopedRefAttribute cannot be applied to an interface implementation because implemented member 'I.P.get' doesn't have this attribute.
                    //         get
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeInterfaceImplementation, "get").WithArguments("I.this[int].get").WithLocation(201, 9)
                    );

                PropertySymbol propertySymbol = comp7.GetMember<PropertySymbol>("C." + WellKnownMemberNames.Indexer);
                Assert.Equal(onProperty, propertySymbol.HasUnscopedRefAttribute);
                Assert.Equal(onGet, propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }

            var src8 = @"
using System.Diagnostics.CodeAnalysis;

public struct C : I
{
    public int f;

    " + (onProperty ? "[UnscopedRef]" : "") + @"
    ref int I.this[int i] 
    {
#line 200
        " + (onGet ? "[UnscopedRef] " : "") + @"
        get
        {
            return ref f;
        }
    }
}
";

            foreach (var comp1Ref in comp1Refs)
            {
                var comp8 = CreateCompilation(src8, references: [comp1Ref], targetFramework: TargetFramework.Net80);
                comp8.VerifyDiagnostics(
                    // (201,9): error CS9102: UnscopedRefAttribute cannot be applied to an interface implementation because implemented member 'I.P.get' doesn't have this attribute.
                    //         get
                    Diagnostic(ErrorCode.ERR_UnscopedRefAttributeInterfaceImplementation, "get").WithArguments("I.this[int].get").WithLocation(201, 9)
                    );

                PropertySymbol propertySymbol = comp8.GetMember<PropertySymbol>("C.I." + WellKnownMemberNames.Indexer);
                Assert.Equal(onProperty, propertySymbol.HasUnscopedRefAttribute);
                Assert.Equal(onGet, propertySymbol.GetMethod.HasUnscopedRefAttribute);
            }
        }

        // This is a clone of MethodArgumentsMustMatch_16 from RefFieldTests.cs
        [Fact]
        public void MethodArgumentsMustMatch_16_DirectInterface()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;
                interface R
                {
                    public ref int FA();
                    [UnscopedRef] public ref int FB();
                }
                class Program
                {
                    static void F1(ref R r1, ref int i1) { }
                    static void F2(ref R r2, [UnscopedRef] ref int i2) { }
                    static void F(ref R x)
                    {
                        R y = default;
                        F1(ref x, ref y.FA());
                        F1(ref x, ref y.FB());
                        F2(ref x, ref y.FA());
                        F2(ref x, ref y.FB()); // 1
                    }
                }
                """;
            var comp = CreateCompilation(new[] { source, UnscopedRefAttributeDefinition });
            comp.VerifyDiagnostics();
        }

        // This is a clone of MethodArgumentsMustMatch_16 from RefFieldTests.cs
        [Fact]
        public void MethodArgumentsMustMatch_16_ConstrainedTypeParameter()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;
                interface R
                {
                    public ref int FA();
                    [UnscopedRef] public ref int FB();
                }
                class Program<T> where T : R, allows ref struct
                {
                    static void F1(ref T r1, ref int i1) { }
                    static void F2(ref T r2, [UnscopedRef] ref int i2) { }
                    static void F(ref T x)
                    {
                        T y = default;
                        F1(ref x, ref y.FA());
                        F1(ref x, ref y.FB());
                        F2(ref x, ref y.FA());
                        F2(ref x, ref y.FB()); // 1
                    }
                }
                """;
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (17,9): error CS8350: This combination of arguments to 'Program<T>.F2(ref T, ref int)' is disallowed because it may expose variables referenced by parameter 'i2' outside of their declaration scope
                //         F2(ref x, ref y.FB()); // 1
                Diagnostic(ErrorCode.ERR_CallArgMixing, "F2(ref x, ref y.FB())").WithArguments("Program<T>.F2(ref T, ref int)", "i2").WithLocation(17, 9),
                // (17,23): error CS8168: Cannot return local 'y' by reference because it is not a ref local
                //         F2(ref x, ref y.FB()); // 1
                Diagnostic(ErrorCode.ERR_RefReturnLocal, "y").WithArguments("y").WithLocation(17, 23));
        }

        // This is a clone of MethodArgumentsMustMatch_16 from RefFieldTests.cs
        [Fact]
        public void MethodArgumentsMustMatch_16_ClassConstrainedTypeParameter()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;
                interface R
                {
                    public ref int FA();
                    [UnscopedRef] public ref int FB();
                }
                class Program<T> where T : class, R
                {
                    static void F1(ref T r1, ref int i1) { }
                    static void F2(ref T r2, [UnscopedRef] ref int i2) { }
                    static void F(ref T x)
                    {
                        T y = default;
                        F1(ref x, ref y.FA());
                        F1(ref x, ref y.FB());
                        F2(ref x, ref y.FA());
                        F2(ref x, ref y.FB()); // 1
                    }
                }
                """;
            var comp = CreateCompilation(new[] { source, UnscopedRefAttributeDefinition });
            comp.VerifyDiagnostics();
        }

        // This is a clone of MethodArgumentsMustMatch_17 from RefFieldTests.cs
        [Fact]
        public void MethodArgumentsMustMatch_17()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;
                interface IR
                {
                    public ref int FA();
                    [UnscopedRef] public ref int FB();
                }
                class Program<R> where R : IR, allows ref struct
                {
                    static void F1(ref R r1, in int i1) { }
                    static void F2(ref R r2, [UnscopedRef] in int i2) { }
                    static void F(ref R x)
                    {
                        R y = default;
                        F1(ref x, y.FA());
                        F1(ref x, y.FB());
                        F2(ref x, y.FA());
                        F2(ref x, y.FB()); // 1
                        F1(ref x, in y.FA());
                        F1(ref x, in y.FB());
                        F2(ref x, in y.FA());
                        F2(ref x, in y.FB()); // 2
                    }
                }
                """;
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (17,9): error CS8350: This combination of arguments to 'Program<R>.F2(ref R, in int)' is disallowed because it may expose variables referenced by parameter 'i2' outside of their declaration scope
                //         F2(ref x, y.FB()); // 1
                Diagnostic(ErrorCode.ERR_CallArgMixing, "F2(ref x, y.FB())").WithArguments("Program<R>.F2(ref R, in int)", "i2").WithLocation(17, 9),
                // (17,19): error CS8168: Cannot return local 'y' by reference because it is not a ref local
                //         F2(ref x, y.FB()); // 1
                Diagnostic(ErrorCode.ERR_RefReturnLocal, "y").WithArguments("y").WithLocation(17, 19),
                // (21,9): error CS8350: This combination of arguments to 'Program<R>.F2(ref R, in int)' is disallowed because it may expose variables referenced by parameter 'i2' outside of their declaration scope
                //         F2(ref x, in y.FB()); // 2
                Diagnostic(ErrorCode.ERR_CallArgMixing, "F2(ref x, in y.FB())").WithArguments("Program<R>.F2(ref R, in int)", "i2").WithLocation(21, 9),
                // (21,22): error CS8168: Cannot return local 'y' by reference because it is not a ref local
                //         F2(ref x, in y.FB()); // 2
                Diagnostic(ErrorCode.ERR_RefReturnLocal, "y").WithArguments("y").WithLocation(21, 22));
        }

        // This is a clone of MethodArgumentsMustMatch_18 from RefFieldTests.cs
        [Fact]
        public void MethodArgumentsMustMatch_18()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;
                interface IR
                {
                    public ref readonly int FA();
                    [UnscopedRef] public ref readonly int FB();
                }
                class Program<R> where R : IR, allows ref struct
                {
                    static void F1(ref R r1, in int i1) { }
                    static void F2(ref R r2, [UnscopedRef] in int i2) { }
                    static void F(ref R x)
                    {
                        R y = default;
                        F1(ref x, y.FA());
                        F1(ref x, y.FB());
                        F2(ref x, y.FA());
                        F2(ref x, y.FB()); // 1
                        F1(ref x, in y.FA());
                        F1(ref x, in y.FB());
                        F2(ref x, in y.FA());
                        F2(ref x, in y.FB()); // 2
                    }
                }
                """;
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (17,9): error CS8350: This combination of arguments to 'Program<R>.F2(ref R, in int)' is disallowed because it may expose variables referenced by parameter 'i2' outside of their declaration scope
                //         F2(ref x, y.FB()); // 1
                Diagnostic(ErrorCode.ERR_CallArgMixing, "F2(ref x, y.FB())").WithArguments("Program<R>.F2(ref R, in int)", "i2").WithLocation(17, 9),
                // (17,19): error CS8168: Cannot return local 'y' by reference because it is not a ref local
                //         F2(ref x, y.FB()); // 1
                Diagnostic(ErrorCode.ERR_RefReturnLocal, "y").WithArguments("y").WithLocation(17, 19),
                // (21,9): error CS8350: This combination of arguments to 'Program<R>.F2(ref R, in int)' is disallowed because it may expose variables referenced by parameter 'i2' outside of their declaration scope
                //         F2(ref x, in y.FB()); // 2
                Diagnostic(ErrorCode.ERR_CallArgMixing, "F2(ref x, in y.FB())").WithArguments("Program<R>.F2(ref R, in int)", "i2").WithLocation(21, 9),
                // (21,22): error CS8168: Cannot return local 'y' by reference because it is not a ref local
                //         F2(ref x, in y.FB()); // 2
                Diagnostic(ErrorCode.ERR_RefReturnLocal, "y").WithArguments("y").WithLocation(21, 22));
        }

        // This is a clone of ReturnOnlyScope_01 from RefFieldTests.cs
        [Fact]
        public void ReturnOnlyScope_01()
        {
            // test that return scope is used in all return-ey locations.
            var source = """
                using System.Diagnostics.CodeAnalysis;

                interface IRS<RSOut> where RSOut : IRSOut, allows ref struct
                {
                    [UnscopedRef]
                    public RSOut ToRSOut();
                }

                interface IRSOut
                {
                }

                class Program<RS, RSOut> where RS : IRS<RSOut>, allows ref struct where RSOut : IRSOut, allows ref struct
                {
                    RS M1(ref RS rs) => rs;
                    void M2(ref RS rs, out RSOut rs1) => rs1 = rs.ToRSOut();

                    RS M3(ref RS rs)
                    {
                        return rs;
                    }
                    void M4(ref RS rs, out RSOut rs1)
                    {
                        rs1 = rs.ToRSOut();
                    }

                    void localContainer()
                    {
                #pragma warning disable 8321
                        RS M1(ref RS rs) => rs;
                        void M2(ref RS rs, out RSOut rs1) => rs1 = rs.ToRSOut();

                        RS M3(ref RS rs)
                        {
                            return rs;
                        }
                        void M4(ref RS rs, out RSOut rs1)
                        {
                            rs1 = rs.ToRSOut(); // 4
                        }
                    }

                    delegate RS ReturnsRefStruct(ref RS rs);
                    delegate void RefStructOut(ref RS rs, out RSOut rs1);

                    void lambdaContainer()
                    {
                        ReturnsRefStruct d1 = (ref RS rs) => rs;
                        RefStructOut d2 = (ref RS rs, out RSOut rs1) => rs1 = rs.ToRSOut();

                        ReturnsRefStruct d3 = (ref RS rs) =>
                        {
                            return rs;
                        };
                        RefStructOut d4 = (ref RS rs, out RSOut rs1) =>
                        {
                            rs1 = rs.ToRSOut();
                        };
                    }
                }
                """;

            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics();

            Assert.True(comp.SupportsRuntimeCapability(RuntimeCapability.ByRefLikeGenerics));
        }

        // This is a clone of ReturnRefToRefStruct_ValEscape_01 from RefFieldTests.cs
        [Fact]
        public void ReturnRefToRefStruct_ValEscape_01()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;

                class Repro<TRefStruct> where TRefStruct : IRefStruct, new() , allows ref struct
                {
                    private static void Bad2(int value)
                    {
                        TRefStruct s1 = new TRefStruct();
                        s1.RefProperty.RefField = ref value; // 2
                    }

                    private static void Bad3(int value)
                    {
                        TRefStruct s1 = new TRefStruct();
                        s1.RefMethod().RefField = ref value; // 3
                    }
                }
                
                ref struct RefStruct
                {
                    public ref int RefField;
                }
                
                interface IRefStruct
                {
                    [UnscopedRef] public ref RefStruct RefProperty {get;}
                    [UnscopedRef] public ref RefStruct RefMethod();
                }
                """;
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (8,9): error CS8374: Cannot ref-assign 'value' to 'RefField' because 'value' has a narrower escape scope than 'RefField'.
                //         s1.RefProperty.RefField = ref value; // 2
                Diagnostic(ErrorCode.ERR_RefAssignNarrower, "s1.RefProperty.RefField = ref value").WithArguments("RefField", "value").WithLocation(8, 9),
                // (14,9): error CS8374: Cannot ref-assign 'value' to 'RefField' because 'value' has a narrower escape scope than 'RefField'.
                //         s1.RefMethod().RefField = ref value; // 3
                Diagnostic(ErrorCode.ERR_RefAssignNarrower, "s1.RefMethod().RefField = ref value").WithArguments("RefField", "value").WithLocation(14, 9));
        }

        // This is a clone of ReturnRefToRefStruct_ValEscape_02 from RefFieldTests.cs
        [Fact]
        public void ReturnRefToRefStruct_ValEscape_02()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;

                class Repro<TRefStruct> where TRefStruct : IRefStruct, allows ref struct
                {
                    private static void Bad2(scoped ref TRefStruct s1, int value)
                    {
                        s1.RefProperty.RefField = ref value; // 2
                    }

                    private static void Bad3(scoped ref TRefStruct s1, int value)
                    {
                        s1.RefMethod().RefField = ref value; // 3
                    }

                    private static void Bad5(scoped in TRefStruct s1, int value)
                    {
                        s1.RefProperty.RefField = ref value; // 5
                    }

                    private static void Bad6(scoped in TRefStruct s1, int value)
                    {
                        s1.RefMethod().RefField = ref value; // 6
                    }

                    private static void Bad8(in TRefStruct s1, int value)
                    {
                        s1.RefProperty.RefField = ref value; // 8
                    }

                    private static void Bad9(in TRefStruct s1, int value)
                    {
                        s1.RefMethod().RefField = ref value; // 9
                    }
                }
                
                ref struct RefStruct
                {
                    public ref int RefField;
                }
                
                interface IRefStruct
                {
                    [UnscopedRef] public ref RefStruct RefProperty {get;}
                    [UnscopedRef] public ref RefStruct RefMethod();
                }
                """;

            // NB: 8 and 9 are not strictly necessary here because they are assigning to an implicit copy of a readonly variable, not to the original variable.
            // However, it is not deeply problematic that an error is given here.
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,9): error CS8374: Cannot ref-assign 'value' to 'RefField' because 'value' has a narrower escape scope than 'RefField'.
                //         s1.RefProperty.RefField = ref value; // 2
                Diagnostic(ErrorCode.ERR_RefAssignNarrower, "s1.RefProperty.RefField = ref value").WithArguments("RefField", "value").WithLocation(7, 9),
                // (12,9): error CS8374: Cannot ref-assign 'value' to 'RefField' because 'value' has a narrower escape scope than 'RefField'.
                //         s1.RefMethod().RefField = ref value; // 3
                Diagnostic(ErrorCode.ERR_RefAssignNarrower, "s1.RefMethod().RefField = ref value").WithArguments("RefField", "value").WithLocation(12, 9),
                // (17,9): error CS8374: Cannot ref-assign 'value' to 'RefField' because 'value' has a narrower escape scope than 'RefField'.
                //         s1.RefProperty.RefField = ref value; // 5
                Diagnostic(ErrorCode.ERR_RefAssignNarrower, "s1.RefProperty.RefField = ref value").WithArguments("RefField", "value").WithLocation(17, 9),
                // (22,9): error CS8374: Cannot ref-assign 'value' to 'RefField' because 'value' has a narrower escape scope than 'RefField'.
                //         s1.RefMethod().RefField = ref value; // 6
                Diagnostic(ErrorCode.ERR_RefAssignNarrower, "s1.RefMethod().RefField = ref value").WithArguments("RefField", "value").WithLocation(22, 9),
                // (27,9): error CS8374: Cannot ref-assign 'value' to 'RefField' because 'value' has a narrower escape scope than 'RefField'.
                //         s1.RefProperty.RefField = ref value; // 8
                Diagnostic(ErrorCode.ERR_RefAssignNarrower, "s1.RefProperty.RefField = ref value").WithArguments("RefField", "value").WithLocation(27, 9),
                // (32,9): error CS8374: Cannot ref-assign 'value' to 'RefField' because 'value' has a narrower escape scope than 'RefField'.
                //         s1.RefMethod().RefField = ref value; // 9
                Diagnostic(ErrorCode.ERR_RefAssignNarrower, "s1.RefMethod().RefField = ref value").WithArguments("RefField", "value").WithLocation(32, 9)
                );
        }

        // This is a clone of ReturnRefToRefStruct_ValEscape_03 from RefFieldTests.cs
        [Fact]
        public void ReturnRefToRefStruct_ValEscape_03()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;

                class Repro<TRefStruct> where TRefStruct : IRefStruct<TRefStruct>, allows ref struct
                {
                    private static void Bad1(ref TRefStruct s1, int value)
                    {
                        s1 = TRefStruct.New(ref value); // 1
                    }

                    private static void Bad2(scoped ref TRefStruct s1, int value)
                    {
                        s1.RefProperty = TRefStruct.New(ref value); // 2
                    }

                    private static void Bad3(scoped ref TRefStruct s1, int value)
                    {
                        s1.RefMethod() = TRefStruct.New(ref value); // 3
                    }

                    private static void Bad4(scoped ref TRefStruct s1, int value)
                    {
                        GetRef(ref s1) = TRefStruct.New(ref value); // 4
                    }

                    private static ref TRefStruct GetRef(ref TRefStruct s) => ref s;
                }
                
                interface IRefStruct<TRefStruct> where TRefStruct : IRefStruct<TRefStruct>, allows ref struct
                {
                    abstract static TRefStruct New(ref int i);
                    [UnscopedRef] public ref TRefStruct RefProperty {get;}
                    [UnscopedRef] public ref TRefStruct RefMethod();
                }
                """;
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,14): error CS8347: Cannot use a result of 'IRefStruct<TRefStruct>.New(ref int)' in this context because it may expose variables referenced by parameter 'i' outside of their declaration scope
                //         s1 = TRefStruct.New(ref value); // 1
                Diagnostic(ErrorCode.ERR_EscapeCall, "TRefStruct.New(ref value)").WithArguments("IRefStruct<TRefStruct>.New(ref int)", "i").WithLocation(7, 14),
                // (7,33): error CS8166: Cannot return a parameter by reference 'value' because it is not a ref parameter
                //         s1 = TRefStruct.New(ref value); // 1
                Diagnostic(ErrorCode.ERR_RefReturnParameter, "value").WithArguments("value").WithLocation(7, 33),
                // (12,26): error CS8347: Cannot use a result of 'IRefStruct<TRefStruct>.New(ref int)' in this context because it may expose variables referenced by parameter 'i' outside of their declaration scope
                //         s1.RefProperty = TRefStruct.New(ref value); // 2
                Diagnostic(ErrorCode.ERR_EscapeCall, "TRefStruct.New(ref value)").WithArguments("IRefStruct<TRefStruct>.New(ref int)", "i").WithLocation(12, 26),
                // (12,45): error CS8166: Cannot return a parameter by reference 'value' because it is not a ref parameter
                //         s1.RefProperty = TRefStruct.New(ref value); // 2
                Diagnostic(ErrorCode.ERR_RefReturnParameter, "value").WithArguments("value").WithLocation(12, 45),
                // (17,26): error CS8347: Cannot use a result of 'IRefStruct<TRefStruct>.New(ref int)' in this context because it may expose variables referenced by parameter 'i' outside of their declaration scope
                //         s1.RefMethod() = TRefStruct.New(ref value); // 3
                Diagnostic(ErrorCode.ERR_EscapeCall, "TRefStruct.New(ref value)").WithArguments("IRefStruct<TRefStruct>.New(ref int)", "i").WithLocation(17, 26),
                // (17,45): error CS8166: Cannot return a parameter by reference 'value' because it is not a ref parameter
                //         s1.RefMethod() = TRefStruct.New(ref value); // 3
                Diagnostic(ErrorCode.ERR_RefReturnParameter, "value").WithArguments("value").WithLocation(17, 45),
                // (22,26): error CS8347: Cannot use a result of 'IRefStruct<TRefStruct>.New(ref int)' in this context because it may expose variables referenced by parameter 'i' outside of their declaration scope
                //         GetRef(ref s1) = TRefStruct.New(ref value); // 4
                Diagnostic(ErrorCode.ERR_EscapeCall, "TRefStruct.New(ref value)").WithArguments("IRefStruct<TRefStruct>.New(ref int)", "i").WithLocation(22, 26),
                // (22,45): error CS8166: Cannot return a parameter by reference 'value' because it is not a ref parameter
                //         GetRef(ref s1) = TRefStruct.New(ref value); // 4
                Diagnostic(ErrorCode.ERR_RefReturnParameter, "value").WithArguments("value").WithLocation(22, 45)
                );
        }

        // This is a clone of ReturnRefToRefStruct_ValEscape_04 from RefFieldTests.cs
        [Fact]
        public void ReturnRefToRefStruct_ValEscape_04()
        {
            // test that the appropriate filtering of escape-values is occurring when the RTRS expression is on the RHS of an an assignment.
            var source = """
                using System.Diagnostics.CodeAnalysis;

                class Repro<TRefStruct> where TRefStruct : IRefStruct<TRefStruct>, allows ref struct
                {
                    private static void M1(ref TRefStruct s1, int value)
                    {
                        // 's2' only contributes STE, not RSTE, to the STE of 'RefMethod()' invocation.
                        // STE is equal to RSTE for 's2', so it doesn't matter.
                        var s2 = TRefStruct.New(ref value);
                        s1 = s2.RefMethod(); // 1
                    }
                    
                    private static void M2(ref TRefStruct s1, ref TRefStruct s2)
                    {
                        // 's2' only contributes STE, not RSTE, to the STE of 'RefMethod()' invocation.
                        // RSTE of `s2` is narrower than STE of 's1', but STE of 's2' equals STE of 's1', so we expect no error here.
                        s1 = s2.RefMethod();
                    }
                }
                
                interface IRefStruct<TRefStruct> where TRefStruct : IRefStruct<TRefStruct>, allows ref struct
                {
                    abstract static TRefStruct New(ref int i);
                    [UnscopedRef] public ref TRefStruct RefMethod();
                }
                """;
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (10,14): error CS8352: Cannot use variable 's2' in this context because it may expose referenced variables outside of their declaration scope
                //         s1 = s2.RefMethod(); // 1
                Diagnostic(ErrorCode.ERR_EscapeVariable, "s2").WithArguments("s2").WithLocation(10, 14));
        }

        // This is a clone of LocalScope_DeclarationExpression_06 from RefEscapingTests.cs
        [Fact]
        public void LocalScope_DeclarationExpression_06()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;

                interface IRS<RS> where RS : IRS<RS>, allows ref struct
                {
                    [UnscopedRef]
                    void M0(out RS rs2);

                    RS M1()
                    {
                        // RSTE of `this` is CurrentMethod
                        // STE of rs4 (local variable) is also CurrentMethod
                        M0(out var rs4);
                        return rs4; // 1
                    }

                    [UnscopedRef]
                    RS M2()
                    {
                        M0(out var rs4);
                        return rs4;
                    }
                }
                """;

            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics();
        }

        // This is a clone of UnscopedRefAttribute_Method_03 from RefFieldTests.cs
        [CombinatorialData]
        [Theory]
        public void UnscopedRefAttribute_Method_03_DirectInterface(bool useCompilationReference)
        {
            var sourceA =
@"using System.Diagnostics.CodeAnalysis;
public interface S<T>
{
    public ref T F1();
    [UnscopedRef] public ref T F2();
}";
            var comp = CreateCompilation(new[] { sourceA, UnscopedRefAttributeDefinition });
            comp.VerifyEmitDiagnostics();
            var refA = AsReference(comp, useCompilationReference);

            var sourceB =
@"class Program
{
    static ref int F1()
    {
        var s = GetS();
        return ref s.F1();
    }
    static ref int F2()
    {
        var s = GetS();
        return ref s.F2(); // 1
    }

    static S<int> GetS() => throw null;
}";
            comp = CreateCompilation(sourceB, references: new[] { refA });
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of UnscopedRefAttribute_Method_03 from RefFieldTests.cs
        [CombinatorialData]
        [Theory]
        public void UnscopedRefAttribute_Method_03_ConstrainedTypeParameter(bool useCompilationReference, bool addStructConstraint)
        {
            var sourceA =
@"using System.Diagnostics.CodeAnalysis;
public interface S<T>
{
    public ref T F1();
    [UnscopedRef] public ref T F2();
}";
            var comp = CreateCompilation(new[] { sourceA, UnscopedRefAttributeDefinition });
            comp.VerifyEmitDiagnostics();
            var refA = AsReference(comp, useCompilationReference);

            var sourceB =
@"class Program<T> where T : " + (addStructConstraint ? "struct, " : "") + @"S<int>
{
    static ref int F1()
    {
        var s = GetS();
        return ref s.F1();
    }
    static ref int F2()
    {
        var s = GetS();
        return ref s.F2(); // 1
    }

    static T GetS() => throw null;
}";
            comp = CreateCompilation(sourceB, references: new[] { refA });
            comp.VerifyEmitDiagnostics(
                // (11,20): error CS8168: Cannot return local 's' by reference because it is not a ref local
                //         return ref s.F2(); // 1
                Diagnostic(ErrorCode.ERR_RefReturnLocal, "s").WithArguments("s").WithLocation(11, 20));
        }

        // This is a clone of UnscopedRefAttribute_Method_03 from RefFieldTests.cs
        [CombinatorialData]
        [Theory]
        public void UnscopedRefAttribute_Method_03_ClassConstrainedTypeParameter(bool useCompilationReference)
        {
            var sourceA =
@"using System.Diagnostics.CodeAnalysis;
public interface S<T>
{
    public ref T F1();
    [UnscopedRef] public ref T F2();
}";
            var comp = CreateCompilation(new[] { sourceA, UnscopedRefAttributeDefinition });
            comp.VerifyEmitDiagnostics();
            var refA = AsReference(comp, useCompilationReference);

            var sourceB =
@"class Program<T> where T : class, S<int>
{
    static ref int F1()
    {
        var s = GetS();
        return ref s.F1();
    }
    static ref int F2()
    {
        var s = GetS();
        return ref s.F2(); // 1
    }

    static T GetS() => throw null;
}";
            comp = CreateCompilation(sourceB, references: new[] { refA });
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of UnscopedRefAttribute_Property_02 from RefFieldTests.cs
        [CombinatorialData]
        [Theory]
        public void UnscopedRefAttribute_Property_02_DirectInterface(bool useCompilationReference)
        {
            var sourceA =
@"using System.Diagnostics.CodeAnalysis;
public interface S<T>
{
    public ref T P1 {get;}
    [UnscopedRef] public ref T P2 {get;}
}";
            var comp = CreateCompilation(new[] { sourceA, UnscopedRefAttributeDefinition });
            comp.VerifyEmitDiagnostics();
            var refA = AsReference(comp, useCompilationReference);

            var sourceB =
@"class Program
{
    static ref int F1()
    {
        var s = default(S<int>);
        return ref s.P1;
    }
    static ref int F2()
    {
        var s = default(S<int>);
        return ref s.P2; // 1
    }
}";
            comp = CreateCompilation(sourceB, references: new[] { refA });
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of UnscopedRefAttribute_Property_02 from RefFieldTests.cs
        [CombinatorialData]
        [Theory]
        public void UnscopedRefAttribute_Property_02_ConstrainedTypeParameter(bool useCompilationReference, bool addStructConstraint)
        {
            var sourceA =
@"using System.Diagnostics.CodeAnalysis;
public interface S<T>
{
    public ref T P1 {get;}
    [UnscopedRef] public ref T P2 {get;}
}";
            var comp = CreateCompilation(new[] { sourceA, UnscopedRefAttributeDefinition });
            comp.VerifyEmitDiagnostics();
            var refA = AsReference(comp, useCompilationReference);

            var sourceB =
@"class Program<T> where T : " + (addStructConstraint ? "struct, " : "") + @"S<int>" + (!addStructConstraint ? ", new()" : "") + @"
{
    static ref int F1()
    {
        var s = new T();
        return ref s.P1;
    }
    static ref int F2()
    {
        var s = new T();
        return ref s.P2; // 1
    }
}";
            comp = CreateCompilation(sourceB, references: new[] { refA });
            comp.VerifyEmitDiagnostics(
                // (11,20): error CS8168: Cannot return local 's' by reference because it is not a ref local
                //         return ref s.P2; // 1
                Diagnostic(ErrorCode.ERR_RefReturnLocal, "s").WithArguments("s").WithLocation(11, 20));
        }

        // This is a clone of UnscopedRefAttribute_Property_02 from RefFieldTests.cs
        [CombinatorialData]
        [Theory]
        public void UnscopedRefAttribute_Property_02_ClassConstrainedTypeParameter(bool useCompilationReference)
        {
            var sourceA =
@"using System.Diagnostics.CodeAnalysis;
public interface S<T>
{
    public ref T P1 {get;}
    [UnscopedRef] public ref T P2 {get;}
}";
            var comp = CreateCompilation(new[] { sourceA, UnscopedRefAttributeDefinition });
            comp.VerifyEmitDiagnostics();
            var refA = AsReference(comp, useCompilationReference);

            var sourceB =
@"class Program<T> where T : class, S<int>, new()
{
    static ref int F1()
    {
        var s = new T();
        return ref s.P1;
    }
    static ref int F2()
    {
        var s = new T();
        return ref s.P2; // 1
    }
}";
            comp = CreateCompilation(sourceB, references: new[] { refA });
            comp.VerifyEmitDiagnostics();
        }

        public enum ThreeState : byte
        {
            Unknown = 0,
            False = 1,
            True = 2,
        }

        // This is a clone of UnscopedRefAttribute_NestedAccess_MethodOrProperty from RefFieldTests.cs
        [Theory, CombinatorialData]
        public void UnscopedRefAttribute_NestedAccess_MethodOrProperty(bool firstIsMethod, bool secondIsMethod, ThreeState tS1IsClass, ThreeState tS2IsClass)
        {
            var source = $$"""
using System.Diagnostics.CodeAnalysis;

{{(tS1IsClass == ThreeState.True || tS2IsClass == ThreeState.True ? "" : """
var c = new C<S1<S2>, S2>();
c.Value() = 12;
System.Console.WriteLine(c.Value());
""")}}

class C<TS1, TS2>
    where TS1 : {{(tS1IsClass switch { ThreeState.False => "struct, ", ThreeState.True => "class, ", _ => "" })}}IS1<TS2>  
    where TS2 : {{(tS2IsClass switch { ThreeState.False => "struct, ", ThreeState.True => "class, ", _ => "" })}}IS2
{
    public ref int Value() => ref s1.S2{{csharp(firstIsMethod)}}.Value{{csharp(secondIsMethod)}};
#line 100
    private TS1 s1;
}

struct S1<TS2> : IS1<TS2> where TS2 : IS2
{
    private TS2 s2;
    [UnscopedRef] public ref TS2 S2{{csharp(firstIsMethod)}} => ref s2;
}

struct S2 : IS2
{
    private int value;
    [UnscopedRef] public ref int Value{{csharp(secondIsMethod)}} => ref value;
}

interface IS1<TS2> where TS2 : IS2
{
    [UnscopedRef] public ref TS2 S2{{(firstIsMethod ? "();" : "{get;}")}}
}

interface IS2
{
    [UnscopedRef] public ref int Value{{(secondIsMethod ? "();" : "{get;}")}}
}
""";
            var verifier = CompileAndVerify(new[] { source, UnscopedRefAttributeDefinition }, expectedOutput: (tS1IsClass == ThreeState.True || tS2IsClass == ThreeState.True ? null : "12"), verify: Verification.Fails);
            verifier.VerifyDiagnostics(
                // 0.cs(100,17): warning CS0649: Field 'C<TS1, TS2>.s1' is never assigned to, and will always have its default value 
                //     private TS1 s1;
                Diagnostic(ErrorCode.WRN_UnassignedInternalField, "s1").WithArguments("C<TS1, TS2>.s1", tS1IsClass == ThreeState.True ? "null" : "").WithLocation(100, 17)
                );
            verifier.VerifyMethodBody("C<TS1, TS2>.Value",
                tS1IsClass == ThreeState.True ? $$"""
{
  // Code size       28 (0x1c)
  .maxstack  1
  // sequence point: s1.S2{{csharp(firstIsMethod)}}.Value{{csharp(secondIsMethod)}}
  IL_0000:  ldarg.0
  IL_0001:  ldfld      "TS1 C<TS1, TS2>.s1"
  IL_0006:  box        "TS1"
  IL_000b:  callvirt   "ref TS2 IS1<TS2>.S2{{il(firstIsMethod)}}"
  IL_0010:  constrained. "TS2"
  IL_0016:  callvirt   "ref int IS2.Value{{il(secondIsMethod)}}"
  IL_001b:  ret
}
""" : $$"""
{
  // Code size       29 (0x1d)
  .maxstack  1
  // sequence point: s1.S2{{csharp(firstIsMethod)}}.Value{{csharp(secondIsMethod)}}
  IL_0000:  ldarg.0
  IL_0001:  ldflda     "TS1 C<TS1, TS2>.s1"
  IL_0006:  constrained. "TS1"
  IL_000c:  callvirt   "ref TS2 IS1<TS2>.S2{{il(firstIsMethod)}}"
  IL_0011:  constrained. "TS2"
  IL_0017:  callvirt   "ref int IS2.Value{{il(secondIsMethod)}}"
  IL_001c:  ret
}
""");

            static string csharp(bool method) => method ? "()" : "";
            static string il(bool method) => method ? "()" : ".get";
        }

        // This is a clone of UnscopedRefAttribute_NestedAccess_Properties_Invalid from RefFieldTests.cs
        [Fact]
        public void UnscopedRefAttribute_NestedAccess_Properties_Invalid_DirectInterface()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;

                class C
                {
                    private S1 s1;
                    public ref int Value() => ref s1.S2.Value;
                }

                struct S1
                {
                    private S2 s2;
                    public S2 S2 => s2;
                }

                interface S2
                {
                    [UnscopedRef] public ref int Value {get;}
                }
                """;
            CreateCompilation(new[] { source, UnscopedRefAttributeDefinition }).VerifyDiagnostics(
                // 0.cs(11,16): warning CS0649: Field 'S1.s2' is never assigned to, and will always have its default value null
                //     private S2 s2;
                Diagnostic(ErrorCode.WRN_UnassignedInternalField, "s2").WithArguments("S1.s2", "null").WithLocation(11, 16));
        }

        // This is a clone of UnscopedRefAttribute_NestedAccess_Properties_Invalid from RefFieldTests.cs
        [CombinatorialData]
        [Theory]
        public void UnscopedRefAttribute_NestedAccess_Properties_Invalid_ConstrainedTypeParameter(bool addStructConstraint)
        {
            var source =
@"using System.Diagnostics.CodeAnalysis;

class C<T> where T : " + (addStructConstraint ? "struct, " : "") + @"S2
{
    private S1<T> s1;
    public ref int Value() => ref s1.S2.Value;
}

struct S1<T> where T : S2
{
    private T s2;
    public T S2 => s2;
}

interface S2
{
    [UnscopedRef] public ref int Value {get;}
}
";
            CreateCompilation(new[] { source, UnscopedRefAttributeDefinition }).VerifyDiagnostics(
                // 0.cs(6,35): error CS8156: An expression cannot be used in this context because it may not be passed or returned by reference
                //     public ref int Value() => ref s1.S2.Value;
                Diagnostic(ErrorCode.ERR_RefReturnLvalueExpected, "s1.S2").WithLocation(6, 35),
                // 0.cs(11,15): warning CS0649: Field 'S1<T>.s2' is never assigned to, and will always have its default value 
                //     private T s2;
                Diagnostic(ErrorCode.WRN_UnassignedInternalField, "s2").WithArguments("S1<T>.s2", "").WithLocation(11, 15));
        }

        // This is a clone of UnscopedRefAttribute_NestedAccess_Properties_Invalid from RefFieldTests.cs
        [Fact]
        public void UnscopedRefAttribute_NestedAccess_Properties_Invalid_ClassConstrainedTypeParameter()
        {
            var source =
@"using System.Diagnostics.CodeAnalysis;

class C<T> where T : class, S2
{
    private S1<T> s1;
    public ref int Value() => ref s1.S2.Value;
}

struct S1<T> where T : S2
{
    private T s2;
    public T S2 => s2;
}

interface S2
{
    [UnscopedRef] public ref int Value {get;}
}
";
            CreateCompilation(new[] { source, UnscopedRefAttributeDefinition }).VerifyDiagnostics(
                // 0.cs(11,15): warning CS0649: Field 'S1<T>.s2' is never assigned to, and will always have its default value 
                //     private T s2;
                Diagnostic(ErrorCode.WRN_UnassignedInternalField, "s2").WithArguments("S1<T>.s2", "").WithLocation(11, 15));
        }

        // This is a clone of UnscopedRef_ArgumentsMustMatch_01 from RefFieldTests.cs
        [Fact]
        public void UnscopedRef_ArgumentsMustMatch_01_DirectInterface()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;

                ref struct RefByteContainer
                {
                    public ref byte RB;

                    public RefByteContainer(ref byte rb)
                    {
                        RB = ref rb;
                    }
                }

                interface ByteContainer
                {
                    [UnscopedRef]
                    public RefByteContainer ByteRef {get;}

                    [UnscopedRef]
                    public RefByteContainer GetByteRef();
                }

                public class Program
                {
                    static void M11(ref ByteContainer bc)
                    {
                        // ok. because ref-safe-to-escape of 'this' in 'ByteContainer.ByteRef.get' is 'ReturnOnly',
                        // we know that 'ref bc' will not end up written to a ref field within 'bc'.
                        _ = bc.ByteRef;
                    }
                    static void M12(ref ByteContainer bc)
                    {
                        // ok. because ref-safe-to-escape of 'this' in 'ByteContainer.GetByteRef()' is 'ReturnOnly',
                        // we know that 'ref bc' will not end up written to a ref field within 'bc'.
                        _ = bc.GetByteRef();
                    }

                    static void M21(ref ByteContainer bc, ref RefByteContainer rbc)
                    {
                        // error. ref-safe-to-escape of 'bc' is 'ReturnOnly', therefore 'bc.ByteRef' can't be assigned to a ref parameter.
                        rbc = bc.ByteRef; // 1
                    }
                    static void M22(ref ByteContainer bc, ref RefByteContainer rbc)
                    {
                        // error. ref-safe-to-escape of 'bc' is 'ReturnOnly', therefore 'bc.ByteRef' can't be assigned to a ref parameter.
                        rbc = bc.GetByteRef(); // 2
                    }

                    static RefByteContainer M31(ref ByteContainer bc)
                        // ok. ref-safe-to-escape of 'bc' is 'ReturnOnly'.
                        => bc.ByteRef;

                    static RefByteContainer M32(ref ByteContainer bc)
                        // ok. ref-safe-to-escape of 'bc' is 'ReturnOnly'.
                        => bc.GetByteRef();

                    static RefByteContainer M41(scoped ref ByteContainer bc)
                        // error: `bc.ByteRef` may contain a reference to `bc`, whose ref-safe-to-escape is CurrentMethod.
                        => bc.ByteRef; // 3

                    static RefByteContainer M42(scoped ref ByteContainer bc)
                        // error: `bc.GetByteRef()` may contain a reference to `bc`, whose ref-safe-to-escape is CurrentMethod.
                        => bc.GetByteRef(); // 4
                }
                """;

            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyDiagnostics();
        }

        // This is a clone of UnscopedRef_ArgumentsMustMatch_01 from RefFieldTests.cs
        [Theory]
        [CombinatorialData]
        public void UnscopedRef_ArgumentsMustMatch_01_ConstrainedTypeParameter(bool addStructConstraint)
        {
            var source = $$"""
                using System.Diagnostics.CodeAnalysis;

                ref struct RefByteContainer
                {
                    public ref byte RB;

                    public RefByteContainer(ref byte rb)
                    {
                        RB = ref rb;
                    }
                }

                interface ByteContainer
                {
                    [UnscopedRef]
                    public RefByteContainer ByteRef {get;}

                    [UnscopedRef]
                    public RefByteContainer GetByteRef();
                }

                class Program<TByteContainer> where TByteContainer : {{(addStructConstraint ? "struct, " : "")}} ByteContainer
                {
                    static void M11(ref TByteContainer bc)
                    {
                        // ok. because ref-safe-to-escape of 'this' in 'ByteContainer.ByteRef.get' is 'ReturnOnly',
                        // we know that 'ref bc' will not end up written to a ref field within 'bc'.
                        _ = bc.ByteRef;
                    }
                    static void M12(ref TByteContainer bc)
                    {
                        // ok. because ref-safe-to-escape of 'this' in 'ByteContainer.GetByteRef()' is 'ReturnOnly',
                        // we know that 'ref bc' will not end up written to a ref field within 'bc'.
                        _ = bc.GetByteRef();
                    }

                    static void M21(ref TByteContainer bc, ref RefByteContainer rbc)
                    {
                        // error. ref-safe-to-escape of 'bc' is 'ReturnOnly', therefore 'bc.ByteRef' can't be assigned to a ref parameter.
                        rbc = bc.ByteRef; // 1
                    }
                    static void M22(ref TByteContainer bc, ref RefByteContainer rbc)
                    {
                        // error. ref-safe-to-escape of 'bc' is 'ReturnOnly', therefore 'bc.ByteRef' can't be assigned to a ref parameter.
                        rbc = bc.GetByteRef(); // 2
                    }

                    static RefByteContainer M31(ref TByteContainer bc)
                        // ok. ref-safe-to-escape of 'bc' is 'ReturnOnly'.
                        => bc.ByteRef;

                    static RefByteContainer M32(ref TByteContainer bc)
                        // ok. ref-safe-to-escape of 'bc' is 'ReturnOnly'.
                        => bc.GetByteRef();

                    static RefByteContainer M41(scoped ref TByteContainer bc)
                        // error: `bc.ByteRef` may contain a reference to `bc`, whose ref-safe-to-escape is CurrentMethod.
                        => bc.ByteRef; // 3

                    static RefByteContainer M42(scoped ref TByteContainer bc)
                        // error: `bc.GetByteRef()` may contain a reference to `bc`, whose ref-safe-to-escape is CurrentMethod.
                        => bc.GetByteRef(); // 4
                }
                """;

            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyDiagnostics(
                // (40,15): error CS9077: Cannot return a parameter by reference 'bc' through a ref parameter; it can only be returned in a return statement
                //         rbc = bc.ByteRef; // 1
                Diagnostic(ErrorCode.ERR_RefReturnOnlyParameter, "bc").WithArguments("bc").WithLocation(40, 15),
                // (45,15): error CS9077: Cannot return a parameter by reference 'bc' through a ref parameter; it can only be returned in a return statement
                //         rbc = bc.GetByteRef(); // 2
                Diagnostic(ErrorCode.ERR_RefReturnOnlyParameter, "bc").WithArguments("bc").WithLocation(45, 15),
                // (58,12): error CS9075: Cannot return a parameter by reference 'bc' because it is scoped to the current method
                //         => bc.ByteRef; // 3
                Diagnostic(ErrorCode.ERR_RefReturnScopedParameter, "bc").WithArguments("bc").WithLocation(58, 12),
                // (62,12): error CS9075: Cannot return a parameter by reference 'bc' because it is scoped to the current method
                //         => bc.GetByteRef(); // 4
                Diagnostic(ErrorCode.ERR_RefReturnScopedParameter, "bc").WithArguments("bc").WithLocation(62, 12)
                );
        }

        // This is a clone of UnscopedRef_ArgumentsMustMatch_01 from RefFieldTests.cs
        [Fact]
        public void UnscopedRef_ArgumentsMustMatch_01_ClassConstrainedTypeParameter()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;

                ref struct RefByteContainer
                {
                    public ref byte RB;

                    public RefByteContainer(ref byte rb)
                    {
                        RB = ref rb;
                    }
                }

                interface ByteContainer
                {
                    [UnscopedRef]
                    public RefByteContainer ByteRef {get;}

                    [UnscopedRef]
                    public RefByteContainer GetByteRef();
                }

                class Program<TByteContainer> where TByteContainer : class, ByteContainer
                {
                    static void M11(ref TByteContainer bc)
                    {
                        // ok. because ref-safe-to-escape of 'this' in 'ByteContainer.ByteRef.get' is 'ReturnOnly',
                        // we know that 'ref bc' will not end up written to a ref field within 'bc'.
                        _ = bc.ByteRef;
                    }
                    static void M12(ref TByteContainer bc)
                    {
                        // ok. because ref-safe-to-escape of 'this' in 'ByteContainer.GetByteRef()' is 'ReturnOnly',
                        // we know that 'ref bc' will not end up written to a ref field within 'bc'.
                        _ = bc.GetByteRef();
                    }

                    static void M21(ref TByteContainer bc, ref RefByteContainer rbc)
                    {
                        // error. ref-safe-to-escape of 'bc' is 'ReturnOnly', therefore 'bc.ByteRef' can't be assigned to a ref parameter.
                        rbc = bc.ByteRef; // 1
                    }
                    static void M22(ref TByteContainer bc, ref RefByteContainer rbc)
                    {
                        // error. ref-safe-to-escape of 'bc' is 'ReturnOnly', therefore 'bc.ByteRef' can't be assigned to a ref parameter.
                        rbc = bc.GetByteRef(); // 2
                    }

                    static RefByteContainer M31(ref TByteContainer bc)
                        // ok. ref-safe-to-escape of 'bc' is 'ReturnOnly'.
                        => bc.ByteRef;

                    static RefByteContainer M32(ref TByteContainer bc)
                        // ok. ref-safe-to-escape of 'bc' is 'ReturnOnly'.
                        => bc.GetByteRef();

                    static RefByteContainer M41(scoped ref TByteContainer bc)
                        // error: `bc.ByteRef` may contain a reference to `bc`, whose ref-safe-to-escape is CurrentMethod.
                        => bc.ByteRef; // 3

                    static RefByteContainer M42(scoped ref TByteContainer bc)
                        // error: `bc.GetByteRef()` may contain a reference to `bc`, whose ref-safe-to-escape is CurrentMethod.
                        => bc.GetByteRef(); // 4
                }
                """;

            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyDiagnostics();
        }

        // This is a clone of UnscopedRef_ArgumentsMustMatch_01 from RefFieldTests.cs
        [Fact]
        public void UnscopedRef_ArgumentsMustMatch_01()
        {
            var source = """
                using System.Diagnostics.CodeAnalysis;

                interface IRefByteContainer
                {


                    //public RefByteContainer(ref byte rb)
                    //{
                    //    RB = ref rb;
                    //}
                }

                interface IByteContainer<RefByteContainer> where RefByteContainer : IRefByteContainer, allows ref struct
                {
                    //public byte B;

                    [UnscopedRef]
                    public RefByteContainer ByteRef {get;}

                    [UnscopedRef]
                    public RefByteContainer GetByteRef();
                }

                class Program<RefByteContainer, ByteContainer> where RefByteContainer : IRefByteContainer, allows ref struct where ByteContainer : IByteContainer<RefByteContainer>, allows ref struct
                {
                    static void M11(ref ByteContainer bc)
                    {
                        // ok. because ref-safe-to-escape of 'this' in 'ByteContainer.ByteRef.get' is 'ReturnOnly',
                        // we know that 'ref bc' will not end up written to a ref field within 'bc'.
                        _ = bc.ByteRef;
                    }
                    static void M12(ref ByteContainer bc)
                    {
                        // ok. because ref-safe-to-escape of 'this' in 'ByteContainer.GetByteRef()' is 'ReturnOnly',
                        // we know that 'ref bc' will not end up written to a ref field within 'bc'.
                        _ = bc.GetByteRef();
                    }

                    static void M21(ref ByteContainer bc, ref RefByteContainer rbc)
                    {
                        // error. ref-safe-to-escape of 'bc' is 'ReturnOnly', therefore 'bc.ByteRef' can't be assigned to a ref parameter.
                        rbc = bc.ByteRef; // 1
                    }
                    static void M22(ref ByteContainer bc, ref RefByteContainer rbc)
                    {
                        // error. ref-safe-to-escape of 'bc' is 'ReturnOnly', therefore 'bc.ByteRef' can't be assigned to a ref parameter.
                        rbc = bc.GetByteRef(); // 2
                    }

                    static RefByteContainer M31(ref ByteContainer bc)
                        // ok. ref-safe-to-escape of 'bc' is 'ReturnOnly'.
                        => bc.ByteRef;

                    static RefByteContainer M32(ref ByteContainer bc)
                        // ok. ref-safe-to-escape of 'bc' is 'ReturnOnly'.
                        => bc.GetByteRef();

                    static RefByteContainer M41(scoped ref ByteContainer bc)
                        // error: `bc.ByteRef` may contain a reference to `bc`, whose ref-safe-to-escape is CurrentMethod.
                        => bc.ByteRef; // 3

                    static RefByteContainer M42(scoped ref ByteContainer bc)
                        // error: `bc.GetByteRef()` may contain a reference to `bc`, whose ref-safe-to-escape is CurrentMethod.
                        => bc.GetByteRef(); // 4
                }
                """;

            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (42,15): error CS9077: Cannot return a parameter by reference 'bc' through a ref parameter; it can only be returned in a return statement
                //         rbc = bc.ByteRef; // 1
                Diagnostic(ErrorCode.ERR_RefReturnOnlyParameter, "bc").WithArguments("bc").WithLocation(42, 15),
                // (47,15): error CS9077: Cannot return a parameter by reference 'bc' through a ref parameter; it can only be returned in a return statement
                //         rbc = bc.GetByteRef(); // 2
                Diagnostic(ErrorCode.ERR_RefReturnOnlyParameter, "bc").WithArguments("bc").WithLocation(47, 15),
                // (60,12): error CS9075: Cannot return a parameter by reference 'bc' because it is scoped to the current method
                //         => bc.ByteRef; // 3
                Diagnostic(ErrorCode.ERR_RefReturnScopedParameter, "bc").WithArguments("bc").WithLocation(60, 12),
                // (64,12): error CS9075: Cannot return a parameter by reference 'bc' because it is scoped to the current method
                //         => bc.GetByteRef(); // 4
                Diagnostic(ErrorCode.ERR_RefReturnScopedParameter, "bc").WithArguments("bc").WithLocation(64, 12));
        }

        // This is a clone of PatternIndex_01 from RefFieldTests.cs
        [Fact]
        public void PatternIndex_01_DirectInterface()
        {
            string source = """
                using System;
                using System.Diagnostics.CodeAnalysis;
                interface R
                {
                    public int Length {get;}
                    [UnscopedRef] public ref int this[int i] {get;}
                }
                class Program
                {
                    static ref int F1(ref R r1)
                    {
                        ref int i1 = ref r1[^1];
                        return ref i1;
                    }
                    static ref int F2(ref R r2, Index i)
                    {
                        ref int i2 = ref r2[i];
                        return ref i2;
                    }
                    static ref int F3()
                    {
                        R r3 = GetR();
                        ref int i3 = ref r3[^3];
                        return ref i3; // 1
                    }
                    static ref int F4(Index i)
                    {
                        R r4 = GetR();
                        ref int i4 = ref r4[i];
                        return ref i4; // 2
                    }

                    static R GetR() => null;
                }
                """;
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyDiagnostics();
        }

        // This is a clone of PatternIndex_01 from RefFieldTests.cs
        [Theory]
        [CombinatorialData]
        public void PatternIndex_01_ConstrainedTypeParameter(bool addStructConstraint)
        {
            string source = $$"""
                using System;
                using System.Diagnostics.CodeAnalysis;
                interface R
                {
                    public int Length {get;}
                    [UnscopedRef] public ref int this[int i] {get;}
                }
                class Program<TR> where TR : {{(addStructConstraint ? "struct, " : "")}} R {{(!addStructConstraint ? ", new()" : "")}}
                {
                    static ref int F1(ref TR r1)
                    {
                        ref int i1 = ref r1[^1];
                        return ref i1;
                    }
                    static ref int F2(ref TR r2, Index i)
                    {
                        ref int i2 = ref r2[i];
                        return ref i2;
                    }
                    static ref int F3()
                    {
                        TR r3 = new TR();
                        ref int i3 = ref r3[^3];
                        return ref i3; // 1
                    }
                    static ref int F4(Index i)
                    {
                        TR r4 = new TR();
                        ref int i4 = ref r4[i];
                        return ref i4; // 2
                    }
                }
                """;
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyDiagnostics(
                // (24,20): error CS8157: Cannot return 'i3' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref i3; // 1
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "i3").WithArguments("i3").WithLocation(24, 20),
                // (30,20): error CS8157: Cannot return 'i4' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref i4; // 2
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "i4").WithArguments("i4").WithLocation(30, 20));
        }

        // This is a clone of PatternIndex_01 from RefFieldTests.cs
        [Fact]
        public void PatternIndex_01_ClassConstrainedTypeParameter()
        {
            string source = """
                using System;
                using System.Diagnostics.CodeAnalysis;
                interface R
                {
                    public int Length {get;}
                    [UnscopedRef] public ref int this[int i] {get;}
                }
                class Program<TR> where TR : class, R, new()
                {
                    static ref int F1(ref TR r1)
                    {
                        ref int i1 = ref r1[^1];
                        return ref i1;
                    }
                    static ref int F2(ref TR r2, Index i)
                    {
                        ref int i2 = ref r2[i];
                        return ref i2;
                    }
                    static ref int F3()
                    {
                        TR r3 = new TR();
                        ref int i3 = ref r3[^3];
                        return ref i3; // 1
                    }
                    static ref int F4(Index i)
                    {
                        TR r4 = new TR();
                        ref int i4 = ref r4[i];
                        return ref i4; // 2
                    }
                }
                """;
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyDiagnostics();
        }

        // This is a clone of MemberOfReadonlyRefLikeEscape from RefEscapingTests.cs
        [Fact]
        public void MemberOfReadonlyRefLikeEscape_DirectInterface()
        {
            var text = @"
    using System;
    using System.Diagnostics.CodeAnalysis;
    public static class Program
    {
        public static void Main()
        {
            Span<int> value1 = stackalloc int[1];

            // Ok, the new value can be copied into SW but not the 
            // ref to the value
            Get_SW().TryGet(out value1);

            // Error as the ref of this can escape into value2
            Span<int> value2 = default;
            Get_SW().TryGet2(out value2);
        }

        static SW Get_SW() => throw null;
    }

    interface SW
    {
        public void TryGet(out Span<int> result); 

        [UnscopedRef]
        public void TryGet2(out Span<int> result);
    }
";
            CreateCompilationWithMscorlibAndSpan(new[] { text, UnscopedRefAttributeDefinition }).VerifyDiagnostics();
        }

        // This is a clone of MemberOfReadonlyRefLikeEscape from RefEscapingTests.cs
        [Theory]
        [CombinatorialData]
        public void MemberOfReadonlyRefLikeEscape_ConstrainedTypeParameter(bool addStructConstraint)
        {
            var text = @"
    using System;
    using System.Diagnostics.CodeAnalysis;
    static class Program<TSW> where TSW : " + (addStructConstraint ? "struct, " : "") + @"SW" + (!addStructConstraint ? ", new()" : "") + @"
    {
        public static void Main()
        {
            Span<int> value1 = stackalloc int[1];

            // Ok, the new value can be copied into SW but not the 
            // ref to the value
            new TSW().TryGet(out value1);

            // Error as the ref of this can escape into value2
            Span<int> value2 = default;
            new TSW().TryGet2(out value2);
        }
    }

    interface SW
    {
        public void TryGet(out Span<int> result);

        [UnscopedRef]
        public void TryGet2(out Span<int> result);
    }
";
            CreateCompilationWithMscorlibAndSpan(new[] { text, UnscopedRefAttributeDefinition }).VerifyDiagnostics(
                // 0.cs(16,13): error CS8156: An expression cannot be used in this context because it may not be passed or returned by reference
                //             new TSW().TryGet2(out value2);
                Diagnostic(ErrorCode.ERR_RefReturnLvalueExpected, "new TSW()").WithLocation(16, 13),
                // 0.cs(16,13): error CS8350: This combination of arguments to 'SW.TryGet2(out Span<int>)' is disallowed because it may expose variables referenced by parameter 'this' outside of their declaration scope
                //             new TSW().TryGet2(out value2);
                Diagnostic(ErrorCode.ERR_CallArgMixing, "new TSW().TryGet2(out value2)").WithArguments("SW.TryGet2(out System.Span<int>)", "this").WithLocation(16, 13)
                );
        }

        // This is a clone of MemberOfReadonlyRefLikeEscape from RefEscapingTests.cs
        [Fact]
        public void MemberOfReadonlyRefLikeEscape_ClassConstrainedTypeParameter()
        {
            var text = @"
    using System;
    using System.Diagnostics.CodeAnalysis;
    static class Program<TSW> where TSW : class, SW, new()
    {
        public static void Main()
        {
            Span<int> value1 = stackalloc int[1];

            // Ok, the new value can be copied into SW but not the 
            // ref to the value
            new TSW().TryGet(out value1);

            // Error as the ref of this can escape into value2
            Span<int> value2 = default;
            new TSW().TryGet2(out value2);
        }
    }

    interface SW
    {
        public void TryGet(out Span<int> result);

        [UnscopedRef]
        public void TryGet2(out Span<int> result);
    }
";
            CreateCompilationWithMscorlibAndSpan(new[] { text, UnscopedRefAttributeDefinition }).VerifyDiagnostics();
        }

        // This is a clone of DefensiveCopy_01 from RefEscapingTests.cs
        [Fact]
        public void DefensiveCopy_01_DirectInterface()
        {
            var source =
@"
using System;
using System.Diagnostics.CodeAnalysis;

internal class Program
{
    private static readonly Vec4 ReadOnlyVec = GetVec4();

    static void Main()
    {
        // This refers to stack memory that has already been left out.
        ref Vec4 local = ref Test1();
        Console.WriteLine(local);
    }

    private static ref Vec4 Test1()
    {
        // Defensive copy occurs and it is placed in stack memory implicitly.
        // The method returns a reference to the copy, which happens invalid memory access.
        ref Vec4 xyzw1 = ref ReadOnlyVec.Self;
        return ref xyzw1;
    }

    private static ref Vec4 Test2()
    {
        var copy = ReadOnlyVec;
        ref Vec4 xyzw2 = ref copy.Self;
        return ref xyzw2;
    }

    private static ref Vec4 Test3()
    {
        ref Vec4 xyzw3 = ref ReadOnlyVec.Self2();
        return ref xyzw3;
    }

    private static ref Vec4 Test4()
    {
        var copy = ReadOnlyVec;
        ref Vec4 xyzw4 = ref copy.Self2();
        return ref xyzw4;
    }

    static Vec4 GetVec4() => throw null;
}

public interface Vec4
{
    [UnscopedRef]
    public ref Vec4 Self {get;}

    [UnscopedRef]
    public ref Vec4 Self2();
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of DefensiveCopy_01 from RefEscapingTests.cs
        [Theory]
        [CombinatorialData]
        public void DefensiveCopy_01_ConstrainedTypeParameter(bool addStructConstraint)
        {
            var source =
@"
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

internal class Program<TVec4> where TVec4 : " + (addStructConstraint ? "struct, " : "") + @" Vec4<TVec4>
{
    private static readonly TVec4 ReadOnlyVec = GetVec4();

    static void Main()
    {
        // This refers to stack memory that has already been left out.
        ref TVec4 local = ref Test1();
        Console.WriteLine(local);
    }

    private static ref TVec4 Test1()
    {
        // Defensive copy occurs and it is placed in stack memory implicitly.
        // The method returns a reference to the copy, which happens invalid memory access.
        ref TVec4 xyzw1 = ref ReadOnlyVec.Self;
        return ref xyzw1;
    }

    private static ref TVec4 Test2()
    {
        var copy = ReadOnlyVec;
        ref TVec4 xyzw2 = ref copy.Self;
        return ref xyzw2;
    }

    private static ref TVec4 Test3()
    {
        ref TVec4 xyzw3 = ref ReadOnlyVec.Self2();
        return ref xyzw3;
    }

    private static ref TVec4 Test4()
    {
        var copy = ReadOnlyVec;
        ref TVec4 xyzw4 = ref copy.Self2();
        return ref xyzw4;
    }

    static TVec4 GetVec4() => throw null;
}

public interface Vec4<TVec4> where TVec4 : Vec4<TVec4>
{
    [UnscopedRef]
    public ref TVec4 Self {get;}

    [UnscopedRef]
    public ref TVec4 Self2();
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics(
                // (22,20): error CS8157: Cannot return 'xyzw1' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref xyzw1;
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "xyzw1").WithArguments("xyzw1").WithLocation(22, 20),
                // (29,20): error CS8157: Cannot return 'xyzw2' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref xyzw2;
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "xyzw2").WithArguments("xyzw2").WithLocation(29, 20),
                // (35,20): error CS8157: Cannot return 'xyzw3' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref xyzw3;
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "xyzw3").WithArguments("xyzw3").WithLocation(35, 20),
                // (42,20): error CS8157: Cannot return 'xyzw4' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref xyzw4;
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "xyzw4").WithArguments("xyzw4").WithLocation(42, 20)
                );
        }

        // This is a clone of DefensiveCopy_01 from RefEscapingTests.cs
        [Fact]
        public void DefensiveCopy_01_ClassConstrainedTypeParameter()
        {
            var source =
@"
using System;
using System.Diagnostics.CodeAnalysis;

internal class Program<TVec4> where TVec4 : class, Vec4<TVec4>
{
    private static readonly TVec4 ReadOnlyVec = GetVec4();

    static void Main()
    {
        // This refers to stack memory that has already been left out.
        ref TVec4 local = ref Test1();
        Console.WriteLine(local);
    }

    private static ref TVec4 Test1()
    {
        // Defensive copy occurs and it is placed in stack memory implicitly.
        // The method returns a reference to the copy, which happens invalid memory access.
        ref TVec4 xyzw1 = ref ReadOnlyVec.Self;
        return ref xyzw1;
    }

    private static ref TVec4 Test2()
    {
        var copy = ReadOnlyVec;
        ref TVec4 xyzw2 = ref copy.Self;
        return ref xyzw2;
    }

    private static ref TVec4 Test3()
    {
        ref TVec4 xyzw3 = ref ReadOnlyVec.Self2();
        return ref xyzw3;
    }

    private static ref TVec4 Test4()
    {
        var copy = ReadOnlyVec;
        ref TVec4 xyzw4 = ref copy.Self2();
        return ref xyzw4;
    }

    static TVec4 GetVec4() => throw null;
}

public interface Vec4<TVec4> where TVec4 : Vec4<TVec4>
{
    [UnscopedRef]
    public ref TVec4 Self {get;}

    [UnscopedRef]
    public ref TVec4 Self2();
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of DefensiveCopy_02 from RefEscapingTests.cs
        [Fact]
        public void DefensiveCopy_02_DirectInterface()
        {
            var source =
@"using System.Diagnostics.CodeAnalysis;

class Program
{
    static ref Wrap m1(in Wrap i)
    {
        ref Wrap r1 = ref i.Self; // defensive copy
        return ref r1; // ref to the local copy
    }

    static ref Wrap m2(in Wrap i)
    {
        var copy = i;
        ref Wrap r2 = ref copy.Self;
        return ref r2; // ref to the local copy
    }

    static ref Wrap m3(in Wrap i)
    {
        ref Wrap r3 = ref i.Self2();
        return ref r3;
    }

    static ref Wrap m4(in Wrap i)
    {
        var copy = i;
        ref Wrap r4 = ref copy.Self2();
        return ref r4; // ref to the local copy
    }
}

interface Wrap
{
    [UnscopedRef]
    public ref Wrap Self {get;}

    [UnscopedRef]
    public ref Wrap Self2();
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of DefensiveCopy_02 from RefEscapingTests.cs
        [Theory]
        [CombinatorialData]
        public void DefensiveCopy_02_ConstrainedTypeParameter(bool addStructConstraint)
        {
            var source =
@"using System.Diagnostics.CodeAnalysis;

class Program<TWrap> where TWrap : " + (addStructConstraint ? "struct, " : "") + @"Wrap<TWrap>
{
    static ref TWrap m1(in TWrap i)
    {
        ref TWrap r1 = ref i.Self; // defensive copy
        return ref r1; // ref to the local copy
    }

    static ref TWrap m2(in TWrap i)
    {
        var copy = i;
        ref TWrap r2 = ref copy.Self;
        return ref r2; // ref to the local copy
    }

    static ref TWrap m3(in TWrap i)
    {
        ref TWrap r3 = ref i.Self2();
        return ref r3;
    }

    static ref TWrap m4(in TWrap i)
    {
        var copy = i;
        ref TWrap r4 = ref copy.Self2();
        return ref r4; // ref to the local copy
    }
}

interface Wrap<T> where T : Wrap<T>
{
    [UnscopedRef]
    public ref T Self {get;}

    [UnscopedRef]
    public ref T Self2();
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics(
                // (8,20): error CS8157: Cannot return 'r1' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref r1; // ref to the local copy
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "r1").WithArguments("r1").WithLocation(8, 20),
                // (15,20): error CS8157: Cannot return 'r2' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref r2; // ref to the local copy
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "r2").WithArguments("r2").WithLocation(15, 20),
                // (21,20): error CS8157: Cannot return 'r3' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref r3;
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "r3").WithArguments("r3").WithLocation(21, 20),
                // (28,20): error CS8157: Cannot return 'r4' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref r4; // ref to the local copy
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "r4").WithArguments("r4").WithLocation(28, 20)
                );
        }

        // This is a clone of DefensiveCopy_02 from RefEscapingTests.cs
        [Fact]
        public void DefensiveCopy_02_ClassConstrainedTypeParameter()
        {
            var source =
@"using System.Diagnostics.CodeAnalysis;

class Program<TWrap> where TWrap : class, Wrap<TWrap>
{
    static ref TWrap m1(in TWrap i)
    {
        ref TWrap r1 = ref i.Self; // defensive copy
        return ref r1; // ref to the local copy
    }

    static ref TWrap m2(in TWrap i)
    {
        var copy = i;
        ref TWrap r2 = ref copy.Self;
        return ref r2; // ref to the local copy
    }

    static ref TWrap m3(in TWrap i)
    {
        ref TWrap r3 = ref i.Self2();
        return ref r3;
    }

    static ref TWrap m4(in TWrap i)
    {
        var copy = i;
        ref TWrap r4 = ref copy.Self2();
        return ref r4; // ref to the local copy
    }
}

interface Wrap<T> where T : Wrap<T>
{
    [UnscopedRef]
    public ref T Self {get;}

    [UnscopedRef]
    public ref T Self2();
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of DefensiveCopy_05 from RefEscapingTests.cs
        [Fact]
        public void DefensiveCopy_05_DirectInterface()
        {
            var source =
@"
using System;
using System.Diagnostics.CodeAnalysis;

internal class Program
{
    private static readonly Vec4 ReadOnlyVec = default;

    static void Main()
    {
    }

    private static Span<float> Test1()
    {
        var xyzw1 = ReadOnlyVec.Self;
        return xyzw1;
    }

    private static Span<float> Test2()
    {
        var r2 = ReadOnlyVec;
        var xyzw2 = r2.Self;
        return xyzw2;
    }
}

public interface Vec4
{
    [UnscopedRef]
    public Span<float> Self
    {  get; set; }
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of DefensiveCopy_05 from RefEscapingTests.cs
        [Theory]
        [CombinatorialData]
        public void DefensiveCopy_05_ConstrainedTypeParameter(bool addStructConstraint)
        {
            var source =
@"
using System;
using System.Diagnostics.CodeAnalysis;

internal class Program<TVec4> where TVec4 : " + (addStructConstraint ? "struct, " : "") + @" Vec4
{
    private static readonly TVec4 ReadOnlyVec = default;

    static void Main()
    {
    }

    private static Span<float> Test1()
    {
        var xyzw1 = ReadOnlyVec.Self;
        return xyzw1;
    }

    private static Span<float> Test2()
    {
        var r2 = ReadOnlyVec;
        var xyzw2 = r2.Self;
        return xyzw2;
    }
}

public interface Vec4
{
    [UnscopedRef]
    public Span<float> Self
    {  get; set; }
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics(
                // (16,16): error CS8352: Cannot use variable 'xyzw1' in this context because it may expose referenced variables outside of their declaration scope
                //         return xyzw1;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "xyzw1").WithArguments("xyzw1").WithLocation(16, 16),
                // (23,16): error CS8352: Cannot use variable 'xyzw2' in this context because it may expose referenced variables outside of their declaration scope
                //         return xyzw2;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "xyzw2").WithArguments("xyzw2").WithLocation(23, 16)
                );
        }

        // This is a clone of DefensiveCopy_05 from RefEscapingTests.cs
        [Fact]
        public void DefensiveCopy_05_ClassConstrainedTypeParameter()
        {
            var source =
@"
using System;
using System.Diagnostics.CodeAnalysis;

internal class Program<TVec4> where TVec4 : class, Vec4
{
    private static readonly TVec4 ReadOnlyVec = default;

    static void Main()
    {
    }

    private static Span<float> Test1()
    {
        var xyzw1 = ReadOnlyVec.Self;
        return xyzw1;
    }

    private static Span<float> Test2()
    {
        var r2 = ReadOnlyVec;
        var xyzw2 = r2.Self;
        return xyzw2;
    }
}

public interface Vec4
{
    [UnscopedRef]
    public Span<float> Self
    {  get; set; }
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of DefensiveCopy_21 from RefEscapingTests.cs
        [Fact]
        public void DefensiveCopy_21_DirectInterface()
        {
            var source =
@"
using System;
using System.Diagnostics.CodeAnalysis;

internal class Program
{
    private static readonly Vec4 ReadOnlyVec = default;

    static void Main()
    {
    }

    private static Span<float> Test1()
    {
        var (xyzw1, _) = ReadOnlyVec;
        return xyzw1;
    }

    private static Span<float> Test2()
    {
        var r2 = ReadOnlyVec;
        var (xyzw2, _) = r2;
        return xyzw2;
    }

    private static Span<float> Test3()
    {
        ReadOnlyVec.Deconstruct(out var xyzw3, out _);
        return xyzw3;
    }

    private static Span<float> Test4()
    {
        var r4 = ReadOnlyVec;
        r4.Deconstruct(out var xyzw4, out _);
        return xyzw4;
    }
}

public interface Vec4
{
    [UnscopedRef]
    public void Deconstruct(out Span<float> x, out int i);
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics();
        }

        // This is a clone of DefensiveCopy_21 from RefEscapingTests.cs
        [Theory]
        [CombinatorialData]
        public void DefensiveCopy_21_ConstrainedTypeParameter(bool addStructConstraint)
        {
            var source =
@"
using System;
using System.Diagnostics.CodeAnalysis;

internal class Program<TVec4> where TVec4 : " + (addStructConstraint ? "struct, " : "") + @" Vec4
{
    private static readonly TVec4 ReadOnlyVec = default;

    static void Main()
    {
    }

    private static Span<float> Test1()
    {
        var (xyzw1, _) = ReadOnlyVec;
        return xyzw1;
    }

    private static Span<float> Test2()
    {
        var r2 = ReadOnlyVec;
        var (xyzw2, _) = r2;
        return xyzw2;
    }

    private static Span<float> Test3()
    {
        ReadOnlyVec.Deconstruct(out var xyzw3, out _);
        return xyzw3;
    }

    private static Span<float> Test4()
    {
        var r4 = ReadOnlyVec;
        r4.Deconstruct(out var xyzw4, out _);
        return xyzw4;
    }
}

public interface Vec4
{
    [UnscopedRef]
    public void Deconstruct(out Span<float> x, out int i);
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics(
                // (16,16): error CS8352: Cannot use variable 'xyzw1' in this context because it may expose referenced variables outside of their declaration scope
                //         return xyzw1;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "xyzw1").WithArguments("xyzw1").WithLocation(16, 16),
                // (23,16): error CS8352: Cannot use variable 'xyzw2' in this context because it may expose referenced variables outside of their declaration scope
                //         return xyzw2;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "xyzw2").WithArguments("xyzw2").WithLocation(23, 16),
                // (29,16): error CS8352: Cannot use variable 'xyzw3' in this context because it may expose referenced variables outside of their declaration scope
                //         return xyzw3;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "xyzw3").WithArguments("xyzw3").WithLocation(29, 16),
                // (36,16): error CS8352: Cannot use variable 'xyzw4' in this context because it may expose referenced variables outside of their declaration scope
                //         return xyzw4;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "xyzw4").WithArguments("xyzw4").WithLocation(36, 16)
                );
        }

        // This is a clone of DefensiveCopy_21 from RefEscapingTests.cs
        [Fact]
        public void DefensiveCopy_21_ClassConstrainedTypeParameter()
        {
            var source =
@"
using System;
using System.Diagnostics.CodeAnalysis;

internal class Program<TVec4> where TVec4 : class, Vec4
{
    private static readonly TVec4 ReadOnlyVec = default;

    static void Main()
    {
    }

    private static Span<float> Test1()
    {
        var (xyzw1, _) = ReadOnlyVec;
        return xyzw1;
    }

    private static Span<float> Test2()
    {
        var r2 = ReadOnlyVec;
        var (xyzw2, _) = r2;
        return xyzw2;
    }

    private static Span<float> Test3()
    {
        ReadOnlyVec.Deconstruct(out var xyzw3, out _);
        return xyzw3;
    }

    private static Span<float> Test4()
    {
        var r4 = ReadOnlyVec;
        r4.Deconstruct(out var xyzw4, out _);
        return xyzw4;
    }
}

public interface Vec4
{
    [UnscopedRef]
    public void Deconstruct(out Span<float> x, out int i);
}
";
            var comp = CreateCompilation(source, targetFramework: TargetFramework.Net70);
            comp.VerifyEmitDiagnostics();
        }

        [Fact]
        public void AllowsConstraint_01_SimpleTypeTypeParameter()
        {
            var src = @"
public class C<T>
    where T : allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                var c = m.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
                var t = c.TypeParameters.Single();
                Assert.False(t.HasReferenceTypeConstraint);
                Assert.False(t.HasValueTypeConstraint);
                Assert.False(t.HasUnmanagedTypeConstraint);
                Assert.False(t.HasNotNullConstraint);
                Assert.True(t.AllowsByRefLike);
                Assert.True(t.GetPublicSymbol().AllowsByRefLike);
            }

            comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();
            Assert.True(comp.SupportsRuntimeCapability(RuntimeCapability.ByRefLikeGenerics));

            CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (3,22): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     where T : allows ref struct
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "ref struct").WithArguments("ref struct interfaces").WithLocation(3, 22)
                );

            comp = CreateCompilation(src, targetFramework: TargetFramework.DesktopLatestExtended, parseOptions: TestOptions.RegularNext).VerifyDiagnostics(
                // (3,22): error CS9500: Target runtime doesn't support by-ref-like generics.
                //     where T : allows ref struct
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "ref struct").WithLocation(3, 22)
                );
            Assert.False(comp.SupportsRuntimeCapability(RuntimeCapability.ByRefLikeGenerics));
        }

        [Fact]
        public void AllowsConstraint_02_SimpleMethodTypeParameter()
        {
            var src = @"
public class C
{
    public void M<T>()
        where T : allows ref struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                var method = m.GlobalNamespace.GetMember<MethodSymbol>("C.M");
                var t = method.TypeParameters.Single();
                Assert.False(t.HasReferenceTypeConstraint);
                Assert.False(t.HasValueTypeConstraint);
                Assert.False(t.HasUnmanagedTypeConstraint);
                Assert.False(t.HasNotNullConstraint);
                Assert.True(t.AllowsByRefLike);
            }

            CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (5,26): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //         where T : allows ref struct
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "ref struct").WithArguments("ref struct interfaces").WithLocation(5, 26)
                );

            CreateCompilation(src, targetFramework: TargetFramework.DesktopLatestExtended, parseOptions: TestOptions.RegularNext).VerifyDiagnostics(
                // (5,26): error CS9500: Target runtime doesn't support by-ref-like generics.
                //         where T : allows ref struct
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "ref struct").WithLocation(5, 26)
                );
        }

        [Fact]
        public void AllowsConstraint_03_TwoRefStructInARow()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, ref struct
{
}

public class D<T>
    where T : allows ref struct, ref
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,34): error CS9501: 'ref struct' is already specified.
                //     where T : allows ref struct, ref struct
                Diagnostic(ErrorCode.ERR_RefStructConstraintAlreadySpecified, "ref struct").WithLocation(3, 34),
                // (8,34): error CS9501: 'ref struct' is already specified.
                //     where T : allows ref struct, ref
                Diagnostic(ErrorCode.ERR_RefStructConstraintAlreadySpecified, @"ref
").WithLocation(8, 34),
                // (8,37): error CS1003: Syntax error, 'struct' expected
                //     where T : allows ref struct, ref
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments("struct").WithLocation(8, 37)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);

            var d = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("D");
            var dt = d.TypeParameters.Single();
            Assert.False(dt.HasReferenceTypeConstraint);
            Assert.False(dt.HasValueTypeConstraint);
            Assert.False(dt.HasUnmanagedTypeConstraint);
            Assert.False(dt.HasNotNullConstraint);
            Assert.True(dt.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_04_TwoAllows()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //     where T : allows ref struct, allows ref struct
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(3, 15)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var ct = c.TypeParameters.Single();
            Assert.False(ct.HasReferenceTypeConstraint);
            Assert.False(ct.HasValueTypeConstraint);
            Assert.False(ct.HasUnmanagedTypeConstraint);
            Assert.False(ct.HasNotNullConstraint);
            Assert.True(ct.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_05_FollowedByStruct()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //     where T : allows ref struct, struct
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(3, 15),
                // (3,34): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                //     where T : allows ref struct, struct
                Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "struct").WithLocation(3, 34)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.True(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_06_AfterStruct()
        {
            var src = @"
public class C<T>
    where T : struct, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.True(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_07_FollowedByClass()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, class
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (2,16): error CS9503: Cannot allow ref structs for a type parameter known from other constraints to be a class
                // public class C<T>
                Diagnostic(ErrorCode.ERR_ClassIsCombinedWithRefStruct, "T").WithLocation(2, 16),
                // (3,15): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //     where T : allows ref struct, class
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(3, 15),
                // (3,34): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                //     where T : allows ref struct, class
                Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "class").WithLocation(3, 34)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.True(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_08_AfterClass()
        {
            var src = @"
public class C<T>
    where T : class, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (2,16): error CS9503: Cannot allow ref structs for a type parameter known from other constraints to be a class
                // public class C<T>
                Diagnostic(ErrorCode.ERR_ClassIsCombinedWithRefStruct, "T").WithLocation(2, 16)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.True(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_09_FollowedByDefault()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, default
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //     where T : allows ref struct, default
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(3, 15),
                // (3,34): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                //     where T : allows ref struct, default
                Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(3, 34),
                // (3,34): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                //     where T : allows ref struct, default
                Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "default").WithLocation(3, 34)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_10_FollowedByDefault()
        {
            var src = @"
public class C
{
    public void M<T>()
        where T : allows ref struct, default
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (5,19): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //         where T : allows ref struct, default
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(5, 19),
                // (5,38): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                //         where T : allows ref struct, default
                Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(5, 38),
                // (5,38): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                //         where T : allows ref struct, default
                Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "default").WithLocation(5, 38)
                );

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_11_FollowedByDefault()
        {
            var src = @"
public class C : B
{
    public override void M<T>()
        where T : allows ref struct, default
    {
    }
}

public class B
{
    public virtual void M<T>()
        where T : allows ref struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (5,19): error CS0460: Constraints for override and explicit interface implementation methods are inherited from the base method, so they cannot be specified directly, except for either a 'class', or a 'struct' constraint.
                //         where T : allows ref struct, default
                Diagnostic(ErrorCode.ERR_OverrideWithConstraints, "allows ref struct").WithLocation(5, 19)
                );

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_12_AfterDefault()
        {
            var src = @"
public class C<T>
    where T : default, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                //     where T : default, allows ref struct
                Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(3, 15)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_13_AfterDefault()
        {
            var src = @"
public class C
{
    public void M<T>()
        where T : default, allows ref struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (5,19): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                //         where T : default, allows ref struct
                Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(5, 19)
                );

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_14_AfterDefault()
        {
            var src = @"
public class C : B
{
    public override void M<T>()
        where T : default, allows ref struct
    {
    }
}

public class B
{
    public virtual void M<T>()
        where T : allows ref struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (5,28): error CS0460: Constraints for override and explicit interface implementation methods are inherited from the base method, so they cannot be specified directly, except for either a 'class', or a 'struct' constraint.
                //         where T : default, allows ref struct
                Diagnostic(ErrorCode.ERR_OverrideWithConstraints, "allows ref struct").WithLocation(5, 28)
                );

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_15_FollowedByUnmanaged()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, unmanaged
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //     where T : allows ref struct, unmanaged
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(3, 15),
                // (3,34): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                //     where T : allows ref struct, unmanaged
                Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "unmanaged").WithLocation(3, 34)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_16_AfterUnmanaged()
        {
            var src = @"
public class C<T>
    where T : unmanaged, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.True(t.HasValueTypeConstraint);
            Assert.True(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_17_FollowedByNotNull()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, notnull
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //     where T : allows ref struct, notnull
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(3, 15),
                // (3,34): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                //     where T : allows ref struct, notnull
                Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "notnull").WithLocation(3, 34)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.True(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_18_AfterNotNull()
        {
            var src = @"
public class C<T>
    where T : notnull, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.True(t.HasNotNullConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_19_FollowedByType()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, I1
{
}

public interface I1 {}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //     where T : allows ref struct, notnull
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(3, 15)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);

            Assert.Equal("I1", t.ConstraintTypesNoUseSiteDiagnostics.Single().ToTestDisplayString());

            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_20_AfterType()
        {
            var src = @"
public class C<T>
    where T : I1, allows ref struct
{
}

public interface I1 {}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);

            Assert.Equal("I1", t.ConstraintTypesNoUseSiteDiagnostics.Single().ToTestDisplayString());

            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_21_AfterClassType()
        {
            var src = @"
public class C<T>
    where T : C1, allows ref struct
{
}

public class C1 {}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (2,16): error CS9503: Cannot allow ref structs for a type parameter known from other constraints to be a class
                // public class C<T>
                Diagnostic(ErrorCode.ERR_ClassIsCombinedWithRefStruct, "T").WithLocation(2, 16)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);

            Assert.Equal("C1", t.ConstraintTypesNoUseSiteDiagnostics.Single().ToTestDisplayString());

            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_22_AfterSystemValueType()
        {
            var src = @"
public class C<T>
    where T : System.ValueType, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS0702: Constraint cannot be special class 'ValueType'
                //     where T : System.ValueType, allows ref struct
                Diagnostic(ErrorCode.ERR_SpecialTypeAsBound, "System.ValueType").WithArguments("System.ValueType").WithLocation(3, 15)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);

            Assert.Empty(t.ConstraintTypesNoUseSiteDiagnostics);

            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_23_AfterSystemEnum()
        {
            var src = @"
public class C<T>
    where T : System.Enum, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);

            Assert.Equal("System.Enum", t.ConstraintTypesNoUseSiteDiagnostics.Single().ToTestDisplayString());

            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_24_FollowedByNew()
        {
            var src = @"
public class C<T>
    where T : allows ref struct, new()
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (3,15): error CS9502: The 'allows' constraint clause must be the last constraint specified
                //     where T : allows ref struct, new()
                Diagnostic(ErrorCode.ERR_AllowsClauseMustBeLast, "allows").WithLocation(3, 15)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.HasConstructorConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_25_AfterNew()
        {
            var src = @"
public class C<T>
    where T : new(), allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.HasReferenceTypeConstraint);
            Assert.False(t.HasValueTypeConstraint);
            Assert.False(t.HasUnmanagedTypeConstraint);
            Assert.False(t.HasNotNullConstraint);
            Assert.True(t.HasConstructorConstraint);
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_26_PartialTypes()
        {
            var src = @"
partial class C<T> where T : allows ref struct
{
}

partial class C<T> where T : allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_27_PartialTypes()
        {
            var src = @"
partial class C<T>
{
}

partial class C<T> where T : allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_28_PartialTypes()
        {
            var src = @"
partial class C<T> where T : allows ref struct
{
}

partial class C<T>
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_29_PartialTypes()
        {
            var src = @"
partial class C<T> where T : struct
{
}

partial class C<T> where T : struct, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (2,15): error CS0265: Partial declarations of 'C<T>' have inconsistent constraints for type parameter 'T'
                // partial class C<T> where T : struct
                Diagnostic(ErrorCode.ERR_PartialWrongConstraints, "C").WithArguments("C<T>", "T").WithLocation(2, 15)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.False(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_30_PartialTypes()
        {
            var src = @"
partial class C<T> where T : struct, allows ref struct
{
}

partial class C<T> where T : struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (2,15): error CS0265: Partial declarations of 'C<T>' have inconsistent constraints for type parameter 'T'
                // partial class C<T> where T : struct, allows ref struct
                Diagnostic(ErrorCode.ERR_PartialWrongConstraints, "C").WithArguments("C<T>", "T").WithLocation(2, 15)
                );

            var c = comp.SourceModule.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            var t = c.TypeParameters.Single();
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_31_PartialMethod()
        {
            var src = @"
partial class C
{
    partial void M<T>() where T : allows ref struct;
}

partial class C
{
    partial void M<T>() where T : allows ref struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_32_PartialMethod()
        {
            var src = @"
partial class C
{
    partial void M<T>();
}

partial class C
{
    partial void M<T>() where T : allows ref struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (9,18): error CS0761: Partial method declarations of 'C.M<T>()' have inconsistent constraints for type parameter 'T'
                //     partial void M<T>() where T : allows ref struct
                Diagnostic(ErrorCode.ERR_PartialMethodInconsistentConstraints, "M").WithArguments("C.M<T>()", "T").WithLocation(9, 18)
                );

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.False(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_33_PartialMethod()
        {
            var src = @"
partial class C
{
    partial void M<T>() where T : allows ref struct;
}

partial class C
{
    partial void M<T>()
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (9,18): error CS0761: Partial method declarations of 'C.M<T>()' have inconsistent constraints for type parameter 'T'
                //     partial void M<T>()
                Diagnostic(ErrorCode.ERR_PartialMethodInconsistentConstraints, "M").WithArguments("C.M<T>()", "T").WithLocation(9, 18)
                );

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_34_PartialMethod()
        {
            var src = @"
partial class C
{
    partial void M<T>() where T : struct;
}

partial class C
{
    partial void M<T>() where T : struct, allows ref struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (9,18): error CS0761: Partial method declarations of 'C.M<T>()' have inconsistent constraints for type parameter 'T'
                //     partial void M<T>() where T : struct, allows ref struct
                Diagnostic(ErrorCode.ERR_PartialMethodInconsistentConstraints, "M").WithArguments("C.M<T>()", "T").WithLocation(9, 18)
                );

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.False(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_35_PartialMethod()
        {
            var src = @"
partial class C
{
    partial void M<T>() where T : struct, allows ref struct;
}

partial class C
{
    partial void M<T>() where T : struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (9,18): error CS0761: Partial method declarations of 'C.M<T>()' have inconsistent constraints for type parameter 'T'
                //     partial void M<T>() where T : struct
                Diagnostic(ErrorCode.ERR_PartialMethodInconsistentConstraints, "M").WithArguments("C.M<T>()", "T").WithLocation(9, 18)
                );

            var method = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C.M");
            var t = method.TypeParameters.Single();
            Assert.True(t.AllowsByRefLike);
        }

        [Fact]
        public void AllowsConstraint_36_InheritedByOverride()
        {
            var src = @"
class C1
{
    public virtual void M1<T>() where T : allows ref struct
    {
    }
    public virtual void M2<T>() where T : unmanaged
    {
    }
}

class C2 : C1
{
    public override void M1<T>() where T : allows ref struct
    {
    }
}

class C3 : C1
{
    public override void M2<T>() where T : unmanaged
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (14,44): error CS0460: Constraints for override and explicit interface implementation methods are inherited from the base method, so they cannot be specified directly, except for either a 'class', or a 'struct' constraint.
                //     public override void M1<T>() where T : allows ref struct
                Diagnostic(ErrorCode.ERR_OverrideWithConstraints, "allows ref struct").WithLocation(14, 44),
                // (21,44): error CS0460: Constraints for override and explicit interface implementation methods are inherited from the base method, so they cannot be specified directly, except for either a 'class', or a 'struct' constraint.
                //     public override void M2<T>() where T : unmanaged
                Diagnostic(ErrorCode.ERR_OverrideWithConstraints, "unmanaged").WithLocation(21, 44)
                );

            var method1 = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.M1");
            var t1 = method1.TypeParameters.Single();
            Assert.True(t1.AllowsByRefLike);

            var method2 = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C3.M2");
            var t2 = method2.TypeParameters.Single();
            Assert.True(t2.HasUnmanagedTypeConstraint);
        }

        [Fact]
        public void AllowsConstraint_37_InheritedByOverride()
        {
            var src1 = @"
public class C1
{
    public virtual void M1<T>() where T : allows ref struct
    {
    }
    public virtual void M2<T>() where T : unmanaged
    {
    }
}
";

            var src2 = @"
class C2 : C1
{
    public override void M1<T>()
    {
    }
    public override void M2<T>()
    {
    }
}
";
            var comp1 = CreateCompilation([src1, src2], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp1.VerifyDiagnostics();

            var method1 = comp1.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.M1");
            var t1 = method1.TypeParameters.Single();
            Assert.True(t1.AllowsByRefLike);

            var method2 = comp1.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.M2");
            var t2 = method2.TypeParameters.Single();
            Assert.True(t2.HasUnmanagedTypeConstraint);

            CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (4,29): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public override void M1<T>()
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "T").WithArguments("ref struct interfaces").WithLocation(4, 29)
                );

            var comp2 = CreateCompilation(src1, targetFramework: TargetFramework.Net70);

            CreateCompilation(src2, references: [comp2.ToMetadataReference()], targetFramework: TargetFramework.Net70).VerifyDiagnostics(
                // (4,29): error CS9500: Target runtime doesn't support by-ref-like generics.
                //     public override void M1<T>()
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "T").WithLocation(4, 29)
                );
        }

        [Fact]
        public void AllowsConstraint_38_InheritedByOverride()
        {
            var src = @"
class C1<S>
{
    public virtual void M1<T>() where T : S, allows ref struct
    {
    }
    public virtual void M2<T>() where T : class, S
    {
    }
}

class C2 : C1<C>
{
    public override void M1<T>()
    {
    }
    public override void M2<T>()
    {
    }
}

class C {}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var method1 = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.M1");
            var t1 = method1.TypeParameters.Single();
            Assert.True(t1.AllowsByRefLike);
            Assert.Equal("C", t1.ConstraintTypesNoUseSiteDiagnostics.Single().ToTestDisplayString());

            var method2 = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.M2");
            var t2 = method2.TypeParameters.Single();
            Assert.True(t2.HasReferenceTypeConstraint);
            Assert.Equal("C", t2.ConstraintTypesNoUseSiteDiagnostics.Single().ToTestDisplayString());
        }

        [Fact]
        public void AllowsConstraint_39_InheritedByExplicitImplementation()
        {
            var src = @"
interface C1
{
    void M1<T>() where T : allows ref struct;
    void M2<T>() where T : unmanaged;
}

class C2 : C1
{
    void C1.M1<T>() where T : allows ref struct
    {
    }

    void C1.M2<T>() where T : unmanaged
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (10,31): error CS0460: Constraints for override and explicit interface implementation methods are inherited from the base method, so they cannot be specified directly, except for either a 'class', or a 'struct' constraint.
                //     void C1.M1<T>() where T : allows ref struct
                Diagnostic(ErrorCode.ERR_OverrideWithConstraints, "allows ref struct").WithLocation(10, 31),
                // (14,31): error CS0460: Constraints for override and explicit interface implementation methods are inherited from the base method, so they cannot be specified directly, except for either a 'class', or a 'struct' constraint.
                //     void C1.M2<T>() where T : unmanaged
                Diagnostic(ErrorCode.ERR_OverrideWithConstraints, "unmanaged").WithLocation(14, 31)
                );

            var method1 = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.C1.M1");
            var t1 = method1.TypeParameters.Single();
            Assert.True(t1.AllowsByRefLike);

            var method2 = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.C1.M2");
            var t2 = method2.TypeParameters.Single();
            Assert.True(t2.HasUnmanagedTypeConstraint);
        }

        [Fact]
        public void AllowsConstraint_40_InheritedByExplicitImplementation()
        {
            var src1 = @"
public interface C1
{
    void M1<T>() where T : allows ref struct;
    void M2<T>() where T : unmanaged;
}
";

            var src2 = @"
class C2 : C1
{
    void C1.M1<T>()
    {
    }
    void C1.M2<T>()
    {
    }
}
";
            var comp1 = CreateCompilation([src1, src2], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp1.VerifyDiagnostics();

            var method1 = comp1.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.C1.M1");
            var t1 = method1.TypeParameters.Single();
            Assert.True(t1.AllowsByRefLike);

            var method2 = comp1.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.C1.M2");
            var t2 = method2.TypeParameters.Single();
            Assert.True(t2.HasUnmanagedTypeConstraint);

            CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (4,16): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     void C1.M1<T>()
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "T").WithArguments("ref struct interfaces").WithLocation(4, 16)
                );

            var comp2 = CreateCompilation(src1, targetFramework: TargetFramework.Net70);

            CreateCompilation(src2, references: [comp2.ToMetadataReference()], targetFramework: TargetFramework.Net70).VerifyDiagnostics(
                // (4,16): error CS9500: Target runtime doesn't support by-ref-like generics.
                //     void C1.M1<T>()
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "T").WithLocation(4, 16)
                );
        }

        [Fact]
        public void AllowsConstraint_41_InheritedByExplicitImplementation()
        {
            var src = @"
interface C1<S>
{
    void M1<T>() where T : S, allows ref struct;
    void M2<T>() where T : class, S;
}

class C2 : C1<C>
{
    void C1<C>.M1<T>()
    {
    }
    void C1<C>.M2<T>()
    {
    }
}

class C {}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();

            var method1 = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.C1<C>.M1");
            var t1 = method1.TypeParameters.Single();
            Assert.True(t1.AllowsByRefLike);
            Assert.Equal("C", t1.ConstraintTypesNoUseSiteDiagnostics.Single().ToTestDisplayString());

            var method2 = comp.SourceModule.GlobalNamespace.GetMember<MethodSymbol>("C2.C1<C>.M2");
            var t2 = method2.TypeParameters.Single();
            Assert.True(t2.HasReferenceTypeConstraint);
            Assert.Equal("C", t2.ConstraintTypesNoUseSiteDiagnostics.Single().ToTestDisplayString());
        }

        [Fact]
        public void AllowsConstraint_42_ImplicitImplementationMustMatch()
        {
            var src = @"
interface C1
{
    void M1<T>() where T : allows ref struct;
    void M2<T>() where T : unmanaged;
}

class C2 : C1
{
    public void M1<T>() where T : allows ref struct
    {
    }

    public void M2<T>() where T : unmanaged
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics();
        }

        [Fact]
        public void AllowsConstraint_43_ImplicitImplementationMustMatch()
        {
            var src = @"
interface C1
{
    void M1<T>() where T : allows ref struct;
    void M2<T>() where T : unmanaged;
}

class C2 : C1
{
    public void M1<T>()
    {
    }
    public void M2<T>()
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (10,17): error CS0425: The constraints for type parameter 'T' of method 'C2.M1<T>()' must match the constraints for type parameter 'T' of interface method 'C1.M1<T>()'. Consider using an explicit interface implementation instead.
                //     public void M1<T>()
                Diagnostic(ErrorCode.ERR_ImplBadConstraints, "M1").WithArguments("T", "C2.M1<T>()", "T", "C1.M1<T>()").WithLocation(10, 17),
                // (13,17): error CS0425: The constraints for type parameter 'T' of method 'C2.M2<T>()' must match the constraints for type parameter 'T' of interface method 'C1.M2<T>()'. Consider using an explicit interface implementation instead.
                //     public void M2<T>()
                Diagnostic(ErrorCode.ERR_ImplBadConstraints, "M2").WithArguments("T", "C2.M2<T>()", "T", "C1.M2<T>()").WithLocation(13, 17)
                );
        }

        [Fact]
        public void AllowsConstraint_44_ImplicitImplementationMustMatch()
        {
            var src = @"
interface C1<S>
{
    void M1<T>() where T : S, allows ref struct;
    void M2<T>() where T : class, S;
}

class C2 : C1<C>
{
    public void M1<T>() where T : C, allows ref struct
    {
    }
    public void M2<T>() where T : class, C
    {
    }
}

class C {}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (10,20): error CS9503: Cannot allow ref structs for a type parameter known from other constraints to be a class
                //     public void M1<T>() where T : C, allows ref struct
                Diagnostic(ErrorCode.ERR_ClassIsCombinedWithRefStruct, "T").WithLocation(10, 20),
                // (13,17): error CS0425: The constraints for type parameter 'T' of method 'C2.M2<T>()' must match the constraints for type parameter 'T' of interface method 'C1<C>.M2<T>()'. Consider using an explicit interface implementation instead.
                //     public void M2<T>() where T : class, C
                Diagnostic(ErrorCode.ERR_ImplBadConstraints, "M2").WithArguments("T", "C2.M2<T>()", "T", "C1<C>.M2<T>()").WithLocation(13, 17),
                // (13,42): error CS0450: 'C': cannot specify both a constraint class and the 'class' or 'struct' constraint
                //     public void M2<T>() where T : class, C
                Diagnostic(ErrorCode.ERR_RefValBoundWithClass, "C").WithArguments("C").WithLocation(13, 42)
                );
        }

        [Fact]
        public void AllowsConstraint_45_NotPresent()
        {
            var src = @"
public class C<T>
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                var c = m.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
                var t = c.TypeParameters.Single();
                Assert.False(t.HasReferenceTypeConstraint);
                Assert.False(t.HasValueTypeConstraint);
                Assert.False(t.HasUnmanagedTypeConstraint);
                Assert.False(t.HasNotNullConstraint);
                Assert.False(t.AllowsByRefLike);
                Assert.False(t.GetPublicSymbol().AllowsByRefLike);
            }
        }

        [Fact]
        public void AllowsConstraint_46()
        {
            var src = @"
class C<T, U>
    where T : allows ref struct
    where U : T, allows ref struct
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                var c = m.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
                var t = c.TypeParameters[0];
                Assert.False(t.HasReferenceTypeConstraint);
                Assert.False(t.HasValueTypeConstraint);
                Assert.False(t.HasUnmanagedTypeConstraint);
                Assert.False(t.HasNotNullConstraint);
                Assert.True(t.AllowsByRefLike);

                var u = c.TypeParameters[1];
                Assert.False(u.HasReferenceTypeConstraint);
                Assert.False(u.HasValueTypeConstraint);
                Assert.False(u.HasUnmanagedTypeConstraint);
                Assert.False(u.HasNotNullConstraint);
                Assert.True(u.AllowsByRefLike);
            }
        }

        [Fact]
        public void AllowsConstraint_47()
        {
            var src = @"
class C<T, U>
    where T : allows ref struct
    where U : T
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                var c = m.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
                var t = c.TypeParameters[0];
                Assert.False(t.HasReferenceTypeConstraint);
                Assert.False(t.HasValueTypeConstraint);
                Assert.False(t.HasUnmanagedTypeConstraint);
                Assert.False(t.HasNotNullConstraint);
                Assert.True(t.AllowsByRefLike);

                var u = c.TypeParameters[1];
                Assert.False(u.HasReferenceTypeConstraint);
                Assert.False(u.HasValueTypeConstraint);
                Assert.False(u.HasUnmanagedTypeConstraint);
                Assert.False(u.HasNotNullConstraint);
                Assert.False(u.AllowsByRefLike);
            }
        }

        [Fact]
        public void ImplementAnInterface_01()
        {
            var src = @"
interface I1
{}

ref struct S1 : I1
{}
";
            var comp = CreateCompilation(src);

            CompileAndVerify(comp, sourceSymbolValidator: verify, symbolValidator: verify).VerifyDiagnostics();

            void verify(ModuleSymbol m)
            {
                var s1 = m.GlobalNamespace.GetMember<NamedTypeSymbol>("S1");
                Assert.Equal("I1", s1.InterfacesNoUseSiteDiagnostics().Single().ToTestDisplayString());
            }

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();

            CreateCompilation(src, targetFramework: TargetFramework.Net80, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (5,17): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                // ref struct S1 : I1
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "I1").WithArguments("ref struct interfaces").WithLocation(5, 17)
                );
        }

        [Fact]
        public void ImplementAnInterface_02_IllegalBoxing()
        {
            var src = @"
interface I1
{}

ref struct S1 : I1
{}

class C
{
    static I1 Test1(S1 x) => x;
    static I1 Test2(S1 x) => (I1)x;
}
";
            var comp = CreateCompilation(src);

            comp.VerifyDiagnostics(
                // (10,30): error CS0029: Cannot implicitly convert type 'S1' to 'I1'
                //     static I1 Test1(S1 x) => x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("S1", "I1").WithLocation(10, 30),
                // (11,30): error CS0030: Cannot convert type 'S1' to 'I1'
                //     static I1 Test2(S1 x) => (I1)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(I1)x").WithArguments("S1", "I1").WithLocation(11, 30)
                );
        }

        [Fact]
        public void ImplementAnInterface_03()
        {
            var src = @"
interface I1
{
    void M1();
}

ref struct S1 : I1
{
    public void M1()
    {
        System.Console.Write(""S1.M1"");
    }
}

class C
{
    static void Main()
    {
        Test(new S1());
    }
    
    static void Test<T>(T x) where T : I1
    {
        x.M1();
    }
}
";
            var comp = CreateCompilation(src);

            comp.VerifyDiagnostics(
                // (19,9): error CS9504: The type 'S1' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'C.Test<T>(T)'
                //         Test(new S1());
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "Test").WithArguments("C.Test<T>(T)", "T", "S1").WithLocation(19, 9)
                );
        }

        [Fact]
        public void ImplementAnInterface_04()
        {
            var src = @"
interface I1
{
    void M1();
}

ref struct S1 : I1
{
    public void M1()
    {
        System.Console.Write(""S1.M1"");
    }
}

class C
{
    static void Main()
    {
        Test1(new S1());
        System.Console.Write("" "");
        Test2(new S1());
    }
    
    static void Test1<T>(T x) where T : I1, allows ref struct
    {
        x.M1();
    }
    
    static void Test2<T>(T x) where T : I1, allows ref struct
    {
        Test1(x);
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"S1.M1 S1.M1" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();
            verifier.VerifyIL("C.Test1<T>(T)",
@"
{
  // Code size       14 (0xe)
  .maxstack  1
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""T""
  IL_0008:  callvirt   ""void I1.M1()""
  IL_000d:  ret
}
");
        }

        [Fact]
        public void ImplementAnInterface_05_Variance()
        {
            var src = @"
interface I1<in T>
{
    void M1(T x);
}

ref struct S1 : I1<object>
{
    public void M1(object x)
    {
        System.Console.Write(""S1.M1"");
        System.Console.Write("" "");
        System.Console.Write(x);
    }
}

class C
{
    static void Main()
    {
        Test(new S1(), ""y"");
    }
    
    static void Test<T>(T x, string y) where T : I1<string>, allows ref struct
    {
        x.M1(y);
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"S1.M1 y" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();
            verifier.VerifyIL("C.Test<T>(T, string)",
@"
{
  // Code size       15 (0xf)
  .maxstack  2
  IL_0000:  ldarga.s   V_0
  IL_0002:  ldarg.1
  IL_0003:  constrained. ""T""
  IL_0009:  callvirt   ""void I1<string>.M1(string)""
  IL_000e:  ret
}
");
        }

        [Fact]
        public void ImplementAnInterface_06_DefaultImplementation()
        {
            var src1 = @"
public interface I1
{
    virtual void M1() {}
    static virtual void M2() {}
    sealed void M3() {}

    public class C1 {}
}

ref struct S1 : I1
{
}

ref struct S2 : I1
{
    public void M1()
    {
    }
}
";

            var src2 = @"
class C
{
    static void Test1(I1 x)
    {
        x.M1();
        x.M3();
    }

    static void Test2<T>(T x) where T : I1, allows ref struct
    {
        x.M1();
        T.M2();
#line 100
        x.M3();
        _ = new T.C1();
    }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.NetCoreApp);

            comp1.VerifyDiagnostics(
                // (11,17): error CS9505: 'I1.M1()' cannot implement interface member 'I1.M1()' for ref struct 'S1'.
                // ref struct S1 : I1
                Diagnostic(ErrorCode.ERR_RefStructDoesNotSupportDefaultInterfaceImplementationForMember, "I1").WithArguments("I1.M1()", "I1.M1()", "S1").WithLocation(11, 17)
                );

            comp1 = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            var comp2 = CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            // PROTOTYPE(RefStructInterfaces): Specification suggests to report a warning for access to virtual (non-abstract) members.
            comp2.VerifyDiagnostics(
                // (100,9): error CS9506: A non-virtual instance interface member cannot be accessed on a type parameter that allows ref struct.
                //         x.M3();
                Diagnostic(ErrorCode.ERR_BadNonVirtualInterfaceMemberAccessOnAllowsRefLike, "x.M3").WithLocation(100, 9),
                // (101,17): error CS0704: Cannot do non-virtual member lookup in 'T' because it is a type parameter
                //         _ = new T.C1();
                Diagnostic(ErrorCode.ERR_LookupInTypeVariable, "T.C1").WithArguments("T").WithLocation(101, 17)
                );
        }

        [Fact]
        public void ImplementAnInterface_07_DefaultImplementation()
        {
            var src1 = @"
public interface I1
{
    virtual int P1 => 1;
    static virtual int P2 => 2;
    sealed int P3 => 3;
}

ref struct S1 : I1
{
}

ref struct S2 : I1
{
    public int P1 => 21;
}
";

            var src2 = @"
class C
{
    static void Test1(I1 x)
    {
        _ = x.P1;
        _ = x.P3;
    }

    static void Test2<T>(T x) where T : I1, allows ref struct
    {
        _ = x.P1;
        _ = T.P2;
#line 100
        _ = x.P3;
    }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.NetCoreApp);

            comp1.VerifyDiagnostics(
                // (9,17): error CS9505: 'I1.P1.get' cannot implement interface member 'I1.P1.get' for ref struct 'S1'.
                // ref struct S1 : I1
                Diagnostic(ErrorCode.ERR_RefStructDoesNotSupportDefaultInterfaceImplementationForMember, "I1").WithArguments("I1.P1.get", "I1.P1.get", "S1").WithLocation(9, 17)
                );

            comp1 = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            var comp2 = CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            // PROTOTYPE(RefStructInterfaces): Specification suggests to report a warning for access to virtual (non-abstract) members.
            comp2.VerifyDiagnostics(
                // (100,13): error CS9506: A non-virtual instance interface member cannot be accessed on a type parameter that allows ref struct.
                //         _ = x.P3;
                Diagnostic(ErrorCode.ERR_BadNonVirtualInterfaceMemberAccessOnAllowsRefLike, "x.P3").WithLocation(100, 13)
                );
        }

        [Fact]
        public void ImplementAnInterface_08_DefaultImplementation()
        {
            var src1 = @"
public interface I1
{
    virtual int P1 {set{}}
    static virtual int P2 {set{}}
    sealed int P3 {set{}}
}

ref struct S1 : I1
{
}

ref struct S2 : I1
{
    public int P1 {set{}}
}
";

            var src2 = @"
class C
{
    static void Test1(I1 x)
    {
        x.P1 = 11;
        x.P3 = 11;
    }

    static void Test2<T>(T x) where T : I1, allows ref struct
    {
        x.P1 = 123;
        T.P2 = 123;
        x.P3 = 123;
    }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.NetCoreApp);

            comp1.VerifyDiagnostics(
                // (9,17): error CS9505: 'I1.P1.set' cannot implement interface member 'I1.P1.set' for ref struct 'S1'.
                // ref struct S1 : I1
                Diagnostic(ErrorCode.ERR_RefStructDoesNotSupportDefaultInterfaceImplementationForMember, "I1").WithArguments("I1.P1.set", "I1.P1.set", "S1").WithLocation(9, 17)
                );

            comp1 = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            var comp2 = CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            // PROTOTYPE(RefStructInterfaces): Specification suggests to report a warning for access to virtual (non-abstract) members.
            comp2.VerifyDiagnostics(
                // (14,9): error CS9506: A non-virtual instance interface member cannot be accessed on a type parameter that allows ref struct.
                //         x.P3 = 123;
                Diagnostic(ErrorCode.ERR_BadNonVirtualInterfaceMemberAccessOnAllowsRefLike, "x.P3").WithLocation(14, 9)
                );
        }

        [Fact]
        public void ImplementAnInterface_09_DefaultImplementation()
        {
            var src1 = @"
public interface I1
{
    virtual int this[int x] => 1;
}

public interface I2
{
    sealed int this[long x] => 1;
}

ref struct S1 : I1, I2
{
}

ref struct S2 : I1, I2
{
    public int this[int x] => 21;
}
";

            var src2 = @"
class C
{
    static void Test1(I1 x)
    {
        _ = x[1];
    }

    static void Test1(I2 x)
    {
        _ = x[2];
    }

    static void Test2<T>(T x) where T : I1, allows ref struct
    {
        _ = x[3];
    }

    static void Test3<T>(T x) where T : I2, allows ref struct
    {
        _ = x[4];
    }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.NetCoreApp);

            comp1.VerifyDiagnostics(
                // (12,17): error CS9505: 'I1.this[int].get' cannot implement interface member 'I1.this[int].get' for ref struct 'S1'.
                // ref struct S1 : I1, I2
                Diagnostic(ErrorCode.ERR_RefStructDoesNotSupportDefaultInterfaceImplementationForMember, "I1").WithArguments("I1.this[int].get", "I1.this[int].get", "S1").WithLocation(12, 17)
                );

            comp1 = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            var comp2 = CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            // PROTOTYPE(RefStructInterfaces): Specification suggests to report a warning for access to virtual (non-abstract) members.
            comp2.VerifyDiagnostics(
                // (21,13): error CS9506: A non-virtual instance interface member cannot be accessed on a type parameter that allows ref struct.
                //         _ = x[4];
                Diagnostic(ErrorCode.ERR_BadNonVirtualInterfaceMemberAccessOnAllowsRefLike, "x[4]").WithLocation(21, 13)
                );
        }

        [Fact]
        public void ImplementAnInterface_10_DefaultImplementation()
        {
            var src1 = @"
public interface I1
{
    virtual int this[int x] {set{}}
}

public interface I2
{
    sealed int this[long x] {set{}}
}

ref struct S1 : I1, I2
{
}

ref struct S2 : I1, I2
{
    public int this[int x] {set{}}
}
";

            var src2 = @"
class C
{
    static void Test1(I1 x)
    {
        x[1] = 1;
    }

    static void Test1(I2 x)
    {
        x[2] = 2;
    }

    static void Test2<T>(T x) where T : I1, allows ref struct
    {
        x[3] = 3;
    }

    static void Test3<T>(T x) where T : I2, allows ref struct
    {
        x[4] = 4;
    }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.NetCoreApp);

            comp1.VerifyDiagnostics(
                // (12,17): error CS9505: 'I1.this[int].set' cannot implement interface member 'I1.this[int].set' for ref struct 'S1'.
                // ref struct S1 : I1, I2
                Diagnostic(ErrorCode.ERR_RefStructDoesNotSupportDefaultInterfaceImplementationForMember, "I1").WithArguments("I1.this[int].set", "I1.this[int].set", "S1").WithLocation(12, 17)
                );

            comp1 = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            var comp2 = CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            // PROTOTYPE(RefStructInterfaces): Specification suggests to report a warning for access to virtual (non-abstract) members.
            comp2.VerifyDiagnostics(
                // (21,9): error CS9506: A non-virtual instance interface member cannot be accessed on a type parameter that allows ref struct.
                //         x[4] = 4;
                Diagnostic(ErrorCode.ERR_BadNonVirtualInterfaceMemberAccessOnAllowsRefLike, "x[4]").WithLocation(21, 9)
                );
        }

        [Fact]
        public void ImplementAnInterface_11_DefaultImplementation()
        {
            var src1 = @"
public interface I1
{
    virtual event System.Action E1 {add{} remove{}}
    static virtual event System.Action E2 {add{} remove{}}
    sealed event System.Action E3 {add{} remove{}}
}

ref struct S1 : I1
{
}

ref struct S2 : I1
{
#pragma warning disable CS0067 // The event 'S2.E1' is never used
    public event System.Action E1;
}
";

            var src2 = @"
class C
{
    static void Test1(I1 x)
    {
        x.E1 += null;
        x.E3 += null;
        x.E1 -= null;
        x.E3 -= null;
    }

    static void Test2<T>(T x) where T : I1, allows ref struct
    {
        x.E1 += null;
        T.E2 += null;
#line 100
        x.E3 += null;
        x.E1 -= null;
        T.E2 -= null;
#line 200
        x.E3 -= null;
    }
}
";
            var comp1 = CreateCompilation(src1, targetFramework: TargetFramework.NetCoreApp);

            comp1.VerifyDiagnostics(
                // (9,17): error CS9505: 'I1.E1.add' cannot implement interface member 'I1.E1.add' for ref struct 'S1'.
                // ref struct S1 : I1
                Diagnostic(ErrorCode.ERR_RefStructDoesNotSupportDefaultInterfaceImplementationForMember, "I1").WithArguments("I1.E1.add", "I1.E1.add", "S1").WithLocation(9, 17),
                // (9,17): error CS9505: 'I1.E1.remove' cannot implement interface member 'I1.E1.remove' for ref struct 'S1'.
                // ref struct S1 : I1
                Diagnostic(ErrorCode.ERR_RefStructDoesNotSupportDefaultInterfaceImplementationForMember, "I1").WithArguments("I1.E1.remove", "I1.E1.remove", "S1").WithLocation(9, 17)
                );

            comp1 = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            var comp2 = CreateCompilation(src2, references: [comp1.ToMetadataReference()], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            // PROTOTYPE(RefStructInterfaces): Specification suggests to report a warning for access to virtual (non-abstract) members.
            comp2.VerifyDiagnostics(
                // (100,9): error CS9506: A non-virtual instance interface member cannot be accessed on a type parameter that allows ref struct.
                //         x.E3 += null;
                Diagnostic(ErrorCode.ERR_BadNonVirtualInterfaceMemberAccessOnAllowsRefLike, "x.E3 += null").WithLocation(100, 9),
                // (200,9): error CS9506: A non-virtual instance interface member cannot be accessed on a type parameter that allows ref struct.
                //         x.E3 -= null;
                Diagnostic(ErrorCode.ERR_BadNonVirtualInterfaceMemberAccessOnAllowsRefLike, "x.E3 -= null").WithLocation(200, 9)
                );
        }

        [Fact]
        public void ImplementAnInterface_12_Variance_ErrorScenarios()
        {
            var src = @"
interface I<T>
{
    void M(T t);
}
interface IOut<out T>
{
    T MOut();
}
ref struct S : I<object>, IOut<object>
{
    public void M(object o) { }
    public object MOut() => null;
}
class Program
{
    static void Main()
    {
        Test1(new S());
        Test2(new S());
    }
    static void Test1<T>(T x) where T : I<string>, allows ref struct
    {
    }
    static void Test2<T>(T x) where T : IOut<string>, allows ref struct
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (19,9): error CS0315: The type 'S' cannot be used as type parameter 'T' in the generic type or method 'Program.Test1<T>(T)'. There is no boxing conversion from 'S' to 'I<string>'.
                //         Test1(new S());
                Diagnostic(ErrorCode.ERR_GenericConstraintNotSatisfiedValType, "Test1").WithArguments("Program.Test1<T>(T)", "I<string>", "T", "S").WithLocation(19, 9),
                // (20,9): error CS0315: The type 'S' cannot be used as type parameter 'T' in the generic type or method 'Program.Test2<T>(T)'. There is no boxing conversion from 'S' to 'IOut<string>'.
                //         Test2(new S());
                Diagnostic(ErrorCode.ERR_GenericConstraintNotSatisfiedValType, "Test2").WithArguments("Program.Test2<T>(T)", "IOut<string>", "T", "S").WithLocation(20, 9)
                );
        }

        [Fact]
        public void Using_01()
        {
            var src = @"
ref struct S2 : System.IDisposable
{
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        using (new S2())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.Passes :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       26 (0x1a)
  .maxstack  1
  .locals init (S2 V_0)
  IL_0000:  ldloca.s   V_0
  IL_0002:  initobj    ""S2""
  .try
  {
    IL_0008:  ldc.i4.s   123
    IL_000a:  call       ""void System.Console.Write(int)""
    IL_000f:  leave.s    IL_0019
  }
  finally
  {
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       ""void S2.Dispose()""
    IL_0018:  endfinally
  }
  IL_0019:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S2()')
              Value:
                IObjectCreationOperation (Constructor: S2..ctor()) (OperationKind.ObjectCreation, Type: S2) (Syntax: 'new S2()')
                  Arguments(0)
                  Initializer:
                    null
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1]
            Statements (1)
                IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Cons ... Write(123);')
                  Expression:
                    IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Cons ... .Write(123)')
                      Instance Receiver:
                        null
                      Arguments(1):
                          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: '123')
                            ILiteralOperation (OperationKind.Literal, Type: System.Int32, Constant: 123) (Syntax: '123')
                            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            Next (Regular) Block[B4]
                Finalizing: {R4}
                Leaving: {R3} {R2} {R1}
    }
    .finally {R4}
    {
        Block[B3] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation ( void S2.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S2()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S2()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");
        }

        [Fact]
        public void Using_02()
        {
            var src = @"
ref struct S2 : System.IDisposable
{
    void System.IDisposable.Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        using (new S2())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.Passes :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       32 (0x20)
  .maxstack  1
  .locals init (S2 V_0)
  IL_0000:  ldloca.s   V_0
  IL_0002:  initobj    ""S2""
  .try
  {
    IL_0008:  ldc.i4.s   123
    IL_000a:  call       ""void System.Console.Write(int)""
    IL_000f:  leave.s    IL_001f
  }
  finally
  {
    IL_0011:  ldloca.s   V_0
    IL_0013:  constrained. ""S2""
    IL_0019:  callvirt   ""void System.IDisposable.Dispose()""
    IL_001e:  endfinally
  }
  IL_001f:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S2()')
              Value:
                IObjectCreationOperation (Constructor: S2..ctor()) (OperationKind.ObjectCreation, Type: S2) (Syntax: 'new S2()')
                  Arguments(0)
                  Initializer:
                    null
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1]
            Statements (1)
                IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Cons ... Write(123);')
                  Expression:
                    IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Cons ... .Write(123)')
                      Instance Receiver:
                        null
                      Arguments(1):
                          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: '123')
                            ILiteralOperation (OperationKind.Literal, Type: System.Int32, Constant: 123) (Syntax: '123')
                            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            Next (Regular) Block[B4]
                Finalizing: {R4}
                Leaving: {R3} {R2} {R1}
    }
    .finally {R4}
    {
        Block[B3] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S2()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S2()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");
        }

        [Fact]
        public void Using_03()
        {
            var src = @"
ref struct S2 : System.IDisposable
{
    void System.IDisposable.Dispose() => throw null;

    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        using (var x = new S2())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.Passes :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       26 (0x1a)
  .maxstack  1
  .locals init (S2 V_0) //x
  IL_0000:  ldloca.s   V_0
  IL_0002:  initobj    ""S2""
  .try
  {
    IL_0008:  ldc.i4.s   123
    IL_000a:  call       ""void System.Console.Write(int)""
    IL_000f:  leave.s    IL_0019
  }
  finally
  {
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       ""void S2.Dispose()""
    IL_0018:  endfinally
  }
  IL_0019:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    Locals: [S2 x]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: S2, IsImplicit) (Syntax: 'x = new S2()')
              Left:
                ILocalReferenceOperation: x (IsDeclaration: True) (OperationKind.LocalReference, Type: S2, IsImplicit) (Syntax: 'x = new S2()')
              Right:
                IObjectCreationOperation (Constructor: S2..ctor()) (OperationKind.ObjectCreation, Type: S2) (Syntax: 'new S2()')
                  Arguments(0)
                  Initializer:
                    null
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1]
            Statements (1)
                IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Cons ... Write(123);')
                  Expression:
                    IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Cons ... .Write(123)')
                      Instance Receiver:
                        null
                      Arguments(1):
                          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: '123')
                            ILiteralOperation (OperationKind.Literal, Type: System.Int32, Constant: 123) (Syntax: '123')
                            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            Next (Regular) Block[B4]
                Finalizing: {R4}
                Leaving: {R3} {R2} {R1}
    }
    .finally {R4}
    {
        Block[B3] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation ( void S2.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'x = new S2()')
                  Instance Receiver:
                    ILocalReferenceOperation: x (OperationKind.LocalReference, Type: S2, IsImplicit) (Syntax: 'x = new S2()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");
        }

        [Fact]
        public void Using_04()
        {
            var src = @"
ref struct S2 : System.IDisposable
{
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test(new S2());
    }

    static void Test<T>(T t) where T : System.IDisposable, allows ref struct
    {
        using (t)
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.Passes :
                    Verification.Skipped).VerifyDiagnostics();

            // The code boxes type parameter that allows ref struct, however 'box' followed by 'brfalse' is
            // documented as a valid sequence at https://github.com/dotnet/runtime/blob/main/docs/design/features/byreflike-generics.md#special-il-sequences
            //
            //  IL_000b:  ldloc.0
            //  IL_000c:  box        ""T""
            //  IL_0011:  brfalse.s  IL_0020

            verifier.VerifyIL("C.Test<T>(T)",
@"
{
  // Code size       34 (0x22)
  .maxstack  1
  .locals init (T V_0)
  IL_0000:  ldarg.0
  IL_0001:  stloc.0
  .try
  {
    IL_0002:  ldc.i4.s   123
    IL_0004:  call       ""void System.Console.Write(int)""
    IL_0009:  leave.s    IL_0021
  }
  finally
  {
    IL_000b:  ldloc.0
    IL_000c:  box        ""T""
    IL_0011:  brfalse.s  IL_0020
    IL_0013:  ldloca.s   V_0
    IL_0015:  constrained. ""T""
    IL_001b:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0020:  endfinally
  }
  IL_0021:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: T) (Syntax: 't')
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1]
            Statements (1)
                IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Cons ... Write(123);')
                  Expression:
                    IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Cons ... .Write(123)')
                      Instance Receiver:
                        null
                      Arguments(1):
                          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: '123')
                            ILiteralOperation (OperationKind.Literal, Type: System.Int32, Constant: 123) (Syntax: '123')
                            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            Next (Regular) Block[B6]
                Finalizing: {R4}
                Leaving: {R3} {R2} {R1}
    }
    .finally {R4}
    {
        Block[B3] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B5]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: T, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B4]
        Block[B4] - Block
            Predecessors: [B3]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: T, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B3] [B4]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B6] - Exit
    Predecessors: [B2]
    Statements (0)
""");
        }

        [Fact]
        public void Using_05()
        {
            var src = @"
ref struct S2 : System.IDisposable
{
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test(new S2());
    }

    static void Test<T>(T t) where T : struct, System.IDisposable, allows ref struct
    {
        using (t)
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.Passes :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<T>(T)",
@"
{
  // Code size       26 (0x1a)
  .maxstack  1
  .locals init (T V_0)
  IL_0000:  ldarg.0
  IL_0001:  stloc.0
  .try
  {
    IL_0002:  ldc.i4.s   123
    IL_0004:  call       ""void System.Console.Write(int)""
    IL_0009:  leave.s    IL_0019
  }
  finally
  {
    IL_000b:  ldloca.s   V_0
    IL_000d:  constrained. ""T""
    IL_0013:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0018:  endfinally
  }
  IL_0019:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: T) (Syntax: 't')
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1]
            Statements (1)
                IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Cons ... Write(123);')
                  Expression:
                    IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Cons ... .Write(123)')
                      Instance Receiver:
                        null
                      Arguments(1):
                          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: '123')
                            ILiteralOperation (OperationKind.Literal, Type: System.Int32, Constant: 123) (Syntax: '123')
                            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            Next (Regular) Block[B4]
                Finalizing: {R4}
                Leaving: {R3} {R2} {R1}
    }
    .finally {R4}
    {
        Block[B3] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: T, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");
        }

        [Fact]
        public void Using_06()
        {
            var src = @"
interface IMyDisposable
{
    void Dispose();
}

ref struct S2 : IMyDisposable
{
    public void Dispose()
    {
    }
}

class C
{
    static void Main()
    {
        Test(new S2(), null);
    }

    static void Test<T>(T t, IMyDisposable s) where T : IMyDisposable, allows ref struct
    {
        using (t)
        {
        }

        using (s)
        {
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            // PROTOTYPE(RefStructInterfaces): Should we adjust wording for the error? Ref struct implement interfaces, but not convertible to them.  
            comp.VerifyDiagnostics(
                // (23,16): error CS1674: 'T': type used in a using statement must be implicitly convertible to 'System.IDisposable'.
                //         using (t)
                Diagnostic(ErrorCode.ERR_NoConvToIDisp, "t").WithArguments("T").WithLocation(23, 16),
                // (27,16): error CS1674: 'IMyDisposable': type used in a using statement must be implicitly convertible to 'System.IDisposable'.
                //         using (s)
                Diagnostic(ErrorCode.ERR_NoConvToIDisp, "s").WithArguments("IMyDisposable").WithLocation(27, 16)
                );
        }

        [Fact]
        public void Foreach_IEnumerableT_01()
        {
            var src = @"
using System.Collections.Generic;

ref struct S : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator<int> Get123()
    {
        yield return 123;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

class C
{
    static void Main()
    {
        foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       49 (0x31)
  .maxstack  2
  .locals init (System.Collections.Generic.IEnumerator<int> V_0,
                S V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S""
  IL_0009:  call       ""System.Collections.Generic.IEnumerator<int> S.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001c
    IL_0011:  ldloc.0
    IL_0012:  callvirt   ""int System.Collections.Generic.IEnumerator<int>.Current.get""
    IL_0017:  call       ""void System.Console.Write(int)""
    IL_001c:  ldloc.0
    IL_001d:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0022:  brtrue.s   IL_0011
    IL_0024:  leave.s    IL_0030
  }
  finally
  {
    IL_0026:  ldloc.0
    IL_0027:  brfalse.s  IL_002f
    IL_0029:  ldloc.0
    IL_002a:  callvirt   ""void System.IDisposable.Dispose()""
    IL_002f:  endfinally
  }
  IL_0030:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation ( System.Collections.Generic.IEnumerator<System.Int32> S.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ImplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> S.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> S.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Fact]
        public void Foreach_IEnumerableT_02()
        {
            var src = @"
using System.Collections.Generic;

ref struct S : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator<int> Get123()
    {
        yield return 123;
    }

    IEnumerator<int> IEnumerable<int>.GetEnumerator() => throw null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

class C
{
    static void Main()
    {
        foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       49 (0x31)
  .maxstack  2
  .locals init (System.Collections.Generic.IEnumerator<int> V_0,
                S V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S""
  IL_0009:  call       ""System.Collections.Generic.IEnumerator<int> S.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001c
    IL_0011:  ldloc.0
    IL_0012:  callvirt   ""int System.Collections.Generic.IEnumerator<int>.Current.get""
    IL_0017:  call       ""void System.Console.Write(int)""
    IL_001c:  ldloc.0
    IL_001d:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0022:  brtrue.s   IL_0011
    IL_0024:  leave.s    IL_0030
  }
  finally
  {
    IL_0026:  ldloc.0
    IL_0027:  brfalse.s  IL_002f
    IL_0029:  ldloc.0
    IL_002a:  callvirt   ""void System.IDisposable.Dispose()""
    IL_002f:  endfinally
  }
  IL_0030:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation ( System.Collections.Generic.IEnumerator<System.Int32> S.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ImplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> S.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> S.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Fact]
        public void Foreach_IEnumerableT_03()
        {
            var src = @"
using System.Collections.Generic;

ref struct S : IEnumerable<int>
{
    IEnumerator<int> IEnumerable<int>.GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator<int> Get123()
    {
        yield return 123;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Get123();
}

class C
{
    static void Main()
    {
        foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       55 (0x37)
  .maxstack  2
  .locals init (System.Collections.Generic.IEnumerator<int> V_0,
                S V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S""
  IL_0009:  constrained. ""S""
  IL_000f:  callvirt   ""System.Collections.Generic.IEnumerator<int> System.Collections.Generic.IEnumerable<int>.GetEnumerator()""
  IL_0014:  stloc.0
  .try
  {
    IL_0015:  br.s       IL_0022
    IL_0017:  ldloc.0
    IL_0018:  callvirt   ""int System.Collections.Generic.IEnumerator<int>.Current.get""
    IL_001d:  call       ""void System.Console.Write(int)""
    IL_0022:  ldloc.0
    IL_0023:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0028:  brtrue.s   IL_0017
    IL_002a:  leave.s    IL_0036
  }
  finally
  {
    IL_002c:  ldloc.0
    IL_002d:  brfalse.s  IL_0035
    IL_002f:  ldloc.0
    IL_0030:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0035:  endfinally
  }
  IL_0036:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation (virtual System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ImplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerableT_04(bool addStructConstraint)
        {
            var src = @"
using System.Collections.Generic;

ref struct S : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator<int> Get123()
    {
        yield return 123;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

class C
{
    static void Main()
    {
        Test(new S());
    }

    static void Test<T>(T t) where T : " + (addStructConstraint ? "struct, " : "") + @"IEnumerable<int>, allows ref struct
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<T>(T)",
@"
{
  // Code size       48 (0x30)
  .maxstack  1
  .locals init (System.Collections.Generic.IEnumerator<int> V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""T""
  IL_0008:  callvirt   ""System.Collections.Generic.IEnumerator<int> System.Collections.Generic.IEnumerable<int>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_001b
    IL_0010:  ldloc.0
    IL_0011:  callvirt   ""int System.Collections.Generic.IEnumerator<int>.Current.get""
    IL_0016:  call       ""void System.Console.Write(int)""
    IL_001b:  ldloc.0
    IL_001c:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0021:  brtrue.s   IL_0010
    IL_0023:  leave.s    IL_002f
  }
  finally
  {
    IL_0025:  ldloc.0
    IL_0026:  brfalse.s  IL_002e
    IL_0028:  ldloc.0
    IL_0029:  callvirt   ""void System.IDisposable.Dispose()""
    IL_002e:  endfinally
  }
  IL_002f:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: T) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ImplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerableT_05(bool addStructConstraint)
        {
            var src = @"
using System.Collections.Generic;

interface IMyEnumerable<T>
{
    IEnumerator<T> GetEnumerator();
}

ref struct S : IMyEnumerable<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator<int> Get123()
    {
        yield return 123;
    }
}

class C
{
    static void Main()
    {
        Test(new S());
    }

    static void Test<T>(T t) where T : " + (addStructConstraint ? "struct, " : "") + @"IMyEnumerable<int>, allows ref struct
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<T>(T)",
@"
{
  // Code size       48 (0x30)
  .maxstack  1
  .locals init (System.Collections.Generic.IEnumerator<int> V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""T""
  IL_0008:  callvirt   ""System.Collections.Generic.IEnumerator<int> IMyEnumerable<int>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_001b
    IL_0010:  ldloc.0
    IL_0011:  callvirt   ""int System.Collections.Generic.IEnumerator<int>.Current.get""
    IL_0016:  call       ""void System.Console.Write(int)""
    IL_001b:  ldloc.0
    IL_001c:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0021:  brtrue.s   IL_0010
    IL_0023:  leave.s    IL_002f
  }
  finally
  {
    IL_0025:  ldloc.0
    IL_0026:  brfalse.s  IL_002e
    IL_0028:  ldloc.0
    IL_0029:  callvirt   ""void System.IDisposable.Dispose()""
    IL_002e:  endfinally
  }
  IL_002f:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual System.Collections.Generic.IEnumerator<System.Int32> IMyEnumerable<System.Int32>.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: T) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ImplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> IMyEnumerable<System.Int32>.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> IMyEnumerable<System.Int32>.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerableT_06(bool addStructConstraint)
        {
            var src = @"
using System.Collections.Generic;

interface IMyEnumerable1<T>
{
    IEnumerator<int> GetEnumerator();
}


interface IMyEnumerable2<T>
{
    IEnumerator<int> GetEnumerator();
}

ref struct S : IMyEnumerable1<int>, IMyEnumerable2<int>
{
    public IEnumerator<int> GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator<int> Get123()
    {
        yield return 123;
    }
}

class C
{
    static void Main()
    {
        Test(new S());
    }

    static void Test<T>(T t) where T : " + (addStructConstraint ? "struct, " : "") + @"IMyEnumerable1<int>, IMyEnumerable2<int>, allows ref struct
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (37,27): warning CS0278: 'T' does not implement the 'collection' pattern. 'IMyEnumerable1<int>.GetEnumerator()' is ambiguous with 'IMyEnumerable2<int>.GetEnumerator()'.
                //         foreach (var i in t)
                Diagnostic(ErrorCode.WRN_PatternIsAmbiguous, "t").WithArguments("T", "collection", "IMyEnumerable1<int>.GetEnumerator()", "IMyEnumerable2<int>.GetEnumerator()").WithLocation(37, 27),
                // (37,27): error CS1579: foreach statement cannot operate on variables of type 'T' because 'T' does not contain a public instance or extension definition for 'GetEnumerator'
                //         foreach (var i in t)
                Diagnostic(ErrorCode.ERR_ForEachMissingMember, "t").WithArguments("T", "GetEnumerator").WithLocation(37, 27)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            Assert.Null(info.GetEnumeratorMethod);
            Assert.Null(info.ElementType);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.Null(op.Info);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerableT_07(bool addStructConstraint)
        {
            var src = @"
using System.Collections.Generic;

interface IMyEnumerable1<T>
{
    IEnumerator<int> GetEnumerator();
}

interface IMyEnumerable2<T>
{
    IEnumerator<int> GetEnumerator();
}

ref struct S : IMyEnumerable1<int>, IMyEnumerable2<int>, IEnumerable<int>
{
    IEnumerator<int> IMyEnumerable1<int>.GetEnumerator() => throw null;
    IEnumerator<int> IMyEnumerable2<int>.GetEnumerator() => throw null;

    public IEnumerator<int> GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator<int> Get123()
    {
        yield return 123;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

class C
{
    static void Main()
    {
        Test(new S());
    }

    static void Test<T>(T t) where T : " + (addStructConstraint ? "struct, " : "") + @"IMyEnumerable1<int>, IMyEnumerable2<int>, IEnumerable<int>, allows ref struct
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics(
                // (41,27): warning CS0278: 'T' does not implement the 'collection' pattern. 'IMyEnumerable1<int>.GetEnumerator()' is ambiguous with 'IMyEnumerable2<int>.GetEnumerator()'.
                //         foreach (var i in t)
                Diagnostic(ErrorCode.WRN_PatternIsAmbiguous, "t").WithArguments("T", "collection", "IMyEnumerable1<int>.GetEnumerator()", "IMyEnumerable2<int>.GetEnumerator()").WithLocation(41, 27)
                );

            verifier.VerifyIL("C.Test<T>(T)",
@"
{
  // Code size       48 (0x30)
  .maxstack  1
  .locals init (System.Collections.Generic.IEnumerator<int> V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""T""
  IL_0008:  callvirt   ""System.Collections.Generic.IEnumerator<int> System.Collections.Generic.IEnumerable<int>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_001b
    IL_0010:  ldloc.0
    IL_0011:  callvirt   ""int System.Collections.Generic.IEnumerator<int>.Current.get""
    IL_0016:  call       ""void System.Console.Write(int)""
    IL_001b:  ldloc.0
    IL_001c:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0021:  brtrue.s   IL_0010
    IL_0023:  leave.s    IL_002f
  }
  finally
  {
    IL_0025:  ldloc.0
    IL_0026:  brfalse.s  IL_002e
    IL_0028:  ldloc.0
    IL_0029:  callvirt   ""void System.IDisposable.Dispose()""
    IL_002e:  endfinally
  }
  IL_002f:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: T) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ImplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IEnumerator<System.Int32>, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IEnumerator<System.Int32> System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_01(bool s1IsRefStruct)
        {
            var src = @"
using System.Collections.Generic;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator<int>
{
    bool stop;
    public int Current => 123;
    object System.Collections.IEnumerator.Current => Current;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       49 (0x31)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001d
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       ""int S2.Current.get""
    IL_0018:  call       ""void System.Console.Write(int)""
    IL_001d:  ldloca.s   V_0
    IL_001f:  call       ""bool S2.MoveNext()""
    IL_0024:  brtrue.s   IL_0011
    IL_0026:  leave.s    IL_0030
  }
  finally
  {
    IL_0028:  ldloca.s   V_0
    IL_002a:  call       ""void S2.Dispose()""
    IL_002f:  endfinally
  }
  IL_0030:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B5]
                IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 S2.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation ( void S2.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B5] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void S2.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            AssertEx.Equal("void S2.Dispose()", op.Info.PatternDisposeMethod.ToTestDisplayString());
            Assert.True(op.Info.DisposeArguments.IsEmpty);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_02(bool s1IsRefStruct)
        {
            var src = @"
using System.Collections.Generic;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator<int>
{
    bool stop;
    public int Current => 123;
    object System.Collections.IEnumerator.Current => Current;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
    public void Dispose()
    {
        System.Console.Write('D');
    }

    int IEnumerator<int>.Current => throw null;
    bool System.Collections.IEnumerator.MoveNext() => throw null;

    void System.IDisposable.Dispose() => throw null;
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       49 (0x31)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001d
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       ""int S2.Current.get""
    IL_0018:  call       ""void System.Console.Write(int)""
    IL_001d:  ldloca.s   V_0
    IL_001f:  call       ""bool S2.MoveNext()""
    IL_0024:  brtrue.s   IL_0011
    IL_0026:  leave.s    IL_0030
  }
  finally
  {
    IL_0028:  ldloca.s   V_0
    IL_002a:  call       ""void S2.Dispose()""
    IL_002f:  endfinally
  }
  IL_0030:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B5]
                IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 S2.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation ( void S2.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B5] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void S2.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            AssertEx.Equal("void S2.Dispose()", op.Info.PatternDisposeMethod.ToTestDisplayString());
            Assert.True(op.Info.DisposeArguments.IsEmpty);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_03(bool s1IsRefStruct, bool currentIsPublic, bool moveNextIsPublic, bool disposeIsPublic)
        {
            if (currentIsPublic && moveNextIsPublic)
            {
                return;
            }

            var src = @"
using System.Collections.Generic;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator<int>
{
    bool stop;

    " + (currentIsPublic ? "public int " : "int IEnumerator<int>.") + @"Current => 123;

    object System.Collections.IEnumerator.Current => 123;

    " + (moveNextIsPublic ? "public bool " : "bool System.Collections.IEnumerator.") + @"MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }

    " + (disposeIsPublic ? "public void " : "void System.IDisposable.") + @"Dispose()
    {
        System.Console.Write('D');
    }

    public void Reset() { }
}

class C
{
    static void Main()
    {
#line 100
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            if (!currentIsPublic)
            {
                comp.VerifyDiagnostics(
                    // (100,27): error CS0117: 'S2' does not contain a definition for 'Current'
                    //         foreach (var i in new S1())
                    Diagnostic(ErrorCode.ERR_NoSuchMember, "new S1()").WithArguments("S2", "Current").WithLocation(100, 27),
                    // (100,27): error CS0202: foreach requires that the return type 'S2' of 'S1.GetEnumerator()' must have a suitable public 'MoveNext' method and public 'Current' property
                    //         foreach (var i in new S1())
                    Diagnostic(ErrorCode.ERR_BadGetEnumerator, "new S1()").WithArguments("S2", "S1.GetEnumerator()").WithLocation(100, 27)
                    );
            }
            else
            {
                Assert.False(moveNextIsPublic);

                comp.VerifyDiagnostics(
                    // (100,27): error CS0117: 'S2' does not contain a definition for 'MoveNext'
                    //         foreach (var i in new S1())
                    Diagnostic(ErrorCode.ERR_NoSuchMember, "new S1()").WithArguments("S2", "MoveNext").WithLocation(100, 27),
                    // (100,27): error CS0202: foreach requires that the return type 'S2' of 'S1.GetEnumerator()' must have a suitable public 'MoveNext' method and public 'Current' property
                    //         foreach (var i in new S1())
                    Diagnostic(ErrorCode.ERR_BadGetEnumerator, "new S1()").WithArguments("S2", "S1.GetEnumerator()").WithLocation(100, 27)
                    );
            }

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
Block[B1] - Block
    Predecessors: [B0]
    Statements (1)
        IInvalidOperation (OperationKind.Invalid, Type: null, IsInvalid, IsImplicit) (Syntax: 'new S1()')
          Children(1):
              IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1, IsInvalid) (Syntax: 'new S1()')
                Arguments(0)
                Initializer:
                  null
    Next (Regular) Block[B2]
Block[B2] - Block
    Predecessors: [B1] [B3]
    Statements (0)
    Jump if False (Regular) to Block[B4]
        IInvalidOperation (OperationKind.Invalid, Type: System.Boolean, IsInvalid, IsImplicit) (Syntax: 'new S1()')
          Children(1):
              IInvalidOperation (OperationKind.Invalid, Type: null, IsInvalid, IsImplicit) (Syntax: 'new S1()')
                Children(0)
    Next (Regular) Block[B3]
        Entering: {R1}
.locals {R1}
{
    Locals: [var i]
    Block[B3] - Block
        Predecessors: [B2]
        Statements (2)
            ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
              Left:
                ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: var, IsImplicit) (Syntax: 'var')
              Right:
                IInvalidOperation (OperationKind.Invalid, Type: null, IsInvalid, IsImplicit) (Syntax: 'new S1()')
                  Children(1):
                      IInvalidOperation (OperationKind.Invalid, Type: null, IsInvalid, IsImplicit) (Syntax: 'new S1()')
                        Children(0)
            IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
              Expression:
                IInvalidOperation (OperationKind.Invalid, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                  Children(2):
                      IOperation:  (OperationKind.None, Type: System.Console) (Syntax: 'System.Console')
                      ILocalReferenceOperation: i (OperationKind.LocalReference, Type: var) (Syntax: 'i')
        Next (Regular) Block[B2]
            Leaving: {R1}
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            Assert.Null(info.ElementType);
            Assert.Null(info.MoveNextMethod);
            Assert.Null(info.CurrentProperty);
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.Null(op.Info);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_04(bool s1IsRefStruct, bool addExplicitImplementationOfCurrentAndMoveNext)
        {
            var src = @"
using System.Collections.Generic;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator<int>
{
    bool stop;
    public int Current => 123;
    object System.Collections.IEnumerator.Current => Current;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
" +
(addExplicitImplementationOfCurrentAndMoveNext ?
@"
    int IEnumerator<int>.Current => throw null;
    bool System.Collections.IEnumerator.MoveNext() => throw null;
"
:
"") +
@"
    void System.IDisposable.Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       55 (0x37)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001d
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       ""int S2.Current.get""
    IL_0018:  call       ""void System.Console.Write(int)""
    IL_001d:  ldloca.s   V_0
    IL_001f:  call       ""bool S2.MoveNext()""
    IL_0024:  brtrue.s   IL_0011
    IL_0026:  leave.s    IL_0036
  }
  finally
  {
    IL_0028:  ldloca.s   V_0
    IL_002a:  constrained. ""S2""
    IL_0030:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0035:  endfinally
  }
  IL_0036:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B5]
                IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 S2.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B5] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void System.IDisposable.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_05(bool s1IsRefStruct, bool addStructConstraintToTEnumerable)
        {
            var src = @"
using System.Collections.Generic;

interface IGetEnumerator<TEnumerator> where TEnumerator : IEnumerator<int>, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator<int>
{
    bool stop;
    public int Current => 123;
    object System.Collections.IEnumerator.Current => Current;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : IEnumerator<int>, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<TEnumerable, TEnumerator>(TEnumerable)",
@"
{
  // Code size       74 (0x4a)
  .maxstack  1
  .locals init (TEnumerator V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""TEnumerable""
  IL_0008:  callvirt   ""TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_0022
    IL_0010:  ldloca.s   V_0
    IL_0012:  constrained. ""TEnumerator""
    IL_0018:  callvirt   ""int System.Collections.Generic.IEnumerator<int>.Current.get""
    IL_001d:  call       ""void System.Console.Write(int)""
    IL_0022:  ldloca.s   V_0
    IL_0024:  constrained. ""TEnumerator""
    IL_002a:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_002f:  brtrue.s   IL_0010
    IL_0031:  leave.s    IL_0049
  }
  finally
  {
    IL_0033:  ldloc.0
    IL_0034:  box        ""TEnumerator""
    IL_0039:  brfalse.s  IL_0048
    IL_003b:  ldloca.s   V_0
    IL_003d:  constrained. ""TEnumerator""
    IL_0043:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0048:  endfinally
  }
  IL_0049:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()) (OperationKind.Invocation, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: TEnumerable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: TEnumerable) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean System.Collections.IEnumerator.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void System.IDisposable.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean System.Collections.IEnumerator.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_06(bool s1IsRefStruct, bool addStructConstraintToTEnumerable)
        {
            var src = @"
using System.Collections.Generic;

interface IGetEnumerator<TEnumerator> where TEnumerator : IEnumerator<int>, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator<int>
{
    bool stop;
    public int Current => 123;
    object System.Collections.IEnumerator.Current => Current;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : struct, IEnumerator<int>, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<TEnumerable, TEnumerator>(TEnumerable)",
@"
{
  // Code size       66 (0x42)
  .maxstack  1
  .locals init (TEnumerator V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""TEnumerable""
  IL_0008:  callvirt   ""TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_0022
    IL_0010:  ldloca.s   V_0
    IL_0012:  constrained. ""TEnumerator""
    IL_0018:  callvirt   ""int System.Collections.Generic.IEnumerator<int>.Current.get""
    IL_001d:  call       ""void System.Console.Write(int)""
    IL_0022:  ldloca.s   V_0
    IL_0024:  constrained. ""TEnumerator""
    IL_002a:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_002f:  brtrue.s   IL_0010
    IL_0031:  leave.s    IL_0041
  }
  finally
  {
    IL_0033:  ldloca.s   V_0
    IL_0035:  constrained. ""TEnumerator""
    IL_003b:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0040:  endfinally
  }
  IL_0041:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()) (OperationKind.Invocation, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: TEnumerable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: TEnumerable) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B5]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B5] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean System.Collections.IEnumerator.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void System.IDisposable.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean System.Collections.IEnumerator.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 System.Collections.Generic.IEnumerator<System.Int32>.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_07(bool s1IsRefStruct, bool addStructConstraintToTEnumerable)
        {
            var src = @"
interface IMyEnumerator<T> : System.IDisposable
{
    T Current {get;}
    bool MoveNext();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : IMyEnumerator<int>, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IMyEnumerator<int>
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : IMyEnumerator<int>, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<TEnumerable, TEnumerator>(TEnumerable)",
@"
{
  // Code size       74 (0x4a)
  .maxstack  1
  .locals init (TEnumerator V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""TEnumerable""
  IL_0008:  callvirt   ""TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_0022
    IL_0010:  ldloca.s   V_0
    IL_0012:  constrained. ""TEnumerator""
    IL_0018:  callvirt   ""int IMyEnumerator<int>.Current.get""
    IL_001d:  call       ""void System.Console.Write(int)""
    IL_0022:  ldloca.s   V_0
    IL_0024:  constrained. ""TEnumerator""
    IL_002a:  callvirt   ""bool IMyEnumerator<int>.MoveNext()""
    IL_002f:  brtrue.s   IL_0010
    IL_0031:  leave.s    IL_0049
  }
  finally
  {
    IL_0033:  ldloc.0
    IL_0034:  box        ""TEnumerator""
    IL_0039:  brfalse.s  IL_0048
    IL_003b:  ldloca.s   V_0
    IL_003d:  constrained. ""TEnumerator""
    IL_0043:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0048:  endfinally
  }
  IL_0049:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()) (OperationKind.Invocation, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: TEnumerable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: TEnumerable) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean IMyEnumerator<System.Int32>.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 IMyEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean IMyEnumerator<System.Int32>.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 IMyEnumerator<System.Int32>.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void System.IDisposable.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean IMyEnumerator<System.Int32>.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 IMyEnumerator<System.Int32>.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_08(bool s1IsRefStruct, bool addStructConstraintToTEnumerable)
        {
            var src = @"
interface IMyEnumerator1<T>
{
    T Current {get;}
    bool MoveNext();
}

interface IMyEnumerator2<T>
{
    T Current {get;}
    bool MoveNext();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : IMyEnumerator1<int>, IMyEnumerator2<int>, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IMyEnumerator1<int>, IMyEnumerator2<int>
{
    bool stop;
    public int Current => 123;

    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : IMyEnumerator1<int>, IMyEnumerator2<int>, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (60,27): error CS0202: foreach requires that the return type 'TEnumerator' of 'IGetEnumerator<TEnumerator>.GetEnumerator()' must have a suitable public 'MoveNext' method and public 'Current' property
                //         foreach (var i in t)
                Diagnostic(ErrorCode.ERR_BadGetEnumerator, "t").WithArguments("TEnumerator", "IGetEnumerator<TEnumerator>.GetEnumerator()").WithLocation(60, 27)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            Assert.Null(info.ElementType);
            Assert.Null(info.MoveNextMethod);
            Assert.Null(info.CurrentProperty);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.Null(op.Info);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumeratorT_09(bool s1IsRefStruct, bool addStructConstraintToTEnumerable)
        {
            var src = @"
using System.Collections.Generic;

interface IMyEnumerator1<T>
{
    T Current {get;}
    bool MoveNext();
}

interface IMyEnumerator2<T>
{
    T Current {get;}
    bool MoveNext();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : IMyEnumerator1<int>, IMyEnumerator2<int>, IEnumerator<int>, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IMyEnumerator1<int>, IMyEnumerator2<int>, IEnumerator<int>
{
    bool stop;
    public int Current => 123;
    object System.Collections.IEnumerator.Current => Current;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : IMyEnumerator1<int>, IMyEnumerator2<int>, IEnumerator<int>, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (62,27): error CS0202: foreach requires that the return type 'TEnumerator' of 'IGetEnumerator<TEnumerator>.GetEnumerator()' must have a suitable public 'MoveNext' method and public 'Current' property
                //         foreach (var i in t)
                Diagnostic(ErrorCode.ERR_BadGetEnumerator, "t").WithArguments("TEnumerator", "IGetEnumerator<TEnumerator>.GetEnumerator()").WithLocation(62, 27)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            Assert.Null(info.ElementType);
            Assert.Null(info.MoveNextMethod);
            Assert.Null(info.CurrentProperty);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.Null(op.Info);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IDisposable_01(bool s1IsRefStruct)
        {
            var src = @"
" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : System.IDisposable
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       49 (0x31)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001d
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       ""int S2.Current.get""
    IL_0018:  call       ""void System.Console.Write(int)""
    IL_001d:  ldloca.s   V_0
    IL_001f:  call       ""bool S2.MoveNext()""
    IL_0024:  brtrue.s   IL_0011
    IL_0026:  leave.s    IL_0030
  }
  finally
  {
    IL_0028:  ldloca.s   V_0
    IL_002a:  call       ""void S2.Dispose()""
    IL_002f:  endfinally
  }
  IL_0030:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B5]
                IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 S2.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation ( void S2.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B5] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void S2.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            AssertEx.Equal("void S2.Dispose()", op.Info.PatternDisposeMethod.ToTestDisplayString());
            Assert.True(op.Info.DisposeArguments.IsEmpty);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IDisposable_02(bool s1IsRefStruct)
        {
            var src = @"
" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : System.IDisposable
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Dispose()
    {
        System.Console.Write('D');
    }

    void System.IDisposable.Dispose() => throw null;
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       49 (0x31)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001d
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       ""int S2.Current.get""
    IL_0018:  call       ""void System.Console.Write(int)""
    IL_001d:  ldloca.s   V_0
    IL_001f:  call       ""bool S2.MoveNext()""
    IL_0024:  brtrue.s   IL_0011
    IL_0026:  leave.s    IL_0030
  }
  finally
  {
    IL_0028:  ldloca.s   V_0
    IL_002a:  call       ""void S2.Dispose()""
    IL_002f:  endfinally
  }
  IL_0030:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B5]
                IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 S2.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation ( void S2.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B5] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void S2.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            AssertEx.Equal("void S2.Dispose()", op.Info.PatternDisposeMethod.ToTestDisplayString());
            Assert.True(op.Info.DisposeArguments.IsEmpty);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IDisposable_03(bool s1IsRefStruct)
        {
            var src = @"
" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : System.IDisposable
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }

    void System.IDisposable.Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       55 (0x37)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001d
    IL_0011:  ldloca.s   V_0
    IL_0013:  call       ""int S2.Current.get""
    IL_0018:  call       ""void System.Console.Write(int)""
    IL_001d:  ldloca.s   V_0
    IL_001f:  call       ""bool S2.MoveNext()""
    IL_0024:  brtrue.s   IL_0011
    IL_0026:  leave.s    IL_0036
  }
  finally
  {
    IL_0028:  ldloca.s   V_0
    IL_002a:  constrained. ""S2""
    IL_0030:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0035:  endfinally
  }
  IL_0036:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B5]
                IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 S2.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B5] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void System.IDisposable.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IDisposable_04(bool s1IsRefStruct, bool addStructConstraintToTEnumerable)
        {
            var src = @"
interface ICustomEnumerator
{
    int Current {get;}
    bool MoveNext();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, System.IDisposable, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : ICustomEnumerator, System.IDisposable
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : ICustomEnumerator, System.IDisposable, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<TEnumerable, TEnumerator>(TEnumerable)",
@"
{
  // Code size       74 (0x4a)
  .maxstack  1
  .locals init (TEnumerator V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""TEnumerable""
  IL_0008:  callvirt   ""TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_0022
    IL_0010:  ldloca.s   V_0
    IL_0012:  constrained. ""TEnumerator""
    IL_0018:  callvirt   ""int ICustomEnumerator.Current.get""
    IL_001d:  call       ""void System.Console.Write(int)""
    IL_0022:  ldloca.s   V_0
    IL_0024:  constrained. ""TEnumerator""
    IL_002a:  callvirt   ""bool ICustomEnumerator.MoveNext()""
    IL_002f:  brtrue.s   IL_0010
    IL_0031:  leave.s    IL_0049
  }
  finally
  {
    IL_0033:  ldloc.0
    IL_0034:  box        ""TEnumerator""
    IL_0039:  brfalse.s  IL_0048
    IL_003b:  ldloca.s   V_0
    IL_003d:  constrained. ""TEnumerator""
    IL_0043:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0048:  endfinally
  }
  IL_0049:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()) (OperationKind.Invocation, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: TEnumerable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: TEnumerable) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean ICustomEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 ICustomEnumerator.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean ICustomEnumerator.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 ICustomEnumerator.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void System.IDisposable.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean ICustomEnumerator.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 ICustomEnumerator.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IDisposable_05(bool s1IsRefStruct, bool addStructConstraintToTEnumerable)
        {
            var src = @"
interface ICustomEnumerator
{
    int Current {get;}
    bool MoveNext();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, System.IDisposable, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : ICustomEnumerator, System.IDisposable
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : struct, ICustomEnumerator, System.IDisposable, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<TEnumerable, TEnumerator>(TEnumerable)",
@"
{
  // Code size       66 (0x42)
  .maxstack  1
  .locals init (TEnumerator V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""TEnumerable""
  IL_0008:  callvirt   ""TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_0022
    IL_0010:  ldloca.s   V_0
    IL_0012:  constrained. ""TEnumerator""
    IL_0018:  callvirt   ""int ICustomEnumerator.Current.get""
    IL_001d:  call       ""void System.Console.Write(int)""
    IL_0022:  ldloca.s   V_0
    IL_0024:  constrained. ""TEnumerator""
    IL_002a:  callvirt   ""bool ICustomEnumerator.MoveNext()""
    IL_002f:  brtrue.s   IL_0010
    IL_0031:  leave.s    IL_0041
  }
  finally
  {
    IL_0033:  ldloca.s   V_0
    IL_0035:  constrained. ""TEnumerator""
    IL_003b:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0040:  endfinally
  }
  IL_0041:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()) (OperationKind.Invocation, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: TEnumerable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: TEnumerable) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B5]
                IInvocationOperation (virtual System.Boolean ICustomEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 ICustomEnumerator.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: TEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B5] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean ICustomEnumerator.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 ICustomEnumerator.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void System.IDisposable.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean ICustomEnumerator.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 ICustomEnumerator.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IDisposable_06(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
interface ICustomEnumerator
{
    int Current {get;}
    bool MoveNext();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

interface IMyDisposable
{
    void Dispose();
}

ref struct S2 : ICustomEnumerator, IMyDisposable
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"ICustomEnumerator, IMyDisposable, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (58,27): error CS9507: foreach statement cannot operate on enumerators of type 'TEnumerator' because it is a type parameter that allows ref struct and it is not known at compile time to implement IDisposable.
                //         foreach (var i in t)
                Diagnostic(ErrorCode.ERR_BadAllowByRefLikeEnumerator, "t").WithArguments("TEnumerator").WithLocation(58, 27)
                );

            var tree = comp.SyntaxTrees.Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean ICustomEnumerator.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 ICustomEnumerator.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean ICustomEnumerator.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 ICustomEnumerator.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.False(op.Info.NeedsDispose);
            Assert.False(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IDisposable_07(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
interface ICustomEnumerator
{
    int Current {get;}
    bool MoveNext();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : ICustomEnumerator, System.IDisposable
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"ICustomEnumerator, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (53,27): error CS9507: foreach statement cannot operate on enumerators of type 'TEnumerator' because it is a type parameter that allows ref struct and it is not known at compile time to implement IDisposable.
                //         foreach (var i in t)
                Diagnostic(ErrorCode.ERR_BadAllowByRefLikeEnumerator, "t").WithArguments("TEnumerator").WithLocation(53, 27)
                );

            var tree = comp.SyntaxTrees.Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean ICustomEnumerator.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32 ICustomEnumerator.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean ICustomEnumerator.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Int32 ICustomEnumerator.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.False(op.Info.NeedsDispose);
            Assert.False(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Fact]
        public void Foreach_Pattern_01()
        {
            var src = @"
ref struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();
        }

        [Fact]
        public void Foreach_Pattern_02()
        {
            var src = @"
struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2
{
    bool stop;
    public int Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();
        }

        [Fact]
        public void AwaitUsing_01()
        {
            var src = @"
using System;
using System.Threading.Tasks;

ref struct S2 : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        System.Console.Write('D');
        return ValueTask.CompletedTask;
    }
}

class C
{
    static async Task Main()
    {
        await using (new S2())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (18,22): error CS9104: A using statement resource of type 'S2' cannot be used in async methods or async lambda expressions.
                //         await using (new S2())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefUsing, "new S2()").WithArguments("S2").WithLocation(18, 22)
                );
        }

        [Fact]
        public void AwaitUsing_03()
        {
            var src = @"
using System;
using System.Threading.Tasks;

ref struct S2 : IAsyncDisposable
{
    ValueTask IAsyncDisposable.DisposeAsync()
    {
        System.Console.Write('D');
        return ValueTask.CompletedTask;
    }
}

class C
{
    static async Task Main()
    {
        await using (new S2())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (18,22): error CS9104: A using statement resource of type 'S2' cannot be used in async methods or async lambda expressions.
                //         await using (new S2())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefUsing, "new S2()").WithArguments("S2").WithLocation(18, 22)
                );
        }

        [Fact]
        public void AwaitUsing_04()
        {
            var src = @"
using System;
using System.Threading.Tasks;

ref struct S2 : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        System.Console.Write('D');
        return ValueTask.CompletedTask;
    }
}

class C
{
    static async Task Main()
    {
        await Test<S2>();
    }

    static async Task Test<T>() where T : IAsyncDisposable, new(), allows ref struct
    {
        await using (new T())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (23,22): error CS9104: A using statement resource of type 'T' cannot be used in async methods or async lambda expressions.
                //         await using (new T())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefUsing, "new T()").WithArguments("T").WithLocation(23, 22)
                );
        }

        [Fact]
        public void AwaitUsing_06()
        {
            var src = @"
using System.Threading.Tasks;

interface IMyAsyncDisposable
{
    ValueTask DisposeAsync();
}

ref struct S2 : IMyAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        System.Console.Write('D');
        return ValueTask.CompletedTask;
    }
}

class C
{
    static async Task Main()
    {
        await Test<S2>();
    }

    static async Task Test<T>() where T : IMyAsyncDisposable, new(), allows ref struct
    {
        await using (new T())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (27,22): error CS9104: A using statement resource of type 'T' cannot be used in async methods or async lambda expressions.
                //         await using (new T())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefUsing, "new T()").WithArguments("T").WithLocation(27, 22)
                );
        }

        [Fact]
        public void AwaitUsing_07()
        {
            var src = @"
using System.Threading.Tasks;

interface IMyAsyncDisposable1
{
    ValueTask DisposeAsync();
}

interface IMyAsyncDisposable2
{
    ValueTask DisposeAsync();
}

ref struct S2 : IMyAsyncDisposable1, IMyAsyncDisposable2
{
    public ValueTask DisposeAsync()
    {
        System.Console.Write('D');
        return ValueTask.CompletedTask;
    }
}

class C
{
    static async Task Main()
    {
        await Test<S2>();
    }

    static async Task Test<T>() where T : IMyAsyncDisposable1, IMyAsyncDisposable2, new(), allows ref struct
    {
        await using (new T())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (32,22): error CS0121: The call is ambiguous between the following methods or properties: 'IMyAsyncDisposable1.DisposeAsync()' and 'IMyAsyncDisposable2.DisposeAsync()'
                //         await using (new T())
                Diagnostic(ErrorCode.ERR_AmbigCall, "new T()").WithArguments("IMyAsyncDisposable1.DisposeAsync()", "IMyAsyncDisposable2.DisposeAsync()").WithLocation(32, 22),
                // (32,22): error CS8410: 'T': type used in an asynchronous using statement must be implicitly convertible to 'System.IAsyncDisposable' or implement a suitable 'DisposeAsync' method.
                //         await using (new T())
                Diagnostic(ErrorCode.ERR_NoConvToIAsyncDisp, "new T()").WithArguments("T").WithLocation(32, 22),
                // (32,22): error CS9104: A using statement resource of type 'T' cannot be used in async methods or async lambda expressions.
                //         await using (new T())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefUsing, "new T()").WithArguments("T").WithLocation(32, 22)
                );
        }

        [Fact]
        [WorkItem("https://github.com/dotnet/roslyn/issues/72819")]
        public void AwaitUsing_08()
        {
            var src = @"
using System;
using System.Threading.Tasks;

interface IMyAsyncDisposable1
{
    ValueTask DisposeAsync();
}

interface IMyAsyncDisposable2
{
    ValueTask DisposeAsync();
}

ref struct S2 : IMyAsyncDisposable1, IMyAsyncDisposable2, IAsyncDisposable
{
    ValueTask IMyAsyncDisposable1.DisposeAsync() => throw null;
    ValueTask IMyAsyncDisposable2.DisposeAsync() => throw null;

    public ValueTask DisposeAsync()
    {
        System.Console.Write('D');
        return ValueTask.CompletedTask;
    }
}

class C
{
    static async Task Main()
    {
        await Test<S2>();
    }

    static async Task Test<T>() where T : IMyAsyncDisposable1, IMyAsyncDisposable2, IAsyncDisposable, new(), allows ref struct
    {
        await using (new T())
        {
            System.Console.Write(123);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            // PROTOTYPE(RefStructInterfaces): The failure is likely unexpected, but is not specific to `allow ref struct` scenario. See https://github.com/dotnet/roslyn/issues/72819. 
            comp.VerifyDiagnostics(
                // (36,22): error CS0121: The call is ambiguous between the following methods or properties: 'IMyAsyncDisposable1.DisposeAsync()' and 'IMyAsyncDisposable2.DisposeAsync()'
                //         await using (new T())
                Diagnostic(ErrorCode.ERR_AmbigCall, "new T()").WithArguments("IMyAsyncDisposable1.DisposeAsync()", "IMyAsyncDisposable2.DisposeAsync()").WithLocation(36, 22),
                // (36,22): error CS9104: A using statement resource of type 'T' cannot be used in async methods or async lambda expressions.
                //         await using (new T())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefUsing, "new T()").WithArguments("T").WithLocation(36, 22)
                );
        }

        [Fact]
        public void AwaitForeach_IAsyncEnumerableT_01()
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

ref struct S : IAsyncEnumerable<int>
{
    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = default)
    {
        return Get123();
    }

    async static IAsyncEnumerator<int> Get123()
    {
        await Task.Yield();
        yield return 123;
    }
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.<Main>d__0.System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext()",
@"
{
  // Code size      405 (0x195)
  .maxstack  3
  .locals init (int V_0,
                S V_1,
                System.Threading.CancellationToken V_2,
                System.Runtime.CompilerServices.ValueTaskAwaiter<bool> V_3,
                System.Threading.Tasks.ValueTask<bool> V_4,
                object V_5,
                System.Runtime.CompilerServices.ValueTaskAwaiter V_6,
                System.Threading.Tasks.ValueTask V_7,
                System.Exception V_8)
  IL_0000:  ldarg.0
  IL_0001:  ldfld      ""int C.<Main>d__0.<>1__state""
  IL_0006:  stloc.0
  .try
  {
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_003c
    IL_000a:  ldloc.0
    IL_000b:  ldc.i4.1
    IL_000c:  beq        IL_0111
    IL_0011:  ldarg.0
    IL_0012:  ldloca.s   V_1
    IL_0014:  dup
    IL_0015:  initobj    ""S""
    IL_001b:  ldloca.s   V_2
    IL_001d:  initobj    ""System.Threading.CancellationToken""
    IL_0023:  ldloc.2
    IL_0024:  call       ""System.Collections.Generic.IAsyncEnumerator<int> S.GetAsyncEnumerator(System.Threading.CancellationToken)""
    IL_0029:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_002e:  ldarg.0
    IL_002f:  ldnull
    IL_0030:  stfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_0035:  ldarg.0
    IL_0036:  ldc.i4.0
    IL_0037:  stfld      ""int C.<Main>d__0.<>7__wrap3""
    IL_003c:  nop
    .try
    {
      IL_003d:  ldloc.0
      IL_003e:  brfalse.s  IL_0093
      IL_0040:  br.s       IL_0052
      IL_0042:  ldarg.0
      IL_0043:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
      IL_0048:  callvirt   ""int System.Collections.Generic.IAsyncEnumerator<int>.Current.get""
      IL_004d:  call       ""void System.Console.Write(int)""
      IL_0052:  ldarg.0
      IL_0053:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
      IL_0058:  callvirt   ""System.Threading.Tasks.ValueTask<bool> System.Collections.Generic.IAsyncEnumerator<int>.MoveNextAsync()""
      IL_005d:  stloc.s    V_4
      IL_005f:  ldloca.s   V_4
      IL_0061:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> System.Threading.Tasks.ValueTask<bool>.GetAwaiter()""
      IL_0066:  stloc.3
      IL_0067:  ldloca.s   V_3
      IL_0069:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.IsCompleted.get""
      IL_006e:  brtrue.s   IL_00af
      IL_0070:  ldarg.0
      IL_0071:  ldc.i4.0
      IL_0072:  dup
      IL_0073:  stloc.0
      IL_0074:  stfld      ""int C.<Main>d__0.<>1__state""
      IL_0079:  ldarg.0
      IL_007a:  ldloc.3
      IL_007b:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_0080:  ldarg.0
      IL_0081:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
      IL_0086:  ldloca.s   V_3
      IL_0088:  ldarg.0
      IL_0089:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, C.<Main>d__0>(ref System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, ref C.<Main>d__0)""
      IL_008e:  leave      IL_0194
      IL_0093:  ldarg.0
      IL_0094:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_0099:  stloc.3
      IL_009a:  ldarg.0
      IL_009b:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_00a0:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool>""
      IL_00a6:  ldarg.0
      IL_00a7:  ldc.i4.m1
      IL_00a8:  dup
      IL_00a9:  stloc.0
      IL_00aa:  stfld      ""int C.<Main>d__0.<>1__state""
      IL_00af:  ldloca.s   V_3
      IL_00b1:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.GetResult()""
      IL_00b6:  brtrue.s   IL_0042
      IL_00b8:  leave.s    IL_00c6
    }
    catch object
    {
      IL_00ba:  stloc.s    V_5
      IL_00bc:  ldarg.0
      IL_00bd:  ldloc.s    V_5
      IL_00bf:  stfld      ""object C.<Main>d__0.<>7__wrap2""
      IL_00c4:  leave.s    IL_00c6
    }
    IL_00c6:  ldarg.0
    IL_00c7:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_00cc:  brfalse.s  IL_0135
    IL_00ce:  ldarg.0
    IL_00cf:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_00d4:  callvirt   ""System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()""
    IL_00d9:  stloc.s    V_7
    IL_00db:  ldloca.s   V_7
    IL_00dd:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter System.Threading.Tasks.ValueTask.GetAwaiter()""
    IL_00e2:  stloc.s    V_6
    IL_00e4:  ldloca.s   V_6
    IL_00e6:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter.IsCompleted.get""
    IL_00eb:  brtrue.s   IL_012e
    IL_00ed:  ldarg.0
    IL_00ee:  ldc.i4.1
    IL_00ef:  dup
    IL_00f0:  stloc.0
    IL_00f1:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_00f6:  ldarg.0
    IL_00f7:  ldloc.s    V_6
    IL_00f9:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_00fe:  ldarg.0
    IL_00ff:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
    IL_0104:  ldloca.s   V_6
    IL_0106:  ldarg.0
    IL_0107:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter, C.<Main>d__0>(ref System.Runtime.CompilerServices.ValueTaskAwaiter, ref C.<Main>d__0)""
    IL_010c:  leave      IL_0194
    IL_0111:  ldarg.0
    IL_0112:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_0117:  stloc.s    V_6
    IL_0119:  ldarg.0
    IL_011a:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_011f:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter""
    IL_0125:  ldarg.0
    IL_0126:  ldc.i4.m1
    IL_0127:  dup
    IL_0128:  stloc.0
    IL_0129:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_012e:  ldloca.s   V_6
    IL_0130:  call       ""void System.Runtime.CompilerServices.ValueTaskAwaiter.GetResult()""
    IL_0135:  ldarg.0
    IL_0136:  ldfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_013b:  stloc.s    V_5
    IL_013d:  ldloc.s    V_5
    IL_013f:  brfalse.s  IL_0158
    IL_0141:  ldloc.s    V_5
    IL_0143:  isinst     ""System.Exception""
    IL_0148:  dup
    IL_0149:  brtrue.s   IL_014e
    IL_014b:  ldloc.s    V_5
    IL_014d:  throw
    IL_014e:  call       ""System.Runtime.ExceptionServices.ExceptionDispatchInfo System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(System.Exception)""
    IL_0153:  callvirt   ""void System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()""
    IL_0158:  ldarg.0
    IL_0159:  ldnull
    IL_015a:  stfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_015f:  ldarg.0
    IL_0160:  ldnull
    IL_0161:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_0166:  leave.s    IL_0181
  }
  catch System.Exception
  {
    IL_0168:  stloc.s    V_8
    IL_016a:  ldarg.0
    IL_016b:  ldc.i4.s   -2
    IL_016d:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_0172:  ldarg.0
    IL_0173:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
    IL_0178:  ldloc.s    V_8
    IL_017a:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetException(System.Exception)""
    IL_017f:  leave.s    IL_0194
  }
  IL_0181:  ldarg.0
  IL_0182:  ldc.i4.s   -2
  IL_0184:  stfld      ""int C.<Main>d__0.<>1__state""
  IL_0189:  ldarg.0
  IL_018a:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
  IL_018f:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetResult()""
  IL_0194:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation ( System.Collections.Generic.IAsyncEnumerator<System.Int32> S.GetAsyncEnumerator([System.Threading.CancellationToken token = default(System.Threading.CancellationToken)])) (OperationKind.Invocation, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(1):
                      IArgumentOperation (ArgumentKind.DefaultValue, Matching Parameter: token) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'await forea ... }')
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: System.Threading.CancellationToken, IsImplicit) (Syntax: 'await forea ... }')
                        InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IAwaitOperation (OperationKind.Await, Type: System.Boolean, IsImplicit) (Syntax: 'await forea ... }')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask<System.Boolean> System.Collections.Generic.IAsyncEnumerator<System.Int32>.MoveNextAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask<System.Boolean>, IsImplicit) (Syntax: 'new S()')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                      Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IAsyncEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IAwaitOperation (OperationKind.Await, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask, IsImplicit) (Syntax: 'new S()')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                      Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> S.GetAsyncEnumerator([System.Threading.CancellationToken token = default(System.Threading.CancellationToken)])", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> S.GetAsyncEnumerator([System.Threading.CancellationToken token = default(System.Threading.CancellationToken)])", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Equal(1, op.Info.GetEnumeratorArguments.Length);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Fact]
        public void AwaitForeach_IAsyncEnumerableT_02()
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

ref struct S : IAsyncEnumerable<int>
{
    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = default)
    {
        return Get123();
    }

    async static IAsyncEnumerator<int> Get123()
    {
        await Task.Yield();
        yield return 123;
    }

    IAsyncEnumerator<int> IAsyncEnumerable<int>.GetAsyncEnumerator(CancellationToken token) => throw null;
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.<Main>d__0.System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext()",
@"
{
  // Code size      405 (0x195)
  .maxstack  3
  .locals init (int V_0,
                S V_1,
                System.Threading.CancellationToken V_2,
                System.Runtime.CompilerServices.ValueTaskAwaiter<bool> V_3,
                System.Threading.Tasks.ValueTask<bool> V_4,
                object V_5,
                System.Runtime.CompilerServices.ValueTaskAwaiter V_6,
                System.Threading.Tasks.ValueTask V_7,
                System.Exception V_8)
  IL_0000:  ldarg.0
  IL_0001:  ldfld      ""int C.<Main>d__0.<>1__state""
  IL_0006:  stloc.0
  .try
  {
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_003c
    IL_000a:  ldloc.0
    IL_000b:  ldc.i4.1
    IL_000c:  beq        IL_0111
    IL_0011:  ldarg.0
    IL_0012:  ldloca.s   V_1
    IL_0014:  dup
    IL_0015:  initobj    ""S""
    IL_001b:  ldloca.s   V_2
    IL_001d:  initobj    ""System.Threading.CancellationToken""
    IL_0023:  ldloc.2
    IL_0024:  call       ""System.Collections.Generic.IAsyncEnumerator<int> S.GetAsyncEnumerator(System.Threading.CancellationToken)""
    IL_0029:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_002e:  ldarg.0
    IL_002f:  ldnull
    IL_0030:  stfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_0035:  ldarg.0
    IL_0036:  ldc.i4.0
    IL_0037:  stfld      ""int C.<Main>d__0.<>7__wrap3""
    IL_003c:  nop
    .try
    {
      IL_003d:  ldloc.0
      IL_003e:  brfalse.s  IL_0093
      IL_0040:  br.s       IL_0052
      IL_0042:  ldarg.0
      IL_0043:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
      IL_0048:  callvirt   ""int System.Collections.Generic.IAsyncEnumerator<int>.Current.get""
      IL_004d:  call       ""void System.Console.Write(int)""
      IL_0052:  ldarg.0
      IL_0053:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
      IL_0058:  callvirt   ""System.Threading.Tasks.ValueTask<bool> System.Collections.Generic.IAsyncEnumerator<int>.MoveNextAsync()""
      IL_005d:  stloc.s    V_4
      IL_005f:  ldloca.s   V_4
      IL_0061:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> System.Threading.Tasks.ValueTask<bool>.GetAwaiter()""
      IL_0066:  stloc.3
      IL_0067:  ldloca.s   V_3
      IL_0069:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.IsCompleted.get""
      IL_006e:  brtrue.s   IL_00af
      IL_0070:  ldarg.0
      IL_0071:  ldc.i4.0
      IL_0072:  dup
      IL_0073:  stloc.0
      IL_0074:  stfld      ""int C.<Main>d__0.<>1__state""
      IL_0079:  ldarg.0
      IL_007a:  ldloc.3
      IL_007b:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_0080:  ldarg.0
      IL_0081:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
      IL_0086:  ldloca.s   V_3
      IL_0088:  ldarg.0
      IL_0089:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, C.<Main>d__0>(ref System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, ref C.<Main>d__0)""
      IL_008e:  leave      IL_0194
      IL_0093:  ldarg.0
      IL_0094:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_0099:  stloc.3
      IL_009a:  ldarg.0
      IL_009b:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_00a0:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool>""
      IL_00a6:  ldarg.0
      IL_00a7:  ldc.i4.m1
      IL_00a8:  dup
      IL_00a9:  stloc.0
      IL_00aa:  stfld      ""int C.<Main>d__0.<>1__state""
      IL_00af:  ldloca.s   V_3
      IL_00b1:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.GetResult()""
      IL_00b6:  brtrue.s   IL_0042
      IL_00b8:  leave.s    IL_00c6
    }
    catch object
    {
      IL_00ba:  stloc.s    V_5
      IL_00bc:  ldarg.0
      IL_00bd:  ldloc.s    V_5
      IL_00bf:  stfld      ""object C.<Main>d__0.<>7__wrap2""
      IL_00c4:  leave.s    IL_00c6
    }
    IL_00c6:  ldarg.0
    IL_00c7:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_00cc:  brfalse.s  IL_0135
    IL_00ce:  ldarg.0
    IL_00cf:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_00d4:  callvirt   ""System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()""
    IL_00d9:  stloc.s    V_7
    IL_00db:  ldloca.s   V_7
    IL_00dd:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter System.Threading.Tasks.ValueTask.GetAwaiter()""
    IL_00e2:  stloc.s    V_6
    IL_00e4:  ldloca.s   V_6
    IL_00e6:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter.IsCompleted.get""
    IL_00eb:  brtrue.s   IL_012e
    IL_00ed:  ldarg.0
    IL_00ee:  ldc.i4.1
    IL_00ef:  dup
    IL_00f0:  stloc.0
    IL_00f1:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_00f6:  ldarg.0
    IL_00f7:  ldloc.s    V_6
    IL_00f9:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_00fe:  ldarg.0
    IL_00ff:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
    IL_0104:  ldloca.s   V_6
    IL_0106:  ldarg.0
    IL_0107:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter, C.<Main>d__0>(ref System.Runtime.CompilerServices.ValueTaskAwaiter, ref C.<Main>d__0)""
    IL_010c:  leave      IL_0194
    IL_0111:  ldarg.0
    IL_0112:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_0117:  stloc.s    V_6
    IL_0119:  ldarg.0
    IL_011a:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_011f:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter""
    IL_0125:  ldarg.0
    IL_0126:  ldc.i4.m1
    IL_0127:  dup
    IL_0128:  stloc.0
    IL_0129:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_012e:  ldloca.s   V_6
    IL_0130:  call       ""void System.Runtime.CompilerServices.ValueTaskAwaiter.GetResult()""
    IL_0135:  ldarg.0
    IL_0136:  ldfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_013b:  stloc.s    V_5
    IL_013d:  ldloc.s    V_5
    IL_013f:  brfalse.s  IL_0158
    IL_0141:  ldloc.s    V_5
    IL_0143:  isinst     ""System.Exception""
    IL_0148:  dup
    IL_0149:  brtrue.s   IL_014e
    IL_014b:  ldloc.s    V_5
    IL_014d:  throw
    IL_014e:  call       ""System.Runtime.ExceptionServices.ExceptionDispatchInfo System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(System.Exception)""
    IL_0153:  callvirt   ""void System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()""
    IL_0158:  ldarg.0
    IL_0159:  ldnull
    IL_015a:  stfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_015f:  ldarg.0
    IL_0160:  ldnull
    IL_0161:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_0166:  leave.s    IL_0181
  }
  catch System.Exception
  {
    IL_0168:  stloc.s    V_8
    IL_016a:  ldarg.0
    IL_016b:  ldc.i4.s   -2
    IL_016d:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_0172:  ldarg.0
    IL_0173:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
    IL_0178:  ldloc.s    V_8
    IL_017a:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetException(System.Exception)""
    IL_017f:  leave.s    IL_0194
  }
  IL_0181:  ldarg.0
  IL_0182:  ldc.i4.s   -2
  IL_0184:  stfld      ""int C.<Main>d__0.<>1__state""
  IL_0189:  ldarg.0
  IL_018a:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
  IL_018f:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetResult()""
  IL_0194:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation ( System.Collections.Generic.IAsyncEnumerator<System.Int32> S.GetAsyncEnumerator([System.Threading.CancellationToken token = default(System.Threading.CancellationToken)])) (OperationKind.Invocation, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(1):
                      IArgumentOperation (ArgumentKind.DefaultValue, Matching Parameter: token) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'await forea ... }')
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: System.Threading.CancellationToken, IsImplicit) (Syntax: 'await forea ... }')
                        InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IAwaitOperation (OperationKind.Await, Type: System.Boolean, IsImplicit) (Syntax: 'await forea ... }')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask<System.Boolean> System.Collections.Generic.IAsyncEnumerator<System.Int32>.MoveNextAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask<System.Boolean>, IsImplicit) (Syntax: 'new S()')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                      Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IAsyncEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IAwaitOperation (OperationKind.Await, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask, IsImplicit) (Syntax: 'new S()')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                      Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> S.GetAsyncEnumerator([System.Threading.CancellationToken token = default(System.Threading.CancellationToken)])", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> S.GetAsyncEnumerator([System.Threading.CancellationToken token = default(System.Threading.CancellationToken)])", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Equal(1, op.Info.GetEnumeratorArguments.Length);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Fact]
        public void AwaitForeach_IAsyncEnumerableT_03()
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

ref struct S : IAsyncEnumerable<int>
{
    async static IAsyncEnumerator<int> Get123()
    {
        await Task.Yield();
        yield return 123;
    }

    IAsyncEnumerator<int> IAsyncEnumerable<int>.GetAsyncEnumerator(CancellationToken token) => Get123();
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.<Main>d__0.System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext()",
@"
{
  // Code size      411 (0x19b)
  .maxstack  3
  .locals init (int V_0,
                S V_1,
                System.Threading.CancellationToken V_2,
                System.Runtime.CompilerServices.ValueTaskAwaiter<bool> V_3,
                System.Threading.Tasks.ValueTask<bool> V_4,
                object V_5,
                System.Runtime.CompilerServices.ValueTaskAwaiter V_6,
                System.Threading.Tasks.ValueTask V_7,
                System.Exception V_8)
  IL_0000:  ldarg.0
  IL_0001:  ldfld      ""int C.<Main>d__0.<>1__state""
  IL_0006:  stloc.0
  .try
  {
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_0042
    IL_000a:  ldloc.0
    IL_000b:  ldc.i4.1
    IL_000c:  beq        IL_0117
    IL_0011:  ldarg.0
    IL_0012:  ldloca.s   V_1
    IL_0014:  dup
    IL_0015:  initobj    ""S""
    IL_001b:  ldloca.s   V_2
    IL_001d:  initobj    ""System.Threading.CancellationToken""
    IL_0023:  ldloc.2
    IL_0024:  constrained. ""S""
    IL_002a:  callvirt   ""System.Collections.Generic.IAsyncEnumerator<int> System.Collections.Generic.IAsyncEnumerable<int>.GetAsyncEnumerator(System.Threading.CancellationToken)""
    IL_002f:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_0034:  ldarg.0
    IL_0035:  ldnull
    IL_0036:  stfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_003b:  ldarg.0
    IL_003c:  ldc.i4.0
    IL_003d:  stfld      ""int C.<Main>d__0.<>7__wrap3""
    IL_0042:  nop
    .try
    {
      IL_0043:  ldloc.0
      IL_0044:  brfalse.s  IL_0099
      IL_0046:  br.s       IL_0058
      IL_0048:  ldarg.0
      IL_0049:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
      IL_004e:  callvirt   ""int System.Collections.Generic.IAsyncEnumerator<int>.Current.get""
      IL_0053:  call       ""void System.Console.Write(int)""
      IL_0058:  ldarg.0
      IL_0059:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
      IL_005e:  callvirt   ""System.Threading.Tasks.ValueTask<bool> System.Collections.Generic.IAsyncEnumerator<int>.MoveNextAsync()""
      IL_0063:  stloc.s    V_4
      IL_0065:  ldloca.s   V_4
      IL_0067:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> System.Threading.Tasks.ValueTask<bool>.GetAwaiter()""
      IL_006c:  stloc.3
      IL_006d:  ldloca.s   V_3
      IL_006f:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.IsCompleted.get""
      IL_0074:  brtrue.s   IL_00b5
      IL_0076:  ldarg.0
      IL_0077:  ldc.i4.0
      IL_0078:  dup
      IL_0079:  stloc.0
      IL_007a:  stfld      ""int C.<Main>d__0.<>1__state""
      IL_007f:  ldarg.0
      IL_0080:  ldloc.3
      IL_0081:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_0086:  ldarg.0
      IL_0087:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
      IL_008c:  ldloca.s   V_3
      IL_008e:  ldarg.0
      IL_008f:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, C.<Main>d__0>(ref System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, ref C.<Main>d__0)""
      IL_0094:  leave      IL_019a
      IL_0099:  ldarg.0
      IL_009a:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_009f:  stloc.3
      IL_00a0:  ldarg.0
      IL_00a1:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Main>d__0.<>u__1""
      IL_00a6:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool>""
      IL_00ac:  ldarg.0
      IL_00ad:  ldc.i4.m1
      IL_00ae:  dup
      IL_00af:  stloc.0
      IL_00b0:  stfld      ""int C.<Main>d__0.<>1__state""
      IL_00b5:  ldloca.s   V_3
      IL_00b7:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.GetResult()""
      IL_00bc:  brtrue.s   IL_0048
      IL_00be:  leave.s    IL_00cc
    }
    catch object
    {
      IL_00c0:  stloc.s    V_5
      IL_00c2:  ldarg.0
      IL_00c3:  ldloc.s    V_5
      IL_00c5:  stfld      ""object C.<Main>d__0.<>7__wrap2""
      IL_00ca:  leave.s    IL_00cc
    }
    IL_00cc:  ldarg.0
    IL_00cd:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_00d2:  brfalse.s  IL_013b
    IL_00d4:  ldarg.0
    IL_00d5:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_00da:  callvirt   ""System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()""
    IL_00df:  stloc.s    V_7
    IL_00e1:  ldloca.s   V_7
    IL_00e3:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter System.Threading.Tasks.ValueTask.GetAwaiter()""
    IL_00e8:  stloc.s    V_6
    IL_00ea:  ldloca.s   V_6
    IL_00ec:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter.IsCompleted.get""
    IL_00f1:  brtrue.s   IL_0134
    IL_00f3:  ldarg.0
    IL_00f4:  ldc.i4.1
    IL_00f5:  dup
    IL_00f6:  stloc.0
    IL_00f7:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_00fc:  ldarg.0
    IL_00fd:  ldloc.s    V_6
    IL_00ff:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_0104:  ldarg.0
    IL_0105:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
    IL_010a:  ldloca.s   V_6
    IL_010c:  ldarg.0
    IL_010d:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter, C.<Main>d__0>(ref System.Runtime.CompilerServices.ValueTaskAwaiter, ref C.<Main>d__0)""
    IL_0112:  leave      IL_019a
    IL_0117:  ldarg.0
    IL_0118:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_011d:  stloc.s    V_6
    IL_011f:  ldarg.0
    IL_0120:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Main>d__0.<>u__2""
    IL_0125:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter""
    IL_012b:  ldarg.0
    IL_012c:  ldc.i4.m1
    IL_012d:  dup
    IL_012e:  stloc.0
    IL_012f:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_0134:  ldloca.s   V_6
    IL_0136:  call       ""void System.Runtime.CompilerServices.ValueTaskAwaiter.GetResult()""
    IL_013b:  ldarg.0
    IL_013c:  ldfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_0141:  stloc.s    V_5
    IL_0143:  ldloc.s    V_5
    IL_0145:  brfalse.s  IL_015e
    IL_0147:  ldloc.s    V_5
    IL_0149:  isinst     ""System.Exception""
    IL_014e:  dup
    IL_014f:  brtrue.s   IL_0154
    IL_0151:  ldloc.s    V_5
    IL_0153:  throw
    IL_0154:  call       ""System.Runtime.ExceptionServices.ExceptionDispatchInfo System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(System.Exception)""
    IL_0159:  callvirt   ""void System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()""
    IL_015e:  ldarg.0
    IL_015f:  ldnull
    IL_0160:  stfld      ""object C.<Main>d__0.<>7__wrap2""
    IL_0165:  ldarg.0
    IL_0166:  ldnull
    IL_0167:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Main>d__0.<>7__wrap1""
    IL_016c:  leave.s    IL_0187
  }
  catch System.Exception
  {
    IL_016e:  stloc.s    V_8
    IL_0170:  ldarg.0
    IL_0171:  ldc.i4.s   -2
    IL_0173:  stfld      ""int C.<Main>d__0.<>1__state""
    IL_0178:  ldarg.0
    IL_0179:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
    IL_017e:  ldloc.s    V_8
    IL_0180:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetException(System.Exception)""
    IL_0185:  leave.s    IL_019a
  }
  IL_0187:  ldarg.0
  IL_0188:  ldc.i4.s   -2
  IL_018a:  stfld      ""int C.<Main>d__0.<>1__state""
  IL_018f:  ldarg.0
  IL_0190:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Main>d__0.<>t__builder""
  IL_0195:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetResult()""
  IL_019a:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation (virtual System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])) (OperationKind.Invocation, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(1):
                      IArgumentOperation (ArgumentKind.DefaultValue, Matching Parameter: cancellationToken) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'new S()')
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: System.Threading.CancellationToken, IsImplicit) (Syntax: 'new S()')
                        InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IAwaitOperation (OperationKind.Await, Type: System.Boolean, IsImplicit) (Syntax: 'await forea ... }')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask<System.Boolean> System.Collections.Generic.IAsyncEnumerator<System.Int32>.MoveNextAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask<System.Boolean>, IsImplicit) (Syntax: 'new S()')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                      Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IAsyncEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IAwaitOperation (OperationKind.Await, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask, IsImplicit) (Syntax: 'new S()')
                      Instance Receiver:
                        IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.IAsyncDisposable, IsImplicit) (Syntax: 'new S()')
                          Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                            (ImplicitReference)
                          Operand:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'new S()')
                      Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Equal(1, op.Info.GetEnumeratorArguments.Length);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerableT_04(bool addStructConstraint)
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

ref struct S : IAsyncEnumerable<int>
{
    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = default)
    {
        return Get123();
    }

    async static IAsyncEnumerator<int> Get123()
    {
        await Task.Yield();
        yield return 123;
    }
}

class C
{
    static async Task Main()
    {
        await Test<S>();
    }

    static async Task Test<T>() where T : " + (addStructConstraint ? "struct, " : "") + @"IAsyncEnumerable<int>, allows ref struct
    {
        await foreach (var i in default(T))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.<Test>d__1<T>.System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext()",
@"
{
  // Code size      411 (0x19b)
  .maxstack  3
  .locals init (int V_0,
                T V_1,
                System.Threading.CancellationToken V_2,
                System.Runtime.CompilerServices.ValueTaskAwaiter<bool> V_3,
                System.Threading.Tasks.ValueTask<bool> V_4,
                object V_5,
                System.Runtime.CompilerServices.ValueTaskAwaiter V_6,
                System.Threading.Tasks.ValueTask V_7,
                System.Exception V_8)
  IL_0000:  ldarg.0
  IL_0001:  ldfld      ""int C.<Test>d__1<T>.<>1__state""
  IL_0006:  stloc.0
  .try
  {
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_0042
    IL_000a:  ldloc.0
    IL_000b:  ldc.i4.1
    IL_000c:  beq        IL_0117
    IL_0011:  ldarg.0
    IL_0012:  ldloca.s   V_1
    IL_0014:  dup
    IL_0015:  initobj    ""T""
    IL_001b:  ldloca.s   V_2
    IL_001d:  initobj    ""System.Threading.CancellationToken""
    IL_0023:  ldloc.2
    IL_0024:  constrained. ""T""
    IL_002a:  callvirt   ""System.Collections.Generic.IAsyncEnumerator<int> System.Collections.Generic.IAsyncEnumerable<int>.GetAsyncEnumerator(System.Threading.CancellationToken)""
    IL_002f:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_0034:  ldarg.0
    IL_0035:  ldnull
    IL_0036:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_003b:  ldarg.0
    IL_003c:  ldc.i4.0
    IL_003d:  stfld      ""int C.<Test>d__1<T>.<>7__wrap3""
    IL_0042:  nop
    .try
    {
      IL_0043:  ldloc.0
      IL_0044:  brfalse.s  IL_0099
      IL_0046:  br.s       IL_0058
      IL_0048:  ldarg.0
      IL_0049:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
      IL_004e:  callvirt   ""int System.Collections.Generic.IAsyncEnumerator<int>.Current.get""
      IL_0053:  call       ""void System.Console.Write(int)""
      IL_0058:  ldarg.0
      IL_0059:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
      IL_005e:  callvirt   ""System.Threading.Tasks.ValueTask<bool> System.Collections.Generic.IAsyncEnumerator<int>.MoveNextAsync()""
      IL_0063:  stloc.s    V_4
      IL_0065:  ldloca.s   V_4
      IL_0067:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> System.Threading.Tasks.ValueTask<bool>.GetAwaiter()""
      IL_006c:  stloc.3
      IL_006d:  ldloca.s   V_3
      IL_006f:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.IsCompleted.get""
      IL_0074:  brtrue.s   IL_00b5
      IL_0076:  ldarg.0
      IL_0077:  ldc.i4.0
      IL_0078:  dup
      IL_0079:  stloc.0
      IL_007a:  stfld      ""int C.<Test>d__1<T>.<>1__state""
      IL_007f:  ldarg.0
      IL_0080:  ldloc.3
      IL_0081:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_0086:  ldarg.0
      IL_0087:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
      IL_008c:  ldloca.s   V_3
      IL_008e:  ldarg.0
      IL_008f:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, C.<Test>d__1<T>>(ref System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, ref C.<Test>d__1<T>)""
      IL_0094:  leave      IL_019a
      IL_0099:  ldarg.0
      IL_009a:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_009f:  stloc.3
      IL_00a0:  ldarg.0
      IL_00a1:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_00a6:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool>""
      IL_00ac:  ldarg.0
      IL_00ad:  ldc.i4.m1
      IL_00ae:  dup
      IL_00af:  stloc.0
      IL_00b0:  stfld      ""int C.<Test>d__1<T>.<>1__state""
      IL_00b5:  ldloca.s   V_3
      IL_00b7:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.GetResult()""
      IL_00bc:  brtrue.s   IL_0048
      IL_00be:  leave.s    IL_00cc
    }
    catch object
    {
      IL_00c0:  stloc.s    V_5
      IL_00c2:  ldarg.0
      IL_00c3:  ldloc.s    V_5
      IL_00c5:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
      IL_00ca:  leave.s    IL_00cc
    }
    IL_00cc:  ldarg.0
    IL_00cd:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_00d2:  brfalse.s  IL_013b
    IL_00d4:  ldarg.0
    IL_00d5:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_00da:  callvirt   ""System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()""
    IL_00df:  stloc.s    V_7
    IL_00e1:  ldloca.s   V_7
    IL_00e3:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter System.Threading.Tasks.ValueTask.GetAwaiter()""
    IL_00e8:  stloc.s    V_6
    IL_00ea:  ldloca.s   V_6
    IL_00ec:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter.IsCompleted.get""
    IL_00f1:  brtrue.s   IL_0134
    IL_00f3:  ldarg.0
    IL_00f4:  ldc.i4.1
    IL_00f5:  dup
    IL_00f6:  stloc.0
    IL_00f7:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_00fc:  ldarg.0
    IL_00fd:  ldloc.s    V_6
    IL_00ff:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_0104:  ldarg.0
    IL_0105:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
    IL_010a:  ldloca.s   V_6
    IL_010c:  ldarg.0
    IL_010d:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter, C.<Test>d__1<T>>(ref System.Runtime.CompilerServices.ValueTaskAwaiter, ref C.<Test>d__1<T>)""
    IL_0112:  leave      IL_019a
    IL_0117:  ldarg.0
    IL_0118:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_011d:  stloc.s    V_6
    IL_011f:  ldarg.0
    IL_0120:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_0125:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter""
    IL_012b:  ldarg.0
    IL_012c:  ldc.i4.m1
    IL_012d:  dup
    IL_012e:  stloc.0
    IL_012f:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_0134:  ldloca.s   V_6
    IL_0136:  call       ""void System.Runtime.CompilerServices.ValueTaskAwaiter.GetResult()""
    IL_013b:  ldarg.0
    IL_013c:  ldfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_0141:  stloc.s    V_5
    IL_0143:  ldloc.s    V_5
    IL_0145:  brfalse.s  IL_015e
    IL_0147:  ldloc.s    V_5
    IL_0149:  isinst     ""System.Exception""
    IL_014e:  dup
    IL_014f:  brtrue.s   IL_0154
    IL_0151:  ldloc.s    V_5
    IL_0153:  throw
    IL_0154:  call       ""System.Runtime.ExceptionServices.ExceptionDispatchInfo System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(System.Exception)""
    IL_0159:  callvirt   ""void System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()""
    IL_015e:  ldarg.0
    IL_015f:  ldnull
    IL_0160:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_0165:  ldarg.0
    IL_0166:  ldnull
    IL_0167:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_016c:  leave.s    IL_0187
  }
  catch System.Exception
  {
    IL_016e:  stloc.s    V_8
    IL_0170:  ldarg.0
    IL_0171:  ldc.i4.s   -2
    IL_0173:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_0178:  ldarg.0
    IL_0179:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
    IL_017e:  ldloc.s    V_8
    IL_0180:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetException(System.Exception)""
    IL_0185:  leave.s    IL_019a
  }
  IL_0187:  ldarg.0
  IL_0188:  ldc.i4.s   -2
  IL_018a:  stfld      ""int C.<Test>d__1<T>.<>1__state""
  IL_018f:  ldarg.0
  IL_0190:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
  IL_0195:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetResult()""
  IL_019a:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'default(T)')
              Value:
                IInvocationOperation (virtual System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])) (OperationKind.Invocation, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 'default(T)')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: T) (Syntax: 'default(T)')
                  Arguments(1):
                      IArgumentOperation (ArgumentKind.DefaultValue, Matching Parameter: cancellationToken) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'await forea ... }')
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: System.Threading.CancellationToken, IsImplicit) (Syntax: 'await forea ... }')
                        InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IAwaitOperation (OperationKind.Await, Type: System.Boolean, IsImplicit) (Syntax: 'await forea ... }')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask<System.Boolean> System.Collections.Generic.IAsyncEnumerator<System.Int32>.MoveNextAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask<System.Boolean>, IsImplicit) (Syntax: 'default(T)')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                      Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IAsyncEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'default(T)')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IAwaitOperation (OperationKind.Await, Type: System.Void, IsImplicit) (Syntax: 'default(T)')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask, IsImplicit) (Syntax: 'default(T)')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                      Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Equal(1, op.Info.GetEnumeratorArguments.Length);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerableT_05(bool addStructConstraint)
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

interface IMyAsyncEnumerable<T>
{
    IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default);
}

ref struct S : IMyAsyncEnumerable<int>
{
    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = default)
    {
        return Get123();
    }

    async static IAsyncEnumerator<int> Get123()
    {
        await Task.Yield();
        yield return 123;
    }
}

class C
{
    static async Task Main()
    {
        await Test<S>();
    }

    static async Task Test<T>() where T : " + (addStructConstraint ? "struct, " : "") + @"IMyAsyncEnumerable<int>, allows ref struct
    {
        await foreach (var i in default(T))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.<Test>d__1<T>.System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext()",
@"
{
  // Code size      411 (0x19b)
  .maxstack  3
  .locals init (int V_0,
                T V_1,
                System.Threading.CancellationToken V_2,
                System.Runtime.CompilerServices.ValueTaskAwaiter<bool> V_3,
                System.Threading.Tasks.ValueTask<bool> V_4,
                object V_5,
                System.Runtime.CompilerServices.ValueTaskAwaiter V_6,
                System.Threading.Tasks.ValueTask V_7,
                System.Exception V_8)
  IL_0000:  ldarg.0
  IL_0001:  ldfld      ""int C.<Test>d__1<T>.<>1__state""
  IL_0006:  stloc.0
  .try
  {
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_0042
    IL_000a:  ldloc.0
    IL_000b:  ldc.i4.1
    IL_000c:  beq        IL_0117
    IL_0011:  ldarg.0
    IL_0012:  ldloca.s   V_1
    IL_0014:  dup
    IL_0015:  initobj    ""T""
    IL_001b:  ldloca.s   V_2
    IL_001d:  initobj    ""System.Threading.CancellationToken""
    IL_0023:  ldloc.2
    IL_0024:  constrained. ""T""
    IL_002a:  callvirt   ""System.Collections.Generic.IAsyncEnumerator<int> IMyAsyncEnumerable<int>.GetAsyncEnumerator(System.Threading.CancellationToken)""
    IL_002f:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_0034:  ldarg.0
    IL_0035:  ldnull
    IL_0036:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_003b:  ldarg.0
    IL_003c:  ldc.i4.0
    IL_003d:  stfld      ""int C.<Test>d__1<T>.<>7__wrap3""
    IL_0042:  nop
    .try
    {
      IL_0043:  ldloc.0
      IL_0044:  brfalse.s  IL_0099
      IL_0046:  br.s       IL_0058
      IL_0048:  ldarg.0
      IL_0049:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
      IL_004e:  callvirt   ""int System.Collections.Generic.IAsyncEnumerator<int>.Current.get""
      IL_0053:  call       ""void System.Console.Write(int)""
      IL_0058:  ldarg.0
      IL_0059:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
      IL_005e:  callvirt   ""System.Threading.Tasks.ValueTask<bool> System.Collections.Generic.IAsyncEnumerator<int>.MoveNextAsync()""
      IL_0063:  stloc.s    V_4
      IL_0065:  ldloca.s   V_4
      IL_0067:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> System.Threading.Tasks.ValueTask<bool>.GetAwaiter()""
      IL_006c:  stloc.3
      IL_006d:  ldloca.s   V_3
      IL_006f:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.IsCompleted.get""
      IL_0074:  brtrue.s   IL_00b5
      IL_0076:  ldarg.0
      IL_0077:  ldc.i4.0
      IL_0078:  dup
      IL_0079:  stloc.0
      IL_007a:  stfld      ""int C.<Test>d__1<T>.<>1__state""
      IL_007f:  ldarg.0
      IL_0080:  ldloc.3
      IL_0081:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_0086:  ldarg.0
      IL_0087:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
      IL_008c:  ldloca.s   V_3
      IL_008e:  ldarg.0
      IL_008f:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, C.<Test>d__1<T>>(ref System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, ref C.<Test>d__1<T>)""
      IL_0094:  leave      IL_019a
      IL_0099:  ldarg.0
      IL_009a:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_009f:  stloc.3
      IL_00a0:  ldarg.0
      IL_00a1:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_00a6:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool>""
      IL_00ac:  ldarg.0
      IL_00ad:  ldc.i4.m1
      IL_00ae:  dup
      IL_00af:  stloc.0
      IL_00b0:  stfld      ""int C.<Test>d__1<T>.<>1__state""
      IL_00b5:  ldloca.s   V_3
      IL_00b7:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.GetResult()""
      IL_00bc:  brtrue.s   IL_0048
      IL_00be:  leave.s    IL_00cc
    }
    catch object
    {
      IL_00c0:  stloc.s    V_5
      IL_00c2:  ldarg.0
      IL_00c3:  ldloc.s    V_5
      IL_00c5:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
      IL_00ca:  leave.s    IL_00cc
    }
    IL_00cc:  ldarg.0
    IL_00cd:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_00d2:  brfalse.s  IL_013b
    IL_00d4:  ldarg.0
    IL_00d5:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_00da:  callvirt   ""System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()""
    IL_00df:  stloc.s    V_7
    IL_00e1:  ldloca.s   V_7
    IL_00e3:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter System.Threading.Tasks.ValueTask.GetAwaiter()""
    IL_00e8:  stloc.s    V_6
    IL_00ea:  ldloca.s   V_6
    IL_00ec:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter.IsCompleted.get""
    IL_00f1:  brtrue.s   IL_0134
    IL_00f3:  ldarg.0
    IL_00f4:  ldc.i4.1
    IL_00f5:  dup
    IL_00f6:  stloc.0
    IL_00f7:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_00fc:  ldarg.0
    IL_00fd:  ldloc.s    V_6
    IL_00ff:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_0104:  ldarg.0
    IL_0105:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
    IL_010a:  ldloca.s   V_6
    IL_010c:  ldarg.0
    IL_010d:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter, C.<Test>d__1<T>>(ref System.Runtime.CompilerServices.ValueTaskAwaiter, ref C.<Test>d__1<T>)""
    IL_0112:  leave      IL_019a
    IL_0117:  ldarg.0
    IL_0118:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_011d:  stloc.s    V_6
    IL_011f:  ldarg.0
    IL_0120:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_0125:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter""
    IL_012b:  ldarg.0
    IL_012c:  ldc.i4.m1
    IL_012d:  dup
    IL_012e:  stloc.0
    IL_012f:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_0134:  ldloca.s   V_6
    IL_0136:  call       ""void System.Runtime.CompilerServices.ValueTaskAwaiter.GetResult()""
    IL_013b:  ldarg.0
    IL_013c:  ldfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_0141:  stloc.s    V_5
    IL_0143:  ldloc.s    V_5
    IL_0145:  brfalse.s  IL_015e
    IL_0147:  ldloc.s    V_5
    IL_0149:  isinst     ""System.Exception""
    IL_014e:  dup
    IL_014f:  brtrue.s   IL_0154
    IL_0151:  ldloc.s    V_5
    IL_0153:  throw
    IL_0154:  call       ""System.Runtime.ExceptionServices.ExceptionDispatchInfo System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(System.Exception)""
    IL_0159:  callvirt   ""void System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()""
    IL_015e:  ldarg.0
    IL_015f:  ldnull
    IL_0160:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_0165:  ldarg.0
    IL_0166:  ldnull
    IL_0167:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_016c:  leave.s    IL_0187
  }
  catch System.Exception
  {
    IL_016e:  stloc.s    V_8
    IL_0170:  ldarg.0
    IL_0171:  ldc.i4.s   -2
    IL_0173:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_0178:  ldarg.0
    IL_0179:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
    IL_017e:  ldloc.s    V_8
    IL_0180:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetException(System.Exception)""
    IL_0185:  leave.s    IL_019a
  }
  IL_0187:  ldarg.0
  IL_0188:  ldc.i4.s   -2
  IL_018a:  stfld      ""int C.<Test>d__1<T>.<>1__state""
  IL_018f:  ldarg.0
  IL_0190:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
  IL_0195:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetResult()""
  IL_019a:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'default(T)')
              Value:
                IInvocationOperation (virtual System.Collections.Generic.IAsyncEnumerator<System.Int32> IMyAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])) (OperationKind.Invocation, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 'default(T)')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: T) (Syntax: 'default(T)')
                  Arguments(1):
                      IArgumentOperation (ArgumentKind.DefaultValue, Matching Parameter: cancellationToken) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'await forea ... }')
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: System.Threading.CancellationToken, IsImplicit) (Syntax: 'await forea ... }')
                        InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IAwaitOperation (OperationKind.Await, Type: System.Boolean, IsImplicit) (Syntax: 'await forea ... }')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask<System.Boolean> System.Collections.Generic.IAsyncEnumerator<System.Int32>.MoveNextAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask<System.Boolean>, IsImplicit) (Syntax: 'default(T)')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                      Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IAsyncEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'default(T)')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IAwaitOperation (OperationKind.Await, Type: System.Void, IsImplicit) (Syntax: 'default(T)')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask, IsImplicit) (Syntax: 'default(T)')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                      Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> IMyAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> IMyAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Equal(1, op.Info.GetEnumeratorArguments.Length);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerableT_06(bool addStructConstraint)
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

interface IMyAsyncEnumerable1<T>
{
    IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default);
}

interface IMyAsyncEnumerable2<T>
{
    IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default);
}

ref struct S : IMyAsyncEnumerable1<int>, IMyAsyncEnumerable2<int>
{
    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = default)
    {
        return Get123();
    }

    async static IAsyncEnumerator<int> Get123()
    {
        await Task.Yield();
        yield return 123;
    }
}

class C
{
    static async Task Main()
    {
        await Test<S>();
    }

    static async Task Test<T>() where T : " + (addStructConstraint ? "struct, " : "") + @"IMyAsyncEnumerable1<int>, IMyAsyncEnumerable2<int>, allows ref struct
    {
        await foreach (var i in default(T))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (39,33): warning CS0278: 'T' does not implement the 'collection' pattern. 'IMyAsyncEnumerable1<int>.GetAsyncEnumerator(CancellationToken)' is ambiguous with 'IMyAsyncEnumerable2<int>.GetAsyncEnumerator(CancellationToken)'.
                //         await foreach (var i in default(T))
                Diagnostic(ErrorCode.WRN_PatternIsAmbiguous, "default(T)").WithArguments("T", "collection", "IMyAsyncEnumerable1<int>.GetAsyncEnumerator(System.Threading.CancellationToken)", "IMyAsyncEnumerable2<int>.GetAsyncEnumerator(System.Threading.CancellationToken)").WithLocation(39, 33),
                // (39,33): error CS8411: Asynchronous foreach statement cannot operate on variables of type 'T' because 'T' does not contain a suitable public instance or extension definition for 'GetAsyncEnumerator'
                //         await foreach (var i in default(T))
                Diagnostic(ErrorCode.ERR_AwaitForEachMissingMember, "default(T)").WithArguments("T", "GetAsyncEnumerator").WithLocation(39, 33)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            Assert.Null(info.GetEnumeratorMethod);
            Assert.Null(info.ElementType);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.Null(op.Info);
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerableT_07(bool addStructConstraint)
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

interface IMyAsyncEnumerable1<T>
{
    IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default);
}

interface IMyAsyncEnumerable2<T>
{
    IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default);
}

ref struct S : IMyAsyncEnumerable1<int>, IMyAsyncEnumerable2<int>, IAsyncEnumerable<int>
{
    IAsyncEnumerator<int> IMyAsyncEnumerable1<int>.GetAsyncEnumerator(CancellationToken token) => throw null;
    IAsyncEnumerator<int> IMyAsyncEnumerable2<int>.GetAsyncEnumerator(CancellationToken token) => throw null;

    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = default)
    {
        return Get123();
    }

    async static IAsyncEnumerator<int> Get123()
    {
        await Task.Yield();
        yield return 123;
    }
}

class C
{
    static async Task Main()
    {
        await Test<S>();
    }

    static async Task Test<T>() where T : " + (addStructConstraint ? "struct, " : "") + @"IMyAsyncEnumerable1<int>, IMyAsyncEnumerable2<int>, IAsyncEnumerable<int>, allows ref struct
    {
        await foreach (var i in default(T))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics(
                // (42,33): warning CS0278: 'T' does not implement the 'collection' pattern. 'IMyAsyncEnumerable1<int>.GetAsyncEnumerator(CancellationToken)' is ambiguous with 'IMyAsyncEnumerable2<int>.GetAsyncEnumerator(CancellationToken)'.
                //         await foreach (var i in default(T))
                Diagnostic(ErrorCode.WRN_PatternIsAmbiguous, "default(T)").WithArguments("T", "collection", "IMyAsyncEnumerable1<int>.GetAsyncEnumerator(System.Threading.CancellationToken)", "IMyAsyncEnumerable2<int>.GetAsyncEnumerator(System.Threading.CancellationToken)").WithLocation(42, 33)
                );

            verifier.VerifyIL("C.<Test>d__1<T>.System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext()",
@"
{
  // Code size      411 (0x19b)
  .maxstack  3
  .locals init (int V_0,
                T V_1,
                System.Threading.CancellationToken V_2,
                System.Runtime.CompilerServices.ValueTaskAwaiter<bool> V_3,
                System.Threading.Tasks.ValueTask<bool> V_4,
                object V_5,
                System.Runtime.CompilerServices.ValueTaskAwaiter V_6,
                System.Threading.Tasks.ValueTask V_7,
                System.Exception V_8)
  IL_0000:  ldarg.0
  IL_0001:  ldfld      ""int C.<Test>d__1<T>.<>1__state""
  IL_0006:  stloc.0
  .try
  {
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_0042
    IL_000a:  ldloc.0
    IL_000b:  ldc.i4.1
    IL_000c:  beq        IL_0117
    IL_0011:  ldarg.0
    IL_0012:  ldloca.s   V_1
    IL_0014:  dup
    IL_0015:  initobj    ""T""
    IL_001b:  ldloca.s   V_2
    IL_001d:  initobj    ""System.Threading.CancellationToken""
    IL_0023:  ldloc.2
    IL_0024:  constrained. ""T""
    IL_002a:  callvirt   ""System.Collections.Generic.IAsyncEnumerator<int> System.Collections.Generic.IAsyncEnumerable<int>.GetAsyncEnumerator(System.Threading.CancellationToken)""
    IL_002f:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_0034:  ldarg.0
    IL_0035:  ldnull
    IL_0036:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_003b:  ldarg.0
    IL_003c:  ldc.i4.0
    IL_003d:  stfld      ""int C.<Test>d__1<T>.<>7__wrap3""
    IL_0042:  nop
    .try
    {
      IL_0043:  ldloc.0
      IL_0044:  brfalse.s  IL_0099
      IL_0046:  br.s       IL_0058
      IL_0048:  ldarg.0
      IL_0049:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
      IL_004e:  callvirt   ""int System.Collections.Generic.IAsyncEnumerator<int>.Current.get""
      IL_0053:  call       ""void System.Console.Write(int)""
      IL_0058:  ldarg.0
      IL_0059:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
      IL_005e:  callvirt   ""System.Threading.Tasks.ValueTask<bool> System.Collections.Generic.IAsyncEnumerator<int>.MoveNextAsync()""
      IL_0063:  stloc.s    V_4
      IL_0065:  ldloca.s   V_4
      IL_0067:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> System.Threading.Tasks.ValueTask<bool>.GetAwaiter()""
      IL_006c:  stloc.3
      IL_006d:  ldloca.s   V_3
      IL_006f:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.IsCompleted.get""
      IL_0074:  brtrue.s   IL_00b5
      IL_0076:  ldarg.0
      IL_0077:  ldc.i4.0
      IL_0078:  dup
      IL_0079:  stloc.0
      IL_007a:  stfld      ""int C.<Test>d__1<T>.<>1__state""
      IL_007f:  ldarg.0
      IL_0080:  ldloc.3
      IL_0081:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_0086:  ldarg.0
      IL_0087:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
      IL_008c:  ldloca.s   V_3
      IL_008e:  ldarg.0
      IL_008f:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, C.<Test>d__1<T>>(ref System.Runtime.CompilerServices.ValueTaskAwaiter<bool>, ref C.<Test>d__1<T>)""
      IL_0094:  leave      IL_019a
      IL_0099:  ldarg.0
      IL_009a:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_009f:  stloc.3
      IL_00a0:  ldarg.0
      IL_00a1:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool> C.<Test>d__1<T>.<>u__1""
      IL_00a6:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter<bool>""
      IL_00ac:  ldarg.0
      IL_00ad:  ldc.i4.m1
      IL_00ae:  dup
      IL_00af:  stloc.0
      IL_00b0:  stfld      ""int C.<Test>d__1<T>.<>1__state""
      IL_00b5:  ldloca.s   V_3
      IL_00b7:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter<bool>.GetResult()""
      IL_00bc:  brtrue.s   IL_0048
      IL_00be:  leave.s    IL_00cc
    }
    catch object
    {
      IL_00c0:  stloc.s    V_5
      IL_00c2:  ldarg.0
      IL_00c3:  ldloc.s    V_5
      IL_00c5:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
      IL_00ca:  leave.s    IL_00cc
    }
    IL_00cc:  ldarg.0
    IL_00cd:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_00d2:  brfalse.s  IL_013b
    IL_00d4:  ldarg.0
    IL_00d5:  ldfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_00da:  callvirt   ""System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()""
    IL_00df:  stloc.s    V_7
    IL_00e1:  ldloca.s   V_7
    IL_00e3:  call       ""System.Runtime.CompilerServices.ValueTaskAwaiter System.Threading.Tasks.ValueTask.GetAwaiter()""
    IL_00e8:  stloc.s    V_6
    IL_00ea:  ldloca.s   V_6
    IL_00ec:  call       ""bool System.Runtime.CompilerServices.ValueTaskAwaiter.IsCompleted.get""
    IL_00f1:  brtrue.s   IL_0134
    IL_00f3:  ldarg.0
    IL_00f4:  ldc.i4.1
    IL_00f5:  dup
    IL_00f6:  stloc.0
    IL_00f7:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_00fc:  ldarg.0
    IL_00fd:  ldloc.s    V_6
    IL_00ff:  stfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_0104:  ldarg.0
    IL_0105:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
    IL_010a:  ldloca.s   V_6
    IL_010c:  ldarg.0
    IL_010d:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.ValueTaskAwaiter, C.<Test>d__1<T>>(ref System.Runtime.CompilerServices.ValueTaskAwaiter, ref C.<Test>d__1<T>)""
    IL_0112:  leave      IL_019a
    IL_0117:  ldarg.0
    IL_0118:  ldfld      ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_011d:  stloc.s    V_6
    IL_011f:  ldarg.0
    IL_0120:  ldflda     ""System.Runtime.CompilerServices.ValueTaskAwaiter C.<Test>d__1<T>.<>u__2""
    IL_0125:  initobj    ""System.Runtime.CompilerServices.ValueTaskAwaiter""
    IL_012b:  ldarg.0
    IL_012c:  ldc.i4.m1
    IL_012d:  dup
    IL_012e:  stloc.0
    IL_012f:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_0134:  ldloca.s   V_6
    IL_0136:  call       ""void System.Runtime.CompilerServices.ValueTaskAwaiter.GetResult()""
    IL_013b:  ldarg.0
    IL_013c:  ldfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_0141:  stloc.s    V_5
    IL_0143:  ldloc.s    V_5
    IL_0145:  brfalse.s  IL_015e
    IL_0147:  ldloc.s    V_5
    IL_0149:  isinst     ""System.Exception""
    IL_014e:  dup
    IL_014f:  brtrue.s   IL_0154
    IL_0151:  ldloc.s    V_5
    IL_0153:  throw
    IL_0154:  call       ""System.Runtime.ExceptionServices.ExceptionDispatchInfo System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(System.Exception)""
    IL_0159:  callvirt   ""void System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()""
    IL_015e:  ldarg.0
    IL_015f:  ldnull
    IL_0160:  stfld      ""object C.<Test>d__1<T>.<>7__wrap2""
    IL_0165:  ldarg.0
    IL_0166:  ldnull
    IL_0167:  stfld      ""System.Collections.Generic.IAsyncEnumerator<int> C.<Test>d__1<T>.<>7__wrap1""
    IL_016c:  leave.s    IL_0187
  }
  catch System.Exception
  {
    IL_016e:  stloc.s    V_8
    IL_0170:  ldarg.0
    IL_0171:  ldc.i4.s   -2
    IL_0173:  stfld      ""int C.<Test>d__1<T>.<>1__state""
    IL_0178:  ldarg.0
    IL_0179:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
    IL_017e:  ldloc.s    V_8
    IL_0180:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetException(System.Exception)""
    IL_0185:  leave.s    IL_019a
  }
  IL_0187:  ldarg.0
  IL_0188:  ldc.i4.s   -2
  IL_018a:  stfld      ""int C.<Test>d__1<T>.<>1__state""
  IL_018f:  ldarg.0
  IL_0190:  ldflda     ""System.Runtime.CompilerServices.AsyncTaskMethodBuilder C.<Test>d__1<T>.<>t__builder""
  IL_0195:  call       ""void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.SetResult()""
  IL_019a:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'default(T)')
              Value:
                IInvocationOperation (virtual System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])) (OperationKind.Invocation, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 'default(T)')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: T) (Syntax: 'default(T)')
                  Arguments(1):
                      IArgumentOperation (ArgumentKind.DefaultValue, Matching Parameter: cancellationToken) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'default(T)')
                        IDefaultValueOperation (OperationKind.DefaultValue, Type: System.Threading.CancellationToken, IsImplicit) (Syntax: 'default(T)')
                        InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IAwaitOperation (OperationKind.Await, Type: System.Boolean, IsImplicit) (Syntax: 'await forea ... }')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask<System.Boolean> System.Collections.Generic.IAsyncEnumerator<System.Int32>.MoveNextAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask<System.Boolean>, IsImplicit) (Syntax: 'default(T)')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                      Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Int32 i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Int32 System.Collections.Generic.IAsyncEnumerator<System.Int32>.Current { get; } (OperationKind.PropertyReference, Type: System.Int32, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Int32 value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Int32) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        Block[B4] - Block
            Predecessors (0)
            Statements (0)
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'default(T)')
                  Operand:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IAwaitOperation (OperationKind.Await, Type: System.Void, IsImplicit) (Syntax: 'default(T)')
                  Expression:
                    IInvocationOperation (virtual System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()) (OperationKind.Invocation, Type: System.Threading.Tasks.ValueTask, IsImplicit) (Syntax: 'default(T)')
                      Instance Receiver:
                        IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.IAsyncDisposable, IsImplicit) (Syntax: 'default(T)')
                          Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                            (ImplicitReference)
                          Operand:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.Generic.IAsyncEnumerator<System.Int32>, IsImplicit) (Syntax: 'default(T)')
                      Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Int32", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.Generic.IAsyncEnumerator<System.Int32> System.Collections.Generic.IAsyncEnumerable<System.Int32>.GetAsyncEnumerator([System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)])", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Equal(1, op.Info.GetEnumeratorArguments.Length);
            AssertEx.Equal("System.Int32", op.Info.ElementType.ToTestDisplayString());
        }

        [Fact]
        public void AwaitForeach_Pattern()
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

ref struct S
{
    public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = default)
    {
        return Get123();
    }

    async static IAsyncEnumerator<int> Get123()
    {
        await Task.Yield();
        yield return 123;
    }
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerator_01(bool s1IsRefStruct)
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IAsyncEnumerator<int>
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (27,15): error CS8344: foreach statement cannot operate on enumerators of type 'S2' in async or iterator methods because 'S2' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in new S1())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("S2").WithLocation(27, 15)
                );
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerator_03(bool s1IsRefStruct)
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IAsyncEnumerator<int>
{
    public int Current => throw null;
    public ValueTask<bool> MoveNextAsync() => throw null;

    int IAsyncEnumerator<int>.Current => throw null;

    ValueTask System.IAsyncDisposable.DisposeAsync() => throw null;

    ValueTask<bool> IAsyncEnumerator<int>.MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (30,15): error CS8344: foreach statement cannot operate on enumerators of type 'S2' in async or iterator methods because 'S2' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in new S1())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("S2").WithLocation(30, 15)
                );
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerator_05(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

interface IGetEnumerator<TEnumerator> where TEnumerator : IAsyncEnumerator<int>, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IAsyncEnumerator<int>
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"IAsyncEnumerator<int>, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (39,15): error CS8344: foreach statement cannot operate on enumerators of type 'TEnumerator' in async or iterator methods because 'TEnumerator' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("TEnumerator").WithLocation(39, 15)
                );
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerator_07(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System.Threading;
using System.Threading.Tasks;

interface IMyAsyncEnumerator<T>
{
    T Current {get;}

    ValueTask<bool> MoveNextAsync();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : IMyAsyncEnumerator<int>, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IMyAsyncEnumerator<int>
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"IMyAsyncEnumerator<int>, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (45,15): error CS8344: foreach statement cannot operate on enumerators of type 'TEnumerator' in async or iterator methods because 'TEnumerator' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("TEnumerator").WithLocation(45, 15)
                );
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerator_08(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System.Threading;
using System.Threading.Tasks;

interface IMyAsyncEnumerator1<T>
{
    T Current {get;}

    ValueTask<bool> MoveNextAsync();
}

interface IMyAsyncEnumerator2<T>
{
    T Current {get;}

    ValueTask<bool> MoveNextAsync();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : IMyAsyncEnumerator1<int>, IMyAsyncEnumerator2<int>, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IMyAsyncEnumerator1<int>, IMyAsyncEnumerator2<int> 
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"IMyAsyncEnumerator1<int>, IMyAsyncEnumerator2<int>, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (52,33): error CS8412: Asynchronous foreach requires that the return type 'TEnumerator' of 'IGetEnumerator<TEnumerator>.GetAsyncEnumerator(CancellationToken)' must have a suitable public 'MoveNextAsync' method and public 'Current' property
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadGetAsyncEnumerator, "default(TEnumerable)").WithArguments("TEnumerator", "IGetEnumerator<TEnumerator>.GetAsyncEnumerator(System.Threading.CancellationToken)").WithLocation(52, 33)
                );
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncEnumerator_09(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

interface IMyAsyncEnumerator1<T>
{
    T Current {get;}

    ValueTask<bool> MoveNextAsync();
}

interface IMyAsyncEnumerator2<T>
{
    T Current {get;}

    ValueTask<bool> MoveNextAsync();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : IMyAsyncEnumerator1<int>, IMyAsyncEnumerator2<int>, IAsyncEnumerator<int>, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IMyAsyncEnumerator1<int>, IMyAsyncEnumerator2<int>, IAsyncEnumerator<int> 
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"IMyAsyncEnumerator1<int>, IMyAsyncEnumerator2<int>, IAsyncEnumerator<int>, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (53,33): error CS8412: Asynchronous foreach requires that the return type 'TEnumerator' of 'IGetEnumerator<TEnumerator>.GetAsyncEnumerator(CancellationToken)' must have a suitable public 'MoveNextAsync' method and public 'Current' property
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadGetAsyncEnumerator, "default(TEnumerable)").WithArguments("TEnumerator", "IGetEnumerator<TEnumerator>.GetAsyncEnumerator(System.Threading.CancellationToken)").WithLocation(53, 33)
                );
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncDisposable_01(bool s1IsRefStruct)
        {
            var src = @"
using System;
using System.Threading;
using System.Threading.Tasks;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IAsyncDisposable
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (27,15): error CS8344: foreach statement cannot operate on enumerators of type 'S2' in async or iterator methods because 'S2' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in new S1())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("S2").WithLocation(27, 15)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Threading.Tasks.ValueTask S2.DisposeAsync()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            AssertEx.Equal("System.Threading.Tasks.ValueTask S2.DisposeAsync()", op.Info.PatternDisposeMethod.ToTestDisplayString());
            Assert.True(op.Info.DisposeArguments.IsEmpty);
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncDisposable_02(bool s1IsRefStruct)
        {
            var src = @"
using System;
using System.Threading;
using System.Threading.Tasks;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IAsyncDisposable
{
    public int Current => throw null;

    ValueTask IAsyncDisposable.DisposeAsync() => throw null;
    public ValueTask DisposeAsync() => throw null;
    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (27,15): error CS8344: foreach statement cannot operate on enumerators of type 'S2' in async or iterator methods because 'S2' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in new S1())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("S2").WithLocation(27, 15)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Threading.Tasks.ValueTask S2.DisposeAsync()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            AssertEx.Equal("System.Threading.Tasks.ValueTask S2.DisposeAsync()", op.Info.PatternDisposeMethod.ToTestDisplayString());
            Assert.True(op.Info.DisposeArguments.IsEmpty);
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncDisposable_03(bool s1IsRefStruct)
        {
            var src = @"
using System;
using System.Threading;
using System.Threading.Tasks;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : IAsyncDisposable
{
    public int Current => throw null;

    ValueTask IAsyncDisposable.DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (27,15): error CS8344: foreach statement cannot operate on enumerators of type 'S2' in async or iterator methods because 'S2' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in new S1())
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("S2").WithLocation(27, 15)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncDisposable_04(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System;
using System.Threading;
using System.Threading.Tasks;

interface ICustomEnumerator
{
    public int Current {get;}

    public ValueTask<bool> MoveNextAsync();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : ICustomEnumerator, IAsyncDisposable
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"ICustomEnumerator, IAsyncDisposable, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (46,15): error CS8344: foreach statement cannot operate on enumerators of type 'TEnumerator' in async or iterator methods because 'TEnumerator' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("TEnumerator").WithLocation(46, 15)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            AssertEx.Equal("System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()", op.Info.PatternDisposeMethod.ToTestDisplayString());
            Assert.True(op.Info.DisposeArguments.IsEmpty);
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncDisposable_06(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System.Threading;
using System.Threading.Tasks;

interface ICustomEnumerator
{
    public int Current {get;}

    public ValueTask<bool> MoveNextAsync();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

interface IMyAsyncDisposable
{
    ValueTask DisposeAsync();
}

ref struct S2 : ICustomEnumerator, IMyAsyncDisposable
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"ICustomEnumerator, IMyAsyncDisposable, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (50,15): error CS8344: foreach statement cannot operate on enumerators of type 'TEnumerator' in async or iterator methods because 'TEnumerator' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("TEnumerator").WithLocation(50, 15)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Threading.Tasks.ValueTask IMyAsyncDisposable.DisposeAsync()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            Assert.True(op.Info.NeedsDispose);
            Assert.False(op.Info.KnownToImplementIDisposable);
            AssertEx.Equal("System.Threading.Tasks.ValueTask IMyAsyncDisposable.DisposeAsync()", op.Info.PatternDisposeMethod.ToTestDisplayString());
            Assert.True(op.Info.DisposeArguments.IsEmpty);
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncDisposable_07(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System.Threading;
using System.Threading.Tasks;

interface ICustomEnumerator
{
    public int Current {get;}

    public ValueTask<bool> MoveNextAsync();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

interface IMyAsyncDisposable1
{
    ValueTask DisposeAsync();
}

interface IMyAsyncDisposable2
{
    ValueTask DisposeAsync();
}

ref struct S2 : ICustomEnumerator, IMyAsyncDisposable1, IMyAsyncDisposable2
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"ICustomEnumerator, IMyAsyncDisposable1, IMyAsyncDisposable2, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (55,15): error CS8344: foreach statement cannot operate on enumerators of type 'TEnumerator' in async or iterator methods because 'TEnumerator' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("TEnumerator").WithLocation(55, 15)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            Assert.False(op.Info.NeedsDispose);
            Assert.False(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncDisposable_08(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System;
using System.Threading;
using System.Threading.Tasks;

interface ICustomEnumerator
{
    public int Current {get;}

    public ValueTask<bool> MoveNextAsync();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

interface IMyAsyncDisposable1
{
    ValueTask DisposeAsync();
}

interface IMyAsyncDisposable2
{
    ValueTask DisposeAsync();
}

ref struct S2 : ICustomEnumerator, IMyAsyncDisposable1, IMyAsyncDisposable2, IAsyncDisposable
{
    ValueTask IMyAsyncDisposable1.DisposeAsync() => throw null;
    ValueTask IMyAsyncDisposable2.DisposeAsync() => throw null;

    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"ICustomEnumerator, IMyAsyncDisposable1, IMyAsyncDisposable2, IAsyncDisposable, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (59,15): error CS8344: foreach statement cannot operate on enumerators of type 'TEnumerator' in async or iterator methods because 'TEnumerator' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("TEnumerator").WithLocation(59, 15)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            AssertEx.Equal("System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void AwaitForeach_IAsyncDisposable_09(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System;
using System.Threading;
using System.Threading.Tasks;

interface ICustomEnumerator
{
    public int Current {get;}

    public ValueTask<bool> MoveNextAsync();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : ICustomEnumerator, allows ref struct 
{
    TEnumerator GetAsyncEnumerator(CancellationToken token = default);
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetAsyncEnumerator(CancellationToken token = default)
    {
        return new S2();
    }
}

ref struct S2 : ICustomEnumerator, IAsyncDisposable
{
    public int Current => throw null;

    public ValueTask DisposeAsync() => throw null;

    public ValueTask<bool> MoveNextAsync() => throw null;
}

class C
{
    static async Task Main()
    {
        await Test<S1, S2>();
    }

    static async Task Test<TEnumerable, TEnumerator>()
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"ICustomEnumerator, allows ref struct 
    {
        await foreach (var i in default(TEnumerable))
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (46,15): error CS8344: foreach statement cannot operate on enumerators of type 'TEnumerator' in async or iterator methods because 'TEnumerator' is a ref struct or a type parameter that allows ref struct.
                //         await foreach (var i in default(TEnumerable))
                Diagnostic(ErrorCode.ERR_BadSpecialByRefIterator, "foreach").WithArguments("TEnumerator").WithLocation(46, 15)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.True(info.IsAsynchronous);
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.True(op.Info.IsAsynchronous);
            Assert.False(op.Info.NeedsDispose);
            Assert.False(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Fact]
        public void Foreach_IEnumerable_01()
        {
            var src = @"
using System.Collections;

ref struct S : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator Get123()
    {
        yield return 123;
    }
}

class C
{
    static void Main()
    {
        foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       56 (0x38)
  .maxstack  2
  .locals init (System.Collections.IEnumerator V_0,
                S V_1,
                System.IDisposable V_2)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S""
  IL_0009:  call       ""System.Collections.IEnumerator S.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001c
    IL_0011:  ldloc.0
    IL_0012:  callvirt   ""object System.Collections.IEnumerator.Current.get""
    IL_0017:  call       ""void System.Console.Write(object)""
    IL_001c:  ldloc.0
    IL_001d:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0022:  brtrue.s   IL_0011
    IL_0024:  leave.s    IL_0037
  }
  finally
  {
    IL_0026:  ldloc.0
    IL_0027:  isinst     ""System.IDisposable""
    IL_002c:  stloc.2
    IL_002d:  ldloc.2
    IL_002e:  brfalse.s  IL_0036
    IL_0030:  ldloc.2
    IL_0031:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0036:  endfinally
  }
  IL_0037:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation ( System.Collections.IEnumerator S.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Object i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Object System.Collections.IEnumerator.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        CaptureIds: [1]
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IFlowCaptureOperation: 1 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
                  Value:
                    IConversionOperation (TryCast: True, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ExplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator S.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator S.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
        }

        [Fact]
        public void Foreach_IEnumerable_02()
        {
            var src = @"
using System.Collections;

ref struct S : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator Get123()
    {
        yield return 123;
    }

    IEnumerator IEnumerable.GetEnumerator() => throw null;
}

class C
{
    static void Main()
    {
        foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       56 (0x38)
  .maxstack  2
  .locals init (System.Collections.IEnumerator V_0,
                S V_1,
                System.IDisposable V_2)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S""
  IL_0009:  call       ""System.Collections.IEnumerator S.GetEnumerator()""
  IL_000e:  stloc.0
  .try
  {
    IL_000f:  br.s       IL_001c
    IL_0011:  ldloc.0
    IL_0012:  callvirt   ""object System.Collections.IEnumerator.Current.get""
    IL_0017:  call       ""void System.Console.Write(object)""
    IL_001c:  ldloc.0
    IL_001d:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0022:  brtrue.s   IL_0011
    IL_0024:  leave.s    IL_0037
  }
  finally
  {
    IL_0026:  ldloc.0
    IL_0027:  isinst     ""System.IDisposable""
    IL_002c:  stloc.2
    IL_002d:  ldloc.2
    IL_002e:  brfalse.s  IL_0036
    IL_0030:  ldloc.2
    IL_0031:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0036:  endfinally
  }
  IL_0037:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation ( System.Collections.IEnumerator S.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Object i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Object System.Collections.IEnumerator.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        CaptureIds: [1]
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IFlowCaptureOperation: 1 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
                  Value:
                    IConversionOperation (TryCast: True, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ExplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator S.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator S.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
        }

        [Fact]
        public void Foreach_IEnumerable_03()
        {
            var src = @"
using System.Collections;

ref struct S : IEnumerable
{
    IEnumerator IEnumerable.GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator Get123()
    {
        yield return 123;
    }
}

class C
{
    static void Main()
    {
        foreach (var i in new S())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
      // Code size       62 (0x3e)
      .maxstack  2
      .locals init (System.Collections.IEnumerator V_0,
                    S V_1,
                    System.IDisposable V_2)
      IL_0000:  ldloca.s   V_1
      IL_0002:  dup
      IL_0003:  initobj    ""S""
      IL_0009:  constrained. ""S""
      IL_000f:  callvirt   ""System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()""
      IL_0014:  stloc.0
      .try
      {
        IL_0015:  br.s       IL_0022
        IL_0017:  ldloc.0
        IL_0018:  callvirt   ""object System.Collections.IEnumerator.Current.get""
        IL_001d:  call       ""void System.Console.Write(object)""
        IL_0022:  ldloc.0
        IL_0023:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
        IL_0028:  brtrue.s   IL_0017
        IL_002a:  leave.s    IL_003d
      }
      finally
      {
        IL_002c:  ldloc.0
        IL_002d:  isinst     ""System.IDisposable""
        IL_0032:  stloc.2
        IL_0033:  ldloc.2
        IL_0034:  brfalse.s  IL_003c
        IL_0036:  ldloc.2
        IL_0037:  callvirt   ""void System.IDisposable.Dispose()""
        IL_003c:  endfinally
      }
      IL_003d:  ret
    }
    ");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
              Value:
                IInvocationOperation (virtual System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S..ctor()) (OperationKind.ObjectCreation, Type: S) (Syntax: 'new S()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Object i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Object System.Collections.IEnumerator.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        CaptureIds: [1]
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IFlowCaptureOperation: 1 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S()')
                  Value:
                    IConversionOperation (TryCast: True, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ExplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 'new S()')
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 'new S()')
                  Operand:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 'new S()')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 'new S()')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerable_04(bool addStructConstraint)
        {
            var src = @"
using System.Collections;

ref struct S : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator Get123()
    {
        yield return 123;
    }
}

class C
{
    static void Main()
    {
        Test(new S());
    }

    static void Test<T>(T t) where T : " + (addStructConstraint ? "struct, " : "") + @"IEnumerable, allows ref struct
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<T>(T)",
@"
{
  // Code size       55 (0x37)
  .maxstack  1
  .locals init (System.Collections.IEnumerator V_0,
                System.IDisposable V_1)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""T""
  IL_0008:  callvirt   ""System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_001b
    IL_0010:  ldloc.0
    IL_0011:  callvirt   ""object System.Collections.IEnumerator.Current.get""
    IL_0016:  call       ""void System.Console.Write(object)""
    IL_001b:  ldloc.0
    IL_001c:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0021:  brtrue.s   IL_0010
    IL_0023:  leave.s    IL_0036
  }
  finally
  {
    IL_0025:  ldloc.0
    IL_0026:  isinst     ""System.IDisposable""
    IL_002b:  stloc.1
    IL_002c:  ldloc.1
    IL_002d:  brfalse.s  IL_0035
    IL_002f:  ldloc.1
    IL_0030:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0035:  endfinally
  }
  IL_0036:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: T) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Object i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Object System.Collections.IEnumerator.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        CaptureIds: [1]
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IFlowCaptureOperation: 1 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
                  Value:
                    IConversionOperation (TryCast: True, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ExplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerable_05(bool addStructConstraint)
        {
            var src = @"
using System.Collections;

interface IMyEnumerable
{
    IEnumerator GetEnumerator();
}

ref struct S : IMyEnumerable
{
    public IEnumerator GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator Get123()
    {
        yield return 123;
    }
}

class C
{
    static void Main()
    {
        Test(new S());
    }

    static void Test<T>(T t) where T : " + (addStructConstraint ? "struct, " : "") + @"IMyEnumerable, allows ref struct
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Test<T>(T)",
@"
{
  // Code size       55 (0x37)
  .maxstack  1
  .locals init (System.Collections.IEnumerator V_0,
                System.IDisposable V_1)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""T""
  IL_0008:  callvirt   ""System.Collections.IEnumerator IMyEnumerable.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_001b
    IL_0010:  ldloc.0
    IL_0011:  callvirt   ""object System.Collections.IEnumerator.Current.get""
    IL_0016:  call       ""void System.Console.Write(object)""
    IL_001b:  ldloc.0
    IL_001c:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0021:  brtrue.s   IL_0010
    IL_0023:  leave.s    IL_0036
  }
  finally
  {
    IL_0025:  ldloc.0
    IL_0026:  isinst     ""System.IDisposable""
    IL_002b:  stloc.1
    IL_002c:  ldloc.1
    IL_002d:  brfalse.s  IL_0035
    IL_002f:  ldloc.1
    IL_0030:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0035:  endfinally
  }
  IL_0036:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual System.Collections.IEnumerator IMyEnumerable.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: T) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Object i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Object System.Collections.IEnumerator.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        CaptureIds: [1]
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IFlowCaptureOperation: 1 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
                  Value:
                    IConversionOperation (TryCast: True, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ExplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator IMyEnumerable.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator IMyEnumerable.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerable_06(bool addStructConstraint)
        {
            var src = @"
using System.Collections;

interface IMyEnumerable1
{
    IEnumerator GetEnumerator();
}

interface IMyEnumerable2
{
    IEnumerator GetEnumerator();
}

ref struct S : IMyEnumerable1, IMyEnumerable2
{
    public IEnumerator GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator Get123()
    {
        yield return 123;
    }
}

class C
{
    static void Main()
    {
        Test(new S());
    }

    static void Test<T>(T t) where T : " + (addStructConstraint ? "struct, " : "") + @"IMyEnumerable1, IMyEnumerable2, allows ref struct
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (36,27): warning CS0278: 'T' does not implement the 'collection' pattern. 'IMyEnumerable1.GetEnumerator()' is ambiguous with 'IMyEnumerable2.GetEnumerator()'.
                //         foreach (var i in t)
                Diagnostic(ErrorCode.WRN_PatternIsAmbiguous, "t").WithArguments("T", "collection", "IMyEnumerable1.GetEnumerator()", "IMyEnumerable2.GetEnumerator()").WithLocation(36, 27),
                // (36,27): error CS1579: foreach statement cannot operate on variables of type 'T' because 'T' does not contain a public instance or extension definition for 'GetEnumerator'
                //         foreach (var i in t)
                Diagnostic(ErrorCode.ERR_ForEachMissingMember, "t").WithArguments("T", "GetEnumerator").WithLocation(36, 27)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            Assert.Null(info.GetEnumeratorMethod);
            Assert.Null(info.ElementType);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.Null(op.Info);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerable_07(bool addStructConstraint)
        {
            var src = @"
using System.Collections;

interface IMyEnumerable1
{
    IEnumerator GetEnumerator();
}

interface IMyEnumerable2
{
    IEnumerator GetEnumerator();
}

ref struct S : IMyEnumerable1, IMyEnumerable2, IEnumerable
{
    IEnumerator IMyEnumerable1.GetEnumerator() => throw null;
    IEnumerator IMyEnumerable2.GetEnumerator() => throw null;

    public IEnumerator GetEnumerator()
    {
        return Get123();
    }

    static IEnumerator Get123()
    {
        yield return 123;
    }
}

class C
{
    static void Main()
    {
        Test(new S());
    }

    static void Test<T>(T t) where T : " + (addStructConstraint ? "struct, " : "") + @"IMyEnumerable1, IMyEnumerable2, IEnumerable, allows ref struct
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics(
                // (39,27): warning CS0278: 'T' does not implement the 'collection' pattern. 'IMyEnumerable1.GetEnumerator()' is ambiguous with 'IMyEnumerable2.GetEnumerator()'.
                //         foreach (var i in t)
                Diagnostic(ErrorCode.WRN_PatternIsAmbiguous, "t").WithArguments("T", "collection", "IMyEnumerable1.GetEnumerator()", "IMyEnumerable2.GetEnumerator()").WithLocation(39, 27)
                );

            verifier.VerifyIL("C.Test<T>(T)",
@"
{
  // Code size       55 (0x37)
  .maxstack  1
  .locals init (System.Collections.IEnumerator V_0,
                System.IDisposable V_1)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""T""
  IL_0008:  callvirt   ""System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_001b
    IL_0010:  ldloc.0
    IL_0011:  callvirt   ""object System.Collections.IEnumerator.Current.get""
    IL_0016:  call       ""void System.Console.Write(object)""
    IL_001b:  ldloc.0
    IL_001c:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_0021:  brtrue.s   IL_0010
    IL_0023:  leave.s    IL_0036
  }
  finally
  {
    IL_0025:  ldloc.0
    IL_0026:  isinst     ""System.IDisposable""
    IL_002b:  stloc.1
    IL_002c:  ldloc.1
    IL_002d:  brfalse.s  IL_0035
    IL_002f:  ldloc.1
    IL_0030:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0035:  endfinally
  }
  IL_0036:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
              Value:
                IInvocationOperation (virtual System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()) (OperationKind.Invocation, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: T, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IParameterReferenceOperation: t (OperationKind.ParameterReference, Type: T) (Syntax: 't')
                  Arguments(0)
        Next (Regular) Block[B2]
            Entering: {R2} {R3}
    .try {R2, R3}
    {
        Block[B2] - Block
            Predecessors: [B1] [B3]
            Statements (0)
            Jump if False (Regular) to Block[B7]
                IInvocationOperation (virtual System.Boolean System.Collections.IEnumerator.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                  Arguments(0)
                Finalizing: {R5}
                Leaving: {R3} {R2} {R1}
            Next (Regular) Block[B3]
                Entering: {R4}
        .locals {R4}
        {
            Locals: [System.Object i]
            Block[B3] - Block
                Predecessors: [B2]
                Statements (2)
                    ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                      Left:
                        ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Right:
                        IPropertyReferenceOperation: System.Object System.Collections.IEnumerator.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                          Instance Receiver:
                            IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
                    IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                      Expression:
                        IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                          Instance Receiver:
                            null
                          Arguments(1):
                              IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                                ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                                InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                                OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                Next (Regular) Block[B2]
                    Leaving: {R4}
        }
    }
    .finally {R5}
    {
        CaptureIds: [1]
        Block[B4] - Block
            Predecessors (0)
            Statements (1)
                IFlowCaptureOperation: 1 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 't')
                  Value:
                    IConversionOperation (TryCast: True, Unchecked) (OperationKind.Conversion, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                      Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
                        (ExplicitReference)
                      Operand:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: System.Collections.IEnumerator, IsImplicit) (Syntax: 't')
            Jump if True (Regular) to Block[B6]
                IIsNullOperation (OperationKind.IsNull, Type: System.Boolean, IsImplicit) (Syntax: 't')
                  Operand:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 't')
            Next (Regular) Block[B5]
        Block[B5] - Block
            Predecessors: [B4]
            Statements (1)
                IInvocationOperation (virtual void System.IDisposable.Dispose()) (OperationKind.Invocation, Type: System.Void, IsImplicit) (Syntax: 't')
                  Instance Receiver:
                    IFlowCaptureReferenceOperation: 1 (OperationKind.FlowCaptureReference, Type: System.IDisposable, IsImplicit) (Syntax: 't')
                  Arguments(0)
            Next (Regular) Block[B6]
        Block[B6] - Block
            Predecessors: [B4] [B5]
            Statements (0)
            Next (StructuredExceptionHandling) Block[null]
    }
}
Block[B7] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()", info.GetEnumeratorMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()", op.Info.GetEnumeratorMethod.ToTestDisplayString());
            Assert.Empty(op.Info.GetEnumeratorArguments);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerator_01(bool s1IsRefStruct)
        {
            var src = @"
using System.Collections;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator
{
    bool stop;
    public object Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       39 (0x27)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  IL_000f:  br.s       IL_001d
  IL_0011:  ldloca.s   V_0
  IL_0013:  call       ""object S2.Current.get""
  IL_0018:  call       ""void System.Console.Write(object)""
  IL_001d:  ldloca.s   V_0
  IL_001f:  call       ""bool S2.MoveNext()""
  IL_0024:  brtrue.s   IL_0011
  IL_0026:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
    Block[B2] - Block
        Predecessors: [B1] [B3]
        Statements (0)
        Jump if False (Regular) to Block[B4]
            IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
              Instance Receiver:
                IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
              Arguments(0)
            Leaving: {R1}
        Next (Regular) Block[B3]
            Entering: {R2}
    .locals {R2}
    {
        Locals: [System.Object i]
        Block[B3] - Block
            Predecessors: [B2]
            Statements (2)
                ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                  Left:
                    ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                  Right:
                    IPropertyReferenceOperation: System.Object S2.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                  Expression:
                    IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                      Instance Receiver:
                        null
                      Arguments(1):
                          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                            ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            Next (Regular) Block[B2]
                Leaving: {R2}
    }
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Object S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.False(op.Info.NeedsDispose);
            Assert.False(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerator_02(bool s1IsRefStruct)
        {
            var src = @"
using System.Collections;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator
{
    bool stop;
    public object Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }

    object System.Collections.IEnumerator.Current => throw null;
    bool System.Collections.IEnumerator.MoveNext() => throw null;
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       39 (0x27)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  IL_000f:  br.s       IL_001d
  IL_0011:  ldloca.s   V_0
  IL_0013:  call       ""object S2.Current.get""
  IL_0018:  call       ""void System.Console.Write(object)""
  IL_001d:  ldloca.s   V_0
  IL_001f:  call       ""bool S2.MoveNext()""
  IL_0024:  brtrue.s   IL_0011
  IL_0026:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
    Block[B2] - Block
        Predecessors: [B1] [B3]
        Statements (0)
        Jump if False (Regular) to Block[B4]
            IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
              Instance Receiver:
                IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
              Arguments(0)
            Leaving: {R1}
        Next (Regular) Block[B3]
            Entering: {R2}
    .locals {R2}
    {
        Locals: [System.Object i]
        Block[B3] - Block
            Predecessors: [B2]
            Statements (2)
                ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                  Left:
                    ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                  Right:
                    IPropertyReferenceOperation: System.Object S2.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                  Expression:
                    IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                      Instance Receiver:
                        null
                      Arguments(1):
                          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                            ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            Next (Regular) Block[B2]
                Leaving: {R2}
    }
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Object S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.False(op.Info.NeedsDispose);
            Assert.False(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerator_03(bool s1IsRefStruct, bool currentIsPublic, bool moveNextIsPublic)
        {
            if (currentIsPublic && moveNextIsPublic)
            {
                return;
            }

            var src = @"
using System.Collections;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator
{
    bool stop;

    " + (currentIsPublic ? "public object " : "object IEnumerator.") + @"Current => 123;

    " + (moveNextIsPublic ? "public bool " : "bool System.Collections.IEnumerator.") + @"MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }

    public void Reset() { }
}

class C
{
    static void Main()
    {
#line 100
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            if (!currentIsPublic)
            {
                comp.VerifyDiagnostics(
                    // (100,27): error CS0117: 'S2' does not contain a definition for 'Current'
                    //         foreach (var i in new S1())
                    Diagnostic(ErrorCode.ERR_NoSuchMember, "new S1()").WithArguments("S2", "Current").WithLocation(100, 27),
                    // (100,27): error CS0202: foreach requires that the return type 'S2' of 'S1.GetEnumerator()' must have a suitable public 'MoveNext' method and public 'Current' property
                    //         foreach (var i in new S1())
                    Diagnostic(ErrorCode.ERR_BadGetEnumerator, "new S1()").WithArguments("S2", "S1.GetEnumerator()").WithLocation(100, 27)
                    );
            }
            else
            {
                Assert.False(moveNextIsPublic);

                comp.VerifyDiagnostics(
                    // (100,27): error CS0117: 'S2' does not contain a definition for 'MoveNext'
                    //         foreach (var i in new S1())
                    Diagnostic(ErrorCode.ERR_NoSuchMember, "new S1()").WithArguments("S2", "MoveNext").WithLocation(100, 27),
                    // (100,27): error CS0202: foreach requires that the return type 'S2' of 'S1.GetEnumerator()' must have a suitable public 'MoveNext' method and public 'Current' property
                    //         foreach (var i in new S1())
                    Diagnostic(ErrorCode.ERR_BadGetEnumerator, "new S1()").WithArguments("S2", "S1.GetEnumerator()").WithLocation(100, 27)
                    );
            }

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
Block[B1] - Block
    Predecessors: [B0]
    Statements (1)
        IInvalidOperation (OperationKind.Invalid, Type: null, IsInvalid, IsImplicit) (Syntax: 'new S1()')
          Children(1):
              IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1, IsInvalid) (Syntax: 'new S1()')
                Arguments(0)
                Initializer:
                  null
    Next (Regular) Block[B2]
Block[B2] - Block
    Predecessors: [B1] [B3]
    Statements (0)
    Jump if False (Regular) to Block[B4]
        IInvalidOperation (OperationKind.Invalid, Type: System.Boolean, IsInvalid, IsImplicit) (Syntax: 'new S1()')
          Children(1):
              IInvalidOperation (OperationKind.Invalid, Type: null, IsInvalid, IsImplicit) (Syntax: 'new S1()')
                Children(0)
    Next (Regular) Block[B3]
        Entering: {R1}
.locals {R1}
{
    Locals: [var i]
    Block[B3] - Block
        Predecessors: [B2]
        Statements (2)
            ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
              Left:
                ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: var, IsImplicit) (Syntax: 'var')
              Right:
                IInvalidOperation (OperationKind.Invalid, Type: null, IsInvalid, IsImplicit) (Syntax: 'new S1()')
                  Children(1):
                      IInvalidOperation (OperationKind.Invalid, Type: null, IsInvalid, IsImplicit) (Syntax: 'new S1()')
                        Children(0)
            IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
              Expression:
                IInvalidOperation (OperationKind.Invalid, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                  Children(2):
                      IOperation:  (OperationKind.None, Type: System.Console) (Syntax: 'System.Console')
                      ILocalReferenceOperation: i (OperationKind.LocalReference, Type: var) (Syntax: 'i')
        Next (Regular) Block[B2]
            Leaving: {R1}
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            Assert.Null(info.ElementType);
            Assert.Null(info.MoveNextMethod);
            Assert.Null(info.CurrentProperty);
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.Null(op.Info);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerator_04(bool s1IsRefStruct, bool addExplicitImplementationOfCurrentAndMoveNext)
        {
            var src = @"
using System.Collections;

" + (s1IsRefStruct ? "ref " : "") + @"struct S1
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator
{
    bool stop;
    public object Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
" +
(addExplicitImplementationOfCurrentAndMoveNext ?
@"
    object IEnumerator.Current => throw null;
    bool System.Collections.IEnumerator.MoveNext() => throw null;
"
:
"") +
@"
}

class C
{
    static void Main()
    {
        foreach (var i in new S1())
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("C.Main",
@"
{
  // Code size       39 (0x27)
  .maxstack  2
  .locals init (S2 V_0,
                S1 V_1)
  IL_0000:  ldloca.s   V_1
  IL_0002:  dup
  IL_0003:  initobj    ""S1""
  IL_0009:  call       ""S2 S1.GetEnumerator()""
  IL_000e:  stloc.0
  IL_000f:  br.s       IL_001d
  IL_0011:  ldloca.s   V_0
  IL_0013:  call       ""object S2.Current.get""
  IL_0018:  call       ""void System.Console.Write(object)""
  IL_001d:  ldloca.s   V_0
  IL_001f:  call       ""bool S2.MoveNext()""
  IL_0024:  brtrue.s   IL_0011
  IL_0026:  ret
}
");

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Main").Single();

            VerifyFlowGraph(comp, node, """
Block[B0] - Entry
    Statements (0)
    Next (Regular) Block[B1]
        Entering: {R1}
.locals {R1}
{
    CaptureIds: [0]
    Block[B1] - Block
        Predecessors: [B0]
        Statements (1)
            IFlowCaptureOperation: 0 (OperationKind.FlowCapture, Type: null, IsImplicit) (Syntax: 'new S1()')
              Value:
                IInvocationOperation ( S2 S1.GetEnumerator()) (OperationKind.Invocation, Type: S2, IsImplicit) (Syntax: 'new S1()')
                  Instance Receiver:
                    IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: S1, IsImplicit) (Syntax: 'new S1()')
                      Conversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                        (Identity)
                      Operand:
                        IObjectCreationOperation (Constructor: S1..ctor()) (OperationKind.ObjectCreation, Type: S1) (Syntax: 'new S1()')
                          Arguments(0)
                          Initializer:
                            null
                  Arguments(0)
        Next (Regular) Block[B2]
    Block[B2] - Block
        Predecessors: [B1] [B3]
        Statements (0)
        Jump if False (Regular) to Block[B4]
            IInvocationOperation ( System.Boolean S2.MoveNext()) (OperationKind.Invocation, Type: System.Boolean, IsImplicit) (Syntax: 'new S1()')
              Instance Receiver:
                IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
              Arguments(0)
            Leaving: {R1}
        Next (Regular) Block[B3]
            Entering: {R2}
    .locals {R2}
    {
        Locals: [System.Object i]
        Block[B3] - Block
            Predecessors: [B2]
            Statements (2)
                ISimpleAssignmentOperation (OperationKind.SimpleAssignment, Type: null, IsImplicit) (Syntax: 'var')
                  Left:
                    ILocalReferenceOperation: i (IsDeclaration: True) (OperationKind.LocalReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                  Right:
                    IPropertyReferenceOperation: System.Object S2.Current { get; } (OperationKind.PropertyReference, Type: System.Object, IsImplicit) (Syntax: 'var')
                      Instance Receiver:
                        IFlowCaptureReferenceOperation: 0 (OperationKind.FlowCaptureReference, Type: S2, IsImplicit) (Syntax: 'new S1()')
                IExpressionStatementOperation (OperationKind.ExpressionStatement, Type: null) (Syntax: 'System.Console.Write(i);')
                  Expression:
                    IInvocationOperation (void System.Console.Write(System.Object? value)) (OperationKind.Invocation, Type: System.Void) (Syntax: 'System.Console.Write(i)')
                      Instance Receiver:
                        null
                      Arguments(1):
                          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: value) (OperationKind.Argument, Type: null) (Syntax: 'i')
                            ILocalReferenceOperation: i (OperationKind.LocalReference, Type: System.Object) (Syntax: 'i')
                            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
                            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            Next (Regular) Block[B2]
                Leaving: {R2}
    }
}
Block[B4] - Exit
    Predecessors: [B2]
    Statements (0)
""");

            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object S2.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            Assert.Null(info.DisposeMethod);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean S2.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Object S2.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.False(op.Info.NeedsDispose);
            Assert.False(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerator_05(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System.Collections;

interface IGetEnumerator<TEnumerator> where TEnumerator : IEnumerator, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IEnumerator, System.IDisposable
{
    bool stop;
    public object Current => 123;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }

    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"IEnumerator, System.IDisposable, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"123D" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ?
                    Verification.FailsILVerify with { ILVerifyMessage = "[GetEnumerator]: Return type is ByRef, TypedReference, ArgHandle, or ArgIterator. { Offset = 0x9 }" } :
                    Verification.Skipped).VerifyDiagnostics();

            if (addStructConstraintToTEnumerator)
            {
                verifier.VerifyIL("C.Test<TEnumerable, TEnumerator>(TEnumerable)",
@"
{
  // Code size       66 (0x42)
  .maxstack  1
  .locals init (TEnumerator V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""TEnumerable""
  IL_0008:  callvirt   ""TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_0022
    IL_0010:  ldloca.s   V_0
    IL_0012:  constrained. ""TEnumerator""
    IL_0018:  callvirt   ""object System.Collections.IEnumerator.Current.get""
    IL_001d:  call       ""void System.Console.Write(object)""
    IL_0022:  ldloca.s   V_0
    IL_0024:  constrained. ""TEnumerator""
    IL_002a:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_002f:  brtrue.s   IL_0010
    IL_0031:  leave.s    IL_0041
  }
  finally
  {
    IL_0033:  ldloca.s   V_0
    IL_0035:  constrained. ""TEnumerator""
    IL_003b:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0040:  endfinally
  }
  IL_0041:  ret
}
");
            }
            else
            {
                verifier.VerifyIL("C.Test<TEnumerable, TEnumerator>(TEnumerable)",
@"
{
  // Code size       74 (0x4a)
  .maxstack  1
  .locals init (TEnumerator V_0)
  IL_0000:  ldarga.s   V_0
  IL_0002:  constrained. ""TEnumerable""
  IL_0008:  callvirt   ""TEnumerator IGetEnumerator<TEnumerator>.GetEnumerator()""
  IL_000d:  stloc.0
  .try
  {
    IL_000e:  br.s       IL_0022
    IL_0010:  ldloca.s   V_0
    IL_0012:  constrained. ""TEnumerator""
    IL_0018:  callvirt   ""object System.Collections.IEnumerator.Current.get""
    IL_001d:  call       ""void System.Console.Write(object)""
    IL_0022:  ldloca.s   V_0
    IL_0024:  constrained. ""TEnumerator""
    IL_002a:  callvirt   ""bool System.Collections.IEnumerator.MoveNext()""
    IL_002f:  brtrue.s   IL_0010
    IL_0031:  leave.s    IL_0049
  }
  finally
  {
    IL_0033:  ldloc.0
    IL_0034:  box        ""TEnumerator""
    IL_0039:  brfalse.s  IL_0048
    IL_003b:  ldloca.s   V_0
    IL_003d:  constrained. ""TEnumerator""
    IL_0043:  callvirt   ""void System.IDisposable.Dispose()""
    IL_0048:  endfinally
  }
  IL_0049:  ret
}
");
            }

            var tree = comp.SyntaxTrees.Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            AssertEx.Equal("System.Object", info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean System.Collections.IEnumerator.MoveNext()", info.MoveNextMethod.ToTestDisplayString());
            AssertEx.Equal("System.Object System.Collections.IEnumerator.Current { get; }", info.CurrentProperty.ToTestDisplayString());
            AssertEx.Equal("void System.IDisposable.Dispose()", info.DisposeMethod.ToTestDisplayString());

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.False(op.Info.IsAsynchronous);
            AssertEx.Equal("System.Object", op.Info.ElementType.ToTestDisplayString());
            AssertEx.Equal("System.Boolean System.Collections.IEnumerator.MoveNext()", op.Info.MoveNextMethod.ToTestDisplayString());
            Assert.Empty(op.Info.MoveNextArguments);
            AssertEx.Equal("System.Object System.Collections.IEnumerator.Current { get; }", op.Info.CurrentProperty.ToTestDisplayString());
            Assert.True(op.Info.CurrentArguments.IsDefault);
            Assert.True(op.Info.NeedsDispose);
            Assert.True(op.Info.KnownToImplementIDisposable);
            Assert.Null(op.Info.PatternDisposeMethod);
            Assert.True(op.Info.DisposeArguments.IsDefault);
        }

        [Theory]
        [CombinatorialData]
        public void Foreach_IEnumerator_09(bool s1IsRefStruct, bool addStructConstraintToTEnumerable, bool addStructConstraintToTEnumerator)
        {
            var src = @"
using System.Collections;

interface IMyEnumerator1<T>
{
    T Current {get;}
    bool MoveNext();
}

interface IMyEnumerator2<T>
{
    T Current {get;}
    bool MoveNext();
}

interface IGetEnumerator<TEnumerator> where TEnumerator : IMyEnumerator1<int>, IMyEnumerator2<int>, IEnumerator, allows ref struct 
{
    TEnumerator GetEnumerator();
}

" + (s1IsRefStruct ? "ref " : "") + @"struct S1 : IGetEnumerator<S2>
{
    public S2 GetEnumerator()
    {
        return new S2();
    }
}

ref struct S2 : IMyEnumerator1<int>, IMyEnumerator2<int>, IEnumerator
{
    bool stop;
    public int Current => 123;
    object System.Collections.IEnumerator.Current => Current;
    public bool MoveNext()
    {
        if (!stop)
        {
            stop = true;
            return true;
        }

        return false;
    }
    public void Reset() { }
    public void Dispose()
    {
        System.Console.Write('D');
    }
}

class C
{
    static void Main()
    {
        Test<S1, S2>(new S1());
    }

    static void Test<TEnumerable, TEnumerator>(TEnumerable t)
        where TEnumerable : " + (addStructConstraintToTEnumerable ? "struct, " : "") + @"IGetEnumerator<TEnumerator>, allows ref struct
        where TEnumerator : " + (addStructConstraintToTEnumerator ? "struct, " : "") + @"IMyEnumerator1<int>, IMyEnumerator2<int>, IEnumerator, allows ref struct 
    {
        foreach (var i in t)
        {
            System.Console.Write(i);
        }
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (62,27): error CS0202: foreach requires that the return type 'TEnumerator' of 'IGetEnumerator<TEnumerator>.GetEnumerator()' must have a suitable public 'MoveNext' method and public 'Current' property
                //         foreach (var i in t)
                Diagnostic(ErrorCode.ERR_BadGetEnumerator, "t").WithArguments("TEnumerator", "IGetEnumerator<TEnumerator>.GetEnumerator()").WithLocation(62, 27)
                );

            var tree = comp.SyntaxTrees.Single();
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText == "Test").Single();
            var model = comp.GetSemanticModel(tree);
            var foreachSyntax = tree.GetRoot().DescendantNodes().OfType<ForEachStatementSyntax>().Single();
            var info = model.GetForEachStatementInfo(foreachSyntax);

            Assert.False(info.IsAsynchronous);
            Assert.Null(info.ElementType);
            Assert.Null(info.MoveNextMethod);
            Assert.Null(info.CurrentProperty);

            var op = (Operations.ForEachLoopOperation)model.GetOperation(foreachSyntax);
            Assert.Null(op.Info);
        }

        [Fact]
        public void ConstraintsCheck_01()
        {
            var src = @"
ref struct S1
{
}

class C
{
    static void Main()
    {
        Test(new S1());
    }
    
    static void Test<T>(T x)
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            comp.VerifyDiagnostics(
                // (10,9): error CS9504: The type 'S1' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'C.Test<T>(T)'
                //         Test(new S1());
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "Test").WithArguments("C.Test<T>(T)", "T", "S1").WithLocation(10, 9)
                );
        }

        [Fact]
        public void ConstraintsCheck_02()
        {
            var src1 = @"
public class Helper
{
#line 100
    public static void Test<T>(T x) where T : allows ref struct
    {
        System.Console.Write(""Called"");
    }
}
";
            var src2 = @"
ref struct S1
{
}

class C
{
    static void Main()
    {
#line 200
        Helper.Test(new S1());
    }
}
";
            var comp = CreateCompilation([src1, src2], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"Called" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            CreateCompilation([src1, src2], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();
            CreateCompilation([src1, src2], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (100,54): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     public static void Test<T>(T x) where T : allows ref struct
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "ref struct").WithArguments("ref struct interfaces").WithLocation(100, 54)
                );

            var comp1Ref = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics).ToMetadataReference();

            comp = CreateCompilation(src2, references: [comp1Ref], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            CompileAndVerify(comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"Called" : null, verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            CreateCompilation(src2, references: [comp1Ref], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe, parseOptions: TestOptions.RegularNext).VerifyDiagnostics();
            CreateCompilation(src2, references: [comp1Ref], targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe, parseOptions: TestOptions.Regular12).VerifyDiagnostics(
                // (200,9): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //         Helper.Test(new S1());
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "Helper.Test").WithArguments("ref struct interfaces").WithLocation(200, 9)
                );

            CreateCompilation([src1, src2], targetFramework: TargetFramework.DesktopLatestExtended, options: TestOptions.ReleaseExe).VerifyDiagnostics(
                // (100,54): error CS9500: Target runtime doesn't support by-ref-like generics.
                //     public static void Test<T>(T x) where T : allows ref struct
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "ref struct").WithLocation(100, 54)
                );

            comp1Ref = CreateCompilation(src1, targetFramework: TargetFramework.DesktopLatestExtended).ToMetadataReference();

            CreateCompilation(src2, references: [comp1Ref], targetFramework: TargetFramework.DesktopLatestExtended, options: TestOptions.ReleaseExe).VerifyDiagnostics(
                // (200,9): error CS9500: Target runtime doesn't support by-ref-like generics.
                //         Helper.Test(new S1());
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "Helper.Test").WithLocation(200, 9)
                );
        }

        [Fact]
        public void ConstraintsCheck_03()
        {
            var src = @"
ref struct S1
{
}

class C
{
    static void Test1<T>(T x) where T : allows ref struct
    {
        Test2(x);
        Test2<T>(x);
    }

    static void Test2<T>(T x)
    {
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (10,9): error CS9504: The type 'T' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'C.Test2<T>(T)'
                //         Test2(x);
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "Test2").WithArguments("C.Test2<T>(T)", "T", "T").WithLocation(10, 9),
                // (11,9): error CS9504: The type 'T' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'C.Test2<T>(T)'
                //         Test2<T>(x);
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "Test2<T>").WithArguments("C.Test2<T>(T)", "T", "T").WithLocation(11, 9)
                );
        }

        [Fact]
        public void ConstraintsCheck_04()
        {
            var src = @"
ref struct S1
{
}

class C
{
    static void Main()
    {
        Test2((byte)2);
        Test3((int)3);
        Test3(new S1());
        Test4((long)4);
    }

    static void Test1<T>(T x) where T : allows ref struct
    {
        System.Console.WriteLine(""Called {0}"", typeof(T));
    }

    static void Test2<T>(T x)
    {
        Test1(x);
        Test1<T>(x);
    }

    static void Test3<T>(T x) where T : allows ref struct
    {
        Test1(x);
        Test1<T>(x);
    }

    static void Test4<T>(T x)
    {
        Test2(x);
        Test2<T>(x);
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            CompileAndVerify(comp, expectedOutput: !ExecutionConditionUtil.IsMonoOrCoreClr ? null : @"
Called System.Byte
Called System.Byte
Called System.Int32
Called System.Int32
Called S1
Called S1
Called System.Int64
Called System.Int64
Called System.Int64
Called System.Int64", verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();
        }

        [Fact]
        public void ConstraintsCheck_05()
        {
            var src = @"
ref struct S1
{
}

class C<T, S>
    where T : allows ref struct
    where S : T
{
    static void Main()
    {
        _ = typeof(C<S1, S1>);
        _ = typeof(C<int, int>);
        _ = typeof(C<object, object>);
        _ = typeof(C<object, string>);
        _ = typeof(C<T, T>);
        _ = typeof(C<S, S>);
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (12,26): error CS9504: The type 'S1' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'S' in the generic type or method 'C<T, S>'
                //         _ = typeof(C<S1, S1>);
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "S1").WithArguments("C<T, S>", "S", "S1").WithLocation(12, 26),
                // (16,25): error CS9504: The type 'T' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'S' in the generic type or method 'C<T, S>'
                //         _ = typeof(C<T, T>);
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "T").WithArguments("C<T, S>", "S", "T").WithLocation(16, 25)
                );
        }

        [Fact]
        public void IllegalBoxing_01()
        {
            var src = @"
public class Helper
{
    public static object Test1<T>(T x) where T : allows ref struct
    {
        return x;
    }

    public static object Test2<T>(T x) where T : allows ref struct
    {
        return (object)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (6,16): error CS0029: Cannot implicitly convert type 'T' to 'object'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "object").WithLocation(6, 16),
                // (11,16): error CS0030: Cannot convert type 'T' to 'object'
                //         return (object)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(object)x").WithArguments("T", "object").WithLocation(11, 16)
                );
        }

        [Fact]
        public void IllegalBoxing_02()
        {
            var src = @"
public interface I1
{
}

public class Helper
{
    public static I1 Test1<T>(T x) where T : I1, allows ref struct
    {
        return x;
    }

    public static I1 Test2<T>(T x) where T : I1, allows ref struct
    {
        return (I1)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (10,16): error CS0029: Cannot implicitly convert type 'T' to 'I1'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "I1").WithLocation(10, 16),
                // (15,16): error CS0030: Cannot convert type 'T' to 'I1'
                //         return (I1)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(I1)x").WithArguments("T", "I1").WithLocation(15, 16)
                );
        }

        [Fact]
        public void IllegalBoxing_03()
        {
            var src = @"
public interface I1
{
}

public class Helper
{
    static U Test1<T, U>(T x)
        where T : U, allows ref struct
    {
        return x;
    }
    static U Test2<T, U>(T x)
        where T : U, allows ref struct
    {
        return (U)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (11,16): error CS0029: Cannot implicitly convert type 'T' to 'U'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "U").WithLocation(11, 16),
                // (16,16): error CS0030: Cannot convert type 'T' to 'U'
                //         return (U)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(U)x").WithArguments("T", "U").WithLocation(16, 16)
                );
        }

        [Fact]
        public void IllegalBoxing_04()
        {
            var src = @"
ref struct S
{
}

public class Helper
{
    static object Test1<T>(T x)
        where T : struct, allows ref struct
    {
        return x;
    }

    static object Test2(S x)
    {
        return x;
    }

    static object Test3<T>(T x)
        where T : struct, allows ref struct
    {
        return (object)x;
    }

    static object Test4(S x)
    {
        return (object)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (11,16): error CS0029: Cannot implicitly convert type 'T' to 'object'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "object").WithLocation(11, 16),
                // (16,16): error CS0029: Cannot implicitly convert type 'S' to 'object'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("S", "object").WithLocation(16, 16),
                // (22,16): error CS0030: Cannot convert type 'T' to 'object'
                //         return (object)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(object)x").WithArguments("T", "object").WithLocation(22, 16),
                // (27,16): error CS0030: Cannot convert type 'S' to 'object'
                //         return (object)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(object)x").WithArguments("S", "object").WithLocation(27, 16)
                );
        }

        [Fact]
        public void IllegalBoxing_05()
        {
            var src = @"
interface I1 {}

ref struct S : I1
{
}

public class Helper
{
    static I1 Test1<T>(T x)
        where T : I1, allows ref struct
    {
        return x;
    }

    static I1 Test2(S x)
    {
        return x;
    }

    static I1 Test3<T>(T x)
        where T : I1, allows ref struct
    {
        return (I1)x;
    }

    static I1 Test4(S x)
    {
        return (I1)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (13,16): error CS0029: Cannot implicitly convert type 'T' to 'I1'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "I1").WithLocation(13, 16),
                // (18,16): error CS0029: Cannot implicitly convert type 'S' to 'I1'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("S", "I1").WithLocation(18, 16),
                // (24,16): error CS0030: Cannot convert type 'T' to 'I1'
                //         return (I1)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(I1)x").WithArguments("T", "I1").WithLocation(24, 16),
                // (29,16): error CS0030: Cannot convert type 'S' to 'I1'
                //         return (I1)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(I1)x").WithArguments("S", "I1").WithLocation(29, 16)
                );
        }

        [Fact]
        public void Unboxing_01()
        {
            var src = @"
public interface I1
{
}

ref struct S : I1
{
}

public class Helper
{
    static S Test1(I1 x)
    {
        return x;
    }

    static S Test2(I1 x)
    {
        return (S)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (14,16): error CS0029: Cannot implicitly convert type 'I1' to 'S'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("I1", "S").WithLocation(14, 16),
                // (19,16): error CS0030: Cannot convert type 'I1' to 'S'
                //         return (S)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(S)x").WithArguments("I1", "S").WithLocation(19, 16)
                );
        }

        [Fact]
        public void Unboxing_02()
        {
            var src = @"
public interface I1
{
}

ref struct S : I1
{
}

public class Helper
{
    static S Test1<T>(T x)
        where T : I1, allows ref struct
    {
        return x;
    }
    static S Test2<T>(T x)
        where T : I1, allows ref struct
    {
        return (S)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (15,16): error CS0029: Cannot implicitly convert type 'T' to 'S'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "S").WithLocation(15, 16),
                // (20,16): error CS0030: Cannot convert type 'T' to 'S'
                //         return (S)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(S)x").WithArguments("T", "S").WithLocation(20, 16)
                );
        }

        [Fact]
        public void Unboxing_03()
        {
            var src = @"
public interface I1
{
}

public class Helper
{
    static U Test1<T, U>(T x)
        where T : allows ref struct
        where U : T
    {
#line 100
        return x;
    }
    static U Test2<T, U>(T x)
        where T : allows ref struct
        where U : T
    {
#line 200
        return (U)x;
        // Not a legal IL according to https://github.com/dotnet/runtime/blob/main/docs/design/features/byreflike-generics.md 
        // IL_0000:  ldarg.0
        // IL_0001:  box        ""T""
        // IL_0006:  unbox.any  ""U""
        // IL_000b:  ret
    }

    static U Test3<T, U>(T x)
        where T : allows ref struct
        where U : class, T
    {
#line 300
        return x;
    }
    static U Test4<T, U>(T x)
        where T : allows ref struct
        where U : class, T
    {
#line 400
        return (U)x;
        // Not a legal IL according to https://github.com/dotnet/runtime/blob/main/docs/design/features/byreflike-generics.md 
        // IL_0000:  ldarg.0
        // IL_0001:  box        ""T""
        // IL_0006:  unbox.any  ""U""
        // IL_000b:  ret
    }

    static T Test5<T, U>(U y)
        where T : allows ref struct
        where U : T
    {
#line 500
        return y;
    }
    static T Test6<T, U>(U y)
        where T : allows ref struct
        where U : T
    {
#line 600
        return (T)y;
        // Not a legal IL according to https://github.com/dotnet/runtime/blob/main/docs/design/features/byreflike-generics.md 
        // IL_0000:  ldarg.0
        // IL_0001:  box        ""U""
        // IL_0006:  unbox.any  ""T""
        // IL_000b:  ret
    }

    static T Test7<T, U>(U y)
        where T : allows ref struct
        where U : class, T
    {
#line 700
        return y;
    }
    static T Test8<T, U>(U y)
        where T : allows ref struct
        where U : class, T
    {
#line 800
        return (T)y;
        // Not a legal IL according to https://github.com/dotnet/runtime/blob/main/docs/design/features/byreflike-generics.md 
        // IL_0000:  ldarg.0
        // IL_0001:  box        ""U""
        // IL_0006:  unbox.any  ""T""
        // IL_000b:  ret
    }
    static U Test9<T, U>(T x)
        where T : class
        where U : T, allows ref struct
    {
#line 900
        return x;
    }
    static U Test10<T, U>(T x)
        where T : class
        where U : T, allows ref struct
    {
#line 1000
        return (U)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (100,16): error CS0029: Cannot implicitly convert type 'T' to 'U'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "U").WithLocation(100, 16),
                // (200,16): error CS0030: Cannot convert type 'T' to 'U'
                //         return (U)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(U)x").WithArguments("T", "U").WithLocation(200, 16),
                // (300,16): error CS0029: Cannot implicitly convert type 'T' to 'U'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "U").WithLocation(300, 16),
                // (400,16): error CS0030: Cannot convert type 'T' to 'U'
                //         return (U)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(U)x").WithArguments("T", "U").WithLocation(400, 16),
                // (500,16): error CS0029: Cannot implicitly convert type 'U' to 'T'
                //         return y;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "y").WithArguments("U", "T").WithLocation(500, 16),
                // (600,16): error CS0030: Cannot convert type 'U' to 'T'
                //         return (T)y;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(T)y").WithArguments("U", "T").WithLocation(600, 16),
                // (700,16): error CS0029: Cannot implicitly convert type 'U' to 'T'
                //         return y;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "y").WithArguments("U", "T").WithLocation(700, 16),
                // (800,16): error CS0030: Cannot convert type 'U' to 'T'
                //         return (T)y;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(T)y").WithArguments("U", "T").WithLocation(800, 16),
                // (900,16): error CS0029: Cannot implicitly convert type 'T' to 'U'
                //         return x;
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "U").WithLocation(900, 16),
                // (1000,16): error CS0030: Cannot convert type 'T' to 'U'
                //         return (U)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(U)x").WithArguments("T", "U").WithLocation(1000, 16)
                );
        }

        [Fact]
        public void Unboxing_04()
        {
            var src = @"
ref struct S
{
}

public class Helper
{
    static T Test1<T>(object x)
        where T : allows ref struct
    {
        return (T)x;
    }

    static S Test2(object x)
    {
        return (S)x;
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (11,16): error CS0030: Cannot convert type 'object' to 'T'
                //         return (T)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(T)x").WithArguments("object", "T").WithLocation(11, 16),
                // (16,16): error CS0030: Cannot convert type 'object' to 'S'
                //         return (S)x;
                Diagnostic(ErrorCode.ERR_NoExplicitConv, "(S)x").WithArguments("object", "S").WithLocation(16, 16)
                );
        }

        [Fact]
        public void CallObjectMember()
        {
            var src = @"
public class Helper
{
    static string Test1<T>(T x)
        where T : allows ref struct
    {
        return x.ToString();
    }

    static string Test2(S y)
    {
        return y.ToString();
    }
}

ref struct S
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,16): error CS0029: Cannot implicitly convert type 'T' to 'object'
                //         return x.ToString();
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "x").WithArguments("T", "object").WithLocation(7, 16),
                // (12,16): error CS0029: Cannot implicitly convert type 'S' to 'System.ValueType'
                //         return y.ToString();
                Diagnostic(ErrorCode.ERR_NoImplicitConv, "y").WithArguments("S", "System.ValueType").WithLocation(12, 16)
                );
        }

        [Fact]
        public void AnonymousTypeMember_01()
        {
            var src = @"
public class Helper
{
    static void Test<T>(T x)
        where T : allows ref struct
    {
        var y = new { x };
    }
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,23): error CS0828: Cannot assign 'T' to anonymous type property
                //         var y = new { x };
                Diagnostic(ErrorCode.ERR_AnonymousTypePropertyAssignedBadValue, "x").WithArguments("T").WithLocation(7, 23)
                );
        }

        [ConditionalFact(typeof(NoUsedAssembliesValidation))] // PROTOTYPE(RefStructInterfaces): Follow up on used assemblies validation failure. Could be an artifact of https://github.com/dotnet/roslyn/issues/72945.
        [WorkItem("https://github.com/dotnet/roslyn/issues/72945")]
        public void AnonymousTypeMember_02()
        {
            var src = @"
public class Helper
{
    static void Test1<T>(MyEnumerable<T> outer, MyEnumerable<T> inner1, MyEnumerable<T> inner2)
        where T : I1, allows ref struct
    {
#line 100
        var q = from x in outer join y in inner1 on x.P equals y.P
                                join z in inner2 on y.P equals z.P
                                select 1;
    }

    static void Test2(MyEnumerable<S1> outer, MyEnumerable<S1> inner1, MyEnumerable<S1> inner2)
    {
#line 200
        var q = from x in outer join y in inner1 on x.P equals y.P
                                join z in inner2 on y.P equals z.P
                                select 1;
    }
}

class MyEnumerable<T>
    where T : allows ref struct
{
    public MyEnumerable<TResult> Join<TInner, TKey,TResult> (MyEnumerable<TInner> inner, MyFunc<T,TKey> outerKeySelector, MyFunc<TInner,TKey> innerKeySelector, MyFunc<T,TInner,TResult> resultSelector)
        where TInner : allows ref struct
        where TKey : allows ref struct
        where TResult : allows ref struct
        => throw null;

    public MyEnumerable<TResult> Select<TResult> (MyFunc<T,TResult> selector)
        where TResult : allows ref struct
        => throw null;
}

delegate TResult MyFunc<in T,out TResult>(T arg)
    where T : allows ref struct
    where TResult : allows ref struct;
delegate TResult MyFunc<in T1,in T2,out TResult>(T1 arg1, T2 arg2)
    where T1 : allows ref struct
    where T2 : allows ref struct
    where TResult : allows ref struct;

interface I1
{
    int P {get;}
}

ref struct S1
{
    public int P {get;set;}
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // Errors are expected for both queries, but it is a pre-existing condition - https://github.com/dotnet/roslyn/issues/72945    
                );
        }

        [Fact]
        public void Makeref()
        {
            var src = @"
public class Helper
{
    static void Test1<T>(T x)
        where T : allows ref struct
    {
        System.TypedReference tr = __makeref(x);
    }

    static void Test2(S y)
    {
        System.TypedReference tr = __makeref(y);
    }
}

ref struct S
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,36): error CS1601: Cannot make reference to variable of type 'T'
                //         System.TypedReference tr = __makeref(x);
                Diagnostic(ErrorCode.ERR_MethodArgCantBeRefAny, "__makeref(x)").WithArguments("T").WithLocation(7, 36),
                // (12,36): error CS1601: Cannot make reference to variable of type 'S'
                //         System.TypedReference tr = __makeref(y);
                Diagnostic(ErrorCode.ERR_MethodArgCantBeRefAny, "__makeref(y)").WithArguments("S").WithLocation(12, 36)
                );
        }

        [Fact]
        public void ScopedTypeParameter_01()
        {
            var src = @"
public class Helper
{
    static void Test1<T>(scoped T x)
        where T : allows ref struct
    {
    }

    static void Test2(scoped S y)
    {
    }

    static void Test3<T>(scoped T z)
    {
    }

    static void Test4<T>()
    {
        var d = void (scoped T u) => {};   
        d(default);
    }
}

ref struct S
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (13,26): error CS9048: The 'scoped' modifier can be used for refs and ref struct values only.
                //     static void Test3<T>(scoped T z)
                Diagnostic(ErrorCode.ERR_ScopedRefAndRefStructOnly, "scoped T z").WithLocation(13, 26),
                // (19,23): error CS9048: The 'scoped' modifier can be used for refs and ref struct values only.
                //         var d = void (scoped T u) => {};   
                Diagnostic(ErrorCode.ERR_ScopedRefAndRefStructOnly, "scoped T u").WithLocation(19, 23)
                );

            var src2 = @"
public class Helper
{
    public static void Test1<T>(scoped T x)
        where T : allows ref struct
    {
    }
}
";
            var comp2 = CreateCompilation(src2, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(
                comp2, symbolValidator: validate, sourceSymbolValidator: validate,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void validate(ModuleSymbol m)
            {
                var p = m.GlobalNamespace.GetMember<MethodSymbol>("Helper.Test1").Parameters[0];
                AssertEx.Equal("scoped T x", p.ToTestDisplayString());
                Assert.Equal(ScopedKind.ScopedValue, p.EffectiveScope);
            }
        }

        [Fact]
        public void ScopedTypeParameter_02()
        {
            var src = @"
#pragma warning disable CS0219 // The variable 'x' is assigned but its value is never used

public class Helper
{
    static void Test1<T>()
        where T : allows ref struct
    {
        scoped T x = default;
    }

    static void Test2()
    {
        scoped S y = default;
    }

    static void Test3<T>()
    {
        scoped T z = default;
    }
}

ref struct S
{
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (19,16): error CS9048: The 'scoped' modifier can be used for refs and ref struct values only.
                //         scoped T z = default;
                Diagnostic(ErrorCode.ERR_ScopedRefAndRefStructOnly, "T").WithLocation(19, 16)
                );

            var tree = comp.SyntaxTrees.Single();
            var model = comp.GetSemanticModel(tree);
            var declarator = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
            AssertEx.Equal("x = default", declarator.ToString());
            var local = model.GetDeclaredSymbol(declarator).GetSymbol<LocalSymbol>();
            AssertEx.Equal("T x", local.ToTestDisplayString());
            Assert.Equal(ScopedKind.ScopedValue, local.Scope);
        }

        [Fact]
        public void LiftedUnaryOperator_InvalidTypeArgument01()
        {
            var code = @"
struct S1<T>
    where T : struct, allows ref struct
{
    public static T operator+(S1<T> s1) => throw null;

    static void Test()
    {
        S1<T>? s1 = default;
        var s2 = +s1;
    }
}
";

            var comp = CreateCompilation(code, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (10,18): error CS0023: Operator '+' cannot be applied to operand of type 'S1<T>?'
                //         var s2 = +s1;
                Diagnostic(ErrorCode.ERR_BadUnaryOp, "+s1").WithArguments("+", "S1<T>?").WithLocation(10, 18)
                );
        }

        [Fact]
        public void RefField()
        {
            var src = @"
#pragma warning disable CS0169 // The field is never used

ref struct S1
{
}

ref struct S2<T>
    where T : allows ref struct
{
    ref S1 a;
    ref T b;
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (11,5): error CS9050: A ref field cannot refer to a ref struct.
                //     ref S1 a;
                Diagnostic(ErrorCode.ERR_RefFieldCannotReferToRefStruct, "ref S1").WithLocation(11, 5),
                // (12,5): error CS9050: A ref field cannot refer to a ref struct.
                //     ref T b;
                Diagnostic(ErrorCode.ERR_RefFieldCannotReferToRefStruct, "ref T").WithLocation(12, 5)
                );
        }

        [Fact]
        public void UnscopedRef_01()
        {
            var sourceA =
@"
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

public class Helper
{
    static void Test1<T>([UnscopedRef] params T x)
        where T : IEnumerable<int>, IAdd, new(), allows ref struct
    {
    }

    static void Test2([UnscopedRef] params S y)
    {
    }

    static void Test3<T>([UnscopedRef] params T z)
        where T : IEnumerable<int>, IAdd, new()
    {
    }
}

interface IAdd
{
    void Add(int x);
}

ref struct S : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => throw null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null;
    public void Add(int x){}
}
";
            var comp = CreateCompilation(sourceA, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (16,27): error CS9063: UnscopedRefAttribute cannot be applied to this parameter because it is unscoped by default.
                //     static void Test3<T>([UnscopedRef] params T z)
                Diagnostic(ErrorCode.ERR_UnscopedRefAttributeUnsupportedTarget, "UnscopedRef").WithLocation(16, 27)
                );

            var src2 = @"
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

public class Helper
{
    public static void Test1<T>([UnscopedRef] params T x)
        where T : IEnumerable<int>, IAdd, new(), allows ref struct
    {
    }
}

public interface IAdd
{
    void Add(int x);
}
";
            var comp2 = CreateCompilation(src2, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(
                comp2, symbolValidator: validate, sourceSymbolValidator: validate,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void validate(ModuleSymbol m)
            {
                var p = m.GlobalNamespace.GetMember<MethodSymbol>("Helper.Test1").Parameters[0];
                AssertEx.Equal("params T x", p.ToTestDisplayString());
                Assert.Equal(ScopedKind.None, p.EffectiveScope);
            }
        }

        [Fact]
        public void UnscopedRef_02()
        {
            var sourceA =
@"
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

public class Helper
{
    static void Test1<T>([UnscopedRef] params scoped T x)
        where T : IEnumerable<int>, IAdd, new(), allows ref struct
    {
    }

    static void Test2([UnscopedRef] params scoped S y)
    {
    }
}

interface IAdd
{
    void Add(int x);
}

ref struct S : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => throw null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null;
    public void Add(int x){}
}
";
            var comp = CreateCompilation(sourceA, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,27): error CS9066: UnscopedRefAttribute cannot be applied to parameters that have a 'scoped' modifier.
                //     static void Test1<T>([UnscopedRef] params scoped T x)
                Diagnostic(ErrorCode.ERR_UnscopedScoped, "UnscopedRef").WithLocation(7, 27),
                // (12,24): error CS9066: UnscopedRefAttribute cannot be applied to parameters that have a 'scoped' modifier.
                //     static void Test2([UnscopedRef] params scoped S y)
                Diagnostic(ErrorCode.ERR_UnscopedScoped, "UnscopedRef").WithLocation(12, 24)
                );

            // PROTOTYPE(RefStructInterfaces): Consider testing similar scenario without params
        }

        [Fact]
        public void ScopedByDefault_01()
        {
            var src =
@"
using System.Collections.Generic;

class Helper
{
    public static void Test1<T>(params T x)
        where T : IEnumerable<int>, IAdd, new(), allows ref struct
    {
    }

    public static void Test2(params S y)
    {
    }

    public static void Test3<T>(params T z)
        where T : IEnumerable<int>, IAdd, new()
    {
    }
}

interface IAdd
{
    void Add(int x);
}

ref struct S : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => throw null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null;
    public void Add(int x){}
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(
                comp, symbolValidator: validate, sourceSymbolValidator: validate,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            void validate(ModuleSymbol m)
            {
                var p = m.GlobalNamespace.GetMember<MethodSymbol>("Helper.Test1").Parameters[0];
                AssertEx.Equal("params T x", p.ToTestDisplayString());
                Assert.Equal(ScopedKind.ScopedValue, p.EffectiveScope);

                p = m.GlobalNamespace.GetMember<MethodSymbol>("Helper.Test2").Parameters[0];
                AssertEx.Equal("params S y", p.ToTestDisplayString());
                Assert.Equal(ScopedKind.ScopedValue, p.EffectiveScope);

                p = m.GlobalNamespace.GetMember<MethodSymbol>("Helper.Test3").Parameters[0];
                AssertEx.Equal("params T z", p.ToTestDisplayString());
                Assert.Equal(ScopedKind.None, p.EffectiveScope);
            }
        }

        [Fact]
        public void ScopedByDefault_02()
        {
            var src =
@"
using System.Collections.Generic;

class Helper
{
    public static void Test1<T>()
        where T : IEnumerable<int>, IAdd, new(), allows ref struct
    {
        var l1 = (params T x) => {};
    }

    public static void Test2()
    {
        var l2 = (params S y) => {};
    }

    public static void Test3<T>()
        where T : IEnumerable<int>, IAdd, new()
    {
        var l3 = (params T z) => {};
    }
}

interface IAdd
{
    void Add(int x);
}

ref struct S : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => throw null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null;
    public void Add(int x){}
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            CompileAndVerify(
                comp,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            var tree = comp.SyntaxTrees.Single();
            var model = comp.GetSemanticModel(tree);
            var lambdas = tree.GetRoot().DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().ToArray();

            var p = model.GetDeclaredSymbol(lambdas[0].ParameterList.Parameters[0]).GetSymbol<ParameterSymbol>();
            AssertEx.Equal("params T x", p.ToTestDisplayString());
            Assert.Equal(ScopedKind.ScopedValue, p.EffectiveScope);

            p = model.GetDeclaredSymbol(lambdas[1].ParameterList.Parameters[0]).GetSymbol<ParameterSymbol>();
            AssertEx.Equal("params S y", p.ToTestDisplayString());
            Assert.Equal(ScopedKind.ScopedValue, p.EffectiveScope);

            p = model.GetDeclaredSymbol(lambdas[2].ParameterList.Parameters[0]).GetSymbol<ParameterSymbol>();
            AssertEx.Equal("params T z", p.ToTestDisplayString());
            Assert.Equal(ScopedKind.None, p.EffectiveScope);
        }

        [Fact]
        public void SystemActivatorCreateInstance_01()
        {
            var sourceA =
@"
public class Helper
{
    static void Test1<T>()
        where T : new(), allows ref struct
    {
        _ = System.Activator.CreateInstance<T>();
    }

    static void Test2()
    {
        _ = System.Activator.CreateInstance<S>();
    }
}

ref struct S
{
}
";
            var comp = CreateCompilation(sourceA, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (7,30): error CS9504: The type 'T' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'Activator.CreateInstance<T>()'
                //         _ = System.Activator.CreateInstance<T>();
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "CreateInstance<T>").WithArguments("System.Activator.CreateInstance<T>()", "T", "T").WithLocation(7, 30),
                // (12,30): error CS9504: The type 'S' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'Activator.CreateInstance<T>()'
                //         _ = System.Activator.CreateInstance<S>();
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "CreateInstance<S>").WithArguments("System.Activator.CreateInstance<T>()", "T", "S").WithLocation(12, 30)
                );
        }

        [Fact]
        public void SystemActivatorCreateInstance_02()
        {
            var sourceA =
@"
public class Helper
{
    static void Test1<T>()
        where T : new(), allows ref struct
    {
        _ = new T();
    }
}

ref struct S
{
}

namespace System
{
    public class Activator
    {
         public static T CreateInstance<T>() => default;
    }
}
";
            var comp = CreateCompilation(sourceA, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyEmitDiagnostics(
                // (7,13): error CS9504: The type 'T' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'T' in the generic type or method 'Activator.CreateInstance<T>()'
                //         _ = new T();
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "new T()").WithArguments("System.Activator.CreateInstance<T>()", "T", "T").WithLocation(7, 13)
                );
        }

        [Fact]
        public void SystemActivatorCreateInstance_03()
        {
            // 'System.Activator.CreateInstance<T>' will be changed to include 'allows ref struct' constraint,
            // see https://github.com/dotnet/runtime/issues/65112.

            var src = @"
public class Helper
{
    public static T Test1<T>()
        where T : new(), allows ref struct
    {
        return new T();
    }
}

ref struct S
{
}

namespace System
{
    public class Activator
    {
         public static T CreateInstance<T>() where T : allows ref struct => default;
    }
}

class Program
{
    static void Main()
    {
        Print(Helper.Test1<S>());
        System.Console.Write(' ');
        Print(Helper.Test1<Program>());
    }

    static void Print<T>(T value) where T : allows ref struct
    {
        System.Console.Write(typeof(T));
        System.Console.Write(' ');
        System.Console.Write(value == null);
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "S False Program True" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper.Test1<T>()",
@"
{
  // Code size        6 (0x6)
  .maxstack  1
  IL_0000:  call       ""T System.Activator.CreateInstance<T>()""
  IL_0005:  ret
}
");
        }

        [Fact]
        public void Field()
        {
            var src = @"
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

class C<T>
    where T : allows ref struct
{
    public T P1;
    public S P2;
}

ref struct S1<T>
    where T : allows ref struct
{
    public static T P3;
    public static S P4;
}

ref struct S2<T>
    where T : allows ref struct
{
    public T P5;
    public S P6;
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,12): error CS8345: Field or auto-implemented property cannot be of type 'T' unless it is an instance member of a ref struct.
                //     public T P1;
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "T").WithArguments("T").WithLocation(7, 12),
                // (8,12): error CS8345: Field or auto-implemented property cannot be of type 'S' unless it is an instance member of a ref struct.
                //     public S P2;
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "S").WithArguments("S").WithLocation(8, 12),
                // (14,19): error CS8345: Field or auto-implemented property cannot be of type 'T' unless it is an instance member of a ref struct.
                //     public static T P3;
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "T").WithArguments("T").WithLocation(14, 19),
                // (15,19): error CS8345: Field or auto-implemented property cannot be of type 'S' unless it is an instance member of a ref struct.
                //     public static S P4;
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "S").WithArguments("S").WithLocation(15, 19)
                );
        }

        [Fact]
        public void AutoProperty()
        {
            var src = @"
class C<T>
    where T : allows ref struct
{
    public T P1 {get; set;}
    public S P2 {get; set;}
}

ref struct S1<T>
    where T : allows ref struct
{
    public static T P3 {get; set;}
    public static S P4 {get; set;}
}

ref struct S2<T>
    where T : allows ref struct
{
    public T P5 {get; set;}
    public S P6 {get; set;}
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (5,12): error CS8345: Field or auto-implemented property cannot be of type 'T' unless it is an instance member of a ref struct.
                //     public T P1 {get; set;}
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "T").WithArguments("T").WithLocation(5, 12),
                // (6,12): error CS8345: Field or auto-implemented property cannot be of type 'S' unless it is an instance member of a ref struct.
                //     public S P2 {get; set;}
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "S").WithArguments("S").WithLocation(6, 12),
                // (12,19): error CS8345: Field or auto-implemented property cannot be of type 'T' unless it is an instance member of a ref struct.
                //     public static T P3 {get; set;}
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "T").WithArguments("T").WithLocation(12, 19),
                // (13,19): error CS8345: Field or auto-implemented property cannot be of type 'S' unless it is an instance member of a ref struct.
                //     public static S P4 {get; set;}
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "S").WithArguments("S").WithLocation(13, 19)
                );
        }

        [Fact]
        public void InlineArrayElement_01()
        {
            var src1 = @"
[System.Runtime.CompilerServices.InlineArray(10)]
ref struct S1<T>
    where T : allows ref struct
{
    T _f;
}

[System.Runtime.CompilerServices.InlineArray(10)]
ref struct S2
{
    S _f;
}

ref struct S
{
}
";

            var comp1 = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            CompileAndVerify(
                comp1,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics(
                // (6,7): warning CS9184: 'Inline arrays' language feature is not supported for an inline array type that is not valid as a type argument, or has element type that is not valid as a type argument.
                //     T _f;
                Diagnostic(ErrorCode.WRN_InlineArrayNotSupportedByLanguage, "_f").WithLocation(6, 7),
                // (12,7): warning CS9184: 'Inline arrays' language feature is not supported for an inline array type that is not valid as a type argument, or has element type that is not valid as a type argument.
                //     S _f;
                Diagnostic(ErrorCode.WRN_InlineArrayNotSupportedByLanguage, "_f").WithLocation(12, 7)
                );

            var src2 = @"
[System.Runtime.CompilerServices.InlineArray(10)]
struct S2<T2>
    where T2 : allows ref struct
{
    T2 _f;
}
";

            var comp2 = CreateCompilation(src2, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp2.VerifyDiagnostics(
                // (6,5): error CS8345: Field or auto-implemented property cannot be of type 'T2' unless it is an instance member of a ref struct.
                //     T2 _f;
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "T2").WithArguments("T2").WithLocation(6, 5),

                // PROTOTYPE(RefStructInterfaces): The warning below is somewhat misleading. 'S2' can be used as a type argument (it is not a ref struct) and 'T2' is a type argument. 
                //                                 However, given the error above, this is probably not worth fixing. There is no way to declare a legal non-ref struct with a field
                //                                 of type 'T2'.

                // (6,8): warning CS9184: 'Inline arrays' language feature is not supported for an inline array type that is not valid as a type argument, or has element type that is not valid as a type argument.
                //     T2 _f;
                Diagnostic(ErrorCode.WRN_InlineArrayNotSupportedByLanguage, "_f").WithLocation(6, 8)
                );
        }

        [Fact]
        public void InlineArrayElement_02()
        {
            var src = @"
[System.Runtime.CompilerServices.InlineArray(2)]
ref struct S1<T>
    where T : allows ref struct
{
    T _f;
}

class C
{
    static void Main()
    {
        var x = new S1<int>();
        x[0] = 123;
    }
}

";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (6,7): warning CS9184: 'Inline arrays' language feature is not supported for an inline array type that is not valid as a type argument, or has element type that is not valid as a type argument.
                //     T _f;
                Diagnostic(ErrorCode.WRN_InlineArrayNotSupportedByLanguage, "_f").WithLocation(6, 7),
                // (14,9): error CS9504: The type 'S1<int>' may not be a ref struct or a type parameter allowing ref structs in order to use it as parameter 'TFrom' in the generic type or method 'Unsafe.As<TFrom, TTo>(ref TFrom)'
                //         x[0] = 123;
                Diagnostic(ErrorCode.ERR_NotRefStructConstraintNotSatisfied, "x[0]").WithArguments("System.Runtime.CompilerServices.Unsafe.As<TFrom, TTo>(ref TFrom)", "TFrom", "S1<int>").WithLocation(14, 9)
                );
        }

        [Fact]
        public void InlineArrayElement_03()
        {
            var src = @"
[System.Runtime.CompilerServices.InlineArray(2)]
ref struct S1<T>
    where T : allows ref struct
{
    T _f;
}

class C
{
    static void Main()
    {
        var x = new S1<int>();
        x[0] = 123;
    }
}

namespace System.Runtime.CompilerServices
{
    public class Unsafe
    {
        public static ref TTo As<TFrom, TTo>(ref TFrom input) where TFrom : allows ref struct => throw null;
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.DebugExe);
            comp.VerifyEmitDiagnostics(
                // (6,7): warning CS9184: 'Inline arrays' language feature is not supported for an inline array type that is not valid as a type argument, or has element type that is not valid as a type argument.
                //     T _f;
                Diagnostic(ErrorCode.WRN_InlineArrayNotSupportedByLanguage, "_f").WithLocation(6, 7)
                );

            // PROTOTYPE(RefStructInterfaces): Here, however, we managed to successfully compile an invalid program. 
            //                                 We should either stop relying on constraints of Unsafe.As as a way to
            //                                 detect ref struct based inline arrays, or should propagate 'allows ref struct' to 
            //                                 the helper methods that we generate in '<PrivateImplementationDetails>', which
            //                                 could be tricky because they often call other generic APIs that might disagree
            //                                 in the 'allows ref struct' constraint with Unsafe.As. 

            // Message:
            //           System.Security.VerificationException : Method<PrivateImplementationDetails>.InlineArrayFirstElementRef: type argument 'S1`1[System.Int32]' violates the constraint of type parameter 'TBuffer'.
            //   
            // Stack Trace: 
            //   C.Main()
            //   RuntimeMethodHandle.InvokeMethod(Object target, Void * *arguments, Signature sig, Boolean isConstructor)
            //   MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
            //
            //CompileAndVerify(comp, verify: Verification.Skipped, expectedOutput: "nothing");
        }

        [Fact]
        public void AsyncParameter()
        {
            var src = @"
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously.

public class Helper
{
    static async void Test1<T>(T x)
        where T : allows ref struct
    {
    }

    static async void Test2(S y)
    {
    }
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (6,34): error CS4012: Parameters or locals of type 'T' cannot be declared in async methods or async lambda expressions.
                //     static async void Test1<T>(T x)
                Diagnostic(ErrorCode.ERR_BadSpecialByRefLocal, "x").WithArguments("T").WithLocation(6, 34),
                // (11,31): error CS4012: Parameters or locals of type 'S' cannot be declared in async methods or async lambda expressions.
                //     static async void Test2(S y)
                Diagnostic(ErrorCode.ERR_BadSpecialByRefLocal, "y").WithArguments("S").WithLocation(11, 31)
                );
        }

        [Fact]
        public void MissingScopedInOverride_01()
        {
            var src = @"
abstract class Base
{
    protected abstract T Test1<T>(scoped T x)
        where T : allows ref struct;

    protected abstract S Test2(scoped S y);
}

abstract class Derived1 : Base
{
    protected abstract override T Test1<T>(T x);

    protected abstract override S Test2(S y);
}

abstract class Derived2 : Base
{
    protected abstract override T Test1<T>(scoped T x);

    protected abstract override S Test2(scoped S y);
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (12,35): error CS8987: The 'scoped' modifier of parameter 'x' doesn't match overridden or implemented member.
                //     protected abstract override T Test1<T>(T x);
                Diagnostic(ErrorCode.ERR_ScopedMismatchInParameterOfOverrideOrImplementation, "Test1").WithArguments("x").WithLocation(12, 35),
                // (14,35): error CS8987: The 'scoped' modifier of parameter 'y' doesn't match overridden or implemented member.
                //     protected abstract override S Test2(S y);
                Diagnostic(ErrorCode.ERR_ScopedMismatchInParameterOfOverrideOrImplementation, "Test2").WithArguments("y").WithLocation(14, 35)
                );

            // PROTOTYPE(RefStructInterfaces): Consider testing similar scenario with implicitly scoped parameter
        }

        [Fact]
        public void MissingScopedInOverride_02()
        {
            var src = @"
abstract class Base
{
    protected abstract void Test1<T>(scoped T x, out T z)
        where T : allows ref struct;

    protected abstract void Test2(scoped S y, out S z);
}

abstract class Derived1 : Base
{
    protected abstract override void Test1<T>(T x, out T z);

    protected abstract override void Test2(S y, out S z);
}

abstract class Derived2 : Base
{
    protected abstract override void Test1<T>(scoped T x, out T z);

    protected abstract override void Test2(scoped S y, out S z);
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (12,38): error CS8987: The 'scoped' modifier of parameter 'x' doesn't match overridden or implemented member.
                //     protected abstract override void Test1<T>(T x, out T z);
                Diagnostic(ErrorCode.ERR_ScopedMismatchInParameterOfOverrideOrImplementation, "Test1").WithArguments("x").WithLocation(12, 38),
                // (14,38): error CS8987: The 'scoped' modifier of parameter 'y' doesn't match overridden or implemented member.
                //     protected abstract override void Test2(S y, out S z);
                Diagnostic(ErrorCode.ERR_ScopedMismatchInParameterOfOverrideOrImplementation, "Test2").WithArguments("y").WithLocation(14, 38)
                );

            // PROTOTYPE(RefStructInterfaces): Consider testing ERR_ScopedMismatchInParameterOfPartial and ERR_ScopedMismatchInParameterOfTarget.
        }

        [Fact(Skip = "'byreflike' in IL is not supported yet")] // PROTOTYPE(RefStructInterfaces): Enable once we get support for 'byreflike' in IL.
        public void RefFieldTypeAllowsRefLike()
        {
            // ref struct R2<T> where T : allows ref struct
            // {
            //     public ref T F;
            // }
            var sourceA =
@"
.class public sealed R2`1<byreflike T> extends [mscorlib]System.ValueType
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.IsByRefLikeAttribute::.ctor() = (01 00 00 00)
  .field public !T& F
}
";
            var refA = CompileIL(sourceA);

            var sourceB =
@"class Program
{
    static void F<T >(ref T r1) where T : allows ref struct
    {
        var r2 = new R2();
        r2.F = ref r1;
    }
}";
            var comp = CreateCompilation(sourceB, references: new[] { refA }, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyEmitDiagnostics(
                // (6,12): error CS0570: 'R2.F' is not supported by the language
                //         r2.F = ref r1;
                Diagnostic(ErrorCode.ERR_BindToBogus, "F").WithArguments("R2.F").WithLocation(6, 12)
                );
        }

        [Fact]
        public void RestrictedTypesInRecords()
        {
            var src = @"
record C<T>(
    T P1
    ) where T : allows ref struct;
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyEmitDiagnostics(
                // (3,5): error CS8345: Field or auto-implemented property cannot be of type 'T' unless it is an instance member of a ref struct.
                //     T P1
                Diagnostic(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, "T").WithArguments("T").WithLocation(3, 5)
                );
        }

        [Theory]
        [CombinatorialData]
        public void AnonymousDelegateType_01_ActionDisallowsRefStruct(bool s2IsRefStruct)
        {
            var src = @"
class C
{
    static void Main()
    {
        Test1(new S1());
    }
    
    static void Test1<T>(T x) where T : allows ref struct
    {
        var d = Helper<T>.Test1;
        System.Console.Write(d.GetType());
        System.Console.Write("" "");
        d(x, new S2());
    }
}

class Helper<T> where T : allows ref struct
{
    public static void Test1(T x, S2 y)
    {
        System.Console.Write(""Test1"");
        System.Console.Write("" "");
        System.Console.Write(typeof(T));
    }
}

ref struct S1 {}

" + (s2IsRefStruct ? "ref " : "") + @"struct S2 {}

namespace System
{
    public delegate void Action<T1, T2>(T1 x, T2 y);
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"<>A`2[S1,S2] Test1 S1" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped,
                symbolValidator: (m) =>
                {
                    foreach (var tp in m.ContainingAssembly.GetTypeByMetadataName("<>A`2").TypeParameters)
                    {
                        Assert.True(tp.AllowsByRefLike);
                    }
                }
                ).VerifyDiagnostics();
        }

        [Theory]
        [CombinatorialData]
        public void AnonymousDelegateType_02_FuncDisallowsRefStruct(bool s2IsRefStruct)
        {
            var src = @"
class C
{
    static void Main()
    {
        Test1(new S1());
    }
    
    static void Test1<T>(T x) where T : allows ref struct
    {
        var d = Helper<T>.Test1;
        System.Console.Write(d.GetType());
        System.Console.Write("" "");
        d(x);
    }
}

class Helper<T> where T : allows ref struct
{
    public static S2 Test1(T x)
    {
        System.Console.Write(""Test1"");
        System.Console.Write("" "");
        System.Console.Write(typeof(T));
        return default;
    }
}

ref struct S1 {}

" + (s2IsRefStruct ? "ref " : "") + @"struct S2 {}

namespace System
{
    public delegate T2 Func<T1, T2>(T1 x);
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"<>F`2[S1,S2] Test1 S1" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr && !s2IsRefStruct ? Verification.Passes : Verification.Skipped,
                symbolValidator: (m) =>
                {
                    foreach (var tp in m.ContainingAssembly.GetTypeByMetadataName("<>F`2").TypeParameters)
                    {
                        Assert.True(tp.AllowsByRefLike);
                    }
                }
                ).VerifyDiagnostics();
        }

        [Theory]
        [CombinatorialData]
        public void AnonymousDelegateType_03_ActionAllowsRefStruct(bool s2IsRefStruct)
        {
            var src = @"
class C
{
    static void Main()
    {
        Test1(new S1());
    }
    
    static void Test1<T>(T x) where T : allows ref struct
    {
        var d = Helper<T>.Test1;
        System.Console.Write(d.GetType());
        System.Console.Write("" "");
        d(x, new S2());
    }
}

class Helper<T> where T : allows ref struct
{
    public static void Test1(T x, S2 y)
    {
        System.Console.Write(""Test1"");
        System.Console.Write("" "");
        System.Console.Write(typeof(T));
    }
}

ref struct S1 {}

" + (s2IsRefStruct ? "ref " : "") + @"struct S2 {}

namespace System
{
    public delegate void Action<T1, T2>(T1 x, T2 y) where T1 : allows ref struct where T2 : allows ref struct;
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"System.Action`2[S1,S2] Test1 S1" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();
        }

        [Theory]
        [CombinatorialData]
        public void AnonymousDelegateType_04_FuncAllowsRefStruct(bool s2IsRefStruct)
        {
            var src = @"
class C
{
    static void Main()
    {
        Test1(new S1());
    }
    
    static void Test1<T>(T x) where T : allows ref struct
    {
        var d = Helper<T>.Test1;
        System.Console.Write(d.GetType());
        System.Console.Write("" "");
        d(x);
    }
}

class Helper<T> where T : allows ref struct
{
    public static S2 Test1(T x)
    {
        System.Console.Write(""Test1"");
        System.Console.Write("" "");
        System.Console.Write(typeof(T));
        return default;
    }
}

ref struct S1 {}

" + (s2IsRefStruct ? "ref " : "") + @"struct S2 {}

namespace System
{
    public delegate T2 Func<T1, T2>(T1 x) where T1 : allows ref struct where T2 : allows ref struct;
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"System.Func`2[S1,S2] Test1 S1" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr && !s2IsRefStruct ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();
        }

        [Fact]
        public void AnonymousDelegateType_05_PartiallyGenericAnonymousDelegate()
        {
            var src = @"
unsafe class C
{
    static void Main()
    {
        Test1(new S1());
    }
    
    static void Test1<T>(T x) where T : allows ref struct
    {
        var d = Helper<T>.Test1;
        System.Console.Write(d.GetType());
        System.Console.Write("" "");
        d(x, null);
    }
}

unsafe class Helper<T> where T : allows ref struct
{
    public static void Test1(T x, void* y)
    {
        System.Console.Write(""Test1"");
        System.Console.Write("" "");
        System.Console.Write(typeof(T));
    }
}

ref struct S1 {}

namespace System
{
    public delegate void Action<T1, T2>(T1 x, T2 y) where T1 : allows ref struct where T2 : allows ref struct;
}
";
            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.UnsafeReleaseExe);

            CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"<>f__AnonymousDelegate0`1[S1] Test1 S1" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped,
                symbolValidator: (m) =>
                {
                    foreach (var tp in m.ContainingAssembly.GetTypeByMetadataName("<>f__AnonymousDelegate0`1").TypeParameters)
                    {
                        Assert.True(tp.AllowsByRefLike);
                    }
                }
                ).VerifyDiagnostics();
        }

        [Fact]
        public void AnonymousDelegateType_06_PartiallyGenericAnonymousDelegate_CannotAllowRefStruct()
        {
            var src = @"
class C
{
    static void Main()
    {
        Test1(new S1());
    }
    
    static void Test1<T>(T x)
    {
        var d = Helper<T>.Test1;
        System.Console.Write(d.GetType());
        System.Console.Write("" "");
        d(x, new S2());
    }
}

class Helper<T>
{
    public static void Test1(T x, S2 y)
    {
        System.Console.Write(""Test1"");
        System.Console.Write("" "");
        System.Console.Write(typeof(T));
    }
}

struct S1 {}

ref struct S2 {}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net70, options: TestOptions.ReleaseExe);

            CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"<>f__AnonymousDelegate0`1[S1] Test1 S1" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped,
                symbolValidator: (m) =>
                {
                    foreach (var tp in m.ContainingAssembly.GetTypeByMetadataName("<>f__AnonymousDelegate0`1").TypeParameters)
                    {
                        Assert.False(tp.AllowsByRefLike);
                    }
                }
                ).VerifyDiagnostics();
        }

        [Fact]
        public void AnonymousDelegateType_07_ActionDisallowsRefStruct_CannotAllowRefStruct()
        {
            var src = @"
class C
{
    static void Main()
    {
        Test1(new S1());
    }
    
    static void Test1(S1 x)
    {
        var d = Helper.Test1;
        System.Console.Write(d.GetType());
        System.Console.Write("" "");
        d(x, new S2());
    }
}

class Helper
{
    public static void Test1(S1 x, S2 y)
    {
        System.Console.Write(""Test1"");
    }
}

ref struct S1 {}

ref struct S2 {}

namespace System
{
    public delegate void Action<T1, T2>(T1 x, T2 y);
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net70, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"<>f__AnonymousDelegate0 Test1" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped
                ).VerifyDiagnostics();
        }

        [Fact]
        public void AnonymousDelegateType_08_FuncDisallowsRefStruct_CannotAllowRefStruct()
        {
            var src = @"
class C
{
    static void Main()
    {
        Test1(new S1());
    }
    
    static void Test1(S1 x)
    {
        var d = Helper.Test1;
        System.Console.Write(d.GetType());
        System.Console.Write("" "");
        d(x);
    }
}

class Helper
{
    public static S2 Test1(S1 x)
    {
        System.Console.Write(""Test1"");
        return default;
    }
}

ref struct S1 {}

ref struct S2 {}

namespace System
{
    public delegate T2 Func<T1, T2>(T1 x);
}
";
            var comp = CreateCompilation(src, targetFramework: TargetFramework.Net70, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp, expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? @"<>f__AnonymousDelegate0 Test1" : null,
                verify: Verification.Skipped
                ).VerifyDiagnostics();
        }

        [Fact]
        public void ExpressionTree_01()
        {
            var src = @"
public class Helper
{
    static void Test1<T>()
        where T : allows ref struct
    {
        System.Linq.Expressions.Expression<D1<T>> e1 = (x) => System.Console.WriteLine();
    }

    static void Test2()
    {
        System.Linq.Expressions.Expression<D2> e2 = (y) => System.Console.WriteLine();
    }

    delegate void D1<T>(T x) where T : allows ref struct;
    delegate void D2(S x);
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,57): error CS8640: Expression tree cannot contain value of ref struct or restricted type 'T'.
                //         System.Linq.Expressions.Expression<D1<T>> e1 = (x) => System.Console.WriteLine();
                Diagnostic(ErrorCode.ERR_ExpressionTreeCantContainRefStruct, "x").WithArguments("T").WithLocation(7, 57),
                // (12,54): error CS8640: Expression tree cannot contain value of ref struct or restricted type 'S'.
                //         System.Linq.Expressions.Expression<D2> e2 = (y) => System.Console.WriteLine();
                Diagnostic(ErrorCode.ERR_ExpressionTreeCantContainRefStruct, "y").WithArguments("S").WithLocation(12, 54)
                );
        }

        [Fact]
        public void ExpressionTree_02()
        {
            var src = @"
public class Helper1<T>
    where T : allows ref struct
{
    static void Test1()
    {
        System.Linq.Expressions.Expression<System.Action> e1 = () => M1();
    }

    static T M1() => throw null;
}

public class Helper2
{
    static void Test2()
    {
        System.Linq.Expressions.Expression<System.Action> e2 = () => M2();
    }

    static S M2() => throw null;
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,70): error CS8640: Expression tree cannot contain value of ref struct or restricted type 'T'.
                //         System.Linq.Expressions.Expression<System.Action> e1 = () => M1();
                Diagnostic(ErrorCode.ERR_ExpressionTreeCantContainRefStruct, "M1()").WithArguments("T").WithLocation(7, 70),
                // (17,70): error CS8640: Expression tree cannot contain value of ref struct or restricted type 'S'.
                //         System.Linq.Expressions.Expression<System.Action> e2 = () => M2();
                Diagnostic(ErrorCode.ERR_ExpressionTreeCantContainRefStruct, "M2()").WithArguments("S").WithLocation(17, 70)
                );
        }

        [Fact]
        public void InArrayType_01()
        {
            var src = @"
public class Helper1<T>
    where T : allows ref struct
{
    static T[] Test1()
        => throw null;
}

public class Helper2
{
    static S[] Test2()
        => throw null;
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (5,12): error CS0611: Array elements cannot be of type 'T'
                //     static T[] Test1()
                Diagnostic(ErrorCode.ERR_ArrayElementCantBeRefAny, "T").WithArguments("T").WithLocation(5, 12),
                // (11,12): error CS0611: Array elements cannot be of type 'S'
                //     static S[] Test2()
                Diagnostic(ErrorCode.ERR_ArrayElementCantBeRefAny, "S").WithArguments("S").WithLocation(11, 12)
                );
        }

        [Fact]
        public void InArrayType_02()
        {
            var src = @"
public class Helper1<T>
    where T : allows ref struct
{
    static void Test1()
    {
        _ = new T[] {};
    }
}

public class Helper2
{
    static void Test2()
    {
        _ = new S[] {};
    }
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,17): error CS0611: Array elements cannot be of type 'T'
                //         _ = new T[] {};
                Diagnostic(ErrorCode.ERR_ArrayElementCantBeRefAny, "T").WithArguments("T").WithLocation(7, 17),
                // (15,17): error CS0611: Array elements cannot be of type 'S'
                //         _ = new S[] {};
                Diagnostic(ErrorCode.ERR_ArrayElementCantBeRefAny, "S").WithArguments("S").WithLocation(15, 17)
                );
        }

        [Fact]
        public void InArrayType_03()
        {
            var src = @"
public class Helper1<T>
    where T : allows ref struct
{
    static void Test1(T x)
    {
        _ = new [] {x};
    }
}

public class Helper2
{
    static void Test2(S y)
    {
        _ = new [] {y};
    }
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0611: Array elements cannot be of type 'T'
                //         _ = new [] {x};
                Diagnostic(ErrorCode.ERR_ArrayElementCantBeRefAny, "new [] {x}").WithArguments("T").WithLocation(7, 13),
                // (15,13): error CS0611: Array elements cannot be of type 'S'
                //         _ = new [] {y};
                Diagnostic(ErrorCode.ERR_ArrayElementCantBeRefAny, "new [] {y}").WithArguments("S").WithLocation(15, 13)
                );
        }

        [Fact]
        public void DelegateReceiver_01()
        {
            var src = @"
class Helper1<T>
    where T : I1, allows ref struct
{
    static System.Action Test1(T x)
        => x.M;
}

class Helper2
{
    static System.Action Test2(S y)
        => y.M;
}

ref struct S
{
    public void M(){}
}

interface I1
{
    void M();
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (6,14): error CS0123: No overload for 'M' matches delegate 'Action'
                //         => x.M;
                Diagnostic(ErrorCode.ERR_MethDelegateMismatch, "M").WithArguments("M", "System.Action").WithLocation(6, 14),
                // (12,14): error CS0123: No overload for 'M' matches delegate 'Action'
                //         => y.M;
                Diagnostic(ErrorCode.ERR_MethDelegateMismatch, "M").WithArguments("M", "System.Action").WithLocation(12, 14)
                );
        }

        [Fact]
        public void DelegateReceiver_02()
        {
            var src = @"
class Helper1<T>
    where T : I1, allows ref struct
{
    static void Test1(T x)
    {
        var d1 = x.M;
    }
}

class Helper2
{
    static void Test2(S y)
    {
        var d2 = y.M;
    }
}

ref struct S
{
    public void M(){}
}

interface I1
{
    void M();
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,20): error CS0123: No overload for 'M' matches delegate 'Action'
                //         var d1 = x.M;
                Diagnostic(ErrorCode.ERR_MethDelegateMismatch, "M").WithArguments("M", "System.Action").WithLocation(7, 20),
                // (15,20): error CS0123: No overload for 'M' matches delegate 'Action'
                //         var d2 = y.M;
                Diagnostic(ErrorCode.ERR_MethDelegateMismatch, "M").WithArguments("M", "System.Action").WithLocation(15, 20)
                );
        }

        [Fact]
        public void DelegateReceiver_03()
        {
            var src = @"
class Helper1<T>
    where T : I1, allows ref struct
{
    static void Test1(T x)
    {
        var d1 = x.M;
    }
}

class Helper2
{
    static void Test2(S y)
    {
        var d2 = y.M;
    }
}

ref struct S
{
    public void M(ref int x){}
}

interface I1
{
    void M(ref int x);
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,20): error CS0123: No overload for 'M' matches delegate '<anonymous delegate>'
                //         var d1 = x.M;
                Diagnostic(ErrorCode.ERR_MethDelegateMismatch, "M").WithArguments("M", "<anonymous delegate>").WithLocation(7, 20),
                // (15,20): error CS0123: No overload for 'M' matches delegate '<anonymous delegate>'
                //         var d2 = y.M;
                Diagnostic(ErrorCode.ERR_MethDelegateMismatch, "M").WithArguments("M", "<anonymous delegate>").WithLocation(15, 20)
                );
        }

        [Fact]
        public void ConditionalAccess_01()
        {
            var src = @"
class Helper1<T>
    where T : struct, allows ref struct
{
    static void Test1(Helper1<T> h1)
    {
        var x = h1?.M1();
    }

    T M1() => default;
}

class Helper2
{
    static void Test2(Helper2 h2)
    {
        var x = h2?.M2();
    }

    S M2() => default;
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,20): error CS8978: 'T' cannot be made nullable.
                //         var x = h1?.M1();
                Diagnostic(ErrorCode.ERR_CannotBeMadeNullable, ".M1()").WithArguments("T").WithLocation(7, 20),
                // (17,20): error CS8978: 'S' cannot be made nullable.
                //         var x = h2?.M2();
                Diagnostic(ErrorCode.ERR_CannotBeMadeNullable, ".M2()").WithArguments("S").WithLocation(17, 20)
                );
        }

        [Fact]
        public void ConditionalAccess_02()
        {
            var src = @"
class Helper1<T>
    where T : struct, allows ref struct
{
    public static void Test1(Helper1<T> h1)
    {
        h1?.M1();
    }

    T M1()
    {
        System.Console.Write(""M1"");
        return default;
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        Helper1<S>.Test1(null);
        System.Console.Write(""_"");
        Helper1<S>.Test1(new Helper1<S>());
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "_M1" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper1<T>.Test1(Helper1<T>)",
@"
{
  // Code size       11 (0xb)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  brfalse.s  IL_000a
  IL_0003:  ldarg.0
  IL_0004:  call       ""T Helper1<T>.M1()""
  IL_0009:  pop
  IL_000a:  ret
}
");
        }

        [Fact]
        public void ConditionalAccess_03()
        {
            var src = @"
class Helper1<T>
    where T : I1, allows ref struct
{
    public static void Test1(T h1)
    {
        System.Console.Write(h1?.M());
    }
}

interface I1
{
    int M();
}

ref struct S : I1
{
    public int M() => 123;
}

class Program
{
    static void Main()
    {
        Helper1<S>.Test1(new S());
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "123" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper1<T>.Test1(T)",
@"
{
  // Code size       48 (0x30)
  .maxstack  1
  .locals init (int? V_0)
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_0013
  IL_0008:  ldloca.s   V_0
  IL_000a:  initobj    ""int?""
  IL_0010:  ldloc.0
  IL_0011:  br.s       IL_0025
  IL_0013:  ldarga.s   V_0
  IL_0015:  constrained. ""T""
  IL_001b:  callvirt   ""int I1.M()""
  IL_0020:  newobj     ""int?..ctor(int)""
  IL_0025:  box        ""int?""
  IL_002a:  call       ""void System.Console.Write(object)""
  IL_002f:  ret
}
");
        }

        [Fact]
        public void ConditionalAccess_04()
        {
            var src = @"
class Helper1<T>
    where T : I1, allows ref struct
{
    public static void Test1(T h1)
    {
        System.Console.Write(h1?.M());
    }
}

interface I1
{
    dynamic M();
}

ref struct S : I1
{
    public dynamic M() => 123;
}

class Program
{
    static void Main()
    {
        Helper1<S>.Test1(new S());
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "123" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper1<T>.Test1(T)",
@"
{
  // Code size      129 (0x81)
  .maxstack  9
  .locals init (T V_0,
                T V_1)
  IL_0000:  ldsfld     ""System.Runtime.CompilerServices.CallSite<System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic>> Helper1<T>.<>o__0.<>p__0""
  IL_0005:  brtrue.s   IL_0046
  IL_0007:  ldc.i4     0x100
  IL_000c:  ldstr      ""Write""
  IL_0011:  ldnull
  IL_0012:  ldtoken    ""Helper1<T>""
  IL_0017:  call       ""System.Type System.Type.GetTypeFromHandle(System.RuntimeTypeHandle)""
  IL_001c:  ldc.i4.2
  IL_001d:  newarr     ""Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo""
  IL_0022:  dup
  IL_0023:  ldc.i4.0
  IL_0024:  ldc.i4.s   33
  IL_0026:  ldnull
  IL_0027:  call       ""Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, string)""
  IL_002c:  stelem.ref
  IL_002d:  dup
  IL_002e:  ldc.i4.1
  IL_002f:  ldc.i4.0
  IL_0030:  ldnull
  IL_0031:  call       ""Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags, string)""
  IL_0036:  stelem.ref
  IL_0037:  call       ""System.Runtime.CompilerServices.CallSiteBinder Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags, string, System.Collections.Generic.IEnumerable<System.Type>, System.Type, System.Collections.Generic.IEnumerable<Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo>)""
  IL_003c:  call       ""System.Runtime.CompilerServices.CallSite<System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic>> System.Runtime.CompilerServices.CallSite<System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic>>.Create(System.Runtime.CompilerServices.CallSiteBinder)""
  IL_0041:  stsfld     ""System.Runtime.CompilerServices.CallSite<System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic>> Helper1<T>.<>o__0.<>p__0""
  IL_0046:  ldsfld     ""System.Runtime.CompilerServices.CallSite<System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic>> Helper1<T>.<>o__0.<>p__0""
  IL_004b:  ldfld      ""System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic> System.Runtime.CompilerServices.CallSite<System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic>>.Target""
  IL_0050:  ldsfld     ""System.Runtime.CompilerServices.CallSite<System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic>> Helper1<T>.<>o__0.<>p__0""
  IL_0055:  ldtoken    ""System.Console""
  IL_005a:  call       ""System.Type System.Type.GetTypeFromHandle(System.RuntimeTypeHandle)""
  IL_005f:  ldarg.0
  IL_0060:  stloc.0
  IL_0061:  ldloc.0
  IL_0062:  box        ""T""
  IL_0067:  brtrue.s   IL_006c
  IL_0069:  ldnull
  IL_006a:  br.s       IL_007b
  IL_006c:  ldloc.0
  IL_006d:  stloc.1
  IL_006e:  ldloca.s   V_1
  IL_0070:  constrained. ""T""
  IL_0076:  callvirt   ""dynamic I1.M()""
  IL_007b:  callvirt   ""void System.Action<System.Runtime.CompilerServices.CallSite, System.Type, dynamic>.Invoke(System.Runtime.CompilerServices.CallSite, System.Type, dynamic)""
  IL_0080:  ret
}
");
        }

        [Fact]
        public void DynamicAccess_01()
        {
            var src = @"
class Helper1<T>
    where T : struct, allows ref struct
{
    static void Test1(dynamic h1)
    {
        h1.M1<T>();
    }
}

class Helper2
{
    static void Test2(dynamic h2)
    {
        h2.M2<S>();
    }
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,15): error CS0306: The type 'T' may not be used as a type argument
                //         h1.M1<T>();
                Diagnostic(ErrorCode.ERR_BadTypeArgument, "T").WithArguments("T").WithLocation(7, 15),
                // (15,15): error CS0306: The type 'S' may not be used as a type argument
                //         h2.M2<S>();
                Diagnostic(ErrorCode.ERR_BadTypeArgument, "S").WithArguments("S").WithLocation(15, 15)
                );
        }

        [Fact]
        public void DynamicAccess_02()
        {
            var src = @"
class Helper1<T>
    where T : struct, allows ref struct
{
    static void Test1(dynamic h1, T x)
    {
        h1.M1(x);
    }
}

class Helper2
{
    static void Test2(dynamic h2, S y)
    {
        h2.M2(y);
    }
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,15): error CS1978: Cannot use an expression of type 'T' as an argument to a dynamically dispatched operation.
                //         h1.M1(x);
                Diagnostic(ErrorCode.ERR_BadDynamicMethodArg, "x").WithArguments("T").WithLocation(7, 15),
                // (15,15): error CS1978: Cannot use an expression of type 'S' as an argument to a dynamically dispatched operation.
                //         h2.M2(y);
                Diagnostic(ErrorCode.ERR_BadDynamicMethodArg, "y").WithArguments("S").WithLocation(15, 15)
                );
        }

        [Fact]
        public void DynamicAccess_03()
        {
            var src = @"
class Helper1<T>
    where T : I1, allows ref struct
{
    static void Test1(dynamic h1, T x)
    {
        x.M(h1);
    }
}

class Helper2
{
    static void Test2(dynamic h2, S y)
    {
        y.M(h2);
    }
}

interface I1
{
    void M(int x);
    void M(long x);
}

ref struct S : I1
{
    public void M(int x) => throw null;
    public void M(long x) => throw null;
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,9): error CS9230: Cannot perform a dynamic invocation on an expression with type 'T'.
                //         x.M(h1);
                Diagnostic(ErrorCode.ERR_CannotDynamicInvokeOnExpression, "x").WithArguments("T").WithLocation(7, 9),
                // (15,9): error CS9230: Cannot perform a dynamic invocation on an expression with type 'S'.
                //         y.M(h2);
                Diagnostic(ErrorCode.ERR_CannotDynamicInvokeOnExpression, "y").WithArguments("S").WithLocation(15, 9)
                );
        }

        [Fact]
        public void IsOperator_01()
        {
            var src = @"
class Helper1<T>
    where T : allows ref struct
{
    public static void Test1(I1 h1)
    {
        if (h1 is T)
        {
            System.Console.Write(1);
        }
        else
        {
            System.Console.Write(2);
        }
    }
}

class Helper2
{
    public static void Test2(I1 h2)
    {
        if (h2 is S)
        {
            System.Console.Write(3);
        }
        else
        {
            System.Console.Write(4);
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}

struct S1 : I1 {}

class Program : I1
{
    static void Main()
    {
        Helper1<S>.Test1(new S1());
        Helper1<Program>.Test1(new Program());
        Helper2.Test2(new S1());
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "214" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics(
                // (22,13): warning CS0184: The given expression is never of the provided ('S') type
                //         if (h2 is S)
                Diagnostic(ErrorCode.WRN_IsAlwaysFalse, "h2 is S").WithArguments("S").WithLocation(22, 13)
                );

            // According to
            // https://github.com/dotnet/runtime/pull/101458#issuecomment-2074169181 and
            // https://github.com/dotnet/runtime/pull/101458#issuecomment-2075815858
            // the following is a valid IL
            verifier.VerifyIL("Helper1<T>.Test1(I1)",
@"
{
  // Code size       22 (0x16)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  isinst     ""T""
  IL_0006:  brfalse.s  IL_000f
  IL_0008:  ldc.i4.1
  IL_0009:  call       ""void System.Console.Write(int)""
  IL_000e:  ret
  IL_000f:  ldc.i4.2
  IL_0010:  call       ""void System.Console.Write(int)""
  IL_0015:  ret
}
");

            verifier.VerifyIL("Helper2.Test2(I1)",
@"
{
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldc.i4.4
  IL_0001:  call       ""void System.Console.Write(int)""
  IL_0006:  ret
}
");
        }

        [Fact]
        public void IsOperator_02()
        {
            var src1 = @"
class Helper1<T>
    where T : allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is I1)
        {
        }
    }
}

interface I1
{
}
";
            var comp = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0019: Operator 'is' cannot be applied to operands of type 'T' and 'I1'
                //         if (h1 is I1)
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h1 is I1").WithArguments("is", "T", "I1").WithLocation(7, 13)
                );

            var src2 = @"
class Helper2
{
    public static void Test2(S h2)
    {
        if (h2 is I1)
        {
            System.Console.Write(3);
        }
        else
        {
            System.Console.Write(4);
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}

class Program
{
    static void Main()
    {
        Helper2.Test2(new S());
    }
}
";

            comp = CreateCompilation(src2, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "4" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics(
                // (6,13): warning CS0184: The given expression is never of the provided ('I1') type
                //         if (h2 is I1)
                Diagnostic(ErrorCode.WRN_IsAlwaysFalse, "h2 is I1").WithArguments("I1").WithLocation(6, 13)
                );

            verifier.VerifyIL("Helper2.Test2(S)",
@"
{
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldc.i4.4
  IL_0001:  call       ""void System.Console.Write(int)""
  IL_0006:  ret
}
");
        }

        [Fact]
        public void IsOperator_03()
        {
            var src = @"
class Helper1<T>
    where T : allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is T)
        {
            System.Console.Write(1);
        }
        else
        {
            System.Console.Write(2);
        }
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        Helper1<S>.Test1(new S());
        Helper1<Program>.Test1(new Program());
        Helper1<Program>.Test1(null);
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "112" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics();

            verifier.VerifyIL("Helper1<T>.Test1(T)",
@"
{
  // Code size       22 (0x16)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000f
  IL_0008:  ldc.i4.1
  IL_0009:  call       ""void System.Console.Write(int)""
  IL_000e:  ret
  IL_000f:  ldc.i4.2
  IL_0010:  call       ""void System.Console.Write(int)""
  IL_0015:  ret
}
");
        }

        [Fact]
        public void IsOperator_04()
        {
            var src = @"
class Helper1<T>
    where T : struct, allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is T)
        {
            System.Console.Write(1);
        }
        else
        {
            System.Console.Write(2);
        }
    }
}

class Helper2
{
    public static void Test2(S h2)
    {
        if (h2 is S)
        {
            System.Console.Write(3);
        }
        else
        {
            System.Console.Write(4);
        }
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        Helper1<S>.Test1(new S());
        Helper1<int>.Test1(1);
        Helper2.Test2(new S());
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "113" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics(
                // (7,13): warning CS0183: The given expression is always of the provided ('T') type
                //         if (h1 is T)
                Diagnostic(ErrorCode.WRN_IsAlwaysTrue, "h1 is T").WithArguments("T").WithLocation(7, 13),
                // (22,13): warning CS0183: The given expression is always of the provided ('S') type
                //         if (h2 is S)
                Diagnostic(ErrorCode.WRN_IsAlwaysTrue, "h2 is S").WithArguments("S").WithLocation(22, 13)
                );

            verifier.VerifyIL("Helper1<T>.Test1(T)",
@"
{ 
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldc.i4.1
  IL_0001:  call       ""void System.Console.Write(int)""
  IL_0006:  ret
}
");

            verifier.VerifyIL("Helper2.Test2(S)",
@"
{
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldc.i4.3
  IL_0001:  call       ""void System.Console.Write(int)""
  IL_0006:  ret
}
");
        }

        [Fact]
        public void IsOperator_05()
        {
            var src1 = @"
class Helper1<T, U>
    where T : allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is U)
        {
        }
    }
}
";

            var comp = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0019: Operator 'is' cannot be applied to operands of type 'T' and 'U'
                //         if (h1 is U)
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h1 is U").WithArguments("is", "T", "U").WithLocation(7, 13)
                );

            var src2 = @"
class Helper2<U>
{
    public static void Test2(S h2)
    {
        if (h2 is U)
        {
            System.Console.Write(3);
        }
        else
        {
            System.Console.Write(4);
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}

class Program
{
    static void Main()
    {
        Helper2<I1>.Test2(new S());
    }
}
";
            comp = CreateCompilation(src2, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "4" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics(
                // (6,13): warning CS0184: The given expression is never of the provided ('U') type
                //         if (h2 is U)
                Diagnostic(ErrorCode.WRN_IsAlwaysFalse, "h2 is U").WithArguments("U").WithLocation(6, 13)
                );

            verifier.VerifyIL("Helper2<U>.Test2(S)",
@"
{
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldc.i4.4
  IL_0001:  call       ""void System.Console.Write(int)""
  IL_0006:  ret
}
");
        }

        [Fact]
        public void IsOperator_06()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
    where U : allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is U)
        {
        }
    }
}

class Helper2<U>
    where U : allows ref struct
{
    public static void Test2(S h2)
    {
        if (h2 is U)
        {
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (8,13): error CS0019: Operator 'is' cannot be applied to operands of type 'T' and 'U'
                //         if (h1 is U)
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h1 is U").WithArguments("is", "T", "U").WithLocation(8, 13),
                // (19,13): error CS0019: Operator 'is' cannot be applied to operands of type 'S' and 'U'
                //         if (h2 is U)
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h2 is U").WithArguments("is", "S", "U").WithLocation(19, 13)
                );
        }

        [Fact]
        public void IsOperator_07()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
{
    static void Test1(U h1)
    {
        if (h1 is T)
        {
        }
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0019: Operator 'is' cannot be applied to operands of type 'U' and 'T'
                //         if (h1 is T)
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h1 is T").WithArguments("is", "U", "T").WithLocation(7, 13)
                );

            var src2 = @"
class Helper2<U>
{
    public static void Test2(U h2)
    {
        if (h2 is S)
        {
            System.Console.Write(3);
        }
        else
        {
            System.Console.Write(4);
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}

class Program : I1
{
    static void Main()
    {
        Helper2<I1>.Test2(new Program());
    }
}
";
            comp = CreateCompilation(src2, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "4" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics(
                // (6,13): warning CS0184: The given expression is never of the provided ('S') type
                //         if (h2 is S)
                Diagnostic(ErrorCode.WRN_IsAlwaysFalse, "h2 is S").WithArguments("S").WithLocation(6, 13)
                );

            verifier.VerifyIL("Helper2<U>.Test2(U)",
@"
{
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldc.i4.4
  IL_0001:  call       ""void System.Console.Write(int)""
  IL_0006:  ret
}
");
        }

        [Fact]
        public void IsOperator_08()
        {
            var src = @"
class Helper2<U>
    where U : allows ref struct
{
    public static void Test2(U h2)
    {
        if (h2 is S)
        {
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0019: Operator 'is' cannot be applied to operands of type 'U' and 'S'
                //         if (h2 is S)
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h2 is S").WithArguments("is", "U", "S").WithLocation(7, 13)
                );
        }

        [Fact]
        public void IsOperator_09()
        {
            var src = @"
class Helper2
{
    public static void Test2(S1 h2)
    {
        if (h2 is S2)
        {
            System.Console.Write(3);
        }
        else
        {
            System.Console.Write(4);
        }
    }
}

ref struct S1
{
}

ref struct S2
{
}

class Program
{
    static void Main()
    {
        Helper2.Test2(new S1());
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "4" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics(
                // (6,13): warning CS0184: The given expression is never of the provided ('S2') type
                //         if (h2 is S2)
                Diagnostic(ErrorCode.WRN_IsAlwaysFalse, "h2 is S2").WithArguments("S2").WithLocation(6, 13)
                );

            verifier.VerifyIL("Helper2.Test2(S1)",
@"
{
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldc.i4.4
  IL_0001:  call       ""void System.Console.Write(int)""
  IL_0006:  ret
}
");
        }

        [Fact]
        public void IsOperator_10()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
    where U : T, allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is U)
        {
        }
    }
    public static void Test2(U h2)
    {
        if (h2 is T)
        {
        }
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (8,13): error CS0019: Operator 'is' cannot be applied to operands of type 'T' and 'U'
                //         if (h1 is U)
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h1 is U").WithArguments("is", "T", "U").WithLocation(8, 13),
                // (14,13): error CS0019: Operator 'is' cannot be applied to operands of type 'U' and 'T'
                //         if (h2 is T)
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h2 is T").WithArguments("is", "U", "T").WithLocation(14, 13)
                );
        }

        [Fact]
        public void IsPattern_01()
        {
            var src1 = @"
class Helper1<T>
    where T : I1, allows ref struct
{
    public static void Test1(I1 h1)
    {
        if (h1 is T t)
        {
            t.M();
        }
        else
        {
            System.Console.Write(2);
        }
    }
}
ref struct S : I1
{
    public void M()
    {
        System.Console.Write(3);
    }
}

interface I1
{
    void M();
}

class Program : I1
{
    static void Main()
    {
        Helper1<S>.Test1(new Program());
        Helper1<Program>.Test1(new Program());
    }

    public void M()
    {
        System.Console.Write(1);
    }
}
";

            var comp1 = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp1,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "21" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped);

            // According to
            // https://github.com/dotnet/runtime/pull/101458#issuecomment-2074169181 and
            // https://github.com/dotnet/runtime/pull/101458#issuecomment-2075815858
            // the following is a valid IL
            verifier.VerifyIL("Helper1<T>.Test1(I1)",
@"
{
  // Code size       41 (0x29)
  .maxstack  1
  .locals init (T V_0) //t
  IL_0000:  ldarg.0
  IL_0001:  isinst     ""T""
  IL_0006:  brfalse.s  IL_0022
  IL_0008:  ldarg.0
  IL_0009:  isinst     ""T""
  IL_000e:  unbox.any  ""T""
  IL_0013:  stloc.0
  IL_0014:  ldloca.s   V_0
  IL_0016:  constrained. ""T""
  IL_001c:  callvirt   ""void I1.M()""
  IL_0021:  ret
  IL_0022:  ldc.i4.2
  IL_0023:  call       ""void System.Console.Write(int)""
  IL_0028:  ret
}
");

            var src2 = @"
class Helper2
{
    static void Test2(I1 h2)
    {
        if (h2 is S s)
        {
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp2 = CreateCompilation(src2, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp2.VerifyDiagnostics(
                // (6,19): error CS8121: An expression of type 'I1' cannot be handled by a pattern of type 'S'.
                //         if (h2 is S s)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "S").WithArguments("I1", "S").WithLocation(6, 19)
                );
        }

        [Fact]
        public void IsPattern_02()
        {
            var src = @"
class Helper1<T>
    where T : allows ref struct
{
    static void Test1(T h1)
    {
        if (h1 is I1 t)
        {
        }
    }
}

class Helper2
{
    static void Test2(S h2)
    {
        if (h2 is I1 s)
        {
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,19): error CS8121: An expression of type 'T' cannot be handled by a pattern of type 'I1'.
                //         if (h1 is I1 t)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "I1").WithArguments("T", "I1").WithLocation(7, 19),
                // (17,19): error CS8121: An expression of type 'S' cannot be handled by a pattern of type 'I1'.
                //         if (h2 is I1 s)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "I1").WithArguments("S", "I1").WithLocation(17, 19)
                );
        }

        [Fact]
        public void IsPattern_03()
        {
            var src = @"
class Helper1<T>
    where T : allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is T t)
        {
            Program.M(t);
            System.Console.Write(1);
        }
        else
        {
            System.Console.Write(2);
        }
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        Helper1<S>.Test1(new S());
        Helper1<Program>.Test1(new Program());
        Helper1<Program>.Test1(null);
    }

    public static void M<T>(T t) where T : allows ref struct {}
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "112" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics();

            verifier.VerifyIL("Helper1<T>.Test1(T)",
@"
{
  // Code size       30 (0x1e)
  .maxstack  1
  .locals init (T V_0) //t
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_0017
  IL_0008:  ldarg.0
  IL_0009:  stloc.0
  IL_000a:  ldloc.0
  IL_000b:  call       ""void Program.M<T>(T)""
  IL_0010:  ldc.i4.1
  IL_0011:  call       ""void System.Console.Write(int)""
  IL_0016:  ret
  IL_0017:  ldc.i4.2
  IL_0018:  call       ""void System.Console.Write(int)""
  IL_001d:  ret
}
");
        }

        [Fact]
        public void IsPattern_04()
        {
            var src = @"
class Helper1<T>
    where T : struct, allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is T t)
        {
            Program.M(t);
            System.Console.Write(1);
        }
        else
        {
            System.Console.Write(2);
        }
    }
}

class Helper2
{
    public static void Test2(S h2)
    {
        if (h2 is S s)
        {
            Program.M(s);
            System.Console.Write(3);
        }
        else
        {
            System.Console.Write(4);
        }
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        Helper1<S>.Test1(new S());
        Helper1<int>.Test1(1);
        Helper2.Test2(new S());
    }

    public static void M<T>(T t) where T : allows ref struct {}
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);
            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "113" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).
            VerifyDiagnostics();

            verifier.VerifyIL("Helper1<T>.Test1(T)",
@"
{
  // Code size       15 (0xf)
  .maxstack  1
  .locals init (T V_0) //t
  IL_0000:  ldarg.0
  IL_0001:  stloc.0
  IL_0002:  ldloc.0
  IL_0003:  call       ""void Program.M<T>(T)""
  IL_0008:  ldc.i4.1
  IL_0009:  call       ""void System.Console.Write(int)""
  IL_000e:  ret
}
");

            verifier.VerifyIL("Helper2.Test2(S)",
@"
{
  // Code size       15 (0xf)
  .maxstack  1
  .locals init (S V_0) //s
  IL_0000:  ldarg.0
  IL_0001:  stloc.0
  IL_0002:  ldloc.0
  IL_0003:  call       ""void Program.M<S>(S)""
  IL_0008:  ldc.i4.3
  IL_0009:  call       ""void System.Console.Write(int)""
  IL_000e:  ret
}
");
        }

        [Fact]
        public void IsPattern_05()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is U u)
        {
        }
    }
}

class Helper2<U>
{
    public static void Test2(S h2)
    {
        if (h2 is U u)
        {
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,19): error CS8121: An expression of type 'T' cannot be handled by a pattern of type 'U'.
                //         if (h1 is U u)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "U").WithArguments("T", "U").WithLocation(7, 19),
                // (17,19): error CS8121: An expression of type 'S' cannot be handled by a pattern of type 'U'.
                //         if (h2 is U u)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "U").WithArguments("S", "U").WithLocation(17, 19)
                );
        }

        [Fact]
        public void IsPattern_06()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
    where U : allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is U u)
        {
        }
    }
}

class Helper2<U>
    where U : allows ref struct
{
    public static void Test2(S h2)
    {
        if (h2 is U u)
        {
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (8,19): error CS8121: An expression of type 'T' cannot be handled by a pattern of type 'U'.
                //         if (h1 is U u)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "U").WithArguments("T", "U").WithLocation(8, 19),
                // (19,19): error CS8121: An expression of type 'S' cannot be handled by a pattern of type 'U'.
                //         if (h2 is U u)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "U").WithArguments("S", "U").WithLocation(19, 19)
                );
        }

        [Fact]
        public void IsPattern_07()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
{
    static void Test1(U h1)
    {
        if (h1 is T t)
        {
        }
    }
}

class Helper2<U>
{
    public static void Test2(U h2)
    {
        if (h2 is S s)
        {
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,19): error CS8121: An expression of type 'U' cannot be handled by a pattern of type 'T'.
                //         if (h1 is T t)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "T").WithArguments("U", "T").WithLocation(7, 19),
                // (17,19): error CS8121: An expression of type 'U' cannot be handled by a pattern of type 'S'.
                //         if (h2 is S s)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "S").WithArguments("U", "S").WithLocation(17, 19)
                );
        }

        [Fact]
        public void IsPattern_08()
        {
            var src = @"
class Helper2<U>
    where U : allows ref struct
{
    public static void Test2(U h2)
    {
        if (h2 is S s)
        {
        }
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,19): error CS8121: An expression of type 'U' cannot be handled by a pattern of type 'S'.
                //         if (h2 is S s)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "S").WithArguments("U", "S").WithLocation(7, 19)
                );
        }

        [Fact]
        public void IsPattern_09()
        {
            var src = @"
class Helper2
{
    public static void Test2(S1 h2)
    {
        if (h2 is S2 s2)
        {
        }
    }
}

ref struct S1
{
}

ref struct S2
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (6,19): error CS8121: An expression of type 'S1' cannot be handled by a pattern of type 'S2'.
                //         if (h2 is S2 s2)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "S2").WithArguments("S1", "S2").WithLocation(6, 19)
                );
        }

        [Fact]
        public void IsPattern_10()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
    where U : T, allows ref struct
{
    public static void Test1(T h1)
    {
        if (h1 is U u)
        {
        }
    }
    public static void Test2(U h2)
    {
        if (h2 is T t)
        {
        }
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (8,19): error CS8121: An expression of type 'T' cannot be handled by a pattern of type 'U'.
                //         if (h1 is U u)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "U").WithArguments("T", "U").WithLocation(8, 19),
                // (14,19): error CS8121: An expression of type 'U' cannot be handled by a pattern of type 'T'.
                //         if (h2 is T t)
                Diagnostic(ErrorCode.ERR_PatternWrongType, "T").WithArguments("U", "T").WithLocation(14, 19)
                );
        }

        [Fact]
        public void AsOperator_01()
        {
            var src = @"
class Helper1<T>
    where T : allows ref struct
{
    public static void Test1(I1 h1)
    {
        _ = h1 as T;
    }
}

class Helper2
{
    public static void Test2(I1 h2)
    {
        _ = h2 as S;
    }
}

ref struct S : I1
{
}

interface I1
{
}

struct S1 : I1 {}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0413: The type parameter 'T' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h1 as T;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h1 as T").WithArguments("T").WithLocation(7, 13),
                // (15,13): error CS0077: The as operator must be used with a reference type or nullable type ('S' is a non-nullable value type)
                //         _ = h2 as S;
                Diagnostic(ErrorCode.ERR_AsMustHaveReferenceType, "h2 as S").WithArguments("S").WithLocation(15, 13)
                );
        }

        [Fact]
        public void AsOperator_02()
        {
            var src1 = @"
class Helper1<T>
    where T : allows ref struct
{
    public static void Test1(T h1)
    {
        _ = h1 as I1;
    }
}

class Helper2
{
    public static void Test2(S h2)
    {
        _ = h2 as I1;
    }
}

ref struct S : I1
{
}

interface I1
{
}
";
            var comp = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0019: Operator 'as' cannot be applied to operands of type 'T' and 'I1'
                //         _ = h1 as I1;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h1 as I1").WithArguments("as", "T", "I1").WithLocation(7, 13),
                // (15,13): error CS0039: Cannot convert type 'S' to 'I1' via a reference conversion, boxing conversion, unboxing conversion, wrapping conversion, or null type conversion
                //         _ = h2 as I1;
                Diagnostic(ErrorCode.ERR_NoExplicitBuiltinConv, "h2 as I1").WithArguments("S", "I1").WithLocation(15, 13)
                );
        }

        [Fact]
        public void AsOperator_03()
        {
            var src = @"
class Helper1<T>
    where T : allows ref struct
{
    public static void Test1(T h1)
    {
        _ = h1 as T;
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0413: The type parameter 'T' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h1 as T;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h1 as T").WithArguments("T").WithLocation(7, 13)
                );
        }

        [Fact]
        public void AsOperator_04()
        {
            var src = @"
class Helper1<T>
    where T : struct, allows ref struct
{
    public static void Test1(T h1)
    {
        _ = h1 as T;
    }
}

class Helper2
{
    public static void Test2(S h2)
    {
        _ = h2 as S;
    }
}

ref struct S
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0413: The type parameter 'T' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h1 as T;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h1 as T").WithArguments("T").WithLocation(7, 13),
                // (15,13): error CS0077: The as operator must be used with a reference type or nullable type ('S' is a non-nullable value type)
                //         _ = h2 as S;
                Diagnostic(ErrorCode.ERR_AsMustHaveReferenceType, "h2 as S").WithArguments("S").WithLocation(15, 13)
                );
        }

        [Fact]
        public void AsOperator_05()
        {
            var src1 = @"
class Helper1<T, U>
    where T : allows ref struct
{
    public static void Test1(T h1)
    {
        _ = h1 as U;
    }
}

class Helper2<U>
{
    public static void Test2(S h2)
    {
        _ = h2 as U;
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src1, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0413: The type parameter 'U' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h1 as U;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h1 as U").WithArguments("U").WithLocation(7, 13),
                // (15,13): error CS0413: The type parameter 'U' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h2 as U;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h2 as U").WithArguments("U").WithLocation(15, 13)
                );

            var src2 = @"
class Helper1<T, U>
    where T : allows ref struct
    where U : class
{
    public static void Test1(T h1)
    {
        _ = h1 as U;
    }
}

class Helper2<U>
    where U : class
{
    public static void Test2(S h2)
    {
        _ = h2 as U;
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            comp = CreateCompilation(src2, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (8,13): error CS0019: Operator 'as' cannot be applied to operands of type 'T' and 'U'
                //         _ = h1 as U;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h1 as U").WithArguments("as", "T", "U").WithLocation(8, 13),
                // (17,13): error CS0019: Operator 'as' cannot be applied to operands of type 'S' and 'U'
                //         _ = h2 as U;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "h2 as U").WithArguments("as", "S", "U").WithLocation(17, 13)
                );
        }

        [Fact]
        public void AsOperator_06()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
    where U : allows ref struct
{
    public static void Test1(T h1)
    {
        _ = h1 as U;
    }
}

class Helper2<U>
    where U : allows ref struct
{
    public static void Test2(S h2)
    {
        _ = h2 as U;
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (8,13): error CS0413: The type parameter 'U' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h1 as U;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h1 as U").WithArguments("U").WithLocation(8, 13),
                // (17,13): error CS0413: The type parameter 'U' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h2 as U;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h2 as U").WithArguments("U").WithLocation(17, 13)
                );
        }

        [Fact]
        public void AsOperator_07()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
{
    static void Test1(U h1)
    {
        _ = h1 as T;
    }
}

class Helper2<U>
{
    public static void Test2(U h2)
    {
        _ = h2 as S;
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0413: The type parameter 'T' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h1 as T;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h1 as T").WithArguments("T").WithLocation(7, 13),
                // (15,13): error CS0077: The as operator must be used with a reference type or nullable type ('S' is a non-nullable value type)
                //         _ = h2 as S;
                Diagnostic(ErrorCode.ERR_AsMustHaveReferenceType, "h2 as S").WithArguments("S").WithLocation(15, 13)
                );
        }

        [Fact]
        public void AsOperator_08()
        {
            var src = @"
class Helper2<U>
    where U : allows ref struct
{
    public static void Test2(U h2)
    {
        _ = h2 as S;
    }
}

ref struct S : I1
{
}

interface I1
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,13): error CS0077: The as operator must be used with a reference type or nullable type ('S' is a non-nullable value type)
                //         _ = h2 as S;
                Diagnostic(ErrorCode.ERR_AsMustHaveReferenceType, "h2 as S").WithArguments("S").WithLocation(7, 13)
                );
        }

        [Fact]
        public void AsOperator_09()
        {
            var src = @"
class Helper2
{
    public static void Test2(S1 h2)
    {
        _ = h2 as S2;
    }
}

ref struct S1
{
}

ref struct S2
{
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (6,13): error CS0077: The as operator must be used with a reference type or nullable type ('S2' is a non-nullable value type)
                //         _ = h2 as S2;
                Diagnostic(ErrorCode.ERR_AsMustHaveReferenceType, "h2 as S2").WithArguments("S2").WithLocation(6, 13)
                );
        }

        [Fact]
        public void AsOperator_10()
        {
            var src = @"
class Helper1<T, U>
    where T : allows ref struct
    where U : T, allows ref struct
{
    public static void Test1(T h1)
    {
        _ = h1 as U;
    }
    public static void Test2(U h2)
    {
        _ = h2 as T;
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (8,13): error CS0413: The type parameter 'U' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h1 as U;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h1 as U").WithArguments("U").WithLocation(8, 13),
                // (12,13): error CS0413: The type parameter 'T' cannot be used with the 'as' operator because it does not have a class type constraint nor a 'class' constraint
                //         _ = h2 as T;
                Diagnostic(ErrorCode.ERR_AsWithTypeVar, "h2 as T").WithArguments("T").WithLocation(12, 13)
                );
        }

        [Fact]
        public void IllegalCapturing_01()
        {
            var source = @"
ref struct R1
{
}

ref struct R2<T>(R1 r1, T t)
    where T : allows ref struct
{
    R1 M1() => r1;
    T M2() => t;
}
";
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyEmitDiagnostics(
                // (9,16): error CS9110: Cannot use primary constructor parameter 'r1' that has ref-like type inside an instance member
                //     R1 M1() => r1;
                Diagnostic(ErrorCode.ERR_UnsupportedPrimaryConstructorParameterCapturingRefLike, "r1").WithArguments("r1").WithLocation(9, 16),
                // (10,15): error CS9110: Cannot use primary constructor parameter 't' that has ref-like type inside an instance member
                //     T M2() => t;
                Diagnostic(ErrorCode.ERR_UnsupportedPrimaryConstructorParameterCapturingRefLike, "t").WithArguments("t").WithLocation(10, 15)
                );
        }

        [Fact]
        public void IllegalCapturing_02()
        {
            var source = @"
ref struct R1
{
}

class C
{
    void M<T>(R1 r1, T t)
        where T : allows ref struct
    {
        var d1 = () => r1;
        var d2 = () => t;
    }
}
";
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyEmitDiagnostics(
                // (11,24): error CS9108: Cannot use parameter 'r1' that has ref-like type inside an anonymous method, lambda expression, query expression, or local function
                //         var d1 = () => r1;
                Diagnostic(ErrorCode.ERR_AnonDelegateCantUseRefLike, "r1").WithArguments("r1").WithLocation(11, 24),
                // (12,24): error CS9108: Cannot use parameter 't' that has ref-like type inside an anonymous method, lambda expression, query expression, or local function
                //         var d2 = () => t;
                Diagnostic(ErrorCode.ERR_AnonDelegateCantUseRefLike, "t").WithArguments("t").WithLocation(12, 24)
                );
        }

        [Fact]
        public void IllegalCapturing_03()
        {
            var source = @"
ref struct R1
{
}

class C
{
    void M<T>(R1 r1, T t)
        where T : allows ref struct
    {
        R1 r2 = r1;
        T t2 = t;

        var d1 = () => r2;
        var d2 = () => t2;
    }
}
";
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyEmitDiagnostics(
                // (14,24): error CS8175: Cannot use ref local 'r2' inside an anonymous method, lambda expression, or query expression
                //         var d1 = () => r2;
                Diagnostic(ErrorCode.ERR_AnonDelegateCantUseLocal, "r2").WithArguments("r2").WithLocation(14, 24),
                // (15,24): error CS8175: Cannot use ref local 't2' inside an anonymous method, lambda expression, or query expression
                //         var d2 = () => t2;
                Diagnostic(ErrorCode.ERR_AnonDelegateCantUseLocal, "t2").WithArguments("t2").WithLocation(15, 24)
                );
        }

        [Fact]
        public void PassingSpansToParameters_Errors()
        {
            var src = @"
using System;
class C
{
    static void Main()
    {
        Span<int> s1 = stackalloc int[1];
        M1(s1);
    }
    
    static void M1<T>(T s1) where T : allows ref struct 
    {
        var obj = new C();
        T s2 = M3<T>(stackalloc int[2]);

        M2(ref s1, out s2);         // one
        M2(ref s2, out s1);         // two

        M2(ref s1, out s2);         // three
        M2(ref s2, out s1);         // four

        M2(y: out s2, x: ref s1);   // five
        M2(y: out s1, x: ref s2);   // six

        M2(ref s1, out s1);         // okay
        M2(ref s2, out s2);         // okay
    }

    static void M2<T>(scoped ref T x, out T y)
        where T : allows ref struct
    {
        y = default;
    }

    static T M3<T>(Span<int> x) where T : allows ref struct => default;
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (17,9): error CS8350: This combination of arguments to 'C.M2<T>(scoped ref T, out T)' is disallowed because it may expose variables referenced by parameter 'x' outside of their declaration scope
                //         M2(ref s2, out s1);         // two
                Diagnostic(ErrorCode.ERR_CallArgMixing, "M2(ref s2, out s1)").WithArguments("C.M2<T>(scoped ref T, out T)", "x").WithLocation(17, 9),
                // (17,16): error CS8352: Cannot use variable 's2' in this context because it may expose referenced variables outside of their declaration scope
                //         M2(ref s2, out s1);         // two
                Diagnostic(ErrorCode.ERR_EscapeVariable, "s2").WithArguments("s2").WithLocation(17, 16),
                // (20,9): error CS8350: This combination of arguments to 'C.M2<T>(scoped ref T, out T)' is disallowed because it may expose variables referenced by parameter 'x' outside of their declaration scope
                //         M2(ref s2, out s1);         // four
                Diagnostic(ErrorCode.ERR_CallArgMixing, "M2(ref s2, out s1)").WithArguments("C.M2<T>(scoped ref T, out T)", "x").WithLocation(20, 9),
                // (20,16): error CS8352: Cannot use variable 's2' in this context because it may expose referenced variables outside of their declaration scope
                //         M2(ref s2, out s1);         // four
                Diagnostic(ErrorCode.ERR_EscapeVariable, "s2").WithArguments("s2").WithLocation(20, 16),
                // (23,9): error CS8350: This combination of arguments to 'C.M2<T>(scoped ref T, out T)' is disallowed because it may expose variables referenced by parameter 'x' outside of their declaration scope
                //         M2(y: out s1, x: ref s2);   // six
                Diagnostic(ErrorCode.ERR_CallArgMixing, "M2(y: out s1, x: ref s2)").WithArguments("C.M2<T>(scoped ref T, out T)", "x").WithLocation(23, 9),
                // (23,30): error CS8352: Cannot use variable 's2' in this context because it may expose referenced variables outside of their declaration scope
                //         M2(y: out s1, x: ref s2);   // six
                Diagnostic(ErrorCode.ERR_EscapeVariable, "s2").WithArguments("s2").WithLocation(23, 30)
                );
        }

        [Fact]
        public void RefLikeReturnEscape1()
        {
            var text = @"
    using System;

    class Program<T> where T : allows ref struct
    {
        static void Main()
        {
        }

        static ref int Test1(T arg)
        {
            throw null;
        }

        static T MayWrap(Span<int> arg)
        {
            return default;
        }

        static ref int Test3()
        {
            Span<int> local = stackalloc int[1];
            var sp = MayWrap(local);
            return ref Test1(sp);
        }
    }
";
            CreateCompilation(text, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics).VerifyDiagnostics(
                // (24,30): error CS8352: Cannot use variable 'sp' in this context because it may expose referenced variables outside of their declaration scope
                //             return ref Test1(sp);
                Diagnostic(ErrorCode.ERR_EscapeVariable, "sp").WithArguments("sp").WithLocation(24, 30),
                // (24,24): error CS8347: Cannot use a result of 'Program<T>.Test1(T)' in this context because it may expose variables referenced by parameter 'arg' outside of their declaration scope
                //             return ref Test1(sp);
                Diagnostic(ErrorCode.ERR_EscapeCall, "Test1(sp)").WithArguments("Program<T>.Test1(T)", "arg").WithLocation(24, 24)
            );
        }

        [Fact]
        public void RefLikeEscapeMixingCall()
        {
            var text = @"
    using System;
    class Program<T> where T : allows ref struct
    {
        static void Main()
        {
        }

        void Test1()
        {
            T rOuter = default;

            Span<int> inner = stackalloc int[1];
            T rInner = MayWrap(ref inner);

            // valid
            MayAssign(ref rOuter, ref rOuter);

            // error
            MayAssign(ref rOuter, ref rInner);

            // error
            MayAssign(ref inner, ref rOuter);
        }

        static void MayAssign(ref Span<int> arg1, ref T arg2)
        {
            arg2 = MayWrap(ref arg1);
        }

        static void MayAssign(ref T arg1, ref T arg2)
        {
            arg1 = arg2;
        }

        static T MayWrap(ref Span<int> arg)
        {
            return default;
        }
    }
";
            var comp = CreateCompilation(text, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (20,39): error CS8352: Cannot use variable 'rInner' in this context because it may expose referenced variables outside of their declaration scope
                //             MayAssign(ref rOuter, ref rInner);
                Diagnostic(ErrorCode.ERR_EscapeVariable, "rInner").WithArguments("rInner").WithLocation(20, 39),
                // (20,13): error CS8350: This combination of arguments to 'Program<T>.MayAssign(ref T, ref T)' is disallowed because it may expose variables referenced by parameter 'arg2' outside of their declaration scope
                //             MayAssign(ref rOuter, ref rInner);
                Diagnostic(ErrorCode.ERR_CallArgMixing, "MayAssign(ref rOuter, ref rInner)").WithArguments("Program<T>.MayAssign(ref T, ref T)", "arg2").WithLocation(20, 13),
                // (23,27): error CS8352: Cannot use variable 'inner' in this context because it may expose referenced variables outside of their declaration scope
                //             MayAssign(ref inner, ref rOuter);
                Diagnostic(ErrorCode.ERR_EscapeVariable, "inner").WithArguments("inner").WithLocation(23, 27),
                // (23,13): error CS8350: This combination of arguments to 'Program<T>.MayAssign(ref Span<int>, ref T)' is disallowed because it may expose variables referenced by parameter 'arg1' outside of their declaration scope
                //             MayAssign(ref inner, ref rOuter);
                Diagnostic(ErrorCode.ERR_CallArgMixing, "MayAssign(ref inner, ref rOuter)").WithArguments("Program<T>.MayAssign(ref System.Span<int>, ref T)", "arg1").WithLocation(23, 13),
                // (28,32): error CS9077: Cannot return a parameter by reference 'arg1' through a ref parameter; it can only be returned in a return statement
                //             arg2 = MayWrap(ref arg1);
                Diagnostic(ErrorCode.ERR_RefReturnOnlyParameter, "arg1").WithArguments("arg1").WithLocation(28, 32),
                // (28,20): error CS8347: Cannot use a result of 'Program<T>.MayWrap(ref Span<int>)' in this context because it may expose variables referenced by parameter 'arg' outside of their declaration scope
                //             arg2 = MayWrap(ref arg1);
                Diagnostic(ErrorCode.ERR_EscapeCall, "MayWrap(ref arg1)").WithArguments("Program<T>.MayWrap(ref System.Span<int>)", "arg").WithLocation(28, 20)
            );

            comp = CreateCompilation(text, targetFramework: TargetFramework.Net60, parseOptions: TestOptions.Regular10);
            comp.VerifyDiagnostics(
                // (3,39): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     class Program<T> where T : allows ref struct
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "ref struct").WithArguments("ref struct interfaces").WithLocation(3, 39),
                // (3,39): error CS9500: Target runtime doesn't support by-ref-like generics.
                //     class Program<T> where T : allows ref struct
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "ref struct").WithLocation(3, 39),
                // (20,39): error CS8352: Cannot use variable 'rInner' in this context because it may expose referenced variables outside of their declaration scope
                //             MayAssign(ref rOuter, ref rInner);
                Diagnostic(ErrorCode.ERR_EscapeVariable, "rInner").WithArguments("rInner").WithLocation(20, 39),
                // (20,13): error CS8350: This combination of arguments to 'Program<T>.MayAssign(ref T, ref T)' is disallowed because it may expose variables referenced by parameter 'arg2' outside of their declaration scope
                //             MayAssign(ref rOuter, ref rInner);
                Diagnostic(ErrorCode.ERR_CallArgMixing, "MayAssign(ref rOuter, ref rInner)").WithArguments("Program<T>.MayAssign(ref T, ref T)", "arg2").WithLocation(20, 13),
                // (23,27): error CS8352: Cannot use variable 'inner' in this context because it may expose referenced variables outside of their declaration scope
                //             MayAssign(ref inner, ref rOuter);
                Diagnostic(ErrorCode.ERR_EscapeVariable, "inner").WithArguments("inner").WithLocation(23, 27),
                // (23,13): error CS8350: This combination of arguments to 'Program<T>.MayAssign(ref Span<int>, ref T)' is disallowed because it may expose variables referenced by parameter 'arg1' outside of their declaration scope
                //             MayAssign(ref inner, ref rOuter);
                Diagnostic(ErrorCode.ERR_CallArgMixing, "MayAssign(ref inner, ref rOuter)").WithArguments("Program<T>.MayAssign(ref System.Span<int>, ref T)", "arg1").WithLocation(23, 13)
                );
        }

        [Fact]
        public void RefSafeToEscape_05()
        {
            var source =
@"
class Program<T> where T : allows ref struct
{
    static ref T F0(T r0)
    {
        scoped ref T l0 = ref r0;
        return ref l0; // 1
    }
    static ref T F1(scoped T r1)
    {
        scoped ref T l1 = ref r1; // 2
        return ref l1; // 3
    }
    static ref T F2(ref T r2)
    {
        scoped ref T l2 = ref r2;
        return ref l2; // 4
    }
    static ref T F3(scoped ref T r3)
    {
        scoped ref T l3 = ref r3;
        return ref l3; // 5
    }
}";
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (7,20): error CS8157: Cannot return 'l0' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref l0; // 1
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "l0").WithArguments("l0").WithLocation(7, 20),
                // (11,31): error CS8352: Cannot use variable 'scoped T r1' in this context because it may expose referenced variables outside of their declaration scope
                //         scoped ref T l1 = ref r1; // 2
                Diagnostic(ErrorCode.ERR_EscapeVariable, "r1").WithArguments("scoped T r1").WithLocation(11, 31),
                // (12,20): error CS8157: Cannot return 'l1' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref l1; // 3
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "l1").WithArguments("l1").WithLocation(12, 20),
                // (17,20): error CS8157: Cannot return 'l2' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref l2; // 4
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "l2").WithArguments("l2").WithLocation(17, 20),
                // (22,20): error CS8157: Cannot return 'l3' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref l3; // 5
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "l3").WithArguments("l3").WithLocation(22, 20)
                );
        }

        [Fact]
        public void RefToRefStructParameter_02()
        {
            var source =
@"
class Program<R> where R : allows ref struct 
{
    static ref R F1()
    {
        int i = 42;
        var r1 = GetR(ref i);
        return ref ReturnRef(ref r1);
    }
    static ref R F2(ref int i)
    {
        var r2 = GetR(ref i);
        return ref ReturnRef(ref r2);
    }
    
    static ref R ReturnRef(scoped ref R r) => throw null;

    static R GetR(ref int x) => throw null;
}";
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics();
        }

        [Fact]
        public void ReturnRefToByValueParameter_01()
        {
            var source =
@"
using System.Diagnostics.CodeAnalysis;

class Program<S> where S : allows ref struct
{
    static ref S F1([UnscopedRef] ref S x1)
    {
        return ref x1;
    }
    static ref S F2(S x2)
    {
        ref var y2 = ref F1(ref x2);
        return ref y2; // 1
    }
}";

            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyEmitDiagnostics(
                // (13,20): error CS8157: Cannot return 'y2' by reference because it was initialized to a value that cannot be returned by reference
                //         return ref y2; // 1
                Diagnostic(ErrorCode.ERR_RefReturnNonreturnableLocal, "y2").WithArguments("y2").WithLocation(13, 20)
                );
        }

        [Fact]
        public void ReturnRefToRefStruct_RefEscape_01()
        {
            var source = """
                public class Repro<RefStruct> where RefStruct : allows ref struct
                {
                    private static ref RefStruct M1(ref RefStruct s1, ref RefStruct s2)
                    {
                        bool b = false;
                        return ref b ? ref s1 : ref s2;
                    }

                    private static ref RefStruct M2(ref RefStruct s1)
                    {
                        RefStruct s2 = default;
                        // RSTE of s1 is ReturnOnly
                        // RSTE of s2 is CurrentMethod
                        return ref M1(ref s1, ref s2); // 1
                    }
                    
                    private static ref RefStruct M3(ref RefStruct s1)
                    {
                        return ref M1(ref s1, ref s1);
                    }
                }
                """;
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (14,20): error CS8347: Cannot use a result of 'Repro<RefStruct>.M1(ref RefStruct, ref RefStruct)' in this context because it may expose variables referenced by parameter 's2' outside of their declaration scope
                //         return ref M1(ref s1, ref s2); // 1
                Diagnostic(ErrorCode.ERR_EscapeCall, "M1(ref s1, ref s2)").WithArguments("Repro<RefStruct>.M1(ref RefStruct, ref RefStruct)", "s2").WithLocation(14, 20),
                // (14,35): error CS8168: Cannot return local 's2' by reference because it is not a ref local
                //         return ref M1(ref s1, ref s2); // 1
                Diagnostic(ErrorCode.ERR_RefReturnLocal, "s2").WithArguments("s2").WithLocation(14, 35)
                );
        }

        [Fact]
        public void RefStructProperty_01()
        {
            var source =
@"
class C<Rint, Robject>
    where Rint : allows ref struct
    where Robject : allows ref struct
{
    Robject this[Rint r] => default;
    static Robject F1(C<Rint, Robject> c)
    {
        int i = 1;
        var r1 = GetRint(ref i);
        return c[r1]; // 1
    }
    static Robject F2(C<Rint, Robject> c)
    {
        var r2 = GetRint();
        return c[r2];
    }
    static Rint GetRint(ref int x) => throw null;
    static Rint GetRint() => throw null;
}";

            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyDiagnostics(
                // (11,16): error CS8347: Cannot use a result of 'C<Rint, Robject>.this[Rint]' in this context because it may expose variables referenced by parameter 'r' outside of their declaration scope
                //         return c[r1]; // 1
                Diagnostic(ErrorCode.ERR_EscapeCall, "c[r1]").WithArguments("C<Rint, Robject>.this[Rint]", "r").WithLocation(11, 16),
                // (11,18): error CS8352: Cannot use variable 'r1' in this context because it may expose referenced variables outside of their declaration scope
                //         return c[r1]; // 1
                Diagnostic(ErrorCode.ERR_EscapeVariable, "r1").WithArguments("r1").WithLocation(11, 18)
                );
        }

        [Fact]
        public void MethodArgumentsMustMatch_08()
        {
            var source =
@"
using static Helper;
class Helper
{
    public static void F0(__arglist) { }
}

class Program<R>
    where R : allows ref struct
{
    static void F1()
    {
        var x = GetR();
        int i = 1;
        var y = GetR(ref i);
        F0(__arglist(ref x)); // 1
        F0(__arglist(ref y));
        F0(__arglist(ref x, ref x)); // 2
        F0(__arglist(ref x, ref y)); // 3
        F0(__arglist(ref y, ref x)); // 4
        F0(__arglist(ref y, ref y));
    }
    static R GetR(ref int x) => throw null;
    static R GetR() => throw null;
}";

            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyEmitDiagnostics(
                // (19,9): error CS8350: This combination of arguments to 'Helper.F0(__arglist)' is disallowed because it may expose variables referenced by parameter '__arglist' outside of their declaration scope
                //         F0(__arglist(ref x, ref y)); // 3
                Diagnostic(ErrorCode.ERR_CallArgMixing, "F0(__arglist(ref x, ref y))").WithArguments("Helper.F0(__arglist)", "__arglist").WithLocation(19, 9),
                // (19,33): error CS8352: Cannot use variable 'y' in this context because it may expose referenced variables outside of their declaration scope
                //         F0(__arglist(ref x, ref y)); // 3
                Diagnostic(ErrorCode.ERR_EscapeVariable, "y").WithArguments("y").WithLocation(19, 33),
                // (20,9): error CS8350: This combination of arguments to 'Helper.F0(__arglist)' is disallowed because it may expose variables referenced by parameter '__arglist' outside of their declaration scope
                //         F0(__arglist(ref y, ref x)); // 4
                Diagnostic(ErrorCode.ERR_CallArgMixing, "F0(__arglist(ref y, ref x))").WithArguments("Helper.F0(__arglist)", "__arglist").WithLocation(20, 9),
                // (20,26): error CS8352: Cannot use variable 'y' in this context because it may expose referenced variables outside of their declaration scope
                //         F0(__arglist(ref y, ref x)); // 4
                Diagnostic(ErrorCode.ERR_EscapeVariable, "y").WithArguments("y").WithLocation(20, 26)
                );
        }

        [Fact]
        public void IsPatternMatchingDoesNotCopyEscapeScopes_05()
        {
            CreateCompilation(@"
using System;
public interface IR<R> where R : IR<R>, allows ref struct
{
    public R RProp {get;}
    public S<R> SProp {get;}
    public abstract static implicit operator R(Span<int> span);
}
public struct S<R> where R : IR<R>, allows ref struct
{
    public R RProp => throw null;
    public S<R> SProp => throw null;
}
public class C<R> where R : IR<R>, allows ref struct
{
    public void M1(ref R r, ref S<R> s)
    {
        R outer = stackalloc int[100];
        if (outer is { RProp.RProp: var rr0 }) r = rr0; // error
        if (outer is { SProp.RProp: var sr0 }) r = sr0; // OK
        if (outer is { SProp.SProp: var ss0 }) s = ss0; // OK
        if (outer is { RProp.SProp: var rs0 }) s = rs0; // OK
        if (outer is { RProp: { RProp: var rr1 }}) r = rr1; // error
        if (outer is { SProp: { RProp: var sr1 }}) r = sr1; // OK
        if (outer is { SProp: { SProp: var ss1 }}) s = ss1; // OK
        if (outer is { RProp: { SProp: var rs1 }}) s = rs1; // OK
    }
}", targetFramework: s_targetFrameworkSupportingByRefLikeGenerics).VerifyDiagnostics(
                // (19,52): error CS8352: Cannot use variable 'rr0' in this context because it may expose referenced variables outside of their declaration scope
                //         if (outer is { RProp.RProp: var rr0 }) r = rr0; // error
                Diagnostic(ErrorCode.ERR_EscapeVariable, "rr0").WithArguments("rr0").WithLocation(19, 52),
                // (23,56): error CS8352: Cannot use variable 'rr1' in this context because it may expose referenced variables outside of their declaration scope
                //         if (outer is { RProp: { RProp: var rr1 }}) r = rr1; // error
                Diagnostic(ErrorCode.ERR_EscapeVariable, "rr1").WithArguments("rr1").WithLocation(23, 56)
                );
        }

        [Fact]
        public void CasePatternMatchingDoesNotCopyEscapeScopes_02()
        {
            CreateCompilation(@"
using System;
public interface IR<R> where R : IR<R>, allows ref struct
{
    public R Prop {get;}
    public void Deconstruct(out R X, out R Y);
    public abstract static implicit operator R(Span<int> span);
}
public class C<R> where R : struct, IR<R>, allows ref struct
{
    public R M1()
    {
        R outer = stackalloc int[100];
        switch (outer)
        {
            case { Prop: var x }: return x; // error 1
        }
    }
    public R M2()
    {
        R outer = stackalloc int[100];
        switch (outer)
        {
            case { Prop: R x }: return x; // error 2
        }
    }
    public R M3()
    {
        R outer = stackalloc int[100];
        switch (outer)
        {
            case (var x, var y): return x; // error 3
        }
    }
    public R M4()
    {
        R outer = stackalloc int[100];
        switch (outer)
        {
            case (R x, R y): return x; // error 4
        }
    }
    public R M5()
    {
        R outer = stackalloc int[100];
        switch (outer)
        {
            case var (x, y): return x; // error 5
        }
    }
    public R M6()
    {
        R outer = stackalloc int[100];
        switch (outer)
        {
            case { } x: return x; // error 6
        }
    }
    public R M7()
    {
        R outer = stackalloc int[100];
        switch (outer)
        {
            case (_, _) x: return x; // error 7
        }
    }
}
", targetFramework: s_targetFrameworkSupportingByRefLikeGenerics).VerifyDiagnostics(
                // (16,42): error CS8352: Cannot use variable 'x' in this context because it may expose referenced variables outside of their declaration scope
                //             case { Prop: var x }: return x; // error 1
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x").WithArguments("x").WithLocation(16, 42),
                // (24,40): error CS8352: Cannot use variable 'x' in this context because it may expose referenced variables outside of their declaration scope
                //             case { Prop: R x }: return x; // error 2
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x").WithArguments("x").WithLocation(24, 40),
                // (32,41): error CS8352: Cannot use variable 'x' in this context because it may expose referenced variables outside of their declaration scope
                //             case (var x, var y): return x; // error 3
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x").WithArguments("x").WithLocation(32, 41),
                // (40,37): error CS8352: Cannot use variable 'x' in this context because it may expose referenced variables outside of their declaration scope
                //             case (R x, R y): return x; // error 4
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x").WithArguments("x").WithLocation(40, 37),
                // (48,37): error CS8352: Cannot use variable 'x' in this context because it may expose referenced variables outside of their declaration scope
                //             case var (x, y): return x; // error 5
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x").WithArguments("x").WithLocation(48, 37),
                // (56,32): error CS8352: Cannot use variable 'x' in this context because it may expose referenced variables outside of their declaration scope
                //             case { } x: return x; // error 6
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x").WithArguments("x").WithLocation(56, 32),
                // (64,35): error CS8352: Cannot use variable 'x' in this context because it may expose referenced variables outside of their declaration scope
                //             case (_, _) x: return x; // error 7
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x").WithArguments("x").WithLocation(64, 35)
                );
        }

        [Fact]
        public void RefLikeScopeEscapeThis()
        {
            var text = @"
    using System;
    class Program<S1> where S1 : IS1<S1>, allows ref struct
    {
        static void Main()
        {
            Span<int> outer = default;

            S1 x = MayWrap(ref outer);

            {
                Span<int> inner = stackalloc int[1];

                // valid
                x = S1.NotSlice(1);

                // valid
                x = MayWrap(ref outer).Slice(1);
    
                // error
                x = MayWrap(ref inner).Slice(1);
            }
        }

        static S1 MayWrap(ref Span<int> arg)
        {
            return default;
        }
    }

    interface IS1<S1> where S1 : IS1<S1>, allows ref struct
    {
        public abstract static S1 NotSlice(int x);

        public S1 Slice(int x);
    }
";
            var comp = CreateCompilation(text, targetFramework: TargetFramework.Net60, parseOptions: TestOptions.Regular10);
            comp.VerifyDiagnostics(
                // (3,50): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     class Program<S1> where S1 : IS1<S1>, allows ref struct
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "ref struct").WithArguments("ref struct interfaces").WithLocation(3, 50),
                // (3,50): error CS9500: Target runtime doesn't support by-ref-like generics.
                //     class Program<S1> where S1 : IS1<S1>, allows ref struct
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "ref struct").WithLocation(3, 50),
                // (31,50): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                //     interface IS1<S1> where S1 : IS1<S1>, allows ref struct
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "ref struct").WithArguments("ref struct interfaces").WithLocation(31, 50),
                // (31,50): error CS9500: Target runtime doesn't support by-ref-like generics.
                //     interface IS1<S1> where S1 : IS1<S1>, allows ref struct
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "ref struct").WithLocation(31, 50),
                // (33,35): error CS8703: The modifier 'abstract' is not valid for this item in C# 10.0. Please use language version '11.0' or greater.
                //         public abstract static S1 NotSlice(int x);
                Diagnostic(ErrorCode.ERR_InvalidModifierForLanguageVersion, "NotSlice").WithArguments("abstract", "10.0", "11.0").WithLocation(33, 35),
                // (15,21): error CS8936: Feature 'static abstract members in interfaces' is not available in C# 10.0. Please use language version 11.0 or greater.
                //                 x = S1.NotSlice(1);
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion10, "S1").WithArguments("static abstract members in interfaces", "11.0").WithLocation(15, 21),
                // (21,33): error CS8352: Cannot use variable 'inner' in this context because it may expose referenced variables outside of their declaration scope
                //                 x = MayWrap(ref inner).Slice(1);
                Diagnostic(ErrorCode.ERR_EscapeVariable, "inner").WithArguments("inner").WithLocation(21, 33),
                // (21,21): error CS8347: Cannot use a result of 'Program<S1>.MayWrap(ref Span<int>)' in this context because it may expose variables referenced by parameter 'arg' outside of their declaration scope
                //                 x = MayWrap(ref inner).Slice(1);
                Diagnostic(ErrorCode.ERR_EscapeCall, "MayWrap(ref inner)").WithArguments("Program<S1>.MayWrap(ref System.Span<int>)", "arg").WithLocation(21, 21)
            );
        }

        [Fact]
        public void RefLikeScopeEscapeThisRef()
        {
            var text = @"
using System;
class Program<S1> where S1 : IS1<S1>, allows ref struct
{
    static void Main()
    {
        Span<int> outer = default;

        ref S1 x = ref MayWrap(ref outer)[0];

        {
            Span<int> inner = stackalloc int[1];

            // valid
            x[0] = MayWrap(ref outer).Slice(1)[0];

            // error, technically rules for this case can be relaxed, 
            // but ref-like typed ref-returning properties are nearly impossible to implement in a useful way
            //
            x[0] = MayWrap(ref inner).Slice(1)[0];

            // error, technically rules for this case can be relaxed, 
            // but ref-like typed ref-returning properties are nearly impossible to implement in a useful way
            //
            x[x] = MayWrap(ref inner).Slice(1)[0];

            // error
            x.ReturnsRefArg(ref x) = MayWrap(ref inner).Slice(1)[0];
        }
    }

    static S1 MayWrap(ref Span<int> arg)
    {
        return default;
    }
}

interface IS1<S1> where S1 : IS1<S1>, allows ref struct
{
    public ref S1 this[int i] {get;}

    public ref S1 this[S1 i] {get;}

    public ref S1 ReturnsRefArg(ref S1 arg);

    public S1 Slice(int x);
}
";
            var comp = CreateCompilation(new[] { text, UnscopedRefAttributeDefinition }, targetFramework: TargetFramework.Net60, parseOptions: TestOptions.Regular10);
            comp.VerifyDiagnostics(
                // 0.cs(38,46): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                // interface IS1<S1> where S1 : IS1<S1>, allows ref struct
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "ref struct").WithArguments("ref struct interfaces").WithLocation(38, 46),
                // 0.cs(38,46): error CS9500: Target runtime doesn't support by-ref-like generics.
                // interface IS1<S1> where S1 : IS1<S1>, allows ref struct
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "ref struct").WithLocation(38, 46),
                // 0.cs(3,46): error CS8652: The feature 'ref struct interfaces' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
                // class Program<S1> where S1 : IS1<S1>, allows ref struct
                Diagnostic(ErrorCode.ERR_FeatureInPreview, "ref struct").WithArguments("ref struct interfaces").WithLocation(3, 46),
                // 0.cs(3,46): error CS9500: Target runtime doesn't support by-ref-like generics.
                // class Program<S1> where S1 : IS1<S1>, allows ref struct
                Diagnostic(ErrorCode.ERR_RuntimeDoesNotSupportByRefLikeGenerics, "ref struct").WithLocation(3, 46),
                // 0.cs(20,32): error CS8352: Cannot use variable 'inner' in this context because it may expose referenced variables outside of their declaration scope
                //             x[0] = MayWrap(ref inner).Slice(1)[0];
                Diagnostic(ErrorCode.ERR_EscapeVariable, "inner").WithArguments("inner").WithLocation(20, 32),
                // 0.cs(20,20): error CS8347: Cannot use a result of 'Program<S1>.MayWrap(ref Span<int>)' in this context because it may expose variables referenced by parameter 'arg' outside of their declaration scope
                //             x[0] = MayWrap(ref inner).Slice(1)[0];
                Diagnostic(ErrorCode.ERR_EscapeCall, "MayWrap(ref inner)").WithArguments("Program<S1>.MayWrap(ref System.Span<int>)", "arg").WithLocation(20, 20),
                // 0.cs(25,32): error CS8352: Cannot use variable 'inner' in this context because it may expose referenced variables outside of their declaration scope
                //             x[x] = MayWrap(ref inner).Slice(1)[0];
                Diagnostic(ErrorCode.ERR_EscapeVariable, "inner").WithArguments("inner").WithLocation(25, 32),
                // 0.cs(25,20): error CS8347: Cannot use a result of 'Program<S1>.MayWrap(ref Span<int>)' in this context because it may expose variables referenced by parameter 'arg' outside of their declaration scope
                //             x[x] = MayWrap(ref inner).Slice(1)[0];
                Diagnostic(ErrorCode.ERR_EscapeCall, "MayWrap(ref inner)").WithArguments("Program<S1>.MayWrap(ref System.Span<int>)", "arg").WithLocation(25, 20),
                // 0.cs(28,50): error CS8352: Cannot use variable 'inner' in this context because it may expose referenced variables outside of their declaration scope
                //             x.ReturnsRefArg(ref x) = MayWrap(ref inner).Slice(1)[0];
                Diagnostic(ErrorCode.ERR_EscapeVariable, "inner").WithArguments("inner").WithLocation(28, 50),
                // 0.cs(28,38): error CS8347: Cannot use a result of 'Program<S1>.MayWrap(ref Span<int>)' in this context because it may expose variables referenced by parameter 'arg' outside of their declaration scope
                //             x.ReturnsRefArg(ref x) = MayWrap(ref inner).Slice(1)[0];
                Diagnostic(ErrorCode.ERR_EscapeCall, "MayWrap(ref inner)").WithArguments("Program<S1>.MayWrap(ref System.Span<int>)", "arg").WithLocation(28, 38)
                );
        }

        [Fact]
        public void RefAssignValueScopeMismatch_05()
        {
            var source =
@"
class Program<S> where S : allows ref struct
{
    static S F()
    {
        S s1 = default;
        scoped ref S r1 = ref s1;
        int i = 0;
        S s2 = GetS(ref i);
        ref S r2 = ref s2;
        r2 = ref r1; // 1
        r2 = s2;
        return s1;
    }

    static S GetS(ref int i) => throw null;
}
";
            var comp = CreateCompilation(source, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);
            comp.VerifyEmitDiagnostics(
                // (11,9): error CS9096: Cannot ref-assign 'r1' to 'r2' because 'r1' has a wider value escape scope than 'r2' allowing assignment through 'r2' of values with narrower escapes scopes than 'r1'.
                //         r2 = ref r1; // 1
                Diagnostic(ErrorCode.ERR_RefAssignValEscapeWider, "r2 = ref r1").WithArguments("r2", "r1").WithLocation(11, 9)
                );
        }

        [Fact]
        public void RefLikeObjInitializers1()
        {
            var text = @"
    using System;

    class Program<S2> where S2 : IS2, new(), allows ref struct
    {
        static void Main()
        {
        }

        static S2 Test1()
        {
            S1 outer = default;
            S1 inner = stackalloc int[1];

            var x1 = new S2() { Field1 = outer, Field2 = inner };

            // error
            return x1;
        }

        static S2 Test2()
        {
            S1 outer = default;
            S1 inner = stackalloc int[1];

            var x2 = new S2() { Field1 = inner, Field2 = outer };

            // error
            return x2;
        }

        static S2 Test3()
        {
            S1 outer = default;
            S1 inner = stackalloc int[1];

            var x3 = new S2() { Field1 = outer, Field2 = outer };

            // ok
            return x3;
        }
    }

    public ref struct S1
    {
        public static implicit operator S1(Span<int> o) => default;
    }

    public interface IS2
    {
        public S1 Field1 {get;set;}
        public S1 Field2 {get;set;}
    }

    namespace System
    {
        public class Activator
        {
             public static T CreateInstance<T>() where T : allows ref struct => default;
        }
    }
";
            CreateCompilation(text, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics).VerifyEmitDiagnostics(
                // (18,20): error CS8352: Cannot use variable 'x1' in this context because it may expose referenced variables outside of their declaration scope
                //             return x1;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x1").WithArguments("x1").WithLocation(18, 20),
                // (29,20): error CS8352: Cannot use variable 'x2' in this context because it may expose referenced variables outside of their declaration scope
                //             return x2;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x2").WithArguments("x2").WithLocation(29, 20)
            );
        }

        [Fact]
        public void RefLikeObjInitializersIndexer1()
        {
            var text = @"
using System;

class Program<S2> where S2 : IS2, new(), allows ref struct
{
    static void Main()
    {
    }

    static S2 Test1()
    {
        S1 outer = default;
        S1 inner = stackalloc int[1];

        var x1 =  new S2() { [inner] = outer, Field2 = outer };

        // error
        return x1;
    }

    static S2 Test2()
    {
        S1 outer = default;
        S1 inner = stackalloc int[1];

        S2 result;

        // error
        result = new S2() { [outer] = inner, Field2 = outer };

        return result;
    }

    static S2 Test3()
    {
        S1 outer = default;
        S1 inner = stackalloc int[1];

        var x3 = new S2() { [outer] = outer, Field2 = outer };

        // ok
        return x3;
    }
}

public ref struct S1
{
    public static implicit operator S1(Span<int> o) => default;
}

public interface IS2
{
    public S1 this[S1 i] {get;set;}
    public S1 Field2 {get;set;}
}

namespace System
{
    public class Activator
    {
         public static T CreateInstance<T>() where T : allows ref struct => default;
    }
}
";
            CreateCompilation(text, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics).VerifyEmitDiagnostics(
                // (18,16): error CS8352: Cannot use variable 'x1' in this context because it may expose referenced variables outside of their declaration scope
                //         return x1;
                Diagnostic(ErrorCode.ERR_EscapeVariable, "x1").WithArguments("x1").WithLocation(18, 16),
                // (29,29): error CS8352: Cannot use variable 'inner' in this context because it may expose referenced variables outside of their declaration scope
                //         result = new S2() { [outer] = inner, Field2 = outer };
                Diagnostic(ErrorCode.ERR_EscapeVariable, "[outer] = inner").WithArguments("inner").WithLocation(29, 29)
                );
        }

        [Fact]
        public void NullCheck_01()
        {
            var src = @"
public class Helper
{
    public static bool Test1<T>(T value)
        where T : allows ref struct
    {
        return value == null;
    }

    public static bool Test2<T>(T value)
        where T : allows ref struct
    {
        return null == value;
    }

    public static bool Test3<T>(T value)
        where T : allows ref struct
    {
        return value != null;
    }

    public static bool Test4<T>(T value)
        where T : allows ref struct
    {
        return null != value;
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        System.Console.Write(Helper.Test1<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<Program>(new Program()));
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "False False True True True True False False False False True True" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper.Test1<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test2<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test3<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test4<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");
        }

        [Fact]
        public void NullCheck_02()
        {
            var src = @"
public class Helper
{
    public static bool Test1<T>(T value)
        where T : allows ref struct
    {
        return value is null;
    }

    public static bool Test3<T>(T value)
        where T : allows ref struct
    {
        return value is not null;
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        System.Console.Write(Helper.Test1<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(new Program()));
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "False True True False False True" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper.Test1<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test3<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");
        }

        [Fact]
        public void NullCheck_03()
        {
            var src = @"
public class Helper
{
    public static bool Test1<T>(T value, object o)
        where T : allows ref struct
    {
        return value == o;
    }

    public static bool Test2<T>(T value, object o)
        where T : allows ref struct
    {
        return o == value;
    }

    public static bool Test3<T>(T value, object o)
        where T : allows ref struct
    {
        return value != o;
    }

    public static bool Test4<T>(T value, object o)
        where T : allows ref struct
    {
        return o != value;
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (7,16): error CS0019: Operator '==' cannot be applied to operands of type 'T' and 'object'
                //         return value == o;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "value == o").WithArguments("==", "T", "object").WithLocation(7, 16),
                // (13,16): error CS0019: Operator '==' cannot be applied to operands of type 'object' and 'T'
                //         return o == value;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "o == value").WithArguments("==", "object", "T").WithLocation(13, 16),
                // (19,16): error CS0019: Operator '!=' cannot be applied to operands of type 'T' and 'object'
                //         return value != o;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "value != o").WithArguments("!=", "T", "object").WithLocation(19, 16),
                // (25,16): error CS0019: Operator '!=' cannot be applied to operands of type 'object' and 'T'
                //         return o != value;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "o != value").WithArguments("!=", "object", "T").WithLocation(25, 16)
                );
        }

        [Fact]
        public void NullCheck_04()
        {
            var src = @"
public class Helper
{
    const object o = null;

    public static bool Test1<T>(T value)
        where T : allows ref struct
    {
        return value == o;
    }

    public static bool Test2<T>(T value)
        where T : allows ref struct
    {
        return o == value;
    }

    public static bool Test3<T>(T value)
        where T : allows ref struct
    {
        return value != o;
    }

    public static bool Test4<T>(T value)
        where T : allows ref struct
    {
        return o != value;
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (9,16): error CS0019: Operator '==' cannot be applied to operands of type 'T' and 'object'
                //         return value == o;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "value == o").WithArguments("==", "T", "object").WithLocation(9, 16),
                // (15,16): error CS0019: Operator '==' cannot be applied to operands of type 'object' and 'T'
                //         return o == value;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "o == value").WithArguments("==", "object", "T").WithLocation(15, 16),
                // (21,16): error CS0019: Operator '!=' cannot be applied to operands of type 'T' and 'object'
                //         return value != o;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "value != o").WithArguments("!=", "T", "object").WithLocation(21, 16),
                // (27,16): error CS0019: Operator '!=' cannot be applied to operands of type 'object' and 'T'
                //         return o != value;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "o != value").WithArguments("!=", "object", "T").WithLocation(27, 16)
                );
        }

        [Fact]
        public void NullCheck_05()
        {
            var src = @"
public class Helper
{
    public static bool Test1<T>(T value)
        where T : allows ref struct
    {
        if (value == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Test2<T>(T value)
        where T : allows ref struct
    {
        if (null == value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Test3<T>(T value)
        where T : allows ref struct
    {
        if (value != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Test4<T>(T value)
        where T : allows ref struct
    {
        if (null != value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        System.Console.Write(Helper.Test1<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<Program>(new Program()));
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "False False True True True True False False False False True True" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper.Test1<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test2<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test3<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test4<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");
        }

        [Fact]
        public void NullCheck_06()
        {
            var src = @"
public class Helper
{
    public static bool Test1<T>(T value)
        where T : struct, allows ref struct
    {
        return value == null;
    }

    public static bool Test2<T>(T value)
        where T : struct, allows ref struct
    {
        return null == value;
    }

    public static bool Test3<T>(T value)
        where T : struct, allows ref struct
    {
        return value != null;
    }

    public static bool Test4<T>(T value)
        where T : struct, allows ref struct
    {
        return null != value;
    }
}

";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics);

            comp.VerifyDiagnostics(
                // (7,16): error CS0019: Operator '==' cannot be applied to operands of type 'T' and '<null>'
                //         return value == null;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "value == null").WithArguments("==", "T", "<null>").WithLocation(7, 16),
                // (13,16): error CS0019: Operator '==' cannot be applied to operands of type '<null>' and 'T'
                //         return null == value;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "null == value").WithArguments("==", "<null>", "T").WithLocation(13, 16),
                // (19,16): error CS0019: Operator '!=' cannot be applied to operands of type 'T' and '<null>'
                //         return value != null;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "value != null").WithArguments("!=", "T", "<null>").WithLocation(19, 16),
                // (25,16): error CS0019: Operator '!=' cannot be applied to operands of type '<null>' and 'T'
                //         return null != value;
                Diagnostic(ErrorCode.ERR_BadBinaryOps, "null != value").WithArguments("!=", "<null>", "T").WithLocation(25, 16)
                );
        }

        [Fact]
        public void NullCheck_07()
        {
            var src = @"
public class Helper
{
    public static bool Test1<T>(T value)
        where T : allows ref struct
    {
        return !(value != null);
    }

    public static bool Test2<T>(T value)
        where T : allows ref struct
    {
        return !(null != value);
    }

    public static bool Test3<T>(T value)
        where T : allows ref struct
    {
        return !(value == null);
    }

    public static bool Test4<T>(T value)
        where T : allows ref struct
    {
        return !(null == value);
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        System.Console.Write(Helper.Test1<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<Program>(new Program()));
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "False False True True True True False False False False True True" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper.Test1<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test2<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test3<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test4<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");
        }

        [Fact]
        public void NullCheck_08()
        {
            var src = @"
public class Helper
{
    public static bool Test1<T>(T value)
        where T : allows ref struct
    {
        return !(value is not null);
    }

    public static bool Test3<T>(T value)
        where T : allows ref struct
    {
        return !(value is null);
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        System.Console.Write(Helper.Test1<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(new Program()));
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "False True True False False True" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper.Test1<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test3<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");
        }

        [Fact]
        public void NullCheck_09()
        {
            var src = @"
public class Helper
{
    public static bool Test1<T>(T value)
        where T : allows ref struct
    {
        if (!(value != null))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Test2<T>(T value)
        where T : allows ref struct
    {
        if (!(null != value))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Test3<T>(T value)
        where T : allows ref struct
    {
        if (!(value == null))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Test4<T>(T value)
        where T : allows ref struct
    {
        if (!(null == value))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

ref struct S
{
}

class Program
{
    static void Main()
    {
        System.Console.Write(Helper.Test1<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<S>(new S()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<Program>(null));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test1<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test2<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test3<Program>(new Program()));
        System.Console.Write(' ');
        System.Console.Write(Helper.Test4<Program>(new Program()));
    }
}
";

            var comp = CreateCompilation(src, targetFramework: s_targetFrameworkSupportingByRefLikeGenerics, options: TestOptions.ReleaseExe);

            var verifier = CompileAndVerify(
                comp,
                expectedOutput: ExecutionConditionUtil.IsMonoOrCoreClr ? "False False True True True True False False False False True True" : null,
                verify: ExecutionConditionUtil.IsMonoOrCoreClr ? Verification.Passes : Verification.Skipped).VerifyDiagnostics();

            verifier.VerifyIL("Helper.Test1<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test2<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brtrue.s   IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test3<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");

            verifier.VerifyIL("Helper.Test4<T>(T)",
@"
{
  // Code size       12 (0xc)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""T""
  IL_0006:  brfalse.s  IL_000a
  IL_0008:  ldc.i4.1
  IL_0009:  ret
  IL_000a:  ldc.i4.0
  IL_000b:  ret
}
");
        }
    }
}
