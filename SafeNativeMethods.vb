'
'  * Trinet.Core.IO.Ntfs - Utilities for working with alternate data streams on NTFS file systems.
'  * Copyright (C) 2002-2016 Richard Deeming
'  * 
'  * This code is free software: you can redistribute it and/or modify it under the terms of either
'  * - the Code Project Open License (CPOL) version 1 or later; or
'  * - the GNU General Public License as published by the Free Software Foundation, version 3 or later; or
'  * - the BSD 2-Clause License;
'  * 
'  * This code is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
'  * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
'  * See the license files for details.
'  * 
'  * You should have received a copy of the licenses along with this code. 
'  * If not, see <http://www.codeproject.com/info/cpol10.aspx>, <http://www.gnu.org/licenses/> 
'  * and <http://opensource.org/licenses/bsd-license.php>.
'


Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.Win32.SafeHandles

''' <summary>
''' Safe native methods.
''' </summary>
Friend Module SafeNativeMethods
	#Region "Constants and flags"

	Public Const MaxPath As Integer = 256
	Private Const LongPathPrefix As String = "\\?\"
	Public Const StreamSeparator As Char = ":"C
	Public Const DefaultBufferSize As Integer = &H1000

	Private Const ErrorFileNotFound As Integer = 2
	Private Const ErrorPathNotFound As Integer = 3

	' "Characters whose integer representations are in the range from 1 through 31, 
	' except for alternate streams where these characters are allowed"
	' http://msdn.microsoft.com/en-us/library/aa365247(v=VS.85).aspx
	Private ReadOnly InvalidStreamNameChars As Char() = Path.GetInvalidFileNameChars().Where(Function(c) c < Chr(1) OrElse c > Chr(31)).ToArray()

	<Flags> _
	Public Enum NativeFileFlags As UInteger
		WriteThrough = &H80000000UI
		Overlapped = &H40000000
		NoBuffering = &H20000000
		RandomAccess = &H10000000
		SequentialScan = &H8000000
		DeleteOnClose = &H4000000
		BackupSemantics = &H2000000
		PosixSemantics = &H1000000
		OpenReparsePoint = &H200000
		OpenNoRecall = &H100000
	End Enum

	<Flags> _
	Public Enum NativeFileAccess As UInteger
		GenericRead = &H80000000UI
		GenericWrite = &H40000000
	End Enum

	#End Region

	#Region "P/Invoke Structures"

	<StructLayout(LayoutKind.Sequential)> _
	Private Structure LargeInteger
		Public ReadOnly Low As Integer
		Public ReadOnly High As Integer

		Public Function ToInt64() As Long
			Return (High * &H100000000L) + Low
		End Function
	End Structure

	<StructLayout(LayoutKind.Sequential)> _
	Private Structure Win32StreamId
		Public ReadOnly StreamId As Integer
		Public ReadOnly StreamAttributes As Integer
		Public Size As LargeInteger
		Public ReadOnly StreamNameSize As Integer
	End Structure

	#End Region

	#Region "P/Invoke Methods"

	<DllImport("kernel32.dll", CharSet := CharSet.Auto, BestFitMapping := False, ThrowOnUnmappableChar := True)> _
	Private Function FormatMessage(dwFlags As Integer, lpSource As IntPtr, dwMessageId As Integer, dwLanguageId As Integer, lpBuffer As StringBuilder, nSize As Integer, vaListArguments As IntPtr) As Integer
	End Function

	<DllImport("kernel32", CharSet := CharSet.Unicode, SetLastError := True)> _
	Private Function GetFileAttributes(fileName As String) As Integer
	End Function

	<DllImport("kernel32", CharSet := CharSet.Unicode, SetLastError := True)> _
	Private Function GetFileSizeEx(handle As SafeFileHandle, ByRef size As LargeInteger) As <MarshalAs(UnmanagedType.Bool)> Boolean
	End Function

	<DllImport("kernel32.dll")> _
	Private Function GetFileType(handle As SafeFileHandle) As Integer
	End Function

	<DllImport("kernel32", CharSet := CharSet.Unicode, SetLastError := True)> _
	Private Function CreateFile(name As String, access As NativeFileAccess, share As FileShare, security As IntPtr, mode As FileMode, flags As NativeFileFlags, template As IntPtr) As SafeFileHandle
	End Function

	<DllImport("kernel32", CharSet := CharSet.Unicode, SetLastError := True)> _
	Private Function DeleteFile(name As String) As <MarshalAs(UnmanagedType.Bool)> Boolean
	End Function

	<DllImport("kernel32", SetLastError := True)> _
	Private Function BackupRead(hFile As SafeFileHandle, ByRef pBuffer As Win32StreamId, numberOfBytesToRead As Integer, ByRef numberOfBytesRead As Integer, <MarshalAs(UnmanagedType.Bool)> abort As Boolean, <MarshalAs(UnmanagedType.Bool)> processSecurity As Boolean, ByRef context As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
	End Function

	<DllImport("kernel32", SetLastError := True)> _
	Private Function BackupRead(hFile As SafeFileHandle, pBuffer As SafeHGlobalHandle, numberOfBytesToRead As Integer, ByRef numberOfBytesRead As Integer, <MarshalAs(UnmanagedType.Bool)> abort As Boolean, <MarshalAs(UnmanagedType.Bool)> processSecurity As Boolean, ByRef context As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
	End Function

	<DllImport("kernel32", SetLastError := True)> _
	Private Function BackupSeek(hFile As SafeFileHandle, bytesToSeekLow As Integer, bytesToSeekHigh As Integer, ByRef bytesSeekedLow As Integer, ByRef bytesSeekedHigh As Integer, ByRef context As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
	End Function

	#End Region

	#Region "Utility Structures"

	Public Structure Win32StreamInfo
		Public StreamType As FileStreamType
		Public StreamAttributes As FileStreamAttributes
		Public StreamSize As Long
		Public StreamName As String
	End Structure

	#End Region

	#Region "Utility Methods"

	Private Function MakeHRFromErrorCode(errorCode As Integer) As Integer
		Return (-2147024896 Or errorCode)
	End Function

	Private Function GetErrorMessage(errorCode As Integer) As String
		Dim lpBuffer = New StringBuilder(&H200)
		If 0 <> FormatMessage(&H3200, IntPtr.Zero, errorCode, 0, lpBuffer, lpBuffer.Capacity, _
			IntPtr.Zero) Then
			Return lpBuffer.ToString()
		End If

		Return Resources.Error_UnknownError(errorCode)
	End Function

	Private Sub ThrowIOError(errorCode As Integer, path As String)
		Select Case errorCode
			Case 0
				If True Then
					Exit Select
				End If
			Case 2
				' File not found
				If True Then
					If String.IsNullOrEmpty(path) Then
						Throw New FileNotFoundException()
					End If
					Throw New FileNotFoundException(Nothing, path)
				End If
			Case 3
				' Directory not found
				If True Then
					If String.IsNullOrEmpty(path) Then
						Throw New DirectoryNotFoundException()
					End If
					Throw New DirectoryNotFoundException(Resources.Error_DirectoryNotFound(path))
				End If
			Case 5
				' Access denied
				If True Then
					If String.IsNullOrEmpty(path) Then
						Throw New UnauthorizedAccessException()
					End If
					Throw New UnauthorizedAccessException(Resources.Error_AccessDenied_Path(path))
				End If
			Case 15
				' Drive not found
				If True Then
					If String.IsNullOrEmpty(path) Then
						Throw New DriveNotFoundException()
					End If
					Throw New DriveNotFoundException(Resources.Error_DriveNotFound(path))
				End If
			Case 32
				' Sharing violation
				If True Then
					If String.IsNullOrEmpty(path) Then
						Throw New IOException(GetErrorMessage(errorCode), MakeHRFromErrorCode(errorCode))
					End If
					Throw New IOException(Resources.Error_SharingViolation(path), MakeHRFromErrorCode(errorCode))
				End If
			Case 80
				' File already exists
				If True Then
					If Not String.IsNullOrEmpty(path) Then
						Throw New IOException(Resources.Error_FileAlreadyExists(path), MakeHRFromErrorCode(errorCode))
					End If
					Exit Select
				End If
			Case 87
				' Invalid parameter
				If True Then
					Throw New IOException(GetErrorMessage(errorCode), MakeHRFromErrorCode(errorCode))
				End If
			Case 183
				' File or directory already exists
				If True Then
					If Not String.IsNullOrEmpty(path) Then
						Throw New IOException(Resources.Error_AlreadyExists(path), MakeHRFromErrorCode(errorCode))
					End If
					Exit Select
				End If
			Case 206
				' Path too long
				If True Then
					Throw New PathTooLongException()
				End If
			Case 995
				' Operation cancelled
				If True Then
					Throw New OperationCanceledException()
				End If
			Case Else
				If True Then
					Marshal.ThrowExceptionForHR(MakeHRFromErrorCode(errorCode))
					Exit Select
				End If
		End Select
	End Sub

	Public Sub ThrowLastIOError(path As String)
		Dim errorCode As Integer = Marshal.GetLastWin32Error()
		If 0 <> errorCode Then
			Dim hr As Integer = Marshal.GetHRForLastWin32Error()
			If 0 <= hr Then
				Throw New Win32Exception(errorCode)
			End If
			ThrowIOError(errorCode, path)
		End If
	End Sub

	<System.Runtime.CompilerServices.Extension> _
	Public Function ToNative(access As FileAccess) As NativeFileAccess
		Dim result As NativeFileAccess = 0
		If FileAccess.Read = (FileAccess.Read And access) Then
			result = result Or NativeFileAccess.GenericRead
		End If
		If FileAccess.Write = (FileAccess.Write And access) Then
			result = result Or NativeFileAccess.GenericWrite
		End If
		Return result
	End Function

	Public Function BuildStreamPath(filePath As String, streamName As String) As String
		If String.IsNullOrEmpty(filePath) Then
			Return String.Empty
		End If

		' Trailing slashes on directory paths don't work:

		Dim result As String = filePath
		Dim length As Integer = result.Length
		While 0 < length AndAlso "\"C = result(length - 1)
			length -= 1
		End While

		If length <> result.Length Then
			result = If(0 = length, ".", result.Substring(0, length))
		End If

		result += StreamSeparator & streamName & StreamSeparator & "$DATA"

		If MaxPath <= result.Length AndAlso Not result.StartsWith(LongPathPrefix) Then
			result = LongPathPrefix & result
		End If

		Return result
	End Function

	Public Sub ValidateStreamName(streamName As String)
		If Not String.IsNullOrEmpty(streamName) AndAlso -1 <> streamName.IndexOfAny(InvalidStreamNameChars) Then
			Throw New ArgumentException(Resources.Error_InvalidFileChars())
		End If
	End Sub

	Private Function SafeGetFileAttributes(name As String) As Integer
		If String.IsNullOrEmpty(name) Then
			Throw New ArgumentNullException("name")
		End If

		Dim result As Integer = GetFileAttributes(name)
		If -1 = result Then
			Dim errorCode As Integer = Marshal.GetLastWin32Error()
			Select Case errorCode
				Case ErrorFileNotFound, ErrorPathNotFound
					If True Then
						Exit Select
					End If
				Case Else
					If True Then
						ThrowLastIOError(name)
						Exit Select
					End If
			End Select
		End If

		Return result
	End Function

	Public Function FileExists(name As String) As Boolean
		Return -1 <> SafeGetFileAttributes(name)
	End Function

	Public Function SafeDeleteFile(name As String) As Boolean
		If String.IsNullOrEmpty(name) Then
			Throw New ArgumentNullException("name")
		End If

		Dim result As Boolean = DeleteFile(name)
		If Not result Then
			Dim errorCode As Integer = Marshal.GetLastWin32Error()
			Select Case errorCode
				Case ErrorFileNotFound, ErrorPathNotFound
					If True Then
						Exit Select
					End If
				Case Else
					If True Then
						ThrowLastIOError(name)
						Exit Select
					End If
			End Select
		End If

		Return result
	End Function

	Public Function SafeCreateFile(path As String, access As NativeFileAccess, share As FileShare, security As IntPtr, mode As FileMode, flags As NativeFileFlags, _
		template As IntPtr) As SafeFileHandle
		Dim result As SafeFileHandle = CreateFile(path, access, share, security, mode, flags, _
			template)
		If Not result.IsInvalid AndAlso 1 <> GetFileType(result) Then
			result.Dispose()
			Throw New NotSupportedException(Resources.Error_NonFile(path))
		End If

		Return result
	End Function

	Private Function GetFileSize(path As String, handle As SafeFileHandle) As Long
		Dim result As Long = 0L
		If handle IsNot Nothing AndAlso Not handle.IsInvalid Then
			Dim temp As Trinet.Core.IO.Ntfs.SafeNativeMethods.LargeInteger
			If GetFileSizeEx(handle, temp) Then
				result = temp.ToInt64()
			Else
				ThrowLastIOError(path)
			End If
		End If

		Return result
	End Function

	Public Function GetFileSize(path As String) As Long
		Dim result As Long = 0L
		If Not String.IsNullOrEmpty(path) Then
			Using handle As SafeFileHandle = SafeCreateFile(path, NativeFileAccess.GenericRead, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, _
				IntPtr.Zero)
				result = GetFileSize(path, handle)
			End Using
		End If

		Return result
	End Function

	Public Function ListStreams(filePath As String) As IList(Of Win32StreamInfo)
		If String.IsNullOrEmpty(filePath) Then
			Throw New ArgumentNullException("filePath")
		End If
		If -1 <> filePath.IndexOfAny(Path.GetInvalidPathChars()) Then
			Throw New ArgumentException(Resources.Error_InvalidFileChars(), "filePath")
		End If

		Dim result = New List(Of Win32StreamInfo)()

		Using hFile As SafeFileHandle = SafeCreateFile(filePath, NativeFileAccess.GenericRead, FileShare.Read, IntPtr.Zero, FileMode.Open, NativeFileFlags.BackupSemantics, _
			IntPtr.Zero)
			Using hName = New StreamName()
				If Not hFile.IsInvalid Then
					Dim streamId = New Win32StreamId()
					Dim dwStreamHeaderSize As Integer = Marshal.SizeOf(streamId)
					Dim finished As Boolean = False
					Dim context As IntPtr = IntPtr.Zero
					Dim bytesRead As Integer

					Try
						While Not finished
							' Read the next stream header:
							If Not BackupRead(hFile, streamId, dwStreamHeaderSize, bytesRead, False, False, _
								context) Then
								finished = True
							ElseIf dwStreamHeaderSize <> bytesRead Then
								finished = True
							Else
								' Read the stream name:
								Dim name As String
								If 0 >= streamId.StreamNameSize Then
									name = Nothing
								Else
									hName.EnsureCapacity(streamId.StreamNameSize)
									If Not BackupRead(hFile, hName.MemoryBlock, streamId.StreamNameSize, bytesRead, False, False, _
										context) Then
										name = Nothing
										finished = True
									Else
										' Unicode chars are 2 bytes:
										name = hName.ReadStreamName(bytesRead >> 1)
									End If
								End If

								' Add the stream info to the result:
								If Not String.IsNullOrEmpty(name) Then
									result.Add(New Win32StreamInfo() With { _
										.StreamType = DirectCast(streamId.StreamId, FileStreamType), _
										.StreamAttributes = DirectCast(streamId.StreamAttributes, FileStreamAttributes), _
										.StreamSize = streamId.Size.ToInt64(), _
										.StreamName = name _
									})
								End If

								' Skip the contents of the stream:
								If 0 <> streamId.Size.Low OrElse 0 <> streamId.Size.High Then
									If Not finished AndAlso Not BackupSeek(hFile, streamId.Size.Low, streamId.Size.High, 0, 0, context) Then
										finished = True
									End If
								End If
							End If
						End While
					Finally
						' Abort the backup:
						BackupRead(hFile, hName.MemoryBlock, 0, bytesRead, True, False, _
							context)
					End Try
				End If
			End Using
		End Using

		Return result
	End Function

	#End Region
End Module
