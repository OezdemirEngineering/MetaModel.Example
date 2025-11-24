using MetaModel.Example.Evaluation;
using MetaModel.Example.Parsing;
using MetaModel.Example.Contracts;
using MetaModel.Example.Properties;
using Xunit;

namespace MetaModel.Example.Tests;

public class EvaluatorTests
{
    [Theory]
    [InlineData("1+2", 3)]
    [InlineData("10 + 2 /10", 10 + 0.2)]
    [InlineData("(2+3)*4", 20)]
    [InlineData("2+3*4", 14)]
    [InlineData("a=1", 1)]
    [InlineData("a=1+2", 3)]
    public void Evaluate_ParsedExpression_ReturnsExpectedNumber(string expr, double expected)
    {
        // Arrange
        var ctx = new EvaluationContext();
        // Act
        var root = ExpressionParser.Parse(expr);
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(expected, number.Value, precision: 10);
    }

    [Fact]
    public void Evaluate_VariableReuse_a_plus_5_Returns_8()
    {
        // Arrange
        var ctx = new EvaluationContext();
        var assign = ExpressionParser.Parse("a=1+2");
        Evaluator.Evaluate(assign, ctx);
        var expr = ExpressionParser.Parse("a+5");
        // Act
        var result = Evaluator.Evaluate(expr, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(8, number.Value);
    }

    [Fact]
    public void Evaluate_DivideByZero_1_div_0_Throws()
    {
        // Arrange
        var ctx = new EvaluationContext();
        var expr = ExpressionParser.Parse("1/0");
        // Act
        System.Action act = () => Evaluator.Evaluate(expr, ctx);
        // Assert
        Assert.Throws<DivideByZeroException>(act);
    }

    [Fact]
    public void Evaluate_VectorLiteral_1_2_3_Returns_Vector()
    {
        // Arrange
        var ctx = new EvaluationContext();
        var root = ExpressionParser.Parse("[1,2,3]");
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[] {1,2,3}, vec.Components);
    }

    [Fact]
    public void Evaluate_VectorAddition_1_2_plus_3_4_Returns_4_6()
    {
        // Arrange
        var ctx = new EvaluationContext();
        var root = ExpressionParser.Parse("[1,2]+[3,4]");
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[] {4,6}, vec.Components);
    }

    [Fact]
    public void Evaluate_VectorSubtraction_5_5_minus_1_2_Returns_4_3()
    {
        // Arrange
        var ctx = new EvaluationContext();
        var root = ExpressionParser.Parse("[5,5]-[1,2]");
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[] {4,3}, vec.Components);
    }

    [Fact]
    public void Evaluate_ScalarVectorMultiplicationLeft_2_times_vec_1_2_Returns_2_4()
    {
        // Arrange
        var ctx = new EvaluationContext();
        var root = ExpressionParser.Parse("2*[1,2]");
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[] {2,4}, vec.Components);
    }

    [Fact]
    public void Evaluate_ScalarVectorMultiplicationRight_vec_1_2_times_3_Returns_3_6()
    {
        // Arrange
        var ctx = new EvaluationContext();
        var root = ExpressionParser.Parse("[1,2]*3");
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[] {3,6}, vec.Components);
    }
}
