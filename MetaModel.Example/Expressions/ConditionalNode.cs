using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class ConditionalNode : BaseNode
{
    public INode Condition { get; }
    public INode ThenBranch { get; }
    public INode ElseBranch { get; }

    public ConditionalNode(INode condition, INode thenBranch, INode elseBranch)
    {
        Name = "Conditional";
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
        AddChild(condition);
        AddChild(thenBranch);
        AddChild(elseBranch);
    }
}
