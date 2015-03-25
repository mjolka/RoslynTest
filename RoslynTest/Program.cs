using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace RoslynTest
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        public async Task<IImmutableList<INamedTypeSymbol>> GetMatchingInterfacesInSolution(string solutionPath, Func<string, bool> predicate)
        {
            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(solutionPath);
            var compilations = await Task.WhenAll(solution.Projects.Select(x => x.GetCompilationAsync()));

            return compilations
                .SelectMany(x => x.SyntaxTrees.Select(y => new { Compilation = x, SyntaxTree = y }))
                .Select(x => x.Compilation.GetSemanticModel(x.SyntaxTree))
                .SelectMany(
                    x => x
                        .SyntaxTree
                        .GetRoot()
                        .DescendantNodes()
                        .OfType<InterfaceDeclarationSyntax>()
                        .Select(y => x.GetDeclaredSymbol(y)))
                .Where(x => predicate(x.ToDisplayString()))
                .ToImmutableList();
        }
    }
}
