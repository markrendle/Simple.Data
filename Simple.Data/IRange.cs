﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data
{
    public interface IRange
    {
        object Start { get; }
        object End { get; }
        IEnumerable<object> AsEnumerable();
    }
}
