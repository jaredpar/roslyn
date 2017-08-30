using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Roslyn.Test.Utilities
{
    public sealed class FixBuild
    {
        public IncrementalHash GetIncrementalHash() => throw new NotSupportedException();
    }
}
