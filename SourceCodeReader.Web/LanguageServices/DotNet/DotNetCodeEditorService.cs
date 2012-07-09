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
using System.Linq.Expressions;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    public class DotNetCodeEditorService : IEditorService
    {
        private IApplicationConfigurationProvider applicationConfigurationProvider;
        private ILogger logger;

        public DotNetCodeEditorService(
            IApplicationConfigurationProvider applicationConfigurationProvider,
            ILogger logger)
        {
            this.applicationConfigurationProvider = applicationConfigurationProvider;
            this.logger = logger;
        }

        public string BuildNavigatableSourceCodeFromFile(string filename)
        {
            var sourceCode = File.ReadAllText(filename);
            var fileExtension = Path.GetExtension(filename).ToLowerInvariant();

            ISyntaxNavigationBuilder syntaxNavigationBuilder = new DotNetSyntaxNavigationBuilder();

            if (fileExtension == ".cs")
            {
                return syntaxNavigationBuilder.GetCodeAsNavigatableHtml(sourceCode, new CSharpCodeNavigationSyntaxWalker());
            }
            else if (fileExtension == ".vb")
            {
                return syntaxNavigationBuilder.GetCodeAsNavigatableHtml(sourceCode, new VisualBasicCodeNavigationSyntaxWalker());
            }
            else
            {
                return sourceCode;
            }
        }


        public FindReferenceResult GoToDefinition(FindReferenceParameter parameter, IFindReferenceProgress findReferenceProgressListener)
        {
            FindReferenceResult result = null;
            var goToDefinitionSyntaxWalker = new GoToDefinitionSyntaxWalker();
            TokenKind searchedTokenKind = (TokenKind)Enum.Parse(typeof(TokenKind), parameter.Kind);
            TraverseThroughAllTheProjectFiles(parameter, findReferenceProgressListener, (documentName, documentRelativePath, syntaxRoot) =>
                {
                    goToDefinitionSyntaxWalker.DoVisit(syntaxRoot, parameter.Text, searchedTokenKind, (foundlocation) =>
                    {
                        result = new FindReferenceResult
                        {
                            FileName = documentName,
                            Path = documentRelativePath,
                            Position = foundlocation
                        };
                    });
                });

            findReferenceProgressListener.OnFindReferenceCompleted();

            return result;
        }

        public List<FindReferenceResult> FindRefernces(FindReferenceParameter parameter, IFindReferenceProgress findReferenceProgressListener)
        {
            var result = new List<FindReferenceResult>();
            var findReferenceSyntaxWalker = new FindReferenceSyntaxWalker();
            TraverseThroughAllTheProjectFiles(parameter, findReferenceProgressListener, (documentName, documentRelativePath, syntaxRoot) =>
            {
                findReferenceSyntaxWalker.DoVisit(syntaxRoot, parameter.Text, (foundlocation) =>
                {
                    result.Add(new FindReferenceResult { FileName = documentName, Path = documentRelativePath, Position = foundlocation });
                });
            });                               

            findReferenceProgressListener.OnFindReferenceCompleted(result.Count);
            return result;
        }

        private void TraverseThroughAllTheProjectFiles(FindReferenceParameter parameter, IFindReferenceProgress findReferenceProgressListener, Action<string, string, CommonSyntaxNode> visitorAction)
        {
            findReferenceProgressListener.OnFindReferenceStarted();

            var projectSourceCodeDirectory = this.applicationConfigurationProvider.GetProjectSourceCodePath(parameter.Username, parameter.Project);
            var projectCodeDirectory = new DirectoryInfo(projectSourceCodeDirectory).GetDirectories()[0];
            var solutionPath = FindSolutionPath(projectCodeDirectory, parameter.Project);
            if (solutionPath == null)
            {
                findReferenceProgressListener.OnFindReferenceCompleted(0);
                return;
            }

            findReferenceProgressListener.OnFindReferenceInProgress();

            var workspace = Roslyn.Services.Workspace.LoadSolution(solutionPath);
            var currentFilePath = Path.Combine(projectCodeDirectory.FullName, parameter.Path.Replace(@"/", @"\"));
            var solution = workspace.CurrentSolution;

            foreach (var project in solution.Projects)
            {

                try
                {
                    if (!project.HasDocuments)
                    {
                        continue;
                    }

                    foreach (var document in project.Documents)
                    {
                        var documentSemanticModel = document.GetSemanticModel();
                        var findReferenceSyntaxtWalker = new FindReferenceSyntaxWalker();
                        CommonSyntaxNode syntaxRoot = null;
                        if (documentSemanticModel.SyntaxTree.TryGetRoot(out syntaxRoot))
                        {
                            var documentRelativePath = new Uri(projectCodeDirectory.FullName + Path.DirectorySeparatorChar).MakeRelativeUri(new Uri(document.FilePath)).ToString();
                            visitorAction(document.Name, documentRelativePath, syntaxRoot);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "An error has occured while loading the project {0}", project.Name);
                }
            }
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