using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = SwitchCompletenessAnalyzer.Test.CSharpAnalyzerVerifier<SwitchCompletenessAnalyzer.SwitchCompletenessAnalyzerAnalyzer>;

namespace SwitchCompletenessAnalyzer.Test
{
    [TestClass]
    public class SwitchExpressionFixture
    {
        /// <summary>
        /// No diagnostics expected to show up
        /// </summary>
        [TestMethod]
        public async Task EmptyTest0()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        /// <summary>
        /// No diagnostics expected to show up
        /// </summary>
        [TestMethod]
        public async Task CompleteTest0()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MyNamespace
{
    enum MyEnum { A, B, C }

    class MyClass
    {
        public void MyMethod(MyEnum myEnum)
        {
            var a = myEnum {|#0:switch|}
            {
                MyEnum.A => 1,
                MyEnum.B => 2,
                MyEnum.C => 3
            };
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test, new DiagnosticResult[0]);
        }

        [TestMethod]
        public async Task NotCompleteEnumTest0()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MyNamespace
{
    enum MyEnum { A, B, C }

    class MyClass
    {
        public void MyMethod(MyEnum myEnum)
        {
            var a = myEnum {|#0:switch|}
            {
                MyEnum.A => 1,
                MyEnum.C => 3
            };
        }
    }
}
";

            var expected = VerifyCS.Diagnostic("SWITCHCOMPLETENESS001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task NotCompleteEnumWithDefaultNodeTest0()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MyNamespace
{
    enum MyEnum { A, B, C }

    class MyClass
    {
        public void MyMethod(MyEnum myEnum)
        {
            var a = myEnum {|#0:switch|}
            {
                MyEnum.A => 1,
                MyEnum.C => 3,
                _ => 0
            };
        }
    }
}
";

            var expected = VerifyCS.Diagnostic("SWITCHCOMPLETENESS001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }


        [TestMethod]
        public async Task EmptyEnumWithDefaultNodeTest0()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MyNamespace
{
    enum MyEnum { }

    class MyClass
    {
        public void MyMethod(MyEnum myEnum)
        {
            var a = myEnum {|#0:switch|}
            {
                _ => 0
            };
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test, new DiagnosticResult[0]);
        }

        [TestMethod]
        public async Task FlagsTest0()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MyNamespace
{
    [Flags]
    enum MyEnum { A, B, C }

    class MyClass
    {
        public void MyMethod(MyEnum myEnum)
        {
            var a = myEnum {|#0:switch|}
            {
                MyEnum.A => 1,
                MyEnum.C => 3
            };
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test, new DiagnosticResult[0]);
        }
    }
}
