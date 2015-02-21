
# SOURCE=~/jaredpar05/dd/ros/Open/Binaries/Debug/
SOURCE=/Volumes/C/Users/jaredpar/Documents/GitHub/roslyn/Binaries/Debug/
DEST=~/Documents/roslyn/packages/Microsoft.Net.ToolsetCompilers.1.0.0-rc1-20150122-03/tools/

cp $SOURCE"Microsoft.CodeAnalysis.Desktop.dll" $DEST
cp $SOURCE"System.Reflection.Metadata.dll" $DEST
cp $SOURCE"Microsoft.CodeAnalysis.dll" $DEST
cp $SOURCE"csc.exe" $DEST
cp $SOURCE"Microsoft.CodeAnalysis.CSharp.Desktop.dll" $DEST
cp $SOURCE"Microsoft.CodeAnalysis.CSharp.dll" $DEST
cp $SOURCE"System.Collections.Immutable.dll" $DEST
cp $SOURCE"System.Runtime.InteropServices.dll" $DEST

cp $SOURCE"../../src/Compilers/Core/Portable/obj/Debug/Microsoft.CodeAnalysis.CodeAnalysisResources.resources" ~/Documents/roslyn/nix



