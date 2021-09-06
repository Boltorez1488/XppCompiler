using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XppCompiler {
    internal enum WriteMode {
        AllInOne, // First namespaces in one global file
        FirstLevel, // First namespaces in end files
        LastLevel, // All structures & enums to end files
        Level // Only namespace level
    }

    internal class Writter {
        public WriteMode WriteMode;
        public int Level;
        public string OutDir;

        public Writter(WriteMode wm, int level, string outDir) {
            WriteMode = wm;
            Level = level;
            OutDir = outDir;
        }

        private string _curDir;
        private void StartSave() {
            if (!Directory.Exists(OutDir)) {
                Directory.CreateDirectory(OutDir);
            }

            _curDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(OutDir);
        }

        private void EndSave() {
            Directory.SetCurrentDirectory(_curDir);
        }

        private List<string> files = new List<string>();
        private void WriteAllLines(string fname, List<string> lines) {
            files.Add($"#include \"{fname}\"");
            File.WriteAllLines(fname, lines);
        }

        private void SaveAll() {
            var build = new List<string> {"#pragma once", ""};
            Registry.Root.Convert(build);
            
            WriteAllLines(Constants.AllInOne, build);
        }

        private void SaveFirst() {
            foreach (var space in Registry.Root.Spaces) {
                var build = new List<string> { "#pragma once", "" };
                space.Convert(build);
                WriteAllLines(space.Name + Constants.FileExt, build);
            }
        }

        private void SaveLast(Namespace root) {
            foreach (var space in root.Spaces) {
                SaveLast(space);
            }

            foreach (var st in root.Nodes) {
                var build = new List<string> { "#pragma once", "" };
                var path = root.GetPath(st.Name);
                Registry.Root.ConvertPath(path.Split('.').ToList(), build);

                var end = path.Replace(".", "/");
                var dir = Path.GetDirectoryName(end + Constants.FileExt);
                if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                WriteAllLines(end + Constants.FileExt, build);
            }
            foreach (var en in root.Enums) {
                var build = new List<string> { "#pragma once", "" };
                var path = root.GetPath(en.Name);
                Registry.Root.ConvertPath(path.Split('.').ToList(), build);

                var end = path.Replace(".", "/");
                var dir = Path.GetDirectoryName(end + Constants.FileExt);
                if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                WriteAllLines(end + Constants.FileExt, build);
            }
        }

        public void SaveLast() {
            SaveLast(Registry.Root);
        }

        public void SaveLevel(Namespace root, int level) {
            if (Level <= level || root.Spaces.Count == 0) {
                var build = new List<string> { "#pragma once", "" };
                root.BackConvert(build);
                var end = root.GetPath().Replace(".", "/");
                var dir = Path.GetDirectoryName(end + Constants.FileExt);
                if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                WriteAllLines(end + Constants.FileExt, build);
                return;
            }
            foreach (var space in root.Spaces) {
                SaveLevel(space, level + 1);
            }
        }

        public void SaveLevel() {
            SaveLevel(Registry.Root, 0);
        }

        public void Save() {
            StartSave();
            switch (WriteMode) {
                case WriteMode.AllInOne:
                    SaveAll();
                    break;
                case WriteMode.FirstLevel:
                    SaveFirst();
                    break;
                case WriteMode.LastLevel:
                    SaveLast();
                    break;
                case WriteMode.Level:
                    SaveLevel();
                    break;
            }
            if (Constants.IsGlobalInclude) {
                files.Insert(0, "#pragma once");
                File.WriteAllLines(Constants.GlobalInclude, files);
                files.Clear();
            }
            EndSave();
        }
    }
}
