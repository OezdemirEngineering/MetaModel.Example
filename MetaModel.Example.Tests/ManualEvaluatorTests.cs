using MetaModel.Example.Evaluation;
using MetaModel.Example.Expressions;
using MetaModel.Example.Properties;
using MetaModel.Example.Contracts;
using Xunit;

namespace MetaModel.Example.Tests;

public class ManualEvaluatorTests
{
    [Fact]
    public void Evaluate_Add_1_plus_2_Returns_3()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new AddNode(new NumberNode(1), new NumberNode(2));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(3, number.Value);
    }

    [Fact]
    public void Evaluate_AddPrecedence_2_plus_3_times_4_Returns_14()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode mult = new MultiplyNode(new NumberNode(3), new NumberNode(4));
        INode root = new AddNode(new NumberNode(2), mult);
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(14, number.Value);
    }

    [Fact]
    public void Evaluate_Assignment_a_equals_1_plus_2_Returns_3()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode assign = new AssignmentNode("a", new AddNode(new NumberNode(1), new NumberNode(2)));
        // Act
        var stored = Evaluator.Evaluate(assign, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(stored);
        Assert.Equal(3, number.Value);
    }

    [Fact]
    public void Evaluate_AssignmentVariableReuse_a_plus_5_Returns_8()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode assign = new AssignmentNode("a", new AddNode(new NumberNode(1), new NumberNode(2))); // a = 3
        Evaluator.Evaluate(assign, ctx);
        INode root = new AddNode(new VariableNode("a", new NumberNode(0)), new NumberNode(5));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(8, number.Value);
    }

    [Fact]
    public void Evaluate_DivideByZero_1_div_0_Throws_DivideByZero()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new DivideNode(new NumberNode(1), new NumberNode(0));
        // Act
        System.Action act = () => Evaluator.Evaluate(root, ctx);
        // Assert
        Assert.Throws<DivideByZeroException>(act);
    }

    [Fact]
    public void Evaluate_VectorLiteral_1_2_3_Returns_Vector_1_2_3()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new VectorNode(new double[]{1,2,3});
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[]{1,2,3}, vec.Components);
    }

    [Fact]
    public void Evaluate_VectorAddition_1_2_plus_3_4_Returns_4_6()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new AddNode(new VectorNode(new double[]{1,2}), new VectorNode(new double[]{3,4}));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[]{4,6}, vec.Components);
    }

    [Fact]
    public void Evaluate_VectorSubtraction_5_5_minus_1_2_Returns_4_3()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new SubtractNode(new VectorNode(new double[]{5,5}), new VectorNode(new double[]{1,2}));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[]{4,3}, vec.Components);
    }

    [Fact]
    public void Evaluate_ScalarVectorMultiplicationLeft_2_times_vec_1_2_Returns_2_4()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new MultiplyNode(new NumberNode(2), new VectorNode(new double[]{1,2}));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[]{2,4}, vec.Components);
    }

    [Fact]
    public void Evaluate_ScalarVectorMultiplicationRight_vec_1_2_times_3_Returns_3_6()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new MultiplyNode(new VectorNode(new double[]{1,2}), new NumberNode(3));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var vec = Assert.IsType<VectorNode>(result);
        Assert.Equal(new double[]{3,6}, vec.Components);
    }

    [Fact]
    public void Evaluate_Equal_3_eq_3_Returns_1()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new EqualNode(new NumberNode(3), new NumberNode(3));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(1, number.Value);
    }

    [Fact]
    public void Evaluate_Equal_3_eq_4_Returns_0()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new EqualNode(new NumberNode(3), new NumberNode(4));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(0, number.Value);
    }

    [Fact]
    public void Evaluate_GreaterThan_5_gt_2_Returns_1()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new GreaterThanNode(new NumberNode(5), new NumberNode(2));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(1, number.Value);
    }

    [Fact]
    public void Evaluate_GreaterThan_2_gt_5_Returns_0()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode root = new GreaterThanNode(new NumberNode(2), new NumberNode(5));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(0, number.Value);
    }

    [Fact]
    public void Evaluate_Conditional_5_gt_2_then_10_else_20_Returns_10()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode cond = new GreaterThanNode(new NumberNode(5), new NumberNode(2));
        INode root = new ConditionalNode(cond, new NumberNode(10), new NumberNode(20));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(10, number.Value);
    }

    [Fact]
    public void Evaluate_Conditional_2_gt_5_then_10_else_20_Returns_20()
    {
        // Arrange
        var ctx = new EvaluationContext();
        INode cond = new GreaterThanNode(new NumberNode(2), new NumberNode(5));
        INode root = new ConditionalNode(cond, new NumberNode(10), new NumberNode(20));
        // Act
        var result = Evaluator.Evaluate(root, ctx);
        // Assert
        var number = Assert.IsType<NumberNode>(result);
        Assert.Equal(20, number.Value);
    }
}
