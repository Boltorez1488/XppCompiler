using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flee.PublicTypes;

namespace XppCompiler {
    class Function {
        public static ExpressionContext Context;

        public static int MathExpression(string exp) {
            if (!Regex.Match(exp, "[+,-,*,/]").Success)
                return ToInt(exp);

            if (Context == null) {
                Context = new ExpressionContext();
                Context.Imports.AddType(typeof(Math));
            }

            var eDynamic = Context.CompileDynamic(exp);
            return (int)eDynamic.Evaluate();
        }

        public static int ToInt(string val) {
            return val.StartsWith("0x") ? Convert.ToInt32(val, 16) : int.Parse(val);
        }

        public static string GenerateSpace(int size) {
            string result = "";
            for (int i = 0; i < size; i++)
                result += ' ';
            return result;
        }
    }
}
