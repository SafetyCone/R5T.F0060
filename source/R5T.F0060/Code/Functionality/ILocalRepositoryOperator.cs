using System;

using Microsoft.Extensions.Logging;

using R5T.T0132;
using R5T.T0146;


namespace R5T.F0060
{
	[FunctionalityMarker]
	public partial interface ILocalRepositoryOperator : IFunctionalityMarker,
		F0042.ILocalRepositoryOperator
	{
		/// <summary>
		/// Verifies that the local repository directory does not exist.
		/// </summary>
		/// <returns>The local repository directory path.</returns>
		public Result<string> VerifyRepositoryDoesNotExist(
			string repositoryOwnerName,
			string repositoryName,
			ILogger logger)
		{
			var repositoryDirectoryPath = Instances.RepositoryDirectoryPathOperator.GetRepositoryDirectoryPath(
				repositoryOwnerName,
				repositoryName);

			var verifyResult = this.VerifyRepositoryDoesNotExist(
				repositoryDirectoryPath,
				logger);

			var output = Instances.ResultOperator.Result(verifyResult, repositoryDirectoryPath);
			return output;
		}

		public Result VerifyRepositoryDoesNotExist(
			string repositoryDirectoryPath,
			ILogger logger)
		{
			var result = Instances.ResultOperator.Result()
				.WithTitle($"Verify local repository does not exist: {repositoryDirectoryPath}");

			bool Internal()
            {
				var directoryExists = Instances.FileSystemOperator_Base.DirectoryExists(repositoryDirectoryPath);

				var success = !directoryExists;

				result.WithOutcome(
					success,
					$"Local repository does not exist: {repositoryDirectoryPath}",
					$"Local repository already exists: {repositoryDirectoryPath}");
				
				return success;
			}

			logger.InSuccessFailureLogContext(
				$"Verifying local repository does not exist...\n\t{repositoryDirectoryPath}",
				$"Local repository does not exist.\n\t{repositoryDirectoryPath}",
				$"Local repository already exists.\n\t{repositoryDirectoryPath}",
				Internal);

			return result;
		}
	}
}