using System;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
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
            //System.Diagnostics.Debugger.Launch();

            // Check that the users compilation references the expected library 
            if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("TheRing.Common", StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "TR0001",
                        "Missing assembly reference",
                        "You must reference TheRing.Common in order to compile the generated code",
                        "Link",
                        DiagnosticSeverity.Error,
                        true), null));
            }

            if (context.SyntaxReceiver is ProtocolInterfaceSyntaxReceiver syntaxReceiver)
            {
                // Loop over the recorded user interfaces
                foreach (var iface in syntaxReceiver.InterfacesToImplement)
                {
                    var model = context.Compilation.GetSemanticModel(iface.SyntaxTree);

                    var ifaceSymbol = model.GetDeclaredSymbol(iface) as ITypeSymbol;
                    if (ifaceSymbol == null)
                        continue;

                    var sb = new StringBuilder();

                    var className = $"Async{iface.Identifier}";

                    var typeParameters = iface.TypeParameterList == null ? string.Empty : iface.TypeParameterList.ToString();
                    var typeConstraints = iface.ConstraintClauses.Any() == false ? string.Empty : iface.ConstraintClauses.ToString();

                    sb.Append($@"
using TheRing.Common;
using System;
namespace {ifaceSymbol.ContainingNamespace}
{{
public static class {iface.Identifier}Ext
{{
    public static {ifaceSymbol} ToAsync{typeParameters}(this ITaskProducer<{ifaceSymbol}> producer) {typeConstraints}
    {{
        return new AsyncWrapper{typeParameters}(producer);
    }}

private class AsyncWrapper{typeParameters}: {ifaceSymbol} {typeConstraints}
{{ 
    private readonly ITaskProducer<{ifaceSymbol}> m_producer;

    internal AsyncWrapper(ITaskProducer<{ifaceSymbol}> producer)
    {{ m_producer = producer; }}");

                    var inheritedMembers = ifaceSymbol.AllInterfaces.SelectMany(x => x.GetMembers());
                    var members = ifaceSymbol.GetMembers().Union(inheritedMembers, SymbolEqualityComparer.Default);

                    foreach (var member in members)
                    {
                        if (member is IMethodSymbol method)
                        {

                            var parameterDeclaration =
                                method.Parameters
                                    .Where(parameterSymbol => parameterSymbol != null)
                                    .Select(parameterSymbol => $"{parameterSymbol.Type} {parameterSymbol.Name}");

                            var actualArguments = method.Parameters.Select(parameter => parameter.Name.ToString());
                           sb.Append($@"
public {method.ReturnType} {method.Name}({string.Join(", ", parameterDeclaration)}) {{ 
     m_producer.Add(__p => __p.{method.Name}({string.Join(", ", actualArguments)}));
}}");
                        }

                    }

                    sb.Append("} } }");

                    // add the generated implementation to the compilation
                    var sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);
                    context.AddSource($"{className}.g.cs", sourceText);
                }
            }
        }
    }
}