using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XppCompiler {
    class Constants {
        public static string SkipType = "BYTE";
        public static string SkipName = "_skip";

        // How space count between <typename> <skiptype>
        public static int SpaceAlign = 5;

        public static string FileExt = ".h";
        public static string AllInOne = "all.h";

        public static bool IsGlobalInclude = false;
        public static string GlobalInclude = "global.h";
    }
}
