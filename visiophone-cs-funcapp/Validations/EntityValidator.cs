using System;
using System.Collections.Generic;

namespace vp.validation
{
    public class EntityValidator : Dictionary<string, Func<string, bool>>
    {
    }
}
