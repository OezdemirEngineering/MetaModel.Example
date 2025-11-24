using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class SubtractNode : BinaryExpressionNode
{
    public SubtractNode(INode left, INode right) : base("Subtract", left, right) { }
}
