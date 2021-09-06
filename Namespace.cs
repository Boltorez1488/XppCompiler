using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XppCompiler {
    class Namespace {
        public string Name;

        public Namespace Parent;
        public List<Namespace> Spaces = new List<Namespace>();
        public List<Struct> Nodes = new List<Struct>();
        public List<Enum> Enums = new List<Enum>();

        public Namespace(string name, Namespace parent) {
            Name = name;
            Parent = parent;
        }

        public string GetPath(string name = "") {
            if (Parent != null)
                return Parent.GetPath(String.IsNullOrEmpty(name) ? Name : Name + "." + name);
            return String.IsNullOrEmpty(Name) ? name : Name + "." + name;
        }

        public void Push(Struct node) {
            foreach (var st in Nodes) {
                if (st.Name == node.Name)
                    throw new Exception($"Struct already exists: {GetPath(Name)}.{st.Name}");
            }
            Nodes.Add(node);
        }

        public void Push(Enum node) {
            foreach (var en in Enums) {
                if (en.Name == node.Name)
                    throw new Exception($"Enum already exists: {GetPath(Name)}.{en.Name}");
            }
            Enums.Add(node);
        }

        public void Push(Namespace node) {
            foreach (var space in Spaces.Where(x => !String.IsNullOrEmpty(x.Name))) {
                if (space.Name == node.Name)
                    throw new Exception($"Namespace already exists: {GetPath(Name)}.{space.Name}");
            }
            Spaces.Add(node);
        }

        public Namespace PushSpace(string name) {
            if (String.IsNullOrEmpty(name))
                return this;
            foreach (var space in Spaces.Where(x => !String.IsNullOrEmpty(x.Name))) {
                if (space.Name == name)
                    return space;
            }
            var ns = new Namespace(name, this);
            Spaces.Add(ns);
            return ns;
        }

        public Struct Find(List<string> path) {
            if (path.Count == 0)
                return null;
            foreach (var st in Nodes) {
                if (st.Name == path[0]) {
                    
                }
            }

            return null;
        }

        public Namespace Get(string name) {
            if (String.IsNullOrEmpty(name))
                return this;
            foreach (var space in Spaces.Where(x => !String.IsNullOrEmpty(x.Name))) {
                if (space.Name == name)
                    return space;
            }

            return null;
        }

        public object GetObj(string name) {
            foreach (var space in Spaces) {
                if (space.Name == name)
                    return space;
            }
            foreach (var en in Enums) {
                if (en.Name == name)
                    return en;
            }
            foreach (var node in Nodes) {
                if (node.Name == name)
                    return node;
            }

            return null;
        }

        public static string TabsConvert(int tabs) {
            string result = "";
            for (int i = 0; i < tabs; i++)
                result += '\t';
            return result;
        }

        public static string ToString(List<string> lines) {
            string result = "";
            foreach (var line in lines)
                result += line + "\n";
            return result;
        }

        public void ConvertPath(List<string> path, List<string> build, int tabs = 0) {
            if (String.IsNullOrEmpty(Name)) {
                foreach (var space in Spaces.Where(x => x.Name == path[0])) {
                    space.ConvertPath(path.Skip(1).ToList(), build, tabs);
                }

                if (path.Count == 0) {
                    foreach (var en in Enums) {
                        en.Convert(build, tabs);
                    }
                    foreach (var node in Nodes) {
                        node.Convert(build, tabs);
                    }
                } else if (path.Count == 1) {
                    foreach (var en in Enums.Where(x => x.Name == path[0])) {
                        en.Convert(build, tabs);
                    }
                    foreach (var node in Nodes.Where(x => x.Name == path[0])) {
                        node.Convert(build, tabs);
                    }
                }

                return;
            }
            var tab = TabsConvert(tabs);
            build.Add(tab + $"namespace {Name} " + "{");
            foreach (var space in Spaces.Where(x => x.Name == path[0])) {
                space.ConvertPath(path.Skip(1).ToList(), build, tabs + 1);
            }
            if (path.Count == 0) {
                foreach (var en in Enums) {
                    en.Convert(build, tabs + 1);
                }
                foreach (var node in Nodes) {
                    node.Convert(build, tabs + 1);
                }
            } else if (path.Count == 1) {
                foreach (var en in Enums.Where(x => x.Name == path[0])) {
                    en.Convert(build, tabs + 1);
                }
                foreach (var node in Nodes.Where(x => x.Name == path[0])) {
                    node.Convert(build, tabs + 1);
                }
            }
            build.Add(tab + "}");
        }

        public void BackConvert(List<string> build, int tabs = 0) {
            var path = GetPath().Split('.');
            foreach (var p in path) {
                var tab = TabsConvert(tabs);
                build.Add(tab + $"namespace {p} " + "{");
                tabs++;
            }
            tabs--;
            foreach (var space in Spaces) {
                space.Convert(build, tabs + 1);
            }
            foreach (var en in Enums) {
                en.Convert(build, tabs + 1);
            }
            foreach (var node in Nodes) {
                node.Convert(build, tabs + 1);
            }
            foreach (var p in path) {
                var tab = TabsConvert(tabs);
                build.Add(tab + "}");
                tabs--;
            }
        }

        public void Convert(List<string> build, int tabs = 0) {
            if (String.IsNullOrEmpty(Name)) {
                foreach (var space in Spaces) {
                    space.Convert(build, tabs);
                }
                foreach (var en in Enums) {
                    en.Convert(build, tabs);
                }
                foreach (var node in Nodes) {
                    node.Convert(build, tabs);
                }

                return;
            }
            var tab = TabsConvert(tabs);
            build.Add(tab + $"namespace {Name} " + "{");
            foreach (var space in Spaces) {
                space.Convert(build, tabs + 1);
            }
            foreach (var en in Enums) {
                en.Convert(build, tabs + 1);
            }
            foreach (var node in Nodes) {
                node.Convert(build, tabs + 1);
            }
            build.Add(tab + "}");
        }
    }
}
