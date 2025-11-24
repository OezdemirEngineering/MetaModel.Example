using MetaModel.Example.Contracts;
using MetaModel.Example.Expressions;
using MetaModel.Example.Properties;

namespace MetaModel.Example.Evaluation;

public static class Evaluator
{
    public static INode Evaluate(INode node, EvaluationContext ctx)
    {
        return node switch
        {
            NumberNode n => n,
            VectorNode v => v,
            BinaryExpressionNode b => EvaluateBinary(b, ctx),
            AssignmentNode a => EvaluateAssignment(a, ctx),
            VariableNode v => EvaluateVariable(v, ctx),
            ConditionalNode c => EvaluateConditional(c, ctx),
            FunctionCallNode f => EvaluateFunctionCall(f, ctx),
            ScriptNode s => EvaluateScript(s, ctx),
            _ => throw new NotSupportedException($"Unsupported node type {node.GetType().Name}")
        };
    }

    private static INode EvaluateBinary(BinaryExpressionNode b, EvaluationContext ctx)
    {
        var leftNode = Evaluate(b.Left, ctx);
        var rightNode = Evaluate(b.Right, ctx);

        // Numeric arithmetic & comparisons
        if (leftNode is NumberNode leftNum && rightNode is NumberNode rightNum)
        {
            var lv = leftNum.Value;
            var rv = rightNum.Value;
            double numResult = b switch
            {
                AddNode => lv + rv,
                SubtractNode => lv - rv,
                MultiplyNode => lv * rv,
                DivideNode => rv == 0 ? throw new DivideByZeroException() : lv / rv,
                EqualNode => lv == rv ? 1.0 : 0.0,
                GreaterThanNode => lv > rv ? 1.0 : 0.0,
                _ => throw new NotSupportedException($"Binary op node type {b.GetType().Name} not supported")
            };
            return new NumberNode(numResult);
        }

        // Vector + Vector, Vector - Vector
        if (leftNode is VectorNode leftVec && rightNode is VectorNode rightVec)
        {
            if (leftVec.Components.Count != rightVec.Components.Count)
                throw new InvalidOperationException("Vector dimension mismatch");
            var comps = leftVec.Components.Zip(rightVec.Components, (l, r) => b switch
            {
                AddNode => l + r,
                SubtractNode => l - r,
                _ => throw new NotSupportedException("Only + or - supported for vector-vector")
            }).ToList();
            return new VectorNode(comps);
        }

        // Vector + Number or Number + Vector (scalar add/subtract each component)
        if ((leftNode is VectorNode && rightNode is NumberNode) || (leftNode is NumberNode && rightNode is VectorNode))
        {
            // Only support Add/Subtract
            if (b is AddNode or SubtractNode)
            {
                VectorNode vec;
                double scalar;
                bool vectorIsLeft = leftNode is VectorNode;
                if (vectorIsLeft)
                {
                    vec = (VectorNode)leftNode;
                    scalar = ((NumberNode)rightNode).Value;
                }
                else
                {
                    vec = (VectorNode)rightNode;
                    scalar = ((NumberNode)leftNode).Value;
                }
                var comps = vec.Components.Select(c => b is AddNode ? c + scalar : c - scalar);
                return new VectorNode(comps);
            }
        }

        // Vector * Number or Number * Vector (scalar multiplication)
        if (b is MultiplyNode && ((leftNode is VectorNode && rightNode is NumberNode) || (leftNode is NumberNode && rightNode is VectorNode)))
        {
            VectorNode vec;
            double scalar;
            if (leftNode is VectorNode v1 && rightNode is NumberNode n2)
            {
                vec = v1;
                scalar = n2.Value;
            }
            else if (leftNode is NumberNode n1 && rightNode is VectorNode v2)
            {
                vec = v2;
                scalar = n1.Value;
            }
            else
            {
                throw new NotSupportedException("Invalid scalar-vector multiplication operands");
            }
            return new VectorNode(vec.Components.Select(c => c * scalar));
        }

        throw new NotSupportedException($"Unsupported operand type combination {leftNode.GetType().Name} {rightNode.GetType().Name} for {b.GetType().Name}");
    }

    private static INode EvaluateAssignment(AssignmentNode a, EvaluationContext ctx)
    {
        var valueNode = Evaluate(a.Expression, ctx);
        ctx.Set(a.VariableName, valueNode);
        return valueNode;
    }

    private static INode EvaluateVariable(VariableNode v, EvaluationContext ctx)
    {
        if (ctx.TryGet(v.Name, out var existing))
        {
            return existing;
        }
        // If variable holds an initial Value subtree, evaluate once and store.
        var evaluated = Evaluate(v.Value, ctx);
        ctx.Set(v.Name, evaluated);
        return evaluated;
    }

    private static INode EvaluateConditional(ConditionalNode c, EvaluationContext ctx)
    {
        var cond = Evaluate(c.Condition, ctx) as NumberNode ?? throw new InvalidOperationException("Condition must evaluate to number");
        return cond.Value != 0 ? Evaluate(c.ThenBranch, ctx) : Evaluate(c.ElseBranch, ctx);
    }

    private static INode EvaluateFunctionCall(FunctionCallNode f, EvaluationContext ctx)
    {
        if (!ctx.TryGetFunction(f.FunctionName, out var fn))
            throw new InvalidOperationException($"Function '{f.FunctionName}' not registered");
        var evaluatedArgs = f.Arguments.Select(a => Evaluate(a, ctx)).ToArray();
        return fn(ctx, evaluatedArgs);
    }

    private static INode EvaluateScript(ScriptNode s, EvaluationContext ctx)
    {
        INode last = new NumberNode(0);
        foreach (var step in s.Statements)
        {
            last = Evaluate(step, ctx);
        }
        return last;
    }
}
