using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class GreaterThanNode : BinaryExpressionNode
{
    public GreaterThanNode(INode left, INode right) : base("GreaterThan", left, right) { }
}
