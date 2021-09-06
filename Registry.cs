using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XppCompiler {
    class Registry {
        // Main namespace tree
        public static Namespace Root = new Namespace("", null);

        public static Namespace PushNamespace(string path) {
            if (String.IsNullOrEmpty(path))
                return Root;
            if (path.StartsWith("."))
                path = path.Substring(1);

            var spaces = path.Split('.');
            if (spaces.Length == 1) {
                return Root.PushSpace(spaces[0]);
            }

            Namespace old = null, now = null;
            foreach (var space in spaces) {
                if (String.IsNullOrEmpty(space))
                    continue;
                if (old == null) {
                    old = now = Root.PushSpace(space);
                } else {
                    old = now;
                    now = now.PushSpace(space);
                }
            }

            return now;
        }

        public static bool IsPathValid(string path, bool isNamespace, bool isStruct, bool isEnum) {
            if (String.IsNullOrEmpty(path))
                return false;

            object cur = Root;
            var split = path.Split('.');
            foreach (var s in split) {
                if (cur is Namespace space) {
                    cur = space.GetObj(s);
                    if (cur == null)
                        return false;
                } else if (cur is Struct st) {
                    cur = st.GetObj(s);
                    if (cur == null)
                        return false;
                } 
            }

            if (cur == null)
                return false;

            if (isNamespace && cur is Namespace)
                return true;
            if (isStruct && cur is Struct)
                return true;
            if (isEnum && cur is Enum)
                return true;
            return false;
        }
    }
}
