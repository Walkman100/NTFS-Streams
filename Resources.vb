Imports System.IO

Friend NotInheritable Class Resources
	Private Sub New()
	End Sub
	' HACK: "dotnet build" doesn't support ResX files when targetting .NET 3.5
	' https://github.com/Microsoft/msbuild/issues/1333
	' https://github.com/Microsoft/msbuild/issues/2272

	Public Shared Function Error_AccessDenied_Path(path As String) As String
		Return "Access to the path '" & path & "' was denied."
	End Function
	Public Shared Function Error_AlreadyExists(path As String) As String
		Return "Cannot create '" & path & "' because a file or directory with the same name already exists."
	End Function
	Public Shared Function Error_DirectoryNotFound(path As String) As String
		Return "Could not find a part of the path '" & path & "'."
	End Function
	Public Shared Function Error_DriveNotFound(path As String) As String
		Return "Could not find the drive '" & path & "'. The drive might not be ready or might not be mapped."
	End Function
	Public Shared Function Error_FileAlreadyExists(path As String) As String
		Return "The file '" & path & "' already exists."
	End Function
	Public Shared Function Error_InvalidFileChars() As String
		Return "The specified stream name contains invalid characters."
	End Function
	Public Shared Function Error_InvalidMode(mode As FileMode) As String
		Return "The specified mode '" & Convert.ToString(mode) & "' is not supported."
	End Function
	Public Shared Function Error_NonFile(path As String) As String
		Return "The specified file name '" & path & "' is not a disk-based file."
	End Function
	Public Shared Function Error_SharingViolation(path As String) As String
		Return "The process cannot access the file '" & path & "' because it is being used by another process."
	End Function
	Public Shared Function Error_StreamExists(streamName As String, path As String) As String
		Return "The specified alternate data stream '" & streamName & "' already exists on file '" & path & "'."
	End Function
	Public Shared Function Error_StreamNotFound(streamName As String, path As String) As String
		Return "The specified alternate data stream '" & streamName & "' does not exist on file '" & path & "'."
	End Function
	Public Shared Function Error_UnknownError(errorCode As Integer) As String
		Return "Unknown error: " & errorCode & ""
	End Function
End Class
