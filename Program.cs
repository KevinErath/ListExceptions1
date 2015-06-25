using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace ListExceptions1
{
    internal static class Program
    {
        private static void Main()
        {
            var documents = ReadDocuments();
            var classes = FindClasses(documents);
            var methods = FindMethods(classes);
            var exceptions = FindExceptions(methods);
            PrintReport(exceptions);
        }

        private static IEnumerable<DocumentInfo> ReadDocuments()
        {
            var workspace = CreateWorkspace();
            var filename = GetFilenameFromCmd();
            var projects = LoadProjectsFromFile(filename, workspace);
            return OpenAllDocuments(projects);
        }

        private static Dictionary<ClassInfo, Dictionary<MethodInfo, List<string>>> FindExceptions(IEnumerable<MethodInfo> methods)
        {
            var result = new Dictionary<ClassInfo, Dictionary<MethodInfo, List<string>>>();
            foreach (var element in methods)
            {
                var throwStatements = element.Node.DescendantNodes().OfType<ThrowStatementSyntax>().ToList();
                if (!throwStatements.Any())
                {
                    continue;
                }

                if (!result.ContainsKey(element.Parent))
                {
                    result[element.Parent] = new Dictionary<MethodInfo, List<string>>();
                }
                var clsDesc = result[element.Parent];

                var name = element;
                if (!clsDesc.ContainsKey(name))
                {
                    clsDesc[name] = new List<string>();
                }
                var nodeDesc = clsDesc[name];

                foreach (var stmt in throwStatements)
                {
                    var objCreation = stmt.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
                    if (objCreation != null)
                    {
                       nodeDesc.Add(objCreation.Type.ToString());
                    }
                }
            }
            return result;
        }

        private static IEnumerable<MethodInfo> FindMethods(IEnumerable<ClassInfo> classes)
        {
            return from cls in classes
                from node in cls.Node.DescendantNodes().OfType<MethodDeclarationSyntax>()
                select new MethodInfo {Parent = cls, Node = node};
        }

        private static void PrintReport(Dictionary<ClassInfo, Dictionary<MethodInfo, List<string>>> exceptions)
        {
            foreach (var clsDesc in exceptions)
            {
                Console.WriteLine("{0}", clsDesc.Key.Name);
                foreach (var methodDesc in clsDesc.Value)
                {
                    Console.WriteLine("- {0}", methodDesc.Key.Name);
                    foreach (var exc in methodDesc.Value)
                    {
                        Console.WriteLine("  - {0}", exc);
                    }
                }
            }
        }

        private static IEnumerable<ClassInfo> FindClasses(IEnumerable<DocumentInfo> documents)
        {
            return documents.SelectMany(x => x.Node.DescendantNodes().OfType<ClassDeclarationSyntax>().Select(y => new ClassInfo {Document = x, Node = y}));
        }

        private static IEnumerable<DocumentInfo> OpenAllDocuments(IEnumerable<Project> projects)
        {
            var documents = projects.SelectMany(x => x.Documents);
            // return documents.Select(x => new DocumentInfo {Node = x.GetSyntaxRootAsync().Result, Model = x.GetSemanticModelAsync().Result});
            return documents.Select(x => new DocumentInfo {Node = x.GetSyntaxRootAsync().Result});
        }

        private static IEnumerable<Project> LoadProjectsFromFile(string filename, MSBuildWorkspace workspace)
        {
            if (Path.GetExtension(filename) == ".sln")
            {
                var solution = workspace.OpenSolutionAsync(filename).Result;
                return solution.Projects;
            }

            var project = workspace.OpenProjectAsync(filename).Result;
            return new List<Project> {project};
        }

        private static MSBuildWorkspace CreateWorkspace()
        {
            return MSBuildWorkspace.Create();
        }

        private static string GetFilenameFromCmd()
        {
            return Environment.GetCommandLineArgs()[1];
        }
    }
}