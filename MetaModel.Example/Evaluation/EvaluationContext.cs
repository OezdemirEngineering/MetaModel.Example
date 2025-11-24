using MetaModel.Example.Contracts;
using MetaModel.Example.Properties;
using System.Collections.Concurrent;

namespace MetaModel.Example.Evaluation;

public class EvaluationContext
{
    private readonly ConcurrentDictionary<string, INode> _variables = new();
    private readonly ConcurrentDictionary<string, Func<EvaluationContext, INode[], INode>> _functions = new();

    public bool TryGet(string name, out INode node) => _variables.TryGetValue(name, out node!);
    public void Set(string name, INode node) => _variables[name] = node;

    public void RegisterFunction(string name, Func<EvaluationContext, INode[], INode> fn) => _functions[name] = fn;
    public bool TryGetFunction(string name, out Func<EvaluationContext, INode[], INode> fn) => _functions.TryGetValue(name, out fn!);
}
