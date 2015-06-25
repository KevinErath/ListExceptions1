using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ListExceptions1
{
    public class ClassInfo
    {
        public DocumentInfo Document { get; set; }
        public SyntaxNode Node { get; set; }
        public string Name => ((ClassDeclarationSyntax)Node).Identifier.ToString();
        public override int GetHashCode() { return Name.GetHashCode(); }
    }
}