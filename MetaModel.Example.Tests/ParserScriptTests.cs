using MetaModel.Example.Parsing;
using MetaModel.Example.Evaluation;
using MetaModel.Example.Expressions;
using MetaModel.Example.Contracts;
using MetaModel.Example.Properties;
using Xunit;

namespace MetaModel.Example.Tests;

public class ParserScriptTests
{
    [Fact]
    public void ParseScript_MultipleAssignments_ProducesScriptNode()
    {
        var script = ExpressionParser.ParseScript("a=1;b=2;c=a+b;");
        Assert.IsType<ScriptNode>(script);
        Assert.Equal(3, script.Children.Count);
    }

    [Fact]
    public void ParseScript_AssignAndAdd_EvaluatesToSum()
    {
        var ctx = new EvaluationContext();
        var script = ExpressionParser.ParseScript("a=10;b=5;a=a+b;");
        var result = Evaluator.Evaluate(script, ctx);
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(15, number.Value);
    }

    [Fact]
    public void ParseScript_VectorLiteral_EvaluatesLastStatement()
    {
        var ctx = new EvaluationContext();
        var script = ExpressionParser.ParseScript("a=[1,2,3];b=2;a=a+b;");
        var result = Evaluator.Evaluate(script, ctx);
        // After adding vector-scalar addition support, a=a+b adds 2 to each component.
        var vector = Assert.IsType<VectorNode>(result);
        Assert.Equal(new[] { 3.0, 4.0, 5.0 }, vector.Components);
    }
}
