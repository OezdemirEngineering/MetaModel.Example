using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class DivideNode : BinaryExpressionNode
{
    public DivideNode(INode left, INode right) : base("Divide", left, right) { }
}
