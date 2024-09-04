.NET Compilers Toolset Package
=

Referencing this package will cause the project to be built using the C# and Visual Basic compilers contained in the package, as opposed to the version installed with MSBuild.

This package is primarily intended as a method for rapidly shipping hotfixes to customers. Using it as a long term solution for providing newer compilers on older MSBuild installations is explicitly not supported. That can and will break on a regular basis.

The supported mechanism for providing new compilers in a build enviroment is updating to the newer .NET SDK or Visual Studio Build Tools SKU.

More details at https://aka.ms/roslyn-packages