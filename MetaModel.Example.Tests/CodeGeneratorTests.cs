using MetaModel.Example.Compilation;
using MetaModel.Example.Expressions;
using MetaModel.Example.Properties;
using MetaModel.Example.Contracts;
using Xunit;
using System.Collections.Generic;

namespace MetaModel.Example.Tests;

public class CodeGeneratorTests
{
    [Fact]
    public void GenerateScriptClass_AssignmentAndAdd_ProducesExpectedCodeFragment()
    {
        var script = new ScriptNode(new INode[]
        {
            new AssignmentNode("a", new AddNode(new NumberNode(1), new NumberNode(2))),
            new AddNode(new VariableNode("a", new NumberNode(0)), new NumberNode(5))
        });
        var code = CodeGenerator.GenerateScriptClass(script, "Gen", "TestNs");
        Assert.Contains("vars[\"a\"] = (1 + 2)", code);
        Assert.Contains("return __result", code);
    }

    [Fact]
    public void GenerateExpression_SimpleAdd_ReturnsParenthesizedExpression()
    {
        var expr = new AddNode(new NumberNode(3), new MultiplyNode(new NumberNode(2), new NumberNode(4))); // 3 + (2*4)
        var codeExpr = CodeGenerator.GenerateExpression(expr);
        Assert.Equal("(3 + (2 * 4))", codeExpr);
    }

    [Fact]
    public void GenerateExpression_AssignmentInline_EmitsDictionaryStore()
    {
        var assign = new AssignmentNode("x", new NumberNode(7));
        var expr = new AddNode(assign, new NumberNode(1));
        var codeExpr = CodeGenerator.GenerateExpression(expr);
        // Should have emitted an assignment line in the side-effect buffer
        Assert.Contains("vars[\"x\"] = 7", codeExpr); // note: inline assignment turns into dictionary access expression, side-effect capture not returned
    }
}
