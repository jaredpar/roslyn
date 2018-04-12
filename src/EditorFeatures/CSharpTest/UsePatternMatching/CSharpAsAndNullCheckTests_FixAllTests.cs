﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.UsePatternMatching
{
    public partial class CSharpAsAndNullCheckTests
    {
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsInlineTypeCheck)]
        public async Task FixAllInDocument1()
        {
            await TestInRegularAndScriptAsync(
@"class C
{
    int M()
    {
        string a;
        {|FixAllInDocument:var|} x = o as string;
        if (x != null)
        {
        }

        var y = o as string;
        if (y != null)
        {
        }

        if ((a = o as string) == null)
        {
        }

        var c = o as string;
        var d = c != null ? 1 : 0;

        var e = o as string;
        return e != null ? 1 : 0;
    }
}",
@"class C
{
    int M()
    {
        if (o is string x)
        {
        }

        if (o is string y)
        {
        }

        if (!(o is string a))
        {
        }

        var d = o is string c ? 1 : 0;

        return o is string e ? 1 : 0;
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsInlineTypeCheck)]
        public async Task FixAllInDocument2()
        {
            await TestInRegularAndScriptAsync(
@"class Symbol
{
    public ContainingSymbol { get; }

    void M(object o, bool b0, bool b1)
    {
        {|FixAllInDocument:var|} symbol = o as Symbol;
        if (symbol != null)
        {
            while ((object)symbol != null && b1)
            {
                symbol = symbol.ContainingSymbol as Symbol;
            }

            if ((object)symbol == null || b2)
            {
                throw null;
            }

            var use = symbol;
        }
    }
}",
@"class Symbol
{
    public ContainingSymbol { get; }

    void M(object o, bool b0, bool b1)
    {
    if (o is Symbol symbol)
    {
        while ((object)symbol != null && b1)
        {
            symbol = symbol.ContainingSymbol as Symbol;
        }

        if ((object)symbol == null || b2)
        {
            throw null;
        }

        var use = symbol;
    }
}
}");
        }
    }
}
