using System;


namespace R5T.F0060
{
    public static class Instances
    {
        public static Z0036.ICommitMessages CommitMessages => Z0036.CommitMessages.Instance;
        public static F0057.IDirectoryPaths DirectoryPaths => F0057.DirectoryPaths.Instance;
        public static IFileSystemOperator FileSystemOperator => F0060.FileSystemOperator.Instance;
        public static F0002.IFileSystemOperator FileSystemOperator_Base => F0002.FileSystemOperator.Instance;
        public static IGitHubOperator GitHubOperator => F0060.GitHubOperator.Instance;
        public static IGitOperator GitOperator => F0060.GitOperator.Instance;
        public static ILocalRepositoryOperator LocalRepositoryOperator => F0060.LocalRepositoryOperator.Instance;
        public static F0002.IPathOperator PathOperator => F0002.PathOperator.Instance;
        public static T0146.IReasonOperator ReasonOperator => T0146.ReasonOperator.Instance;
        public static IRemoteRepositoryOperator RemoteRepositoryOperator => F0060.RemoteRepositoryOperator.Instance;
        public static F0057.IRepositoryDirectoryNameOperator RepositoryDirectoryNameOperator => F0057.RepositoryDirectoryNameOperator.Instance;
        public static F0057.IRepositoryDirectoryPathOperator RepositoryDirectoryPathOperator => F0057.RepositoryDirectoryPathOperator.Instance;
        public static F0046.IRepositoryNameOperator RepositoryNameOperator => F0046.RepositoryNameOperator.Instance;
        public static F0042.IRepositoryPathsOperator RepositoryPathsOperator => F0042.RepositoryPathsOperator.Instance;
        public static T0146.IResultOperator ResultOperator => T0146.ResultOperator.Instance;
        public static F0042.ITemplateFilePaths TemplateFilePaths => F0042.TemplateFilePaths.Instance;
    }
}