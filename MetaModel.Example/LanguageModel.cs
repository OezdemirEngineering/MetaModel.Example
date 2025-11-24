namespace MetaModel.Example;

using MetaModel.Example.Expressions;
using MetaModel.Example.Evaluation;
using MetaModel.Example.Contracts;

public class LanguageModel
{
    public ScriptNode Script { get; }

    public LanguageModel(IEnumerable<INode> statements)
    {
        Script = new ScriptNode(statements);
    }

    public INode Run(EvaluationContext ctx)
    {
        return Evaluator.Evaluate(Script, ctx);
    }
}
