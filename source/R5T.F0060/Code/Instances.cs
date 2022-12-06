using System;

using R5T.F0000;
using R5T.F0002;
using R5T.F0042;
using R5T.F0046;
using R5T.F0057;
using R5T.T0146;


namespace R5T.F0060
{
    public static class Instances
    {
        public static ICommitMessages CommitMessages { get; } = F0042.CommitMessages.Instance;
        public static F0057.IDirectoryPaths DirectoryPaths { get; } = F0057.DirectoryPaths.Instance;
        public static IFileSystemOperator FileSystemOperator { get; } = F0060.FileSystemOperator.Instance;
        public static F0002.IFileSystemOperator FileSystemOperator_Base { get; } = F0002.FileSystemOperator.Instance;
        public static IGitHubOperator GitHubOperator { get; } = F0060.GitHubOperator.Instance;
        public static IGitOperator GitOperator { get; } = F0060.GitOperator.Instance;
        public static ILocalRepositoryOperator LocalRepositoryOperator { get; } = F0060.LocalRepositoryOperator.Instance;
        public static F0002.IPathOperator PathOperator { get; } = F0002.PathOperator.Instance;
        public static IReasonOperator ReasonOperator { get; } = T0146.ReasonOperator.Instance;
        public static IRemoteRepositoryOperator RemoteRepositoryOperator { get; } = F0060.RemoteRepositoryOperator.Instance;
        public static IRepositoryDirectoryNameOperator RepositoryDirectoryNameOperator { get; } = F0057.RepositoryDirectoryNameOperator.Instance;
        public static IRepositoryDirectoryPathOperator RepositoryDirectoryPathOperator { get; } = F0057.RepositoryDirectoryPathOperator.Instance;
        public static IRepositoryNameOperator RepositoryNameOperator { get; } = F0046.RepositoryNameOperator.Instance;
        public static F0042.IRepositoryPathsOperator RepositoryPathsOperator { get; } = F0042.RepositoryPathsOperator.Instance;
        public static IResultOperator ResultOperator { get; } = T0146.ResultOperator.Instance;
        public static ITemplateFilePaths TemplateFilePaths { get; } = F0042.TemplateFilePaths.Instance;
    }
}