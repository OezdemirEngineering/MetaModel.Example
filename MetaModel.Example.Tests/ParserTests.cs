using MetaModel.Example.Parsing;
using MetaModel.Example.Expressions;
using MetaModel.Example.Contracts;
using Xunit;
using MetaModel.Example.Properties;

namespace MetaModel.Example.Tests;

public class ParserTests
{
    [Fact]
    public void Parse_Assignment_x_equals_1_plus_2_Returns_AssignmentNode()
    {
        // Arrange
        var input = "x=1+2";
        // Act
        var node = ExpressionParser.Parse(input);
        // Assert
        Assert.IsType<AssignmentNode>(node);
    }

    [Fact]
    public void Parse_BinaryPrecedence_1_plus_2_times_3_Returns_AddNodeWithMultiplyRight()
    {
        // Arrange
        var input = "1+2*3";
        // Act
        var node = ExpressionParser.Parse(input);
        // Assert
        var add = Assert.IsType<AddNode>(node);
        Assert.IsType<MultiplyNode>(add.Right);
    }

    [Fact]
    public void Parse_Parentheses_paren_1_plus_2_end_times_3_Returns_MultiplyNodeWithAddLeft()
    {
        // Arrange
        var input = "(1+2)*3";
        // Act
        var node = ExpressionParser.Parse(input);
        // Assert
        var mult = Assert.IsType<MultiplyNode>(node);
        Assert.IsType<AddNode>(mult.Left);
    }

    [Fact]
    public void Parse_VariableReuse_a_plus_5_Returns_AddNodeWithVariableLeft()
    {
        // Arrange
        var input = "a+5";
        // Act
        var node = ExpressionParser.Parse(input);
        // Assert
        var add = Assert.IsType<AddNode>(node);
        Assert.IsType<VariableNode>(add.Left);
        Assert.IsType<NumberNode>(add.Right);
    }

    [Fact]
    public void Parse_VectorLiteral_brackets_1_2_3_Returns_VectorNode()
    {
        // Arrange
        var input = "[1,2,3]";
        // Act
        var node = ExpressionParser.Parse(input);
        // Assert
        var vec = Assert.IsType<VectorNode>(node);
        Assert.Equal(new double[]{1,2,3}, vec.Components);
    }

    [Fact]
    public void Parse_VectorAddition_bracket_1_2_plus_bracket_3_4_Returns_AddNode()
    {
        // Arrange
        var input = "[1,2]+[3,4]";
        // Act
        var node = ExpressionParser.Parse(input);
        // Assert
        var add = Assert.IsType<AddNode>(node);
        Assert.IsType<VectorNode>(add.Left);
        Assert.IsType<VectorNode>(add.Right);
    }

    [Fact]
    public void Parse_ScalarVectorMultiplication_2_star_bracket_1_2_Returns_MultiplyNode()
    {
        // Arrange
        var input = "2*[1,2]";
        // Act
        var node = ExpressionParser.Parse(input);
        // Assert
        var mult = Assert.IsType<MultiplyNode>(node);
        Assert.IsType<NumberNode>(mult.Left);
        Assert.IsType<VectorNode>(mult.Right);
    }

    [Fact]
    public void Parse_Script_multiple_statements_Returns_ScriptNodeWithStatements()
    {
        // Arrange
        var input = "a=1+2; b=3+4; a+5";
        // Act
        var script = ExpressionParser.ParseScript(input);
        // Assert
        Assert.IsType<ScriptNode>(script);
        Assert.Equal(3, script.Statements.Count);
        Assert.IsType<AssignmentNode>(script.Statements[0]);
        Assert.IsType<AssignmentNode>(script.Statements[1]);
        Assert.IsType<AddNode>(script.Statements[2]);
    }
}
