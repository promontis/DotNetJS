﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator
{
    internal class GeneratedClass
    {
        private readonly ClassDeclarationSyntax syntax;
        private readonly IReadOnlyList<GeneratedMethod> methods;

        public GeneratedClass (ClassDeclarationSyntax syntax, IReadOnlyList<GeneratedMethod> methods)
        {
            this.syntax = syntax;
            this.methods = methods;
        }

        public string EmitSource (Compilation compilation)
        {
            return WrapDefines(
                EmitImport() +
                WrapNamespace(
                    EmitHeader() +
                    EmitMethods(compilation) +
                    EmitFooter()
                )
            );
        }

        private string WrapDefines (string source)
        {
            return "\n#nullable enable\n#pragma warning disable\n" +
                   source +
                   "\n#pragma warning restore\n#nullable restore\n";
        }

        private string EmitImport ()
        {
            var imports = syntax.SyntaxTree.GetRoot().DescendantNodesAndSelf().OfType<UsingDirectiveSyntax>();
            var result = string.Join("\n", imports);
            return string.IsNullOrEmpty(result) ? "" : result + "\n";
        }

        private string EmitHeader () => $"{syntax.Modifiers} class {syntax.Identifier}\n{{\n";

        private string EmitMethods (Compilation compilation)
        {
            var sources = methods.Select(m => "    " + m.EmitSource(compilation));
            return string.Join("\n", sources);
        }

        private string EmitFooter () => "\n}";

        private string WrapNamespace (string source)
        {
            if (syntax.Parent is NamespaceDeclarationSyntax space)
                return $"namespace {space.Name}\n{{\n{source}\n}}";
            if (syntax.Parent is FileScopedNamespaceDeclarationSyntax fileSpace)
                return $"namespace {fileSpace.Name};\n{source}";
            return source;
        }
    }
}
