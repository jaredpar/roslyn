// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using HelixUtil;

var options = Options.Parse(args);
if (options is null)
{
    return 1;
}

return 0;
