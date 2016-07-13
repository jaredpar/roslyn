
The 

## Command: consumes

The `consumes` command returns json output that describes all of the external artifacts consumed by this repo.  This includes NuGet feeds, packages and arbitrary files from the web or file system.  The format of all these items is describe in the Artifact Specification section below. 

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

## Command: produces

The `produces` command returs json output which describes the artifacts produced by the the repo.  This includes NuGet packages and file artifacts.  

The output format for artifacts is special for `produces` because it lacks any hard location information.  For example: 

- NuGet artifacts lack feeds
- File artifacts lack a `"kind"` and supporting values

This is because the `produces` command represents "what" a repo produces, not "where" the repo produces it.  The "where" portion is controled by the `publish` command.  External components will take the output of `produces`, add the location information in and feed it back to `publish`.  

Like `consumes` the `produces` output is also grouped by the operating system:

``` json
{
    "os-windows": {
        "nuget": { },
        "file": { }
    },
    "os-linux": {
        "nuget": { },
        "file": { }
    }
}
```

A ful sample output for `produces` is available in the Samples section.

## Command: change

TDB

## Command: publish

## Artifact Specification

The json describing artifacts is the same between the `consume`, `produces` and `publish` commands.  These items can be used anywhere artifacts are listed above.

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
            "value": "https://dotnet.myget.org/F/dotnet-core/api/v3/index.json"
        }
    ],
    "packages": {
        "MicroBuild.Core": "0.2.0",
        "Microsoft.NETCore.Platforms": "1.0.1"
    }
}
```

### File 

Any file which is not a NuGet package should be listed as a file artifact.  These can be downloaded from the web or copied from local places on the hard drive.  Each type of file entry will have a name uniquely identifying the artifact and a kind property specifying the remainder of the properties:

    - uri: a property named `"uri"` will contain an absolute Uri for the artifact.
    - filesystem: a property named `"location"` will contain an OS specific file path for the artifact.

Example: 

``` json
"file": {
    "nuget.exe":  {
        "kind": "uri",
        "uri": "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
    }, 
    "run.exe": { 
        "kind": "filesystem",
        "location": "c:\\tools\\run.exe"
    }
}
```

## Samples

### consumes

``` json
{
    "os-all": {
        "dependencies": {
            "build": { 
                "nuget": {
                    "feeds": [
                        { 
                           "name": "core-clr",
                           "value": "https://dotnet.myget.org/F/dotnet-coreclr/api/v3/index.json" 
                        },
                        {
                            "name": "dotnet-core",
                            "value": "https://dotnet.myget.org/F/dotnet-core/api/v3/index.json"
                        }
                    ],
                    "packages" {
                        "MicroBuild.Core": "0.2.0",
                        "Microsoft.NETCore.Platforms": "1.0.1"
                    }
                },
                "file": {
                    "nuget.exe":  {
                        "uri": "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
                    }
                }
            },
            "toolset": { 
            },
            "static": { 
            }
        },
        "prereq": { 
            "Microsoft Visual Studio": "2015",
            "CMake" : "1.0"
        } 
    }
}
```

### produces

``` json
{
    "os-all": {
        "nuget": {
            "packages" {
                "MicroBuild.Core": "0.2.0",
                "Microsoft.NETCore.Platforms": "1.0.1"
            }
        },
        "file": {
            "nuget.exe": { } 
        }
    }
}
```

Open Issues

- why can't we use project.json + NuGet.config
- full set of operating system identifiers
- how can we relate file names between repos.  It's easy for us to understand that Microsoft.CodeAnalysis.nupkg is the same between repos.  How do we know that a repo which produces foo.msi is the input for a repo that consumes foo.msi? 


## Q/A

### This looks a lot like a package manager. 

Indeed it does. 

### Your samples have comments in JSON.  That's not legal.

Understood.  That's why they are samples.  It's meant to clarify the problem. 