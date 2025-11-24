using MetaModel.Example.Contracts;

namespace MetaModel.Example.Properties;

public class VectorNode : BaseNode
{
    public IReadOnlyList<double> Components { get; }

    public VectorNode(IEnumerable<double> components)
    {
        var list = components.ToList();
        Components = list;
        Name = "Vector";
        // Optionally add scalar children nodes for uniform traversal
        foreach (var c in list)
        {
            AddChild(new NumberNode(c));
        }
    }
}
