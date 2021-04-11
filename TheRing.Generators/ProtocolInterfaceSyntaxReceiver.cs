using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheRing.Generators
{
    class ProtocolInterfaceSyntaxReceiver : ISyntaxReceiver
    {
        public ProtocolInterfaceSyntaxReceiver()
        {
            InterfacesToImplement = new List<InterfaceDeclarationSyntax>();
        }

        public List<InterfaceDeclarationSyntax> InterfacesToImplement { get; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is InterfaceDeclarationSyntax cds &&
                cds.AttributeLists.Any(x => x.Attributes.Any(a =>
                {
                    return a.Name.ToString().Equals("RingProtocol");
                })))
            {
                InterfacesToImplement.Add(cds);
            }
        }
    }
}
