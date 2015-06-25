using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ListExceptions1
{
    public class MethodInfo
    {
        public ClassInfo Parent { get; set; }
        public SyntaxNode Node { get; set; }
        public string Name => ((MethodDeclarationSyntax)Node).Identifier.ToString();
        public override int GetHashCode() { return Name.GetHashCode(); }
    }
}