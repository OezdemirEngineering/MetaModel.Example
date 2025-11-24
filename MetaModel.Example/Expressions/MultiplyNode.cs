using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class MultiplyNode : BinaryExpressionNode
{
    public MultiplyNode(INode left, INode right) : base("Multiply", left, right) { }
}
