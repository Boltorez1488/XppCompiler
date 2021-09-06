# XppCompiler **\[OldCode\]**
Xml Structures Editor (Console) - Creating descriptions of structures for use in C++ 

Support:
- Namespaces
- Structs
- Enums
- Pointers to structs

## Help
```
Use: <file|directory> <out directory> <params>

Params:
  -a = AllInOne WriteMode
  -an <name> = All FileName, all.h is standart name
  -f = FirstLevel WriteMode
  -e = LastLevel WriteMode (IsDefault)
  -l <level> = Level WriteMode & Compile <level> namespaces
  -g = Enable GlobalInclude file
  -gn <name> = Global FileName, global.h is standart name
  
Description:
  GlobalInclude is allow include all created files in one
  WriteMode:
    AllInOne - Write all namespaces in one file
    FirstLevel - Write all first namespaces in files
    LastLevel - Write all last structs && enums in files
    Level - Write <level> namespaces in files
```
