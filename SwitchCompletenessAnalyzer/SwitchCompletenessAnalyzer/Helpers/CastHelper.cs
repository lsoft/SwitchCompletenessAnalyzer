using System;
using System.Collections.Generic;
using System.Text;

namespace SwitchCompletenessAnalyzer.Helpers
{
    internal static class CastHelper
    {
        public static long ToInt64(this object o)
        {
            return o is ulong ? unchecked((long)(ulong)o) : System.Convert.ToInt64(o);
        }
    }
}
