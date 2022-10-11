using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using R5T.F0041;
using R5T.T0132;
using R5T.T0146;


namespace R5T.F0060
{
	[FunctionalityMarker]
	public partial interface IRepositoryOperator : IFunctionalityMarker,
		F0042.IRepositoryOperator,
		F0046.IRepositoryOperator
	{
		public new Result<string> Create_GitIgnoreFile_Idempotent(
			string repositoryDirectoryPath,
			ILogger logger)
		{
			var result = T0146.Instances.ResultOperator.Result<string>()
				.WithTitle("Create GitIgnore File")
				;

			var gitIgnoreFilePath = Instances.RepositoryPathsOperator.GetGitIgnoreFilePath(repositoryDirectoryPath);

			result.WithValue(gitIgnoreFilePath);

			logger.LogInformation("Checking if gitignore file exists...");

			var gitIgnoreFileExistsResult = Instances.FileSystemOperator.FileExists(gitIgnoreFilePath);

			var gitIgnoreFileExistsReason = gitIgnoreFileExistsResult.ToReason(
				$"GitIgnore file does not exist: {gitIgnoreFilePath}",
				$"GitIgnore file exists: {gitIgnoreFilePath}");

			result.WithReason(gitIgnoreFileExistsReason).WithChild(gitIgnoreFileExistsResult);

			var gitIgnoreFileExists = gitIgnoreFileExistsResult.Value;
			if (gitIgnoreFileExists)
			{
				logger.LogInformation($"Gitignore file exists:{Environment.NewLine}\t{gitIgnoreFilePath}");

				result.WithReason(
					T0146.Instances.ReasonOperator.Success("GitIgnore file already exists, no need to create it."));
			}
			else
			{
				logger.LogInformation($"Gitignore file does not exist. Copying template file...{Environment.NewLine}\tSource: {Instances.TemplateFilePaths.GitIgnoreTemplateFile}{Environment.NewLine}\tDestination: {gitIgnoreFilePath}");

				var copyGitIgnoreTemplateFileResult = Instances.FileSystemOperator.CopyFile(
					Instances.TemplateFilePaths.GitIgnoreTemplateFile,
					gitIgnoreFilePath);

				var copyGitIgnoreTemplateFileReason = copyGitIgnoreTemplateFileResult.ToReason(
					"Copied gitignore file template.",
					"Failed to copy gitignore file template.");

				result.WithReason(copyGitIgnoreTemplateFileReason).WithChild(copyGitIgnoreTemplateFileResult);

				logger.LogInformation($"Copied gitignore file:{Environment.NewLine}\t{gitIgnoreFilePath}");
			}

			return result;
		}

		public new Result<string> Create_SourceDirectory_Idempotent(
			string repositoryDirectoryPath,
			ILogger logger)
		{
			var result = Instances.ResultOperator.Result<string>()
				.WithTitle("Create Source Directory")
				.WithMetadata("Repository Directory path", repositoryDirectoryPath);
			;

			var repositorySourceDirectoryPath = Instances.RepositoryPathsOperator.GetSourceDirectoryPath(repositoryDirectoryPath);

			result.WithValue(repositorySourceDirectoryPath);

			logger.LogInformation("Checking if repository source directory exists.");

			var repositorySourceDirectoryExistsResult = Instances.FileSystemOperator.DirectoryExists(repositorySourceDirectoryPath);
			var repositorySourceDirectoryExists = repositorySourceDirectoryExistsResult.Value;

			var successMessage = repositorySourceDirectoryExists
				? "Source directory already exists."
				: "Source directory does not already exist."
				;

			result
				.WithReason(Instances.ReasonOperator.Success(successMessage))
				.WithChild(repositorySourceDirectoryExistsResult)
				;

			if (repositorySourceDirectoryExists)
			{
				logger.LogInformation($"Repository source directory exists:{Environment.NewLine}\t{repositorySourceDirectoryPath}");

				result.WithReason(Instances.ReasonOperator.Success("Source directory already exists, no need to create it."));
			}
			else
			{
				logger.LogInformation($"Repository source directory does not exist. Creating directory...{Environment.NewLine}\t{repositorySourceDirectoryPath}");

				var createDirectoryResult = Instances.FileSystemOperator.CreateDirectory(repositorySourceDirectoryPath);

				result.WithChild(createDirectoryResult);

				if (createDirectoryResult.IsSuccess())
				{
					logger.LogInformation($"Created repository source directory:{Environment.NewLine}\t{repositorySourceDirectoryPath}");

					result.WithSuccess("Created source directory.");
				}
				else
				{
					logger.LogError($"Failed to create repository source directory:{Environment.NewLine}\t{repositorySourceDirectoryPath}");

					result.WithFailure("Failed to create source directory.", createDirectoryResult.Failures);
				}
			}

			return result;
		}

		/// <summary>
		/// Creates a repository and returns the local repository directory path.
		/// </summary>
		/// <returns>A result containing the local repository directory path.</returns>
		public new async Task<Result<string>> CreateNew_NonIdempotent(
			GitHubRepositorySpecification repositorySpecification,
			ILogger logger)
		{
			var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(
				repositorySpecification.Organization,
				repositorySpecification.Name);

			var repositoryName = repositorySpecification.Name;

			var result = T0146.Instances.ResultOperator.Result<string>()
				.WithTitle("Create new repository");

			async Task CreateRepository()
            {
				var createRepositoryResult = await Instances.GitHubOperator.CreateRepository_NonIdempotent(repositorySpecification);

				var createRepositoryReason = createRepositoryResult.ToReason(
					"Created remote repository.",
					"Failed to create remote repository.");

				result.WithReason(createRepositoryReason).WithChild(createRepositoryResult);
			}

			await logger.InLogContext(
				$"Creating new remote GitHub repository '{ownedRepositoryName}'...",
				$"Created new remote GitHub repository '{ownedRepositoryName}'.",
				CreateRepository);

			// Clone local.
			async Task<string> CloneLocal()
            {
				var cloneLocalResult = await Instances.GitOperator.Clone_NonIdempotent(
					repositoryName,
					repositorySpecification.Organization);

				var cloneLocalReason = cloneLocalResult.ToReason(
					"Cloned remote repository to local directory.",
					"Failed to clone remote repository to local directory.");

				result.WithReason(cloneLocalReason).WithChild(cloneLocalResult);

				return cloneLocalResult.Value;
			}

			var localRepositoryDirectoryPath = await logger.InLogContext(
				$"Cloning to local directory repository...",
				$"Cloned to local directory repository.",
				CloneLocal);

			logger.LogInformation($"New empty repository created.");

			result.WithValue(localRepositoryDirectoryPath);

			return result;
		}

		public new async Task<Result> Delete_Idempotent(
			string repositoryName,
			string repositoryOwnerName,
			ILogger logger)
		{
			var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(repositoryOwnerName, repositoryName);

			var repositoryDirectoryPath = Instances.RepositoryDirectoryPathOperator.GetRepositoryDirectory(repositoryOwnerName, repositoryName);

			logger.LogInformation($"Deleting repository '{repositoryName}'...");

			// Delete local.
			logger.LogInformation("Deleting local directory repository...");

			var deleteLocalDirectoryResult = Instances.FileSystemOperator.DeleteDirectory_OkIfNotExists(repositoryDirectoryPath);

			var message = deleteLocalDirectoryResult.Value
				? $"Deleted local repository directory: {repositoryDirectoryPath}"
				: $"Local repository directory already did not exist. No need to delete: {repositoryDirectoryPath}"
				;

			var deleteLocalDirectoryReason = T0146.Instances.ReasonOperator.Success(message);

			logger.LogInformation("Deleted local directory repository.");

			// Delete remote.
			logger.LogInformation("Deleting remote GitHub repository...");

			var deleteRepositoryResult = await Instances.GitHubOperator.DeleteRepository_Idempotent(
				repositoryOwnerName,
				repositoryName);

			var successMessage = deleteLocalDirectoryResult.Value
				? $"Deleted remote repository: {ownedRepositoryName}"
				: $"Remote repository already did not exist. No need to delete: {ownedRepositoryName}"
				;

			var deleteRepositoryReason = deleteRepositoryResult.ToReason(
				successMessage,
				$"Unable to delete remote repository: {ownedRepositoryName}");

			logger.LogInformation("Deleted remote GitHub repository.");

			logger.LogInformation($"Deleted repository '{repositoryName}'.");

			var result = T0146.Instances.ResultOperator.Result()
				.WithTitle("Delete Repository")
				.WithReasons(deleteLocalDirectoryReason, deleteRepositoryReason)
				.WithChildren(deleteLocalDirectoryResult, deleteRepositoryResult)
				;

			return result;
		}

		public new Result PerformInitialCommit(
			string repositoryLocalDirectoryPath,
			ILogger logger)
		{
			var result = Instances.ResultOperator.Result<bool>()
				.WithTitle("Perform Initial Commit")
				.WithMetadata("Repository directory path", repositoryLocalDirectoryPath)
				;

			var pushAllChangesResult = Instances.GitHubOperator.PushAllChanges(
				repositoryLocalDirectoryPath,
				Instances.CommitMessages.InitialCommit,
				logger);

			var pushAllChangesReason = pushAllChangesResult.ToReason(
				"Push all changes succeeded.",
				"Push all changes failed.");

			result.WithChild(pushAllChangesResult).WithReason(pushAllChangesReason);

			return result;
		}

		public new Result<(string gitIgnoreFilePath, string sourceDirectoryPath)> SetupRepository(
			string repositoryDirectoryPath,
			ILogger logger)
		{
			var result = Instances.ResultOperator.Result<(string gitIgnoreFilePath, string sourceDirectoryPath)>()
				.WithTitle("Setup Repository")
				.WithMetadata("Repository directory path", repositoryDirectoryPath)
				;

			// Gitignore file.
			var createGitIgnoreFileResult = this.Create_GitIgnoreFile_Idempotent(
				repositoryDirectoryPath,
				logger);

			IReason createGitIgnoreFileReason = createGitIgnoreFileResult.ToReason(
				"Created GitIgnore file.",
				"Failed to create GitIgnore file.");

			result.WithReason(createGitIgnoreFileReason).WithChild(createGitIgnoreFileResult);

			// Create repository source directory.
			var createSourceDirectoryResult = this.Create_SourceDirectory_Idempotent(
				repositoryDirectoryPath,
				logger);

			IReason createSourceDirectoryReason = createGitIgnoreFileResult.ToReason(
				"Created source directory.",
				"Failed to create source directory.");

			result.WithReason(createSourceDirectoryReason).WithChild(createSourceDirectoryResult);

			result.WithValue((createGitIgnoreFileResult.Value, createSourceDirectoryResult.Value));

			return result;
		}

		public async Task<Result> Verify_RepositoryDoesNotExist(
			string repositoryOwnerName,
			string repositoryName,
			ILogger logger)
		{
			var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(repositoryOwnerName, repositoryName);

			var result = Instances.ResultOperator.Result()
				.WithTitle($"Verify repository does not exist: {ownedRepositoryName}.");

			var verifyRemoteResult = await Instances.RemoteRepositoryOperator.VerifyRepositoryDoesNotExist(
				repositoryOwnerName,
				repositoryName,
				logger);

			var verifyLocalResult = Instances.LocalRepositoryOperator.VerifyRepositoryDoesNotExist(
				repositoryOwnerName,
				repositoryName,
				logger);

			var verifyRemoteReason = verifyRemoteResult.ToReason(
				$"Remote repository does not exist: {ownedRepositoryName}.",
				$"Remote repository already exists: {ownedRepositoryName}.");

			var verifyLocalReason = verifyRemoteResult.ToReason(
				$"Local repository does not exist: {verifyLocalResult.Value}.",
				$"Local repository already exists: {verifyLocalResult.Value}.");

			result = result
				.WithReasons(verifyRemoteReason, verifyLocalReason)
				.WithChildren(verifyRemoteResult, verifyLocalResult)
				;

			return result;
		}
	}
}