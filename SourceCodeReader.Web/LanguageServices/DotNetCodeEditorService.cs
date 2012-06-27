using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using SourceCodeReader.Web.Models;
using SourceCodeReader.Web.Infrastructure;
using Roslyn.Services;
using Roslyn.Compilers.Common;
using Ninject.Extensions.Logging;

namespace SourceCodeReader.Web.LanguageServices
{
    public class DotNetCodeEditorService : IEditorService
    {
        private IApplicationConfigurationProvider applicationConfigurationProvider;
        private ILogger logger;

        public DotNetCodeEditorService(IApplicationConfigurationProvider applicationConfigurationProvider, ILogger logger)
        {
            this.applicationConfigurationProvider = applicationConfigurationProvider;
            this.logger = logger;
        }

        public string BuildNavigatableSourceCodeFromFile(string filename)
        {
            var sourceCode = File.ReadAllText(filename);
            var fileExtension = Path.GetExtension(filename).ToLowerInvariant();
           
            if (fileExtension == ".cs")
            {
                var syntaxHighlighter = new CSharpSyntaxNavigationBuilder();
                return syntaxHighlighter.GetCodeAsNavigatableHtml(sourceCode);
            }
            else if (fileExtension == ".vb")
            {
                return sourceCode;
            }
            else
            {
                return sourceCode;
            }
        }

        public List<FindReferenceResult> FindRefernces(FindReferenceParameter parameter)
        {
            var projectSourceCodeDirectory =  this.applicationConfigurationProvider.GetProjectSourceCodePath(parameter.Username, parameter.Project);
            var projectCodeDirectory = new DirectoryInfo(projectSourceCodeDirectory).GetDirectories()[0];
            var solutionPath = FindSolutionPath(projectCodeDirectory, parameter.Project);
            var result = new List<FindReferenceResult>();
            if (solutionPath == null)
            {
                return result;
            }

            try
            {
                var workspace = Roslyn.Services.Workspace.LoadSolution(solutionPath);
                var currentFilePath = Path.Combine(projectCodeDirectory.FullName, parameter.Path.Replace(@"/", @"\"));
                var solution = workspace.CurrentSolution;

                var currentDocument = solution.Projects.SelectMany(project => project.Documents).SingleOrDefault(document => document.FilePath.Equals(currentFilePath, StringComparison.OrdinalIgnoreCase));
                if (currentDocument == null)
                {
                    return result;
                }

                var currentFileSemanticModel = currentDocument.GetSemanticModel();
                if (currentFileSemanticModel == null)
                {
                    return result;
                }

                var searchedWordSyntaxNode = currentFileSemanticModel.SyntaxTree.GetRoot().FindToken(parameter.Position).Parent;

                var symbolInfo = currentFileSemanticModel.GetSymbolInfo(searchedWordSyntaxNode);

                if (symbolInfo.Symbol == null)
                {
                    return result;
                }

                var foundReferences = symbolInfo.Symbol.FindReferences(solution);

                foreach (var reference in foundReferences)
                {
                    foreach (var location in reference.Locations)
                    {
                        result.Add(new FindReferenceResult
                      {
                          FileName = location.Document.Name,
                          Path = location.Document.FilePath,
                          Position = location.Location.GetLineSpan(true).StartLinePosition.Line
                      });
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Error occured while trying to find reference in project {0} for a symbol {1}", parameter.Project, parameter.Text);
            }

            return result;
        }

        private string FindSolutionPath(DirectoryInfo projectDirectory, string project)
        {
            var solutions = projectDirectory.GetFiles("*.sln", SearchOption.AllDirectories);
            if (solutions.Length > 0)
            {
                // Check for a solution with project name
                var selectedSolution = solutions.SingleOrDefault(x => Path.GetFileNameWithoutExtension(x.FullName).Equals(project, StringComparison.OrdinalIgnoreCase));
                if (selectedSolution != null)
                {
                    return selectedSolution.FullName;
                }

                return solutions[0].FullName;
            }

            return null;
        }
    }
}