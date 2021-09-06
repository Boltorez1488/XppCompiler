using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XppCompiler {
    class Enum {
        public string Name;
        public string Comment;
        public string Var;

        public class EnumVal {
            public string Name;
            public int Value;
            public string Comment;
        }
        public List<EnumVal> Vals = new List<EnumVal>();

        public Enum(string name) {
            Name = name;
        }

        public void Push(string name, int value, string comment = null) {
            Vals.Add(new EnumVal{Name = name, Value = value, Comment = comment});
        }

        public void ConvertVals(List<string> build, int tabs = 0) {
            var tab = Namespace.TabsConvert(tabs);
            if (Vals.Count == 0)
                throw new Exception($"Enum({Name}) is empty!");
            foreach (var val in Vals) {
                if(!String.IsNullOrEmpty(val.Comment))
                    build.Add(tab + $"/* {val.Comment} */");
                build.Add(tab + $"{val.Name} = 0x{val.Value:X},");
            }

            build[build.Count - 1] = build[build.Count - 1].TrimEnd(',');
        }

        public void Convert(List<string> build, int tabs = 0) {
            var tab = Namespace.TabsConvert(tabs);
            if (!String.IsNullOrEmpty(Comment))
                build.Add(tab + $"/* {Comment} */");
            build.Add(tab + $"enum {Name} " + "{");
            if (Vals.Count != 0) {
                ConvertVals(build, tabs + 1);
                if (!String.IsNullOrEmpty(Var)) {
                    build.Add(tab + "} " + Var + ";");
                } else {
                    build.Add(tab + "};");
                }
            } else {
                if (!String.IsNullOrEmpty(Var)) {
                    build[build.Count - 1] += "} " + Var + ";";
                } else {
                    build[build.Count - 1] += "};";
                }
            }
        }

    }
}
