using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XppCompiler {
    class PathParser {
        public Struct Current;
        public string Path;

        public PathParser(string path, Struct current) {
            Path = path;
            Current = current;

            bool test = Regex.IsMatch(Path,
                "([A-z]{1}[A-z,0-9]*(\\.[A-z][A-z,0-9]*)*)||(../){1,}[A-z]{1}[A-z,0-9]*(/[A-z][A-z,0-9]*)*");
            if (!test)
                throw new Exception($"Path({Path}) is bad");
        }

        public string Build() {
            if (!Path.StartsWith("../")) {
                return Path.Replace(".", "::");
            }
            var path = Path.Substring(3);
            var cur = Current.Parent;
            while (cur != null) {
                if (!path.StartsWith("../"))
                    break;
                path = path.Substring(3);
                cur = cur is Namespace ? (cur as Namespace).Parent : (cur as Struct)?.Parent;
            }

            path = path.Replace("/", ".");
            

            string result;
            if (cur is Namespace space) {
                result = space.GetPath(path);
            } else {
                result = (cur as Struct)?.GetPath(path);
            }

            return result?.Replace(".", "::");
        }
    }
}
