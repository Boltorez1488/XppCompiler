using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Flee.PublicTypes;

namespace XppCompiler {
    class Reader {
        private List<string> ptrs = new List<string>();
        public string FDName;

        public Reader(string fdname) {
            FDName = fdname;
        }

        private void LoadFile(string fname) {
            XDocument xDoc = XDocument.Load(fname);

            var root = xDoc.Root;
            if (root == null) return;
            if (root.Name == "namespace") {
                ReadNamespace(root);
            } else if (root.Name.LocalName == "struct") {
                Registry.Root.Push(ReadStruct(root, Registry.Root));
            }

            PtrChecker();
            ptrs.Clear();
        }

        public void Load() {
            if (Path.HasExtension(FDName)) {
                LoadFile(FDName);
            } else {
                var files = Directory.GetFiles(FDName, "*.xpp", SearchOption.AllDirectories);
                foreach (var file in files) {
                    LoadFile(file);
                }
            }
        }

        public void PtrChecker() {
            foreach (var ptr in ptrs) {
                if (!Registry.IsPathValid(ptr.Replace("::", "."), false, true, true))
                    throw new Exception($"Path({ptr}) is not found");
            }
        }

        public Namespace ReadNamespace(XElement elem, string path = "") {
            var name = elem.Attribute("name")?.Value;
            
            Namespace root, space;
            if (String.IsNullOrEmpty(name)) {
                root = space = Registry.Root;
            } else {
                root = Registry.Root;
                space = Registry.PushNamespace(String.IsNullOrEmpty(path) ? name : path + "." + name);
            }

            var lastComment = "";
            foreach (var xNode in elem.Nodes().Where(x => x.NodeType != XmlNodeType.Comment)) {
                var e = (XElement) xNode;
                if (e.Name.LocalName == "struct") {
                    var st = ReadStruct(e, space);
                    if (lastComment != "")
                        st.Comment = lastComment;
                    space.Push(st);
                } else if (e.Name.LocalName == "namespace") {
                    ReadNamespace(e, String.IsNullOrEmpty(path) ? name : path + "." + name);
                } else if (e.Name.LocalName == "enum") {
                    var en = ReadEnum(e);
                    if (lastComment != "")
                        en.Comment = lastComment;
                    space.Push(en);
                } else if (e.Name.LocalName == "comment") {
                    lastComment = e.Value;
                }
                if (e.Name.LocalName != "comment") {
                    lastComment = "";
                }
            }

            return root;
        }

        public Enum ReadEnum(XElement elem) {
            var root = new Enum(elem.Attribute("name")?.Value);
            var enumVar = elem.Attribute("var")?.Value;
            if (enumVar != null)
                root.Var = enumVar;

            foreach (var xNode in elem.Nodes().Where(x => x.NodeType != XmlNodeType.Comment)) {
                var e = (XElement)xNode;
                if (e.Name.LocalName == "val") {
                    var name = e.Attribute("name")?.Value;
                    if(String.IsNullOrEmpty(name))
                        throw new Exception($"Ptr[{elem}] name is not empty: [{GetNodePath(elem)}]");

                    var value = e.Value;
                    if(String.IsNullOrEmpty(value))
                        throw new Exception($"Ptr[{elem}] value is not empty: [{GetNodePath(elem)}]");

                    var comment = e.Attribute("comment")?.Value;
                    root.Push(name, Function.MathExpression(value), comment);
                }
            }
            return root;
        }

        public Var ReadVarEnum(XElement elem) {
            Var var = new Var(VarType.Enum);
            var v = elem.Attribute("offset")?.Value;
            var.LoadOffset(v);

            bool sizeReaded = false;
            var en = ReadEnum(elem);
            if (!String.IsNullOrEmpty(en.Var)) {
                var ptrPos = en.Var.IndexOf("*", StringComparison.Ordinal);
                if (ptrPos == -1) {
                    var.Size = 4;
                    sizeReaded = true;
                }
            } else {
                var.Size = 0;
                sizeReaded = true;
            }

            if (!sizeReaded) {
                var size = elem.Attribute("size")?.Value;
                var.LoadSize(size);
            }

            var array = elem.Attribute("array")?.Value;
            var.LoadArray(array);

            var.Enum = en;
            return var;
        }

        public Struct ReadStruct(XElement elem, object parent) {
            var root = new Struct(elem.Attribute("name")?.Value, parent);
            var structVar = elem.Attribute("var")?.Value;
            if (structVar != null)
                root.Var = structVar;

            string lastComment = "";
            foreach (var xNode in elem.Nodes().Where(x => x.NodeType != XmlNodeType.Comment)) {
                var e = (XElement)xNode;
                if (e.Name.LocalName == "var") {
                    var var = ReadVar(e);
                    if (lastComment != "")
                        var.Comment = lastComment;
                    root.Push(var);
                } else if (e.Name.LocalName == "struct") {
                    var s = ReadVarStruct(e, root);
                    if (lastComment != "")
                        s.Struct.Comment = lastComment;
                    root.Push(s);
                } else if (e.Name.LocalName == "enum") {
                    var en = ReadVarEnum(e);
                    if (lastComment != "")
                        en.Enum.Comment = lastComment;
                    root.Push(en);
                } else if (e.Name.LocalName == "ptr") {
                    var var = ReadPtr(e, root);
                    if (lastComment != "")
                        var.Comment = lastComment;
                    root.Push(var);
                } else if (e.Name.LocalName == "comment") {
                    lastComment = e.Value;
                }
                if (e.Name.LocalName != "comment") {
                    lastComment = "";
                }
            }
            root.CalcOffsets();
            return root;
        }

        public Var ReadVarStruct(XElement elem, object parent) {
            Var var = new Var(VarType.Struct);
            // Attach offset
            var v = elem.Attribute("offset")?.Value;
            var.LoadOffset(v);

            bool sizeReaded = false;
            var st = ReadStruct(elem, parent);
            if (!String.IsNullOrEmpty(st.Var)) {
                var ptrPos = st.Var.IndexOf("*", StringComparison.Ordinal);
                if (ptrPos == -1) {
                    var last = st.Vars.Last();
                    if (last != null) {
                        var.Size = last.Offset + last.Size;
                        sizeReaded = true;
                    }
                }
            } else {
                var.Size = 0;
                sizeReaded = true;
            }

            if (!sizeReaded) {
                var size = elem.Attribute("size")?.Value;
                var.LoadSize(size);
            }

            var array = elem.Attribute("array")?.Value;
            var.LoadArray(array);

            var.Struct = st;
            return var;
        }

        public Var ReadVar(XElement elem) {
            Var var = new Var(VarType.Var);
            // Attach offset
            var v = elem.Attribute("offset")?.Value;
            var.LoadOffset(v);

            // Attach typename
            if(elem.Name.LocalName != "struct")
                var.LoadTypename(elem.Value);

            // Attach array mark
            var size = elem.Attribute("size")?.Value;
            var.LoadSize(size);

            var array = elem.Attribute("array")?.Value;
            var.LoadArray(array);

            return var;
        }

        public string GetNodePath(XElement elem) {
            var cur = elem;
            var build = new List<string>();
            while (cur != null) {
                var name = cur.Attribute("name")?.Value;
                build.Add(String.IsNullOrEmpty(name) ? cur.Name.LocalName : name);
                cur = cur.Parent;
            }
            build.Reverse();
            var result = "";
            if (build.Count < 1)
                return elem.ToString();
            result += build[0];
            foreach (var s in build.Skip(1)) {
                result += " > " + s;
            }

            return result;
        }

        public Var ReadPtr(XElement elem, Struct parent) {
            Var var = new Var(VarType.Var);
            // Attach offset
            var v = elem.Attribute("offset")?.Value;
            var.LoadOffset(v);

            var path = elem.Attribute("path")?.Value;
            if (path == null)
                throw new Exception($"Ptr[{elem}] path not found: [{GetNodePath(elem)}]");

            var pp = new PathParser(path, parent);
            var type = pp.Build();
            ptrs.Add(type);
            var name = elem.Value;

            var ptr = 0;
            int pos = 0;
            for (; pos < name.Length; pos++) {
                if (name[pos] == '*') {
                    ptr++;
                    continue;
                }

                if (name[pos] == ' ' || name[pos] == '\t')
                    continue;
                break;
            }

            if (ptr != 0) {
                name = name.Substring(pos);
            }

            for (int i = 0; i < ptr; i++) {
                type += "*";
            }

            // Attach typename
            var.LoadTypename(type + " " + name);

            // Attach array mark
            var size = elem.Attribute("size")?.Value;
            var.LoadSize(size);

            var array = elem.Attribute("array")?.Value;
            var.LoadArray(array);

            return var;
        }
    }
}
