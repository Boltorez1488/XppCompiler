using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;

namespace XppCompiler {
    class Program {
        static void Help() {
            Console.WriteLine("Use: <file|directory> <out directory> <params>");
            Console.WriteLine("<out directory> - default directory = 'structs'");
            Console.WriteLine("-----Params-----");
            Console.WriteLine("-a = AllInOne WriteMode");
            Console.WriteLine($"-an <name> = All FileName, {Constants.AllInOne} is standart name");
            Console.WriteLine("-f = FirstLevel WriteMode");
            Console.WriteLine("-e = LastLevel WriteMode (IsDefault)");
            Console.WriteLine("-l <level> = Level WriteMode & Compile <level> namespaces");
            Console.WriteLine("-g = Enable GlobalInclude file");
            Console.WriteLine($"-gn <name> = Global FileName, {Constants.GlobalInclude} is standart name");
            Console.ReadKey();
        }

        static void Main(string[] args) {
            if (args.Length == 0) {
                Help();
                return;
            }

            string fdname = args[0];
            var reader = new Reader(fdname);
            reader.Load();

            Writter wr = null;
            WriteMode wm = WriteMode.LastLevel;
            int level = 0;
            string outDir = "structs";
            if (args.Length > 1) {
                outDir = args[1];
            }

            for (int i = 2; i < args.Length; i++) {
                switch (args[i]) {
                    case "-g":
                        Constants.IsGlobalInclude = true;
                        break;
                    case "-gn":
                        if (i + 1 < args.Length) {
                            Constants.GlobalInclude = args[i + 1];
                        } else {
                            throw new Exception("-gn is bad arg");
                        }
                        break;
                    case "-a":
                        wm = WriteMode.AllInOne;
                        break;
                    case "-an":
                        if (i + 1 < args.Length) {
                            Constants.AllInOne = args[i + 1];
                        } else {
                            throw new Exception("-an is bad arg");
                        }
                        break;
                    case "-f":
                        wm = WriteMode.FirstLevel;
                        break;
                    case "-e":
                        wm = WriteMode.LastLevel;
                        break;
                    case "-l":
                        wm = WriteMode.Level;
                        if (i + 1 < args.Length) {
                            level = Convert.ToInt32(args[i + 1]);
                        } else {
                            throw new Exception("-l is bad arg");
                        }
                        break;
                }
            }

            wr = new Writter(wm, level, outDir);
            wr.Save();
        }
    }
}
