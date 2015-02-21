
CSC_TOOL=~/Documents/roslyn/packages/Microsoft.Net.ToolsetCompilers.1.0.0-rc1-20150122-03/tools/csc.exe
OUT_PATH=~/Documents/roslyn/Binaries/Debug/
REF_PATH=/usr/local/Cellar/mono/3.10.0/lib/mono/4.5/Facades/
SYS_IMM_PATH=~/Documents/roslyn/packages/System.Collections.Immutable.1.1.33-beta/lib/portable-net45+win8+wp8+wpa81/System.Collections.Immutable.dll
SYS_MD_PATH=~/Documents/roslyn/packages/System.Reflection.Metadata.1.0.18-beta/lib/portable-net45+win8/System.Reflection.Metadata.dll

pushd ~/Documents/roslyn

pushd src/Compilers/Core/Portable
mono $CSC_TOOL @/Users/jaredpar/Documents/roslyn/nix/MS.CA.rsp
popd

pushd src/Compilers/Core/Desktop
mono $CSC_TOOL @/Users/jaredpar/Documents/roslyn/nix/MS.CA.Desktop.rsp
popd

#pushd src/Tools/Source/FakeSign

#mono $CSC_TOOL Program.cs /parallel- /target:exe /unsafe+ /r:$REF_PATH"System.IO.dll" /r:$REF_PATH"System.Runtime.dll" /r:$SYS_IMM_PATH /r:$SYS_MD_PATH /out:$OUT_PATH"FakeSign.exe"


popd
