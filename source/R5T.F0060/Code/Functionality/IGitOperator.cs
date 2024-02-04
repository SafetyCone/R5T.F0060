using System;
using System.Extensions;
using System.Threading.Tasks;

using R5T.T0046;
using R5T.T0132;
using R5T.T0144;
using R5T.T0146;


namespace R5T.F0060
{
	[FunctionalityMarker]
	public partial interface IGitOperator : IFunctionalityMarker,
		F0041.IGitOperator
	{
		/// <summary>
		/// Clones a remote repository to a local directory in the GitHub repositories directory.
		/// Returns the local repository directory path.
		/// </summary>
		/// <returns>A result containing the local repository directory path.</returns>
		public new async Task<Result<string>> Clone_NonIdempotent(
			string repositoryName,
			string repositoryOwnerName)
		{
			var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(repositoryOwnerName, repositoryName);

			var cloneUrl = await Instances.GitHubOperator.GetCloneUrl(
				repositoryOwnerName,
				repositoryName);

			var ownerDirectoryName = Instances.RepositoryDirectoryNameOperator.GetRepositoryOwnerDirectoryName(repositoryOwnerName);

			var localOwnerRepositoryDirectoryPath = Instances.PathOperator.Get_DirectoryPath(
				Instances.DirectoryPaths.GitHubRepositoriesDirectory,
				ownerDirectoryName);

			var repositoryDirectoryName = Instances.RepositoryDirectoryNameOperator.GetRepositoryDirectoryName(repositoryName);

			var localRepositoryDirectoryPath = Instances.PathOperator.Get_DirectoryPath(
				localOwnerRepositoryDirectoryPath,
				repositoryDirectoryName);

			var authentication = await Instances.GitHubOperator.GetGitHubAuthentication();

			var result = T0146.Instances.ResultOperator.Result<string>()
				.WithTitle("Clone GitHub Repository Locally")
				;

			try
			{
				var _ = this.Clone_NonIdempotent(
					cloneUrl,
					localRepositoryDirectoryPath,
					authentication);

				result
					.WithValue(localRepositoryDirectoryPath)
					.WithSuccess($"Cloned GitHub repository '{ownedRepositoryName}' to local directory:\n{localRepositoryDirectoryPath}")
					;
			}
			catch (Exception exception)
			{
				result.WithFailure($"Failed to clone GitHub repository '{ownedRepositoryName}' to local directory:\n{localRepositoryDirectoryPath}", exception);
			}

			return result;
		}

		public new Result Commit(
			string localRepositoryDirectoryPath,
			string commitMessage,
			Author author)
		{
			var result = Instances.ResultOperator.Result<bool>()
				.WithTitle("Commit Changes")
				.WithMetadata("Repository directory path", localRepositoryDirectoryPath)
				.WithMetadata("Commit message", commitMessage)
				.WithMetadata("Author", author)
				;

			try
			{
				this.As<IGitOperator, F0041.IGitOperator>().Commit(
					localRepositoryDirectoryPath,
					commitMessage,
					author);

				result.WithSuccess("Git success: committed changes.");
			}
			catch (Exception exception)
			{
				result.WithFailure("Git failure: commit changes failed.", exception);
			}

			return result;
		}

		public new Result Commit(
			string localRepositoryDirectoryPath,
			string commitMessage)
		{
			var author = this.GetAuthor();

			var output = this.Commit(
				localRepositoryDirectoryPath,
				commitMessage,
				author);

			return output;
		}

		public Result<bool> HasUnpushedLocalChanges(string repositoryDirectoryPath)
		{
			var result = Instances.ResultOperator.Result<bool>()
				.WithTitle("Check if Any Unpushed Changes")
				.WithMetadata("Repository directory path", repositoryDirectoryPath)
				;

			try
			{
				var hasAnyUnpushedLocalChanges = this.Has_UnpushedChanges(repositoryDirectoryPath);

				result
					.WithValue(hasAnyUnpushedLocalChanges)
					.WithSuccess("Git success: check of any unpushed changes succeeded.")
					;
			}
			catch (Exception exception)
			{
				result.WithFailure("Git failure: check of any unpushed changes failed.", exception);
			}

			return result;
		}

		/// <inheritdoc cref="F0041.IGitOperator.Push(string, Authentication)"/>
		public new Result Push(
			string localRepositoryDirectoryPath,
			Authentication authentication)
		{
			var result = Instances.ResultOperator.Result<bool>()
				.WithTitle("Push Changes")
				.WithMetadata("Repository directory path", localRepositoryDirectoryPath)
				.WithMetadata("Username", authentication.Username)
				;

			try
			{
				this.As<IGitOperator, F0041.IGitOperator>().Push(
					localRepositoryDirectoryPath,
					authentication);

				result.WithSuccess("Git success: pushed changed.");
			}
			catch (Exception exception)
			{
				result.WithFailure("Git failure: pushed changes failed.", exception);
			}

			return result;
		}

		public new Result Push(string localRepositoryDirectoryPath)
		{
			var authentication = Instances.GitHubOperator.GetGitHubAuthentication_Synchronous();

			var output = this.Push(
				localRepositoryDirectoryPath,
				authentication);

			return output;
		}

		public Result StageAllUnstagedPaths(string repositoryDirectoryPath)
		{
			var result = Instances.ResultOperator.Result<bool>()
				.WithTitle("Stage All Unstaged Paths")
				.WithMetadata("Repository directory path", repositoryDirectoryPath)
				;

			try
			{
				this.Stage_UnstagedPaths(repositoryDirectoryPath);

				result.WithSuccess("Git success: staging all unstaged paths succeeded.");
			}
			catch (Exception exception)
			{
				result.WithFailure("Git failure: staging all unstaged paths failed.", exception);
			}

			return result;
		}
	}
}