﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Ninject.Extensions.Logging;
using Roslyn.Services;
using SourceCodeReader.Web.Infrastructure;
using SourceCodeReader.Web.Models;

namespace SourceCodeReader.Web.LanguageServices.DotNet
{
    public class DotNetSourceCodeSearchService : ISourceCodeIndexingService, ISourceCodeQueryService, IDisposable
    {
        private IApplicationConfigurationProvider applicationConfigurationProvider;
        private ILogger logger;
        private static IndexWriter SourceCodeIndexWriter;
        private static readonly Object __indexWriterLock = new Object();
        private readonly Lucene.Net.Store.Directory indexDirectory;
        private readonly Analyzer analyzer;
        private static readonly ConcurrentDictionary<string, object> ProjectSpecificLock = new ConcurrentDictionary<string, object>();
        private bool disposed;

        private const string ItemIdentifier = "Identifier";
        private const string ItemPath = "Path";
        private const string ItemLocation = "Location";
        private const string ItemProjectIdentifier = "ProjectIdentifier";
        private const string ItemProjectPath = "ProjectPath";
        private const string ItemSolutionPath = "SolutionPath";

        public DotNetSourceCodeSearchService(IApplicationConfigurationProvider applicationConfigurationProvider,  
            Lucene.Net.Store.Directory indexDirectory,  
            Analyzer analyzer,
            ILogger logger)
        {
            this.applicationConfigurationProvider = applicationConfigurationProvider;
            this.analyzer = analyzer;
            this.indexDirectory = indexDirectory;
            this.logger = logger;
        }

        private void DoWriterAction(Action<IndexWriter> action)
        {
            lock (__indexWriterLock)
            {
                EnsureIndexWriter();
            }
            action(SourceCodeIndexWriter);
        }

        private T DoWriterAction<T>(Func<IndexWriter, T> action)
        {
            lock (__indexWriterLock)
            {
                EnsureIndexWriter();
            }
            return action(SourceCodeIndexWriter);
        }

        // Method should only be called from within a lock.
        void EnsureIndexWriter()
        {
            if (SourceCodeIndexWriter == null)
            {
                if (IndexWriter.IsLocked(this.indexDirectory))
                {
                    IndexWriter.Unlock(this.indexDirectory);
                }
                SourceCodeIndexWriter = new IndexWriter(this.indexDirectory, this.analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            }
        }     

        private IndexSearcher Searcher
        {
            get
            {
                return DoWriterAction(writer => new IndexSearcher(writer.GetReader()));
            }
        }

        public void IndexProject(string username, string projectName, string projectDirectory)
        {
            var projectRoot = new DirectoryInfo(projectDirectory);
            string projectIdentifier = string.Format("{0}_{1}", username, projectName).ToLower();
            string indexCreatedStatusCheckFilePath = Path.Combine(this.applicationConfigurationProvider.ApplicationDataRoot, string.Format("{0}.status", projectIdentifier));

            // Indexing is already done
            if (File.Exists(indexCreatedStatusCheckFilePath))
            {
                return;
            }

            object __projectSpecificLockObject = ProjectSpecificLock.GetOrAdd(projectIdentifier, new object());

            lock (__projectSpecificLockObject)
            {
                // Double checking
                if (File.Exists(indexCreatedStatusCheckFilePath))
                {
                    return;
                }

                var solutionsInTheProject = projectRoot.GetFiles("*.sln", SearchOption.AllDirectories).Select(fileInfo => fileInfo.FullName);
                var declaredItemsToIndex = BuildDocumentsForIndexing(projectRoot, solutionsInTheProject);

                // Delete everthing in the index first
                var deleteQuery = new TermQuery(new Term(ItemProjectIdentifier, projectIdentifier));
                DoWriterAction(writer => writer.DeleteDocuments(deleteQuery));

                foreach (var declaredItem in declaredItemsToIndex)
                {
                    var document = new Document();

                    document.Add(new Field(ItemIdentifier, declaredItem.Identifier, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    document.Add(new Field(ItemPath, declaredItem.Path, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    document.Add(new Field(ItemLocation, declaredItem.Location.ToString(), Field.Store.YES, Field.Index.NO));
                    document.Add(new Field(ItemProjectIdentifier, projectIdentifier, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    document.Add(new Field(ItemProjectPath, declaredItem.ProjectPath, Field.Store.YES, Field.Index.NO));
                    document.Add(new Field(ItemSolutionPath, declaredItem.SolutionPath, Field.Store.YES, Field.Index.NO));
                    try
                    {
                        DoWriterAction(writer => writer.AddDocument(document));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "An error has occured while indexing item {0}", declaredItem.Identifier);
                    }
                }

                DoWriterAction(writer => writer.Commit());

                File.WriteAllText(indexCreatedStatusCheckFilePath, "Created");
            }

            ProjectSpecificLock.TryRemove(projectIdentifier, out __projectSpecificLockObject);
        }     

        private IList<DeclaredItemDocument> BuildDocumentsForIndexing(DirectoryInfo projectRoot, IEnumerable<string> solutionsInTheProject)
        {
            var lockObject = new Object();
            var declaredItems = new List<DeclaredItemDocument>();

            Parallel.ForEach(solutionsInTheProject,
                    () => new List<DeclaredItemDocument>(),
                    (solutionFilename, loopState, declaredItemDocuments) =>
                    {
                        var solution = Workspace.LoadSolution(solutionFilename).CurrentSolution;
                        foreach (var project in solution.Projects)
                        {
                            try
                            {
                                foreach (var document in project.Documents)
                                {
                                    var semanticModel = document.GetSemanticModel();
                                    var syntaxRoot = semanticModel.SyntaxTree.GetRoot();
                                    var csharpCodeIndexingSyntaxWalker = new CSharpCodeIndexingSyntaxWalker();
                                    var declaredItemsInDocument = csharpCodeIndexingSyntaxWalker.DoVisit(semanticModel, syntaxRoot);
                                    // Update the remaining properties
                                    declaredItemsInDocument.Select(item => 
                                    {
                                        item.Path = projectRoot.MakeRelativePath(document.FilePath); 
                                        item.ProjectPath = projectRoot.MakeRelativePath(document.Project.FilePath);
                                        item.SolutionPath = projectRoot.MakeRelativePath(document.Project.Solution.FilePath);
                                        return item; 
                                    }).ToList();
                                    declaredItemDocuments.AddRange(declaredItemsInDocument);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "An error has occured while opening project {0}", project.FilePath);
                            }
                        }

                        return declaredItemDocuments;
                    },
                    (declaredItemDocuments) => // Result aggregation
                    {
                        lock (lockObject)
                        {
                            if (declaredItemDocuments != null)
                            {
                                declaredItems.AddRange(declaredItemDocuments);
                            }
                        }
                    }
                );

            return declaredItems;
        }


        public TokenResult FindExact(TokenParameter parameter)
        {
            IndexSearcher indexSearcher = Searcher;

            string projectIdentifier = string.Format("{0}_{1}", parameter.Username, parameter.Project).ToLower();
            var projectQuery = new TermQuery(new Term(ItemProjectIdentifier, projectIdentifier));
            var identfierQuery = new TermQuery(new Term(ItemIdentifier, string.Format("{0}", parameter.FullyQualifiedName)));

            var booleanQuery = new BooleanQuery();
            booleanQuery.Add(projectQuery, BooleanClause.Occur.MUST);
            booleanQuery.Add(identfierQuery, BooleanClause.Occur.MUST);

            Hits hits = indexSearcher.Search(booleanQuery);
            if (hits.Length() > 0)
            {
                var document = hits.Doc(0); // Take only the first one
                string filePath = document.Get(ItemPath);
                int location = Int32.Parse(document.Get(ItemLocation));

                var projectCodeDirectory = this.applicationConfigurationProvider.GetProjectSourceCodePath(parameter.Username, parameter.Project);
                //string relativePath = filePath //projectCodeDirectory.MakeRelativePath(filePath);
                return new Models.TokenResult
                {
                    FileName = Path.GetFileName(filePath),
                    Position = location,
                    Path = filePath
                };
            }
      
            return null;
        }

        public Models.DocumentInfo GetFileDetails(string filePath)
        {
            var query = new TermQuery(new Term(ItemPath, filePath));
            IndexSearcher indexSearcher = Searcher;
            Hits hits = indexSearcher.Search(query);
            if (hits.Length() > 0)
            {
                var document = hits.Doc(0); // Take only the first one
                return new Models.DocumentInfo
                {
                    Name = Path.GetFileName(document.Get(ItemPath)),
                    ProjectPath = document.Get(ItemProjectPath),
                    SolutionPath = document.Get(ItemSolutionPath)
                };
            }

            return null;
        }

        ~DotNetSourceCodeSearchService()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (__indexWriterLock)
            {
                if (!disposed)
                {
                    //Never checking for disposing = true because there are
                    //no managed resources to dispose

                    var writer = SourceCodeIndexWriter;

                    if (writer != null)
                    {
                        try
                        {
                            writer.Close();
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                        SourceCodeIndexWriter = null;
                    }

                    var directory = indexDirectory;
                    if (directory != null)
                    {
                        try
                        {
                            directory.Close();
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }

                    disposed = true;
                }
            }

            GC.SuppressFinalize(this);
        }     
    }
}