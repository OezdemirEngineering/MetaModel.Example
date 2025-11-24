using System;
using System.Collections.Generic;
using System.Text;

namespace MetaModel.Example.Properties
{
    public class NumberNode(double value) : BaseNode
    {
        public double Value { get; set; } = value;
    }
}
