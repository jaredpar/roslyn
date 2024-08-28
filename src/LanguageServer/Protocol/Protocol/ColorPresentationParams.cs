﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Roslyn.LanguageServer.Protocol
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Class representing the parameters sent for a textDocument/colorPresentation request.
    ///
    /// See the <see href="https://microsoft.github.io/language-server-protocol/specifications/specification-current/#colorPresentationParams">Language Server Protocol specification</see> for additional information.
    /// </summary>
    internal class ColorPresentationParams : ITextDocumentParams
    {
        /// <summary>
        /// Gets or sets the <see cref="TextDocumentIdentifier"/> to provide links for.
        /// </summary>
        [JsonPropertyName("textDocument")]
        public TextDocumentIdentifier TextDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        [JsonPropertyName("color")]
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the text range representing the color.
        /// </summary>
        [JsonPropertyName("range")]
        public Range Range { get; set; }
    }
}
