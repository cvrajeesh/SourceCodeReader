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
using System.Threading.Tasks;

namespace SourceCodeReader.Web.LanguageServices
{
    public class DotNetCodeEditorService : IEditorService
    {
        private IApplicationConfigurationProvider applicationConfigurationProvider;
        private IFindReferenceProgress findReferenceProgressListener;
        private ILogger logger;

        public DotNetCodeEditorService(
            IApplicationConfigurationProvider applicationConfigurationProvider, 
            IFindReferenceProgress findReferenceProgressListener,
            ILogger logger)
        {
            this.applicationConfigurationProvider = applicationConfigurationProvider;
            this.findReferenceProgressListener = findReferenceProgressListener;
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
            this.findReferenceProgressListener.OnFindReferenceStarted();

            var projectSourceCodeDirectory = this.applicationConfigurationProvider.GetProjectSourceCodePath(parameter.Username, parameter.Project);
            var projectCodeDirectory = new DirectoryInfo(projectSourceCodeDirectory).GetDirectories()[0];
            var solutionPath = FindSolutionPath(projectCodeDirectory, parameter.Project);
            var result = new List<FindReferenceResult>();
            if (solutionPath == null)
            {
                this.findReferenceProgressListener.OnFindReferenceCompleted(0);
                return result;
            }

            this.findReferenceProgressListener.OnFindReferenceInProgress();

            var workspace = Roslyn.Services.Workspace.LoadSolution(solutionPath);
            var currentFilePath = Path.Combine(projectCodeDirectory.FullName, parameter.Path.Replace(@"/", @"\"));
            var solution = workspace.CurrentSolution;

            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var documentSemanticModel = document.GetSemanticModel();
                    var findReferenceSyntaxtWalker = new FindReferenceSyntaxWalker();
                    CommonSyntaxNode syntaxRoot = null;
                    if (documentSemanticModel.SyntaxTree.TryGetRoot(out syntaxRoot))
                    {
                        findReferenceSyntaxtWalker.DoVisit(syntaxRoot, parameter.Text, (foundLocation) =>
                        {
                            result.Add(new FindReferenceResult { FileName = document.Name, Path = document.FilePath, Position = foundLocation });
                        });
                    }
                }
            }

            this.findReferenceProgressListener.OnFindReferenceCompleted(result.Count);
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