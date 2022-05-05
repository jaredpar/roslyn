﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.EmbeddedLanguages.VirtualChars;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.EmbeddedLanguages.VirtualChars;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.EmbeddedLanguages.VirtualChars
{
    public class CSharpVirtualCharServiceTests
    {
        private const string _statementPrefix = "var v = ";

        private static IEnumerable<SyntaxToken>? GetStringTokens(string text, bool allowFailure)
        {
            var statement = _statementPrefix + text;
            var parsedStatement = (LocalDeclarationStatementSyntax)SyntaxFactory.ParseStatement(statement);
            var expression = parsedStatement.Declaration.Variables[0].Initializer!.Value;

            if (expression is LiteralExpressionSyntax literal)
            {
                return SpecializedCollections.SingletonEnumerable(literal.Token);
            }
            else if (expression is InterpolatedStringExpressionSyntax interpolation)
            {
                return interpolation.Contents.OfType<InterpolatedStringTextSyntax>().Select(t => t.TextToken);
            }
            else if (allowFailure)
            {
                return null;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static void Test(string stringText, string expected)
        {
            var tokens = GetStringTokens(stringText, allowFailure: false);
            Contract.ThrowIfNull(tokens);
            foreach (var token in tokens)
                Assert.False(token.ContainsDiagnostics);

            var virtualCharsArray = tokens.Select(CSharpVirtualCharService.Instance.TryConvertToVirtualChars);
            foreach (var virtualChars in virtualCharsArray)
            {
                foreach (var ch in virtualChars)
                {
                    for (var i = ch.Span.Start; i < ch.Span.End; i++)
                        Assert.Equal(ch, virtualChars.Find(i));
                }
            }

            var actual = string.Join("", virtualCharsArray.Select(ConvertToString));
            AssertEx.Equal(expected, actual);
        }

        private static void TestFailure(string stringText)
        {
            var tokens = GetStringTokens(stringText, allowFailure: true);
            if (tokens == null)
                return;

            foreach (var token in tokens)
            {
                var virtualChars = CSharpVirtualCharService.Instance.TryConvertToVirtualChars(token);
                Assert.True(virtualChars.IsDefault);
            }
        }

        private static string ConvertToString(VirtualCharSequence virtualChars)
        {
            using var _ = ArrayBuilder<string>.GetInstance(out var strings);
            foreach (var ch in virtualChars)
                strings.Add(ConvertToString(ch));

            return string.Join("", strings);
        }

        private static string ConvertToString(VirtualChar vc)
            => $"[{ConvertRuneToString(vc)},[{vc.Span.Start - _statementPrefix.Length},{vc.Span.End - _statementPrefix.Length}]]";

        private static string ConvertRuneToString(VirtualChar c)
            => PrintAsUnicodeEscape(c)
                ? c <= char.MaxValue ? $"'\\u{c.Value:X4}'" : $"'\\U{c.Value:X8}'"
                : $"'{(char)c.Value}'";

        private static bool PrintAsUnicodeEscape(VirtualChar c)
        {
            if (c < (char)127 && char.IsLetterOrDigit((char)c.Value))
                return false;

            if (c == '{' || c == '}')
                return false;

            return true;
        }

        [Fact]
        public void TestEmptyString()
            => Test("\"\"", "");

        [Fact]
        public void TestEmptyVerbatimString()
            => Test("@\"\"", "");

        [Fact]
        public void TestSimpleString()
            => Test("\"a\"", "['a',[1,2]]");

        [Fact]
        public void TestSimpleMultiCharString()
            => Test("\"abc\"", "['a',[1,2]]['b',[2,3]]['c',[3,4]]");

        [Fact]
        public void TestBracesInSimpleString()
            => Test("\"{{\"", "['{',[1,2]]['{',[2,3]]");

        [Fact]
        public void TestBracesInInterpolatedSimpleString()
            => Test("$\"{{\"", "['{',[2,4]]");

        [Fact]
        public void TestBracesInInterpolatedVerbatimSimpleString()
            => Test("$@\"{{\"", "['{',[3,5]]");

        [Fact]
        public void TestBracesInReverseInterpolatedVerbatimSimpleString()
            => Test("@$\"{{\"", "['{',[3,5]]");

        [Fact]
        public void TestEscapeInInterpolatedSimpleString()
            => Test("$\"\\n\"", @"['\u000A',[2,4]]");

        [Fact]
        public void TestEscapeInInterpolatedVerbatimSimpleString()
            => Test("$@\"\\n\"", @"['\u005C',[3,4]]['n',[4,5]]");

        [Fact]
        public void TestSimpleVerbatimString()
            => Test("@\"a\"", "['a',[2,3]]");

        [Fact]
        public void TestUnterminatedString()
            => TestFailure("\"");

        [Fact]
        public void TestUnterminatedVerbatimString()
            => TestFailure("@\"");

        [Fact]
        public void TestSimpleEscape()
            => Test(@"""a\ta""", "['a',[1,2]]['\\u0009',[2,4]]['a',[4,5]]");

        [Fact]
        public void TestMultipleSimpleEscape()
            => Test(@"""a\t\ta""", "['a',[1,2]]['\\u0009',[2,4]]['\\u0009',[4,6]]['a',[6,7]]");

        [Fact]
        public void TestNonEscapeInVerbatim()
            => Test(@"@""a\ta""", "['a',[2,3]]['\\u005C',[3,4]]['t',[4,5]]['a',[5,6]]");

        [Fact]
        public void TestInvalidHexEscape()
            => TestFailure(@"""\xZ""");

        [Fact]
        public void TestValidHex1Escape()
            => Test(@"""\xa""", @"['\u000A',[1,4]]");

        [Fact]
        public void TestValidHex1EscapeInInterpolatedString()
            => Test(@"$""\xa""", @"['\u000A',[2,5]]");

        [Fact]
        public void TestValidHex2Escape()
            => Test(@"""\xaa""", @"['\u00AA',[1,5]]");

        [Fact]
        public void TestValidHex3Escape()
            => Test(@"""\xaaa""", @"['\u0AAA',[1,6]]");

        [Fact]
        public void TestValidHex4Escape()
            => Test(@"""\xaaaa""", @"['\uAAAA',[1,7]]");

        [Fact]
        public void TestValidHex5Escape()
            => Test(@"""\xaaaaa""", @"['\uAAAA',[1,7]]['a',[7,8]]");

        [Fact]
        public void TestValidHex6Escape()
            => Test(@"""a\xaaaaa""", @"['a',[1,2]]['\uAAAA',[2,8]]['a',[8,9]]");

        [Fact]
        public void TestInvalidUnicodeEscape()
            => TestFailure(@"""\u000""");

        [Fact]
        public void TestValidUnicodeEscape1()
            => Test(@"""\u0000""", @"['\u0000',[1,7]]");

        [Fact]
        public void TestValidUnicodeEscape2()
            => Test(@"""a\u0000a""", @"['a',[1,2]]['\u0000',[2,8]]['a',[8,9]]");

        [Fact]
        public void TestInvalidLongUnicodeEscape1()
            => TestFailure(@"""\U0000""");

        [Fact]
        public void TestInvalidLongUnicodeEscape2()
            => TestFailure(@"""\U10000000""");

        [Fact]
        public void TestValidLongEscape1_InCharRange()
            => Test(@"""\U00000000""", @"['\u0000',[1,11]]");

        [Fact]
        public void TestValidLongEscape2_InCharRange()
            => Test(@"""\U0000ffff""", @"['\uFFFF',[1,11]]");

        [Fact]
        public void TestValidLongEscape3_InCharRange()
            => Test(@"""a\U00000000a""", @"['a',[1,2]]['\u0000',[2,12]]['a',[12,13]]");

        [Fact]
        public void TestValidLongEscape1_NotInCharRange()
            => Test(@"""\U00010000""", @"['\U00010000',[1,11]]");

        [Fact]
        public void TestValidLongEscape2_NotInCharRange()
            => Test(@"""\U0002A6A5𪚥""", @"['\U0002A6A5',[1,11]]['\U0002A6A5',[11,13]]");

        [Fact]
        public void TestSurrogate1()
            => Test(@"""😊""", @"['\U0001F60A',[1,3]]");

        [Fact]
        public void TestSurrogate2()
            => Test(@"""\U0001F60A""", @"['\U0001F60A',[1,11]]");

        [Fact]
        public void TestSurrogate3()
            => Test(@"""\ud83d\ude0a""", @"['\U0001F60A',[1,13]]");

        [Fact]
        public void TestHighSurrogate()
            => Test(@"""\ud83d""", @"['\uD83D',[1,7]]");

        [Fact]
        public void TestLowSurrogate()
            => Test(@"""\ude0a""", @"['\uDE0A',[1,7]]");

        [Fact]
        public void TestMixedSurrogate1()
            => Test("\"\ud83d\\ude0a\"", @"['\U0001F60A',[1,8]]");

        [Fact]
        public void TestMixedSurrogate2()
            => Test("\"\\ud83d\ude0a\"", @"['\U0001F60A',[1,8]]");

        [Fact]
        public void TestEscapedQuoteInVerbatimString()
            => Test("@\"a\"\"a\"", @"['a',[2,3]]['\u0022',[3,5]]['a',[5,6]]");

        [Fact]
        public void TestSingleLineRawString()
            => Test(@"""""""goo""""""", @"['g',[3,4]]['o',[4,5]]['o',[5,6]]");

        [Fact]
        public void TestMultiLineRawString1()
            => Test(@"""""""
    goo
    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]");

        [Fact]
        public void TestMultiLineRawString2()
            => Test(@"""""""
    goo
    bar
    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['b',[18,19]]['a',[19,20]]['r',[20,21]]");

        [Fact]
        public void TestMultiLineRawString3()
            => Test(@"""""""
    goo
        bar
    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['\u0020',[18,19]]['\u0020',[19,20]]['\u0020',[20,21]]['\u0020',[21,22]]['b',[22,23]]['a',[23,24]]['r',[24,25]]");

        [Fact]
        public void TestMultiLineRawString4()
            => Test(@"""""""
        goo
    bar
    """"""", @"['\u0020',[9,10]]['\u0020',[10,11]]['\u0020',[11,12]]['\u0020',[12,13]]['g',[13,14]]['o',[14,15]]['o',[15,16]]['\u000D',[16,17]]['\u000A',[17,18]]['b',[22,23]]['a',[23,24]]['r',[24,25]]");

        [Fact]
        public void TestMultiLineRawString5()
            => Test(@"""""""
    goo

    bar
    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['\u000D',[14,15]]['\u000A',[15,16]]['b',[20,21]]['a',[21,22]]['r',[22,23]]");

        [Fact]
        public void TestMultiLineRawString6()
            => Test(@"""""""
    goo
    
    bar
    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['\u000D',[18,19]]['\u000A',[19,20]]['b',[24,25]]['a',[25,26]]['r',[26,27]]");

        [Fact]
        public void TestMultiLineRawString7()
            => Test(@"""""""
    goo
      
    bar
    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['\u0020',[18,19]]['\u0020',[19,20]]['\u000D',[20,21]]['\u000A',[21,22]]['b',[26,27]]['a',[27,28]]['r',[28,29]]");

        [Fact]
        public void TestMultiLineRawString8()
            => Test(@"""""""
    goo

    bar

    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['\u000D',[14,15]]['\u000A',[15,16]]['b',[20,21]]['a',[21,22]]['r',[22,23]]['\u000D',[23,24]]['\u000A',[24,25]]");

        [Fact]
        public void TestMultiLineRawString9()
            => Test(@"""""""
    goo

    bar
    
    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['\u000D',[14,15]]['\u000A',[15,16]]['b',[20,21]]['a',[21,22]]['r',[22,23]]['\u000D',[23,24]]['\u000A',[24,25]]");

        [Fact]
        public void TestMultiLineRawString10()
            => Test(@"""""""
    goo

    bar
      
    """"""", @"['g',[9,10]]['o',[10,11]]['o',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['\u000D',[14,15]]['\u000A',[15,16]]['b',[20,21]]['a',[21,22]]['r',[22,23]]['\u000D',[23,24]]['\u000A',[24,25]]['\u0020',[29,30]]['\u0020',[30,31]]");

        [Fact]
        public void TestMultiLineRawString11()
            => Test(@"""""""
goo

bar
""""""", @"['g',[5,6]]['o',[6,7]]['o',[7,8]]['\u000D',[8,9]]['\u000A',[9,10]]['\u000D',[10,11]]['\u000A',[11,12]]['b',[12,13]]['a',[13,14]]['r',[14,15]]");

        [Fact]
        public void TestMultiLineRawString12()
            => Test(@"""""""
  goo

  bar
""""""", @"['\u0020',[5,6]]['\u0020',[6,7]]['g',[7,8]]['o',[8,9]]['o',[9,10]]['\u000D',[10,11]]['\u000A',[11,12]]['\u000D',[12,13]]['\u000A',[13,14]]['\u0020',[14,15]]['\u0020',[15,16]]['b',[16,17]]['a',[17,18]]['r',[18,19]]");

        [Fact]
        public void TestMultiLineRawString13()
            => Test(@"""""""

    goo

    bar
    """"""", @"['\u000D',[5,6]]['\u000A',[6,7]]['g',[11,12]]['o',[12,13]]['o',[13,14]]['\u000D',[14,15]]['\u000A',[15,16]]['\u000D',[16,17]]['\u000A',[17,18]]['b',[22,23]]['a',[23,24]]['r',[24,25]]");

        [Fact]
        public void TestMultiLineRawString14()
            => Test(@"""""""
    
    goo

    bar
    """"""", @"['\u000D',[9,10]]['\u000A',[10,11]]['g',[15,16]]['o',[16,17]]['o',[17,18]]['\u000D',[18,19]]['\u000A',[19,20]]['\u000D',[20,21]]['\u000A',[21,22]]['b',[26,27]]['a',[27,28]]['r',[28,29]]");

        [Fact]
        public void TestMultiLineRawString15()
            => Test(@"""""""
      
    goo

    bar
    """"""", @"['\u0020',[9,10]]['\u0020',[10,11]]['\u000D',[11,12]]['\u000A',[12,13]]['g',[17,18]]['o',[18,19]]['o',[19,20]]['\u000D',[20,21]]['\u000A',[21,22]]['\u000D',[22,23]]['\u000A',[23,24]]['b',[28,29]]['a',[29,30]]['r',[30,31]]");

        [Fact]
        public void TestMultiLineRawString16()
            => Test(@"""""""

    """"""", @"");

        [Fact]
        public void TestMultiLineRawString17()
            => Test(@"""""""
    
    """"""", @"");

        [Fact]
        public void TestMultiLineRawString18()
            => Test(@"""""""
      
    """"""", @"['\u0020',[9,10]]['\u0020',[10,11]]");

        [Fact]
        public void TestMultiLineRawString19()
            => Test(@"""""""  
    goo
    """"""", @"['g',[11,12]]['o',[12,13]]['o',[13,14]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString1()
            => Test(@"$""""""goo""""""", @"['g',[4,5]]['o',[5,6]]['o',[6,7]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString2()
            => Test(@"$""""""goo{0}""""""", @"['g',[4,5]]['o',[5,6]]['o',[6,7]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString3()
            => Test(@"$""""""{0}goo""""""", @"['g',[7,8]]['o',[8,9]]['o',[9,10]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString4()
            => Test(@"$""""""{0}goo{1}""""""", @"['g',[7,8]]['o',[8,9]]['o',[9,10]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString5()
            => Test(@"$""""""goo{0}{1}""""""", @"['g',[4,5]]['o',[5,6]]['o',[6,7]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString6()
            => Test(@"$""""""{0}{1}goo""""""", @"['g',[10,11]]['o',[11,12]]['o',[12,13]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString7()
            => Test(@"$""""""goo{0}bar{1}""""""", @"['g',[4,5]]['o',[5,6]]['o',[6,7]]['b',[10,11]]['a',[11,12]]['r',[12,13]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString8()
            => Test(@"$""""""{0}goo{1}bar""""""", @"['g',[7,8]]['o',[8,9]]['o',[9,10]]['b',[13,14]]['a',[14,15]]['r',[15,16]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString9()
            => Test(@"$""""""goo{0}{1}bar""""""", @"['g',[4,5]]['o',[5,6]]['o',[6,7]]['b',[13,14]]['a',[14,15]]['r',[15,16]]");

        [Fact]
        public void TestSingleLineInterpolatedRawString10()
            => Test(@"$""""""goo{0}bar{1}baz""""""", @"['g',[4,5]]['o',[5,6]]['o',[6,7]]['b',[10,11]]['a',[11,12]]['r',[12,13]]['b',[16,17]]['a',[17,18]]['z',[18,19]]");
    }
}
