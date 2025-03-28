# Roslyn Coding Guidelines for Claude

## Build/Test Commands
- Build (Windows): `Build.cmd`, (Unix): `./build.sh`
- Restore packages: `Restore.cmd` or `./build.sh --restore`
- Run tests: `Test.cmd` or `./build.sh --test`
- Run single test: `build.ps1 -test --include "NamespaceOrClassName"`
- Run compiler tests only: `build.ps1 -testCompilerOnly`

## Code Style
- Use `.editorconfig` settings - 4 space indentation, spaces not tabs
- Follow naming conventions: instance fields with `_prefix`, static with `s_prefix`
- Use `var` when type is obvious, explicit type declarations otherwise
- Add required file headers with MIT license
- No LINQ in compiler hot paths for performance reasons
- Use `Debug.Assert()` for internal validation not needed in retail builds
- Prefer object pools for frequently allocated objects

## Error Handling
- Use `Contract.ThrowIfNull<T>()`, `ThrowIfFalse()`, etc. for validation
- Use `ExceptionUtilities` for consistent error patterns
- `Unreachable()` for theoretically impossible code paths

## Testing
- Unit tests end with `.UnitTests`, integration with `.IntegrationTests`
- Test file naming should match component structure