using MetaModel.Example.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
namespace MetaModel.Example.Properties;

public class VariableNode(string name, INode value ) : BaseNode
{
    public string Name { get; set; } = name;
    public INode Value { get; set; } = value;
}