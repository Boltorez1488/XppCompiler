using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XppCompiler {
    class Struct {
        public object Parent;

        public string Name;
        public string Comment;
        public string Var;
        //public int Size = 4;

        public int MaxStrLen;
        public List<Var> Vars = new List<Var>();

        public Struct(string name, object parent) {
            Name = name;
            Parent = parent;
        }

        public void Push(Var var) {
            Vars.Add(var);
        }

        public object GetObj(string name) {
            foreach (var v in Vars) {
                if (v.Type == VarType.Var)
                    continue;
                if (v.Type == VarType.Struct && v.Struct.Name == name)
                    return v.Struct;
                if (v.Type == VarType.Enum && v.Enum.Name == name)
                    return v.Enum;
            }
            return null;
        }

        public string GetPath(string name = "") {
            if (Parent != null) {
                if(Parent is Namespace space)
                    return space.GetPath(Name + "." + name);
                return (Parent as Struct)?.GetPath(Name + "." + name);
            }
                
            return String.IsNullOrEmpty(Name) ? name : Name + "." + name;
        }

        public void CalcOffsets() {
            var globalOffset = 0;
            foreach (var var in Vars) {
                var strSize = var.GetEndSize();
                if (strSize > MaxStrLen)
                    MaxStrLen = strSize;

                if (var.IsRelative) {
                    var.Offset += globalOffset;
                    var.IsRelative = false;
                }
                if (var.Offset == 0x0) {
                    var.Offset = globalOffset;
                }
                globalOffset = var.Offset + var.GetFullSize();
            }
            Vars = Vars.OrderBy(x => x.Offset).ToList();
        }

        public void ConvertVars(List<string> build, int tabs = 0) {
            var counter = 0;
            var lastVar = Vars[0];
            lastVar.Convert(build, this, null, ref counter, tabs);

            foreach (var var in Vars.Skip(1)) {
                var.Convert(build, this, lastVar, ref counter, tabs);
                lastVar = var;
            }
        }

        public void Convert(List<string> build, int tabs = 0) {
            var tab = Namespace.TabsConvert(tabs);
            if(!String.IsNullOrEmpty(Comment))
                build.Add(tab + $"/* {Comment} */");
            build.Add(tab + $"struct {Name} " + "{");
            if (Vars.Count != 0) {
                ConvertVars(build, tabs + 1);
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
