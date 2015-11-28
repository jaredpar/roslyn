gps vbcscompiler | kill

$source = join-path $PSScriptRoot "..\Binaries\Debug"
$dest = join-path $PSScriptRoot "tools"
pushd $dest

foreach ($item in gci *) {
    $name = split-path -leaf $item
    $sourcePath = join-path $source $name
    if (test-path $sourcePath) { 
        write-host "Copying $sourcePath"
        cp $sourcePath .
    }
}

popd
