# MetaModel.Example

A minimal expression language and evaluation engine in C# targeting .NET 10.0. The project demonstrates parsing, AST construction, evaluation, and code generation of arithmetic and conditional expressions, including variable assignment and vector literals.

## Projects
- `MetaModel.Example`: Core library containing AST node definitions (`INode`), parser (`ExpressionParser`), evaluator (`Evaluator`), and code generator (`CodeGenerator`).
- `MetaModel.Example.Tests`: xUnit test suite covering parsing, evaluation, manual evaluation, script execution, and code generation behaviors.
- `MetaModel.UI` (Windows only): WPF sample UI (MahApps.Metro) demonstrating potential visualization. Skipped on non-Windows CI agents.

## Key Features
- Arithmetic operations (`+`, `-`, `*`, `/`)
- Inline and statement variable assignments
- Conditional expressions (`ConditionalNode`) with comparisons (`EqualNode`, `GreaterThanNode`)
- Function call placeholder nodes (`FunctionCallNode`)
- Script grouping (`ScriptNode`) for sequential execution
- Vector literals (`VectorNode`)
- Code generation producing C# fragments with side-effect buffering for inline assignments

## DSL Overview
The project defines a small DSL (domain-specific language) for numeric computation. Source text is parsed into an AST of `INode` implementations.

### Basic Expressions
```
1 + 2 * 4        # precedence: => (1 + (2 * 4))
(x = 7) + 1      # inline assignment: store then use value
```

### Variables
Unassigned variables default to 0 (via dictionary lookup logic).
```
a + 5            # if 'a' was not set previously => 0 + 5
```

### Assignments
```
a = 1 + 2        # statement assignment when part of script
(x = 7) + (y = 3)
```
Inline assignments return the stored dictionary value so they can participate in larger expressions.

### Conditionals
```
if (a == 5) then 10 else 0
if (x > 3) then (x * 2) else (x + 1)
```
(Parsed into `ConditionalNode`, comparisons into `EqualNode` / `GreaterThanNode`.)

### Vectors
```
[1, 2, 3]        # becomes VectorNode of double[] {1,2,3}
```

### Functions (Placeholder)
```
foo(1, a, 3 + 4)
```
Resolved as `FunctionCallNode` – runtime binding left for extension.

### Scripts
A script is a sequence of expressions executed in order; the last assigned `__result` becomes the script output.
```
a = 1 + 2
b = a + 5
b * 2
```
`ScriptNode` preserves ordering and side-effects.

### Generated Code Snippet Example
Expression: `(x = 7) + 1` generates:
```
vars["x"] = 7;
(vars["x"] + 1)
```
Script generation wraps statements in a static class with a variables dictionary.

## Building
```bash
# Build core and tests
 dotnet build MetaModel.Example/MetaModel.Example.csproj -c Release
 dotnet build MetaModel.Example.Tests/MetaModel.Example.Tests.csproj -c Release
# (Optional) Windows only
 dotnet build MetaModel.UI/MetaModel.UI.csproj -c Release
```

## Testing
```bash
 dotnet test MetaModel.Example.Tests/MetaModel.Example.Tests.csproj -c Release --logger trx
```

## Code Generation
`CodeGenerator.GenerateExpression(node)` returns an expression string plus any side-effect lines (e.g., assignments) preceding the final expression. `GenerateScriptClass(script)` emits a static class wrapping execution of a series of nodes with a shared variable dictionary and returns the last computed result.

## CI Pipeline
GitHub Actions workflow runs:
- Gitleaks secret scan
- Restore, build, and test core + test projects
- Collect test results and coverage artifacts
The WPF UI project is excluded on Linux runners using conditional build logic.

## Contributing
Open issues or PRs for enhancements. Include tests for new behaviors. Run `dotnet test` before submission.

## License
See repository for license details.