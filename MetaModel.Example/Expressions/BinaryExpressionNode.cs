using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public abstract class BinaryExpressionNode : BaseNode
{
    public INode Left { get; }
    public INode Right { get; }

    protected BinaryExpressionNode(string name, INode left, INode right)
    {
        Name = name;
        Left = left;
        Right = right;
        // Register operands as children for traversal purposes
        AddChild(left);
        AddChild(right);
    }
}
