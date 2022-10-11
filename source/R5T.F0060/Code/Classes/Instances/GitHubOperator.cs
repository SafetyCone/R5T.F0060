using System;


namespace R5T.F0060
{
	public class GitHubOperator : IGitHubOperator
	{
		#region Infrastructure

	    public static IGitHubOperator Instance { get; } = new GitHubOperator();

	    private GitHubOperator()
	    {
        }

	    #endregion
	}
}