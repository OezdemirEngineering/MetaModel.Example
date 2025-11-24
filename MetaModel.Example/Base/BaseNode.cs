using MetaModel.Example.Contracts;

public abstract class BaseNode : INode
{
    public string Name { get; set; }
    public List<INode> Children { get; set; } = [];
    public INode Parent { get; set; }

    protected void AddChild(INode child)
    {
        child.Parent = this;
        Children.Add(child);
    }
}
