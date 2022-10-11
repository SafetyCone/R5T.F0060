using System;
using System.Extensions;

using R5T.T0132;
using R5T.T0146;


namespace R5T.F0060
{
	[FunctionalityMarker]
	public partial interface IFileSystemOperator : IFunctionalityMarker,
		F0002.IFileSystemOperator
	{
		public new Result CopyFile(
			string sourceFilePath,
			string destinationFilePath)
		{
			var result = Instances.ResultOperator.Result()
				.WithTitle("Copy File")
				.WithMetadata("Source File Path", sourceFilePath)
				.WithMetadata("Destination File Path", destinationFilePath)
				;

			try
			{
				this.As<IFileSystemOperator, F0000.IFileSystemOperator>().CopyFile(
					sourceFilePath,
					destinationFilePath);

				result.WithReason(Instances.ReasonOperator.Success($"Successfully copied file."));
			}
			catch (Exception exception)
			{
				result.WithReason(Instances.ReasonOperator.Failure($"Failed to copied file.", exception));
			}

			return result;
		}

		public new Result CreateDirectory(string directoryPath)
		{
			var result = Instances.ResultOperator.Result()
				.WithTitle("Create Directory")
				.WithMetadata("Directory path", directoryPath)
				;

			try
			{
				this.As<IFileSystemOperator, F0000.IFileSystemOperator>().CreateDirectory(directoryPath);

				result.WithSuccess("Created directory.");
			}
			catch (Exception exception)
			{
				result.WithFailure("Unable to create directory.", exception);
			}

			return result;
		}

		public new Result<bool> DeleteDirectory_OkIfNotExists(string directoryPath)
		{
			var directoryExists = Instances.FileSystemOperator_Base.DirectoryExists(directoryPath);

			Instances.FileSystemOperator_Base.DeleteDirectory_OkIfNotExists(directoryPath);

			var message = directoryExists
				? $"Deleted directory: {directoryPath}"
				: $"Directory already did not exist; no need to delete: {directoryPath}"
				;

			var result = Instances.ResultOperator.Result<bool>()
				.WithTitle("Delete Directory")
				.WithValue(directoryExists)
				.WithSuccess(message)
				;

			return result;
		}

		public new Result<bool> DirectoryExists(string directoryPath)
		{
			var directoryExists = this.As<IFileSystemOperator, F0000.IFileSystemOperator>().DirectoryExists(directoryPath);

			// Always success, whether or not the directory exists is a different story.
			var successMessage = directoryExists
				? $"Directory exists."
				: $"Directory does not exist, or there was an error (perhaps pemissions?) accessing it."
				;

			var result = Instances.ResultOperator.Result<bool>()
				.WithTitle("Check Directory Exists")
				.WithValue(directoryExists)
				.WithSuccess(successMessage)
				;

			return result;
		}

		public new Result<bool> FileExists(string filePath)
		{
			var fileExists = F0000.Instances.FileSystemOperator.FileExists(filePath);

			var successMessage = fileExists
				? $"File exists: {filePath}"
				: $"File does not exist: {filePath}"
				;

			var result = T0146.Instances.ResultOperator.SuccessWithMessage(fileExists, successMessage)
				.WithTitle("Check File Exists")
				;

			return result;
		}
	}
}