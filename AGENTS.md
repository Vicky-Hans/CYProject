# AGENTS.md

## Cursor Cloud specific instructions

### Codebase overview

This is a **Unity mobile game client** (CYProject) split across three top-level directories:

| Directory | Purpose |
|-----------|---------|
| `cy_unity/` | Unity 2022.3.30f1 game client (C# source, assets, packages, shaders) |
| `cy_excel/` | Game config data (~90+ Excel spreadsheets) |
| `cy_proto/` | Protocol Buffer definitions (4 `.proto` files) |

The game server (Go) and gateway are **external** and not in this repo.

### What can be built/run on a headless Linux VM

The Unity Editor is **not** available in Cloud Agent VMs, so the full game client cannot be opened or built. However, the following **toolchain components** can be built and run:

- **CodeGenerator solution** (`cy_unity/CodeGenerator/CodeGenerator.sln`): 5 .NET projects (Roslyn source generators + Google.ProtoBuf + UnitTest). Build with `dotnet build` from `cy_unity/CodeGenerator/`. The UnitTest project has a pre-existing build error in auto-generated DataBinding code (`CS0507`); the 4 library projects build cleanly.
- **ConfigGenerator** (`cy_unity/ExcelTool/ConfigGenerator/ConfigGenerator.sln`): .NET 7 project for generating config C# code. Build with `dotnet build` from its directory.
- **ExcelTool binaries**: `exporter_linux` and `json2bson_linux` are statically-linked Go binaries that run on x86_64 Linux. They require a `config.json` (generated from `config_template.json` by `fixConfigPath.sh`) and the Excel data in `cy_excel/`.
- **Protocol Buffers**: Compile `.proto` files with system `protoc` from `cy_proto/`.
- **Python i18n tool** (`cy_unity/ExcelTool/excel_i18n.py`): requires `xlrd` and `openpyxl`.

### Build commands

```bash
# CodeGenerator library projects (all 4 succeed)
cd cy_unity/CodeGenerator && dotnet build Generator/Generator.csproj
cd cy_unity/CodeGenerator && dotnet build ProtoWrapGenerator/ProtoWrapGenerator.csproj
cd cy_unity/CodeGenerator && dotnet build DataBindingGenerator/DataBindingGenerator.csproj
cd cy_unity/CodeGenerator && dotnet build Google.ProtoBuf/Google.ProtoBuf.csproj

# ConfigGenerator
cd cy_unity/ExcelTool/ConfigGenerator && dotnet build

# Proto compilation check
cd cy_proto && protoc --proto_path=. --descriptor_set_out=/dev/null comm.proto role.proto logic.proto Ugate.proto
```

### Gotchas

- The UnitTest project (`cy_unity/CodeGenerator/UnitTest/`) fails to build due to a pre-existing `CS0507` error in auto-generated `View_DataBindingCode_g.cs`. This is not a Cloud Agent environment issue.
- `fixConfigPath.sh` is macOS-specific (`sed -i ""` syntax) and checks for username `droidhang`. On Linux, create `config.json` manually from `config_template.json`.
- `checkTmpTag.py` uses Python 2 print syntax (`print "..."`) and will not run under Python 3 without modification.
- The NU1608 warnings in CodeGenerator builds are pre-existing version-constraint mismatches between `Microsoft.CodeAnalysis` and `Microsoft.CodeAnalysis.Common`; they do not affect the build.
- The ExcelTool `exporter_linux` binary expects a `config.json` in the current directory. Run it from `cy_unity/ExcelTool/`.
