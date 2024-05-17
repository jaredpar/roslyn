# Goal

Get all of the helix generation to occur on the same machine as the build. That makes it much easier to debug and reason about. It also reduces the total work done in our CI machines as we only need to do test discovery and history download one time

## Plan

1. Change RunTests to just drop the files on disk. Those are then uploaded with the rest of the payload
2. Make sure there is a README in the folder of all of our build tools that better explain their purpose and how they fit into the build process

## Todo

- [ ] Figure out why we are calling `SetEnvironmentVariable` everywhere. Ah looks like the [job sender][job-sender] code does read these. Need to makes sure we preserve all of them.
- [ ] Do we care about local runs?
- [ ] thread through the new options HelixJobName and HelixOsName
- [ ] How do we get dotnet onto the middle mcahine to call dotnet build helix.csproj
- [ ] Debugging instructions on how to set the HELIX_CORRELATION_PAYLOAD etc ... variables locally
- [ ] Randomly copying  "ROSLYN_TEST_IOPERATION", "ROSLYN_TEST_USEDASSEMBLIES" seems weird
- [ ] BUILD_REASON is always set to PR even when it's a CI run. That seems wrong.
- [ ] remove Helix from the other parts of tset runner
- [ ] delete Option.HelixAccessToken

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