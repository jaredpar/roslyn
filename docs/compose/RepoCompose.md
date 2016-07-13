
The 

## consumes

The `consumes` command produces a json file describing all of the artifacts consumed by this repo.  This can include:

- NuGet packages
- NuGet feeds
- Files from Azure
- Files from an arbitrary URI

This list must be complete and describe all artifacts external to the repo which are downloaded / copied during a build process.  Here is an example file:

## Artifact Specification

The JSON describing artifacts is the same between the `consume` and `produces` command.  These items can be used anywhere artifacts are listed above.

### NuGet packages

The description for NuGet artifacts is it two parts:

1. The set of feeds packages are being read from.
2. The set of packages that are being consumed and their respective versions.

Example:

``` json
"nuget": {
    "feeds": [
        { 
           "name": "core-clr",
           "value": "https://dotnet.myget.org/F/dotnet-coreclr/api/v3/index.json" 
        },
        {
            "name": "dotnet-core",
            "value="https://dotnet.myget.org/F/dotnet-core/api/v3/index.json"
        }
    ],
    "packages" {
        "MicroBuild.Core": "0.2.0",
        "Microsoft.NETCore.Platforms": "1.0.1"
    }
}
```

### File 

Any file which is not a NuGet package should be listed as a file artifact.  These can be downloaded from the web, copied from local places on the hard drive or obtained from blob storage.  Each type of file entry has a common set of values:

``` json
"files": {
    "identifier": {
        "kind": "uri / filesystem / blobstorage"
    }
}
```

#### File System

Artifacts expressied via `filesystem` are available as simple file paths on the specified operating system.  No authentication should be necessary to access these values.

``` json
"files": {

}


General purpose files can be downloaded from the web, copied from local storage or downloaded from Azure storage.  Any file which is not a NuGet package should be described this way.  

The contents of the file artifacts will change based on the location they are downloaded from:

``` json 




- why can't we use project.json + NuGet.config

## Q/A

### This looks a lot like a package manager. 

Indeed it does. 

### Your samples have comments in JSON.  That's not legal.

Understood.  That's why they are samples.  It's meant to clarify the problem. 