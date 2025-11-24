using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class EqualNode : BinaryExpressionNode
{
    public EqualNode(INode left, INode right) : base("Equal", left, right) { }
}
