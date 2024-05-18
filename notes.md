# Goal

Get all of the helix generation to occur on the same machine as the build. That makes it much easier to debug and reason about. It also reduces the total work done in our CI machines as we only need to do test discovery and history download one time

## Plan

### Remove helix support from RunTests

Create the HelixUtil project. The job of this is to put all of the helix assets onto disk. The helix support will change to using this project to generate artifacts. There will be a test-helix.ps1 script that will be used to run the helix job.

Possibly create `dotnet.ps1/sh` scripts that make it easier to invoke the `dotnet` command from 
our YAML files.

## How it works today

Existing infra:

- calls prepare-tests.ps1/sh
  - calls PrepareTests
    - calls TestDiscoveryWorker
  - writes testlist.json
  - packs up eng in the test payload

## Todo

- [ ] better define responsibilities. Like YML should probably be setting most environment variables.
- [ ] Figure out why we are calling `SetEnvironmentVariable` everywhere. Ah looks like the [job sender][job-sender] code does read these. Need to makes sure we preserve all of them.
- [ ] Do we care about local runs?
- [ ] thread through the new options HelixJobName and HelixOsName
- [ ] How do we get dotnet onto the middle mcahine to call dotnet build helix.csproj
- [ ] Debugging instructions on how to set the HELIX_CORRELATION_PAYLOAD etc ... variables locally
- [ ] Randomly copying  "ROSLYN_TEST_IOPERATION", "ROSLYN_TEST_USEDASSEMBLIES" seems weird
- [ ] BUILD_REASON is always set to PR even when it's a CI run. That seems wrong.
- [ ] remove Helix from the other parts of tset runner
- [ ] delete Option.HelixAccessToken
- [ ] only upload eng/common maybe?
- [ ] As long as the helix coordination machine is same OS we should just include the .NET in the test payload so we don't hit the network again.

    <EnableAzurePipelinesReporter>" + (isAzureDevOpsRun ? "true" : "false") + @"</EnableAzurePipelinesReporter>

```csharp
        Environment.SetEnvironmentVariable("BUILD_SOURCEBRANCH", sourceBranch);
        Environment.SetEnvironmentVariable("BUILD_REPOSITORY_NAME", "dotnet/roslyn");
        Environment.SetEnvironmentVariable("SYSTEM_TEAMPROJECT", "dnceng");
        Environment.SetEnvironmentVariable("BUILD_REASON", "pr");
```

Full [helix documentation][helix-doc]

[helix-doc]: https://github.com/dotnet/arcade/tree/main/src/Microsoft.DotNet.Helix/Sdk
[job-sender]: https://github.com/dotnet/arcade/blob/15eea424d3b2dd25a5c0b10e8adc8aeed50129a1/src/Microsoft.DotNet.Helix/JobSender/JobDefinition.cs#L205