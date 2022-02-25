using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;

namespace SwitchCompletenessAnalyzer.Console
{
    class Program
    {
        private static Silent1Enum _silent1 = Silent1Enum.S0;
        private static Silent2Enum _silent2 = Silent2Enum.S0;

        static void Main(string[] args)
        {
        }

        static void SwitchExpression(string[] args)
        {
            var subject = SubjectEnum.S0;
            var a = subject switch
            {
                SubjectEnum.S0 => "0",
                SubjectEnum.S2 => "2",
                _ => null,
            };

            var silent1 = Silent1Enum.S0;
            var b1 = silent1 switch
            {
                Silent1Enum.S0 => "0",
                Silent1Enum.S2 => "2",
                _ => null,
            };
            var b2 = silent1 switch
            {
                Silent1Enum.S0 => "0",
                Silent1Enum.S2 => "2",
                _ => null,
            };

            var silent2 = Silent2Enum.S0;
            var c1 = silent2 switch
            {
                Silent2Enum.S0 => "0",
                Silent2Enum.S2 => "2",
                _ => null,
            };
            var c2 = silent2 switch
            {
                Silent2Enum.S0 => "0",
                Silent2Enum.S2 => "2",
                _ => null,
            };
        }

        static void SwitchStatement(string[] args)
        {
            var subject = SubjectEnum.S0;
            switch (subject)
            {
                case SubjectEnum.S0:
                    break;
                case SubjectEnum.S2:
                    break;
                default:
                    break;
            }

            var silent1 = Silent1Enum.S0;
            switch (silent1)
            {
                case Silent1Enum.S0:
                    break;
                case Silent1Enum.S2:
                    break;
                default:
                    break;
            }
            switch (_silent1)
            {
                case Silent1Enum.S0:
                    break;
                case Silent1Enum.S2:
                    break;
                default:
                    break;
            }

            var silent2 = Silent2Enum.S0;
            switch (silent2)
            {
                case Silent2Enum.S0:
                    break;
                case Silent2Enum.S2:
                    break;
                default:
                    break;
            }
            switch (_silent2)
            {
                case Silent2Enum.S0:
                    break;
                case Silent2Enum.S2:
                    break;
                default:
                    break;
            }
        }
    }


    //public class MyGenericClass<T1, T2>
    //{
    public enum SubjectEnum { S0, S1, S2 };
    public enum Silent1Enum { S0, S1, S2 };
    public enum Silent2Enum { S0, S1, S2 };
    //}
}
