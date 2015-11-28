
$i = 0;
while ($true) {

    rm .\Binaries\Debug\Microsoft.CodeAnalysis.CSharp.*
    rm -re -fo .\Binaries\Obj\CSharpCodeAnalysis

    msbuild /v:m /m /p:TestBuild=true .\src\Compilers\CSharp\Portable\CSharpCodeAnalysis.csproj

    $dllHash = get-md5 Binaries\Debug\Microsoft.CodeAnalysis.CSharp.dll

    pushd e:\temp

    $callHashFile = (gci *hash.txt)[0]
    $callHashHash = get-md5 $callHashFile
    rm $callHashFile 

    $callFile = (gci *call.txt)[0]
    mv $callFile "e:\temp\log\$i.fulllog.txt"

    $ilFile = (gci *.il.txt)[0]
    $ilHash = get-md5 $ilFile
    ildasm /out="e:\temp\log\$i.il" $ilFile
    rm $ilFile

    popd

    $text = @"
DLL Hash: $dllHash
IL Hash: $ilHash
Call Log Hash: $callHashHash
"@

    $text | out-file "e:\temp\log\$i.summary.txt"

    $i++ 
}

