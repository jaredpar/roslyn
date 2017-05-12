# Contributing to Roslyn

Guidelines for contributing to the Roslyn repo.

## Submitting Pull Requests

- **DO** ensure submissions pass all Jenkins legs and are merge conflict free.
- **DO** ensure submissions meet functional and performance expectations, including scenarios for which the team doesn’t yet have open source tests. This means you may be asked to fix and resubmit your pull request against a new open test case if it fails one of these tests.
- **DO** follow the [.editorconfig](http://editorconfig.org/) settings for each directory. 
- **DO NOT** submit changes beyond the level of a bug fix should be discussed with the team first, or they will likely be declined.
- **DO NOT** submit unsolicited language features: all language feature discussion should begin on the [C# language](https://github.com/dotnet/csharplang#discussion) / [VB language](https://github.com/dotnet/vblang) repos.  Once a feature is championed and validated by LDM a developer will be assigned to help begin a prototype on this repo inside a feature branch.
- **DO NOT** submit large code formatting changes. Such changes will be declined.

When you are ready to proceed with making a change, get set up to [build](https://github.com/dotnet/roslyn/blob/master/docs/contributing/Building%2C%20Debugging%2C%20and%20Testing%20on%20Windows.md) the code and familiarize yourself with our developer workflow. 

These two blogs posts on contributing code to open source projects are good too: [Open Source Contribution Etiquette](http://tirania.org/blog/archive/2010/Dec-31.html) by Miguel de Icaza and [Don’t “Push” Your Pull Requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) by Ilya Grigorik.

## Creating Issues

- **DO** use a descriptive title that identifies the issue to be addressed or the requested feature. For example when describing an issue where the compiler is not behaving as expected, write your bug title in terms of what the compiler should do rather than what it is doing – “C# compiler should report CS1234 when Xyz is used in Abcd.”
- **DO** specify a detailed description of the issue or requested feature.
- **DO** provide the following for bug reports
    - Describe the expected behavior and the actual behavior. If it is not self-evident such as in the case of a crash, provide an explanation for why the expected behavior is expected.
    - Provide example code that reproduces the issue.
    - Specify any relevant exception messages and stack traces.
- **DO** subscribe to notifications for the created issue in case there are any follow up questions.

## Coding Style

The Roslyn project is a member of the [.NET Foundation](https://github.com/orgs/dotnet) and follow the same [developer guide](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md).  The repo also includes [.editorconfig](http://editorconfig.org) files to help enforce this convention.  Contributors should ensure they follow these guidelines when making submissions.  

### CSharp

- **DO** use the coding style outlined in the [.NET Foundation Coding Guidelines](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md)
- **DO** use plain code to validate parameters at public boundaries. Do not use Contracts or magic helpers.

```csharp
if (argument == null)
{
    throw new ArgumentNullException("argument");
}
```

- **DO** use `Debug.Assert()` for checks not needed in retail builds. Always include a “message” string in your assert to identify failure conditions. Add assertions to document assumptions on non-local program state or parameter values, e.g. “At this point in parsing the scanner should have been advanced to a ‘.’ token by the caller”.
- **DO** avoid allocations in compiler hot paths:
    - Avoid LINQ.
    - Avoid using foreach over collections that do not have a struct enumerator.
    - Consider using an object pool. There are many usages of object pools in the compiler to see an example.

### Visual Basic Conventions

For all of the C# guidelines which have analogs in Visual Basic, the team applies the spirit of the guideline to Visual Basic. Guidelines surrounding spacing, indentation, parameter names, and the use of named parameters are all generally applicable to Visual Basic. ‘Dim’ statements should also follow the guidelines for the use of ‘var’ in C#. Specific to Visual Basic, field names should begin with ‘m_’ or ‘_’. And the team prefers that all field declarations be placed at the beginning of a type definition. The Visual Studio members dropdown does not show fields in VB. Having them at the beginning of the type aids in navigation.

### Tips 'n' Tricks
Our team finds using [this enhanced source view](http://source.roslyn.io/) of Roslyn helpful when developing.