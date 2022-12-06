using System;
using System.Extensions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Octokit;

using R5T.F0041;
using R5T.T0132;
using R5T.T0146;


namespace R5T.F0060
{
	[FunctionalityMarker]
	public partial interface IGitHubOperator : IFunctionalityMarker,
        F0041.IGitHubOperator
	{
        /// <summary>
        /// Creates a repository, and returns the repository ID.
        /// </summary>
        /// <returns>A result containing the repository ID.</returns>
        public new async Task<Result<long>> CreateRepository_NonIdempotent(GitHubRepositorySpecification repositorySpecification)
        {
            var autoInit = repositorySpecification.InitializeWithReadMe;

            var licenseTemplate = repositorySpecification.License.GetLicenseIdentifier();

            var @private = repositorySpecification.Visibility.IsPrivate();

            var newRepository = new NewRepository(repositorySpecification.Name)
            {
                AutoInit = autoInit,
                Description = repositorySpecification.Description,
                LicenseTemplate = licenseTemplate,
                Private = @private, // Default is public (private = false).
            };

            var gitHubClient = await this.GetGitHubClient();

            var currentUser = await gitHubClient.User.Current();

            var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(
                repositorySpecification.Organization,
                repositorySpecification.Name);

            var result = T0146.Instances.ResultOperator.Result<long>()
                .WithTitle("Create new GitHub Repository")
                ;

            try
            {
                var createdRepository = currentUser.Login == repositorySpecification.Organization
                    ? await gitHubClient.Repository.Create(newRepository)
                    : await gitHubClient.Repository.Create(
                        repositorySpecification.Organization,
                        newRepository)
                    ;

                // Wait a few seconds for the repository to be fully created on GitHub's side. This allows any following operations that request the repository to succeed.
                await Task.Delay(3000);

                result
                    .WithValue(createdRepository.Id)
                    .WithSuccess($"Created new GitHub repository: {ownedRepositoryName}")
                    ;
            }
            catch (Exception exception)
            {
                result.WithFailure($"GitHub repository creation failed: {ownedRepositoryName}", exception);
            }

            return result;
        }

        public new async Task<Result<bool>> DeleteRepository_Idempotent(string owner, string name)
        {
            var result = T0146.Instances.ResultOperator.Result<bool>()
                .WithTitle("Delete GitHub Repository")
                ;

            var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(owner, name);

            var repositoryExistsResult = await this.RepositoryExists(owner, name);
            var repositoryExists = repositoryExistsResult.Value;

            result.WithValue(repositoryExists);

            var repositoryExistsReason = repositoryExistsResult.ToReason(
                $"Repository '{ownedRepositoryName}' exists: {repositoryExists}",
                $"Repository existence check failed: {ownedRepositoryName}");

            result.WithReason(repositoryExistsReason).WithChild(repositoryExistsResult);

            if (repositoryExists)
            {
                var deleteRepositoryResult = await this.DeleteRepository_NonIdempotent(owner, name);

                var deleteRepositoryReason = deleteRepositoryResult.ToReason(
                    $"Deleted GitHub repository: {ownedRepositoryName}",
                    $"Unable to delete GitHub repository: {ownedRepositoryName}");

                result.WithReason(deleteRepositoryReason).WithChild(deleteRepositoryResult);
            }
            else
            {
                result.WithReason(T0146.Instances.ReasonOperator.Success($"Repository '{ownedRepositoryName}' already did not exist, no need to delete"));
            }

            return result;
        }

        public new async Task<Result> DeleteRepository_NonIdempotent(string owner, string name)
        {
            var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(owner, name);

            var result = T0146.Instances.ResultOperator.Result()
                .WithTitle("Delete GitHub Repository")
                ;

            try
            {
                var gitHubClient = await this.GetGitHubClient();

                await gitHubClient.Repository.Delete(owner, name);

                result.WithSuccess($"Deleted GitHub repository: {ownedRepositoryName}");
            }
            catch (Exception exception)
            {
                result.WithFailure($"Unable to delete GitHub repository: {ownedRepositoryName}", exception);
            }

            return result;
        }

        /// <summary>
        /// Stages, commits, and pushes all changes in a local directory path using the given commit message.
        /// </summary>
        /// <returns>True if any unpushed changes were pushed (there were any changes to push), false if not (there were no unpushed changes to push).</returns>
        public new Result<bool> PushAllChanges(
            string repositoryLocalDirectoryPath,
            string commitMessage,
            ILogger logger)
        {
            var result = Instances.ResultOperator.Result<bool>()
                .WithTitle("Push All Changes")
                .WithMetadata("Repository local eirectory path", repositoryLocalDirectoryPath)
                .WithMetadata("Commit message", commitMessage)
                ;

            logger.LogInformation($"Checking whether repository has any unpushed changes...\n\t{repositoryLocalDirectoryPath}");

            var hasAnyUnpushedChangesResult = Instances.GitOperator.HasUnpushedLocalChanges(repositoryLocalDirectoryPath);
            var hasAnyUnpushedChanges = hasAnyUnpushedChangesResult.Value;

            logger.LogInformation($"Checked whether repository has any unpushed changes.\n\t{repositoryLocalDirectoryPath}");

            IReason hasAnyUnpushedChangesReason = hasAnyUnpushedChangesResult.ToReason(
                $"Unpushed changes check succeed. Any unpushed changes: {hasAnyUnpushedChanges}",
                "Unpushed changes check failed.");

            result.WithChild(hasAnyUnpushedChangesResult).WithReason(hasAnyUnpushedChangesReason);

            if (hasAnyUnpushedChanges)
            {
                result.WithValue(true);

                logger.LogInformation("Unpushed changes detected.");

                // Stage all unstaged paths.
                var stageAllUnstagedPathsResult = Instances.GitOperator.StageAllUnstagedPaths(repositoryLocalDirectoryPath);

                var stageAllUnstagedPathsReason = stageAllUnstagedPathsResult.ToReason(
                    "All unstaged paths staged.",
                    "Failed to stage all unstaged paths.");

                result.WithChild(stageAllUnstagedPathsResult).WithReason(stageAllUnstagedPathsReason);

                // Commit changes with commit message.
                var commitChangesResult = Instances.GitOperator.Commit(
                    repositoryLocalDirectoryPath,
                    commitMessage);

                var commitChangesReason = commitChangesResult.ToReason(
                    "Commited changes.",
                    "Failed to commit changes.");

                result.WithChild(commitChangesResult).WithReason(commitChangesReason);

                // Push changes to GitHub.
                var pushChangesResult = Instances.GitOperator.Push(repositoryLocalDirectoryPath);

                IReason pushChangesReason = pushChangesResult.ToReason(
                    "Pushed changes.",
                    "Failed to push changes.");

                result.WithChild(pushChangesResult).WithReason(pushChangesReason);

                result.WithSuccess("Unpushed changes pushed.");
            }
            else
            {
                logger.LogInformation("No unpushed changes detected. No need to push changes.");

                result
                    .WithValue(false)
                    .WithSuccess("No unpushed changes detected. No need to push changes.")
                    ;
            }

            return result;
        }

        public new async Task<Result<bool>> RepositoryExists(string owner, string name)
        {
            var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(owner, name);

            var result = T0146.Instances.ResultOperator.Result<bool>()
                .WithTitle("Check GitHub Repository Exists")
                ;

            try
            {
                var repositoryExists = await this.As<IGitHubOperator, F0041.IGitHubOperator>().RepositoryExists(owner, name);

                var message = repositoryExists
                    ? $"GitHub repository exists: {ownedRepositoryName}"
                    : $"GitHub repository does not exist: {ownedRepositoryName}"
                    ;

                result
                    .WithValue(repositoryExists)
                    .WithSuccess(message)
                    ;
            }
            catch (Exception exception)
            {
                result.WithFailure($"Repository existence check failed: {ownedRepositoryName}", exception);
            }

            return result;
        }

        public async Task<Result> VerifyRepositoryDoesNotExist(
            string owner,
            string name,
            ILogger logger)
        {
            var ownedRepositoryName = Instances.RepositoryNameOperator.GetOwnedRepositoryName(owner, name);

            var result = Instances.ResultOperator.Result()
                .WithTitle($"Verify GitHub repository does not exist: {ownedRepositoryName}.");

            async Task<bool> Internal()
            {
                var repositoryExists = await this.As<IGitHubOperator, F0041.IGitHubOperator>().RepositoryExists(owner, name);

                var success = !repositoryExists;

                result.WithOutcome(
                    success,
                    $"GitHub repository does not exist: {ownedRepositoryName}.",
                    $"GitHub repository already exists: {ownedRepositoryName}.");

                return success;
            }

            await logger.InSuccessFailureLogContext(
                $"{ownedRepositoryName}: Verifying GitHub repository does not exist...",
                $"{ownedRepositoryName}: GitHub repository does not exist.",
                $"{ownedRepositoryName}: GitHub repository already exists.",
                Internal);

            return result;
        }
    }
}