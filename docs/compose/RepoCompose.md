
The 

## consumes

The `consumes` command produces json output that describes all of the external artifacts consumed by this repo.  This includes NuGet feeds, packages and arbitrary files from the web or file system.  The format of all these items is describe in the Artifact Specification section below. 

The artifacts are grouped into three sections: 

- Build Dependencies: artifacts which are referenced as a part of the build output.  This commonly includes CoreFx, CoreClr, etc ... 
- Toolset Dependencies: artifacts used in the production of build output.  This commonly includes NuGet.exe, compilers, etc ... 
- Static Dependenciies: artifacts referenced as a part of the build output but are never intended to change.  This are generally used as SDK components, resources, etc ...  

These sections are identified respectively by the following JSON sections:

``` json
"dependencies": {
    "build": { 
        // Build artifacts
    },
    "toolset": { 
        // Toolset artifacts
    },
    "static": { 
        // Static artifacts
    }
}
```

The data in the output is further grouped by operating system.  Builds of the same repo on different operating system can reasonably consume a different set of resources.  The output of `consumes` reflect this and allows per operating system dependencies:

``` json
{
    "os-windows": {
        "dependencies": { } 
    },
    "os-linux": {
        "dependencies": { }
    }
}
```

In the case the `consumes` output doesn't want to take advantage of OS specific dependencies it can specify `"os-all"` as a catch all. 

In addition to artifacts the consume feed can also optionally list any machine prerequitsites needed to build or test the repo:

``` json
"prereq": { 
    "Microsoft Visual Studio": "2015",
    "CMake" : "1.0",
}
```

A full sample output for `consumes` is available in the Samples section.

## Artifact Specificatio
The JSON describing artifacts is the same between the `consume` and `produces` command.  These items can be used anywhere artifacts are listed above.

### NuGt packages

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

```

Open Issues


- why can't we use project.json + NuGet.config

## Q/A

### This looks a lot like a package manager. 

Indeed it does. 

### Your samples have comments in JSON.  That's not legal.

Understood.  That's why they are samples.  It's meant to clarify the problem. 