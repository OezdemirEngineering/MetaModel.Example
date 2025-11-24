using System;
using System.Collections.Generic;
using System.Text;

namespace MetaModel.Example.Contracts;

public interface INode
{
    public string Name { get; set; }
    public List<INode> Children { get; set; }
    public INode Parent { get; set; }
}
