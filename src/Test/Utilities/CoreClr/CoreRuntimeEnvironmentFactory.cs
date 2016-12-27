// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Test.Utilities;
using static Roslyn.Test.Utilities.RuntimeUtilities; 

namespace Roslyn.Test.Utilities
{
    public class CoreRuntimeEnvironmentFactory : IRuntimeEnvironmentFactory
    {
        public IRuntimeEnvironment Create(IEnumerable<ModuleData> additionalDependencies)
        {
            return new CoreRuntimeEnvironment(additionalDependencies);
        }
    }
}
