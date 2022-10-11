using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using R5T.T0132;
using R5T.T0146;


namespace R5T.F0060
{
	[FunctionalityMarker]
	public partial interface IRemoteRepositoryOperator : IFunctionalityMarker
	{
		public Task<Result> VerifyRepositoryDoesNotExist(
			string repositoryOwnerName,
			string repositoryName,
			ILogger logger)
		{
			return Instances.GitHubOperator.VerifyRepositoryDoesNotExist(
				repositoryOwnerName,
				repositoryName,
				logger);
		}
	}
}