using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HorizontalPrivilegeEscalation.CodeAnalyzer;

public class ExpressionCallingCollector : CSharpSyntaxWalker
{
    private ICollection<MemberAccessExpressionSyntax> MemberAccessExpressions { get; } = new List<MemberAccessExpressionSyntax>();
    private ICollection<InvocationExpressionSyntax> InvocationExpressions { get; } = new List<InvocationExpressionSyntax>();

    public bool CallingAnyOfTheseMembers(params string[] memberName)
    {
        // This for calling members like "something.Member()"
        var anyEligibleMemberExpression = MemberAccessExpressions.Any(exp => memberName.Contains(exp.Name.ToString(), System.StringComparer.Ordinal));

        // This for calling members like "Member()" without "something"
        var anyEligibleInvocationExpression = InvocationExpressions.Any(exp =>
         {
             if (exp.Expression is not IdentifierNameSyntax nameSyntax)
             {
                 return false;
             }

             return memberName.Contains(nameSyntax.Identifier.Text, System.StringComparer.Ordinal);

         });

        return anyEligibleMemberExpression || anyEligibleInvocationExpression;
    }

    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        base.VisitMemberAccessExpression(node);

        MemberAccessExpressions.Add(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        base.VisitInvocationExpression(node);

        InvocationExpressions.Add(node);
    }
}