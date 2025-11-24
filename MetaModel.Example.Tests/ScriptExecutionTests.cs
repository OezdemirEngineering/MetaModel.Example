using MetaModel.Example.Evaluation;
using MetaModel.Example.Expressions;
using MetaModel.Example.Properties;
using MetaModel.Example.Contracts;
using Xunit;
using System.Linq;

namespace MetaModel.Example.Tests;

public class ScriptExecutionTests
{
    [Fact]
    public void Script_Run_Assign_and_Add_Returns_6()
    {
        // Arrange
        var ctx = new EvaluationContext();
        ctx.RegisterFunction("sum3", (c, args) => {
            var nums = args.OfType<NumberNode>().Select(n => n.Value).ToArray();
            return new NumberNode(nums.Sum());
        });
        var statements = new INode[]
        {
            new AssignmentNode("a", new AddNode(new NumberNode(1), new NumberNode(2))), // a = 3
            new AssignmentNode("b", new FunctionCallNode("sum3", new INode[]{ new NumberNode(1), new NumberNode(1), new NumberNode(1) })), // b = 3
            new AddNode(new VariableNode("a", new NumberNode(0)), new VariableNode("b", new NumberNode(0))) // a + b = 6
        };
        var model = new LanguageModel(statements);
        // Act
        var result = model.Run(ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(6, number.Value);
    }

    [Fact]
    public void Script_Run_Conditional_and_Function_Returns_42()
    {
        // Arrange
        var ctx = new EvaluationContext();
        ctx.RegisterFunction("make", (c, args) => new NumberNode(args.OfType<NumberNode>().First().Value));
        var statements = new INode[]
        {
            new AssignmentNode("x", new NumberNode(5)),
            new AssignmentNode("y", new NumberNode(7)),
            new AssignmentNode("b", new GreaterThanNode(new VariableNode("x", new NumberNode(0)), new VariableNode("y", new NumberNode(0)))), // b = x>y => 0
            new ConditionalNode(new VariableNode("b", new NumberNode(0)), new NumberNode(100), new FunctionCallNode("make", new INode[]{ new NumberNode(42) }))
        };
        var model = new LanguageModel(statements);
        // Act
        var result = model.Run(ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(42, number.Value);
    }
}