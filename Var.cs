using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XppCompiler {
    enum VarType {
        Var,
        Struct,
        Enum
    }
    class Var {
        public VarType Type;
        public string Comment;

        public int Offset;
        public bool IsRelative;
        public string Typename;
        public bool IsArray;
        public int ArraySize = 1;
        public int Size;

        public Struct Struct = null;
        public Enum Enum = null;

        public Var(VarType type) {
            Type = type;
        }

        public void CalcSize() {
            if (Typename == null) {
                Size = 4;
                return;
            }
            if (Typename.IndexOf("*", StringComparison.Ordinal) != -1) {
                Size = 4;
            } else if (Typename.StartsWith("double")) {
                Size = 8;
            } else if (Typename.StartsWith("short")) {
                Size = 2;
            } else if (Typename.StartsWith("bool") || Typename.StartsWith("char")) {
                Size = 1;
            } else {
                Size = 4;
            }
        }

        // [ Start Loader-Block ]
        public void LoadOffset(string offset) {
            if (String.IsNullOrEmpty(offset))
                return;
            if (offset.StartsWith("+")) {
                offset = offset.Substring(1);
                Offset = Function.MathExpression(offset);
                IsRelative = true;
            } else {
                Offset = Function.MathExpression(offset);
            }
        }

        public void LoadTypename(string typename) {
            if (String.IsNullOrEmpty(typename))
                return;
            Typename = typename;
        }

        public void LoadSize(string size) {
            if (!String.IsNullOrEmpty(size)) {
                Size = Function.MathExpression(size);
            } else {
                CalcSize();
            }
        }

        public void LoadArray(string array) {
            if (String.IsNullOrEmpty(array))
                return;
            ArraySize = Function.MathExpression(array);
            IsArray = true;
        }
        // [ End Loader-Block ]

        public int GetFullSize() {
            return ArraySize * Size;
        }

        public int GetEndSize() {
            string build = "";
            if (Struct != null) {
                if (IsArray) {
                    build += "}" + $"{Struct.Var}[0x{ArraySize:X}];";
                } else {
                    build += "}" + $"{Struct.Var};";
                }
            } else {
                if (IsArray) {
                    build += $"{Typename}[0x{ArraySize:X}];";
                } else {
                    build += $"{Typename};";
                }
            }
            
            return build.Length;
        }

        /*
         * [ Convertation Block ]
         */
        private void ConvertStruct(List<string> build, Struct caller, Var lastVar, ref int counter, int tabs = 0) {
            var tab = Namespace.TabsConvert(tabs);
            if (lastVar == null) {
                // FirstValue
                if (Offset != 0x0) {
                    build.Add(tab + $"{Constants.SkipType} {Constants.SkipName}0[0x{Offset:X}];");
                    if (IsArray) {
                        Struct.Convert(build, tabs);
                        var str = build[build.Count - 1];
                        build[build.Count - 1] = str.Remove(str.Length - 1) + $"{Typename}[0x{ArraySize:X}];";
                    } else {
                        Struct.Convert(build, tabs);
                    }
                } else {
                    if (IsArray) {
                        Struct.Convert(build, tabs);
                        var str = build[build.Count - 1];
                        build[build.Count - 1] = str.Remove(str.Length - 1) + $"{Typename}[0x{ArraySize:X}];";
                    } else {
                        Struct.Convert(build, tabs);
                    }
                    counter++;
                }
            } else {
                // NextValue
                var sub = Offset - lastVar.Offset - lastVar.GetFullSize();
                if (sub != 0x0) {
                    var strSize = build[build.Count - 1].Length - tab.Length;
                    var strSub = caller.MaxStrLen - strSize;
                    if (!String.IsNullOrEmpty(lastVar.Comment)) {
                        build.Add(tab + $"{Constants.SkipType} {Constants.SkipName}{counter}[0x{sub:X}];");
                    } else {
                        build[build.Count - 1] += (Function.GenerateSpace(strSub + Constants.SpaceAlign) + $"{Constants.SkipType} {Constants.SkipName}{counter}[0x{sub:X}];");
                    }
                    counter++;
                }

                if (IsArray) {
                    Struct.Convert(build, tabs);
                    var str = build[build.Count - 1];
                    build[build.Count - 1] = str.Remove(str.Length - 1) + $"{Typename}[0x{ArraySize:X}];";
                } else {
                    Struct.Convert(build, tabs);
                }
            }
        }

        private void ConvertEnum(List<string> build, Struct caller, Var lastVar, ref int counter, int tabs = 0) {
            var tab = Namespace.TabsConvert(tabs);
            if (lastVar == null) {
                // FirstValue
                if (Offset != 0x0) {
                    build.Add(tab + $"{Constants.SkipType} {Constants.SkipName}0[0x{Offset:X}];");
                    if (IsArray) {
                        Enum.Convert(build, tabs);
                        var str = build[build.Count - 1];
                        build[build.Count - 1] = str.Remove(str.Length - 1) + $"{Typename}[0x{ArraySize:X}];";
                    } else {
                        Enum.Convert(build, tabs);
                    }
                } else {
                    if (IsArray) {
                        Enum.Convert(build, tabs);
                        var str = build[build.Count - 1];
                        build[build.Count - 1] = str.Remove(str.Length - 1) + $"{Typename}[0x{ArraySize:X}];";
                    } else {
                        Enum.Convert(build, tabs);
                    }
                    counter++;
                }
            } else {
                // NextValue
                var sub = Offset - lastVar.Offset - lastVar.GetFullSize();
                if (sub != 0x0) {
                    var strSize = build[build.Count - 1].Length - tab.Length;
                    var strSub = caller.MaxStrLen - strSize;
                    if (!String.IsNullOrEmpty(lastVar.Comment)) {
                        build.Add(tab + $"{Constants.SkipType} {Constants.SkipName}{counter}[0x{sub:X}];");
                    } else {
                        build[build.Count - 1] += (Function.GenerateSpace(strSub + Constants.SpaceAlign) + $"{Constants.SkipType} {Constants.SkipName}{counter}[0x{sub:X}];");
                    }
                    counter++;
                }

                if (IsArray) {
                    Enum.Convert(build, tabs);
                    var str = build[build.Count - 1];
                    build[build.Count - 1] = str.Remove(str.Length - 1) + $"{Typename}[0x{ArraySize:X}];";
                } else {
                    Enum.Convert(build, tabs);
                }
            }
        }

        private void ConvertVar(List<string> build, Struct caller, Var lastVar, ref int counter, int tabs = 0) {
            var tab = Namespace.TabsConvert(tabs);
            if (lastVar == null) {
                if (Offset == 0x0) {
                    if (!String.IsNullOrEmpty(Comment))
                        build.Add(tab + $"/* {Comment} */");
                    if (IsArray) {
                        build.Add(tab + $"{Typename}[0x{ArraySize:X}];");
                    } else {
                        build.Add(tab + $"{Typename};");
                    }
                } else {
                    build.Add(tab + $"{Constants.SkipType} {Constants.SkipName}0[0x{Offset:X}];");
                    if (IsArray) {
                        if (!String.IsNullOrEmpty(Comment)) {
                            var strSub = caller.MaxStrLen - GetEndSize();
                            build.Add(tab + $"{Typename}[0x{ArraySize:X}];" + Function.GenerateSpace(strSub + Constants.SpaceAlign) + $"/* {Comment} */");
                        } else {
                            build.Add(tab + $"{Typename}[0x{ArraySize:X}];");
                        }
                    } else {
                        if (!String.IsNullOrEmpty(Comment)) {
                            var strSub = caller.MaxStrLen - GetEndSize();
                            build.Add(tab + $"{Typename};" + Function.GenerateSpace(strSub + Constants.SpaceAlign) + $"/* {Comment} */");
                        } else {
                            build.Add(tab + $"{Typename};");
                        }
                    }
                    counter++;
                }
            } else {
                var sub = Offset - lastVar.Offset - lastVar.GetFullSize();
                if (sub < 0)
                    throw new Exception("Offset error, typename: " + Typename);
                if (sub != 0x0) {
                    var strSize = build[build.Count - 1].Length - tab.Length;
                    var strSub = caller.MaxStrLen - strSize;
                    if (!String.IsNullOrEmpty(lastVar.Comment)) {
                        build.Add(tab + $"{Constants.SkipType} {Constants.SkipName}{counter}[0x{sub:X}];");
                    } else {
                        build[build.Count - 1] += (Function.GenerateSpace(strSub + Constants.SpaceAlign) + $"{Constants.SkipType} {Constants.SkipName}{counter}[0x{sub:X}];");
                    }
                    counter++;
                }

                if (IsArray) {
                    if (!String.IsNullOrEmpty(Comment)) {
                        var strSub = caller.MaxStrLen - GetEndSize();
                        build.Add(tab + $"{Typename}[0x{ArraySize:X}];" + Function.GenerateSpace(strSub + Constants.SpaceAlign) + $"/* {Comment} */");
                    } else {
                        build.Add(tab + $"{Typename}[0x{ArraySize:X}];");
                    }
                } else {
                    if (!String.IsNullOrEmpty(Comment)) {
                        var strSub = caller.MaxStrLen - GetEndSize();
                        build.Add(tab + $"{Typename};" + Function.GenerateSpace(strSub + Constants.SpaceAlign) + $"/* {Comment} */");
                    } else {
                        build.Add(tab + $"{Typename};");
                    }
                }
            }
        }

        public void Convert(List<string> build, Struct caller, Var lastVar, ref int counter, int tabs = 0) {
            switch (Type) {
                case VarType.Var:
                    ConvertVar(build, caller, lastVar, ref counter, tabs);
                    return;
                case VarType.Struct:
                    ConvertStruct(build, caller, lastVar, ref counter, tabs);
                    return;
                case VarType.Enum:
                    ConvertEnum(build, caller, lastVar, ref counter, tabs);
                    return;
            }
        }
    }
}

