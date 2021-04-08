using System.Diagnostics;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TheRing.Generators
{
    [Generator]
    public class AsyncGenerator: ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ProtocolInterfaceSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // the generator infrastructure will create a receiver and populate it
            // we can retrieve the populated instance via the context
            if (context.SyntaxReceiver is ProtocolInterfaceSyntaxReceiver syntaxReceiver)
            {
                // get the recorded user class
                foreach (var iface in syntaxReceiver.InterfacesToImplement)
                {
                    var model = context.Compilation.GetSemanticModel(iface.SyntaxTree);

                    var ifaceSymbol = model.GetDeclaredSymbol(iface) as ITypeSymbol;

                    // TODO: base interfaces too

                    var sb = new StringBuilder();

                    var className = $"Async{iface.Identifier.ToString().TrimStart('I')}";

                    sb.Append($@"
using TheRing.Common;
using System;
namespace {ifaceSymbol.ContainingNamespace}
{{
public class {className}: {ifaceSymbol}
{{ 
    private readonly ITaskProducer<{ifaceSymbol}> m_producer;

    public AsyncSomething(ITaskProducer<{ifaceSymbol}> producer)
    {{ m_producer = producer; }}");

                    foreach (var member in iface.Members)
                    {
                        if (member is MethodDeclarationSyntax method)
                        {
                            
                            var parameterDeclaration =
                                method.ParameterList.Parameters.Select(parameter =>
                                {
                                    var parameterSymbol = (IParameterSymbol)model.GetDeclaredSymbol(parameter);
                                    return $"{parameterSymbol.Type} {parameter.Identifier}";
                                });
                            var actualArguments = method.ParameterList.Parameters.Select(parameter => parameter.Identifier.ToString());
                           sb.Append($@"
public {method.ReturnType} {method.Identifier}({string.Join(", ", parameterDeclaration)}) {{ 
     m_producer.Add(__p => __p.{method.Identifier}({string.Join(", ", actualArguments)}));
}}");
                        }

                    }

                    sb.Append("} }");

                    // add the generated implementation to the compilation
                    SourceText sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);
                    context.AddSource($"{className}.g.cs", sourceText);
                }
            }
        }
    }
}