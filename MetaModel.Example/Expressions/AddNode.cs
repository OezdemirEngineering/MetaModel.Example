using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class AddNode : BinaryExpressionNode
{
    public AddNode(INode left, INode right) : base("Add", left, right) { }
}
