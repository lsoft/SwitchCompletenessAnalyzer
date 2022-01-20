using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = SwitchCompletenessAnalyzer.Test.CSharpAnalyzerVerifier<SwitchCompletenessAnalyzer.SwitchCompletenessAnalyzerAnalyzer>;

namespace SwitchCompletenessAnalyzer.Test
{
    [TestClass]
    public class SwitchCompletenessAnalyzerUnitTest
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
            {|#0:switch|}(myEnum)
            {
                case MyEnum.A:
                    break;
                case MyEnum.B:
                    break;
                case MyEnum.C:
                    break;
            }
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
            {|#0:switch|}(myEnum)
            {
                case MyEnum.A:
                    break;
                case MyEnum.C:
                    break;
            }
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
            {|#0:switch|}(myEnum)
            {
                case MyEnum.A:
                    break;
                case MyEnum.C:
                    break;
                default:
                    break;
            }
        }
    }
}
";

            var expected = VerifyCS.Diagnostic("SWITCHCOMPLETENESS001").WithLocation(0);
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task EmptyEnumTest0()
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
            {|#0:switch|}(myEnum)
            {
            }
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test, new DiagnosticResult[0]);
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
            {|#0:switch|}(myEnum)
            {
                default:
                    break;
            }
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
            {|#0:switch|}(myEnum)
            {
                case MyEnum.A:
                    break;
                case MyEnum.C:
                    break;
            }
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test, new DiagnosticResult[0]);
        }
    }
}
