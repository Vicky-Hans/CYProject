using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace UnitySourceGenerator
{
    //[Generator]
    public class FileTransformGenerator //: ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new FileSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is FileSyntaxReceiver receiver))
            {
                return;
            }

            if(receiver.namedTypeSymbols.Count == 0)
            {
                return;
            }

            if(context.AdditionalFiles.Length== 0)
            {
                return;
            }

            // find anything that matches our files
            var myFiles = context.AdditionalFiles;
            string output = ProcessClass($"{myFiles.Length.ToString()} {receiver.namedTypeSymbols[0].ToDisplayString()}",myFiles);
            var sourceText = SourceText.From(output, Encoding.UTF8);

            context.AddSource($"testgenerated.cs", sourceText);
        }


        private void WriteContent(int intent, string content, StringBuilder stringBuilder)
        {
            for (int i = 0; i < intent; i++)
            {
                stringBuilder.Append("\t");
            }

            stringBuilder.Append(content);
            stringBuilder.Append(Environment.NewLine);
        }

        private string ProcessClass(string name, ImmutableArray<AdditionalText> files)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"// {name}");
            if(files.Length > 0)
            {
                stringBuilder.AppendLine($"// {files[0].Path}");
            }
            stringBuilder.AppendLine($"namespace DH.Config");
            stringBuilder.AppendLine("{");
            WriteContent(1, $"public static partial class ConfigCenter", stringBuilder);
            WriteContent(1, "{", stringBuilder);
            WriteContent(2, "private static int testCode;", stringBuilder);
            WriteContent(1, "}", stringBuilder);
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }
    }

    public partial class FileSyntaxReceiver : ISyntaxContextReceiver
    {
        public string ErrorMsg;
        public List<INamedTypeSymbol> namedTypeSymbols = new List<INamedTypeSymbol>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            try
            {
                if(context.Node is ClassDeclarationSyntax declarationSyntax)
                {
                    var classSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;
                    if(classSymbol?.ToDisplayString() == "DH.Config.ConfigCenter")
                    {
                        Console.WriteLine(classSymbol.ToDisplayString());
                        namedTypeSymbols.Add(classSymbol);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString();
            }
        }
    }
    }
