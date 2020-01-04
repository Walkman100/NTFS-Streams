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

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Security.Permissions

Namespace Trinet.Core.IO.Ntfs
	''' <summary>
	''' File-system utilities.
	''' </summary>
	Public Module FileSystem
		#Region "List Streams"
	
		''' <summary>
		''' <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
		''' Returns a read-only list of alternate data streams for the specified file.
		''' </summary>
		''' <param name="file">
		''' The <see cref="FileSystemInfo"/> to inspect.
		''' </param>
		''' <returns>
		''' A read-only list of <see cref="AlternateDataStreamInfo"/> objects
		''' representing the alternate data streams for the specified file, if any.
		''' If no streams are found, returns an empty list.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="file"/> is <see langword="null"/>.
		''' </exception>
		''' <exception cref="FileNotFoundException">
		''' The specified <paramref name="file"/> does not exist.
		''' </exception>
		''' <exception cref="SecurityException">
		''' The caller does not have the required permission. 
		''' </exception>
		''' <exception cref="UnauthorizedAccessException">
		''' The caller does not have the required permission.
		''' </exception>
		<System.Runtime.CompilerServices.Extension> _
		Public Function ListAlternateDataStreams(file As FileSystemInfo) As IList(Of AlternateDataStreamInfo)
			If file Is Nothing Then
				Throw New ArgumentNullException("file")
			End If
			If Not file.Exists Then
				Throw New FileNotFoundException(Nothing, file.FullName)
			End If
	
			Dim path As String = file.FullName
	
	#If NET35 Then
			New FileIOPermission(FileIOPermissionAccess.Read, path).Demand()
	#End If
	
			Return SafeNativeMethods.ListStreams(path).[Select](Function(s) New AlternateDataStreamInfo(path, s)).ToList().AsReadOnly()
		End Function
	
		''' <summary>
		''' Returns a read-only list of alternate data streams for the specified file.
		''' </summary>
		''' <param name="filePath">
		''' The full path of the file to inspect.
		''' </param>
		''' <returns>
		''' A read-only list of <see cref="AlternateDataStreamInfo"/> objects
		''' representing the alternate data streams for the specified file, if any.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="filePath"/> is <see langword="null"/> or an empty string.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <paramref name="filePath"/> is not a valid file path.
		''' </exception>
		''' <exception cref="FileNotFoundException">
		''' The specified <paramref name="filePath"/> does not exist.
		''' </exception>
		''' <exception cref="SecurityException">
		''' The caller does not have the required permission. 
		''' </exception>
		''' <exception cref="UnauthorizedAccessException">
		''' The caller does not have the required permission.
		''' </exception>
		Public Function ListAlternateDataStreams(filePath As String) As IList(Of AlternateDataStreamInfo)
			If String.IsNullOrEmpty(filePath) Then
				Throw New ArgumentNullException("filePath")
			End If
			If Not SafeNativeMethods.FileExists(filePath) Then
				Throw New FileNotFoundException(Nothing, filePath)
			End If
	
	#If NET35 Then
			New FileIOPermission(FileIOPermissionAccess.Read, filePath).Demand()
	#End If
	
			Return SafeNativeMethods.ListStreams(filePath).[Select](Function(s) New AlternateDataStreamInfo(filePath, s)).ToList().AsReadOnly()
		End Function
	
		#End Region
	
		#Region "Stream Exists"
	
		''' <summary>
		''' <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
		''' Returns a flag indicating whether the specified alternate data stream exists.
		''' </summary>
		''' <param name="file">
		''' The <see cref="FileInfo"/> to inspect.
		''' </param>
		''' <param name="streamName">
		''' The name of the stream to find.
		''' </param>
		''' <returns>
		''' <see langword="true"/> if the specified stream exists;
		''' otherwise, <see langword="false"/>.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="file"/> is <see langword="null"/>.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <paramref name="streamName"/> contains invalid characters.
		''' </exception>
		<System.Runtime.CompilerServices.Extension> _
		Public Function AlternateDataStreamExists(file As FileSystemInfo, streamName As String) As Boolean
			If file Is Nothing Then
				Throw New ArgumentNullException("file")
			End If
			SafeNativeMethods.ValidateStreamName(streamName)
	
			Dim path As String = SafeNativeMethods.BuildStreamPath(file.FullName, streamName)
			Return SafeNativeMethods.FileExists(path)
		End Function
	
		''' <summary>
		''' Returns a flag indicating whether the specified alternate data stream exists.
		''' </summary>
		''' <param name="filePath">
		''' The path of the file to inspect.
		''' </param>
		''' <param name="streamName">
		''' The name of the stream to find.
		''' </param>
		''' <returns>
		''' <see langword="true"/> if the specified stream exists;
		''' otherwise, <see langword="false"/>.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="filePath"/> is <see langword="null"/> or an empty string.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <para><paramref name="filePath"/> is not a valid file path.</para>
		''' <para>-or-</para>
		''' <para><paramref name="streamName"/> contains invalid characters.</para>
		''' </exception>
		Public Function AlternateDataStreamExists(filePath As String, streamName As String) As Boolean
			If String.IsNullOrEmpty(filePath) Then
				Throw New ArgumentNullException("filePath")
			End If
			SafeNativeMethods.ValidateStreamName(streamName)
	
			Dim path As String = SafeNativeMethods.BuildStreamPath(filePath, streamName)
			Return SafeNativeMethods.FileExists(path)
		End Function
	
		#End Region
	
		#Region "Open Stream"
	
		''' <summary>
		''' <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
		''' Opens an alternate data stream.
		''' </summary>
		''' <param name="file">
		''' The <see cref="FileInfo"/> which contains the stream.
		''' </param>
		''' <param name="streamName">
		''' The name of the stream to open.
		''' </param>
		''' <param name="mode">
		''' One of the <see cref="FileMode"/> values, indicating how the stream is to be opened.
		''' </param>
		''' <returns>
		''' An <see cref="AlternateDataStreamInfo"/> representing the stream.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="file"/> is <see langword="null"/>.
		''' </exception>
		''' <exception cref="FileNotFoundException">
		''' The specified <paramref name="file"/> was not found.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <paramref name="streamName"/> contains invalid characters.
		''' </exception>
		''' <exception cref="NotSupportedException">
		''' <paramref name="mode"/> is either <see cref="FileMode.Truncate"/> or <see cref="FileMode.Append"/>.
		''' </exception>
		''' <exception cref="IOException">
		''' <para><paramref name="mode"/> is <see cref="FileMode.Open"/>, and the stream doesn't exist.</para>
		''' <para>-or-</para>
		''' <para><paramref name="mode"/> is <see cref="FileMode.CreateNew"/>, and the stream already exists.</para>
		''' </exception>
		''' <exception cref="SecurityException">
		''' The caller does not have the required permission. 
		''' </exception>
		''' <exception cref="UnauthorizedAccessException">
		''' The caller does not have the required permission, or the file is read-only.
		''' </exception>
		<System.Runtime.CompilerServices.Extension> _
		Public Function GetAlternateDataStream(file As FileSystemInfo, streamName As String, mode As FileMode) As AlternateDataStreamInfo
			If file Is Nothing Then
				Throw New ArgumentNullException("file")
			End If
			If Not file.Exists Then
				Throw New FileNotFoundException(Nothing, file.FullName)
			End If
			SafeNativeMethods.ValidateStreamName(streamName)
	
			If FileMode.Truncate = mode OrElse FileMode.Append = mode Then
				Throw New NotSupportedException(Resources.Error_InvalidMode(mode))
			End If
	
	#If NET35 Then
			Dim permAccess As FileIOPermissionAccess = If((FileMode.Open = mode), FileIOPermissionAccess.Read, FileIOPermissionAccess.Read Or FileIOPermissionAccess.Write)
			New FileIOPermission(permAccess, file.FullName).Demand()
	#End If
	
			Dim path As String = SafeNativeMethods.BuildStreamPath(file.FullName, streamName)
			Dim exists As Boolean = SafeNativeMethods.FileExists(path)
	
			If Not exists AndAlso FileMode.Open = mode Then
				Throw New IOException(Resources.Error_StreamNotFound(streamName, file.Name))
			End If
			If exists AndAlso FileMode.CreateNew = mode Then
				Throw New IOException(Resources.Error_StreamExists(streamName, file.Name))
			End If
	
			Return New AlternateDataStreamInfo(file.FullName, streamName, path, exists)
		End Function
	
		''' <summary>
		''' <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
		''' Opens an alternate data stream.
		''' </summary>
		''' <param name="file">
		''' The <see cref="FileInfo"/> which contains the stream.
		''' </param>
		''' <param name="streamName">
		''' The name of the stream to open.
		''' </param>
		''' <returns>
		''' An <see cref="AlternateDataStreamInfo"/> representing the stream.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="file"/> is <see langword="null"/>.
		''' </exception>
		''' <exception cref="FileNotFoundException">
		''' The specified <paramref name="file"/> was not found.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <paramref name="streamName"/> contains invalid characters.
		''' </exception>
		''' <exception cref="SecurityException">
		''' The caller does not have the required permission. 
		''' </exception>
		''' <exception cref="UnauthorizedAccessException">
		''' The caller does not have the required permission, or the file is read-only.
		''' </exception>
		<System.Runtime.CompilerServices.Extension> _
		Public Function GetAlternateDataStream(file As FileSystemInfo, streamName As String) As AlternateDataStreamInfo
			Return file.GetAlternateDataStream(streamName, FileMode.OpenOrCreate)
		End Function
	
		''' <summary>
		''' Opens an alternate data stream.
		''' </summary>
		''' <param name="filePath">
		''' The path of the file which contains the stream.
		''' </param>
		''' <param name="streamName">
		''' The name of the stream to open.
		''' </param>
		''' <param name="mode">
		''' One of the <see cref="FileMode"/> values, indicating how the stream is to be opened.
		''' </param>
		''' <returns>
		''' An <see cref="AlternateDataStreamInfo"/> representing the stream.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="filePath"/> is <see langword="null"/> or an empty string.
		''' </exception>
		''' <exception cref="FileNotFoundException">
		''' The specified <paramref name="filePath"/> was not found.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <para><paramref name="filePath"/> is not a valid file path.</para>
		''' <para>-or-</para>
		''' <para><paramref name="streamName"/> contains invalid characters.</para>
		''' </exception>
		''' <exception cref="NotSupportedException">
		''' <paramref name="mode"/> is either <see cref="FileMode.Truncate"/> or <see cref="FileMode.Append"/>.
		''' </exception>
		''' <exception cref="IOException">
		''' <para><paramref name="mode"/> is <see cref="FileMode.Open"/>, and the stream doesn't exist.</para>
		''' <para>-or-</para>
		''' <para><paramref name="mode"/> is <see cref="FileMode.CreateNew"/>, and the stream already exists.</para>
		''' </exception>
		''' <exception cref="SecurityException">
		''' The caller does not have the required permission. 
		''' </exception>
		''' <exception cref="UnauthorizedAccessException">
		''' The caller does not have the required permission, or the file is read-only.
		''' </exception>
		Public Function GetAlternateDataStream(filePath As String, streamName As String, mode As FileMode) As AlternateDataStreamInfo
			If String.IsNullOrEmpty(filePath) Then
				Throw New ArgumentNullException("filePath")
			End If
			If Not SafeNativeMethods.FileExists(filePath) Then
				Throw New FileNotFoundException(Nothing, filePath)
			End If
			SafeNativeMethods.ValidateStreamName(streamName)
	
			If FileMode.Truncate = mode OrElse FileMode.Append = mode Then
				Throw New NotSupportedException(Resources.Error_InvalidMode(mode))
			End If
	
	#If NET35 Then
			Dim permAccess As FileIOPermissionAccess = If((FileMode.Open = mode), FileIOPermissionAccess.Read, FileIOPermissionAccess.Read Or FileIOPermissionAccess.Write)
			New FileIOPermission(permAccess, filePath).Demand()
	#End If
	
			Dim path As String = SafeNativeMethods.BuildStreamPath(filePath, streamName)
			Dim exists As Boolean = SafeNativeMethods.FileExists(path)
	
			If Not exists AndAlso FileMode.Open = mode Then
				Throw New IOException(Resources.Error_StreamNotFound(streamName, filePath))
			End If
			If exists AndAlso FileMode.CreateNew = mode Then
				Throw New IOException(Resources.Error_StreamExists(streamName, filePath))
			End If
	
			Return New AlternateDataStreamInfo(filePath, streamName, path, exists)
		End Function
	
		''' <summary>
		''' Opens an alternate data stream.
		''' </summary>
		''' <param name="filePath">
		''' The path of the file which contains the stream.
		''' </param>
		''' <param name="streamName">
		''' The name of the stream to open.
		''' </param>
		''' <returns>
		''' An <see cref="AlternateDataStreamInfo"/> representing the stream.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="filePath"/> is <see langword="null"/> or an empty string.
		''' </exception>
		''' <exception cref="FileNotFoundException">
		''' The specified <paramref name="filePath"/> was not found.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <para><paramref name="filePath"/> is not a valid file path.</para>
		''' <para>-or-</para>
		''' <para><paramref name="streamName"/> contains invalid characters.</para>
		''' </exception>
		''' <exception cref="SecurityException">
		''' The caller does not have the required permission. 
		''' </exception>
		''' <exception cref="UnauthorizedAccessException">
		''' The caller does not have the required permission, or the file is read-only.
		''' </exception>
		Public Function GetAlternateDataStream(filePath As String, streamName As String) As AlternateDataStreamInfo
			Return GetAlternateDataStream(filePath, streamName, FileMode.OpenOrCreate)
		End Function
	
		#End Region
	
		#Region "Delete Stream"
	
		''' <summary>
		''' <span style="font-weight:bold;color:#a00;">(Extension Method)</span><br />
		''' Deletes the specified alternate data stream if it exists.
		''' </summary>
		''' <param name="file">
		''' The <see cref="FileInfo"/> to inspect.
		''' </param>
		''' <param name="streamName">
		''' The name of the stream to delete.
		''' </param>
		''' <returns>
		''' <see langword="true"/> if the specified stream is deleted;
		''' otherwise, <see langword="false"/>.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="file"/> is <see langword="null"/>.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <paramref name="streamName"/> contains invalid characters.
		''' </exception>
		''' <exception cref="SecurityException">
		''' The caller does not have the required permission. 
		''' </exception>
		''' <exception cref="UnauthorizedAccessException">
		''' The caller does not have the required permission, or the file is read-only.
		''' </exception>
		''' <exception cref="IOException">
		''' The specified file is in use. 
		''' </exception>
		<System.Runtime.CompilerServices.Extension> _
		Public Function DeleteAlternateDataStream(file As FileSystemInfo, streamName As String) As Boolean
			If file Is Nothing Then
				Throw New ArgumentNullException("file")
			End If
			SafeNativeMethods.ValidateStreamName(streamName)
	
	#If NET35 Then
			Const  permAccess As FileIOPermissionAccess = FileIOPermissionAccess.Write
			New FileIOPermission(permAccess, file.FullName).Demand()
	#End If
	
			Dim result = False
			If file.Exists Then
				Dim path As String = SafeNativeMethods.BuildStreamPath(file.FullName, streamName)
				If SafeNativeMethods.FileExists(path) Then
					result = SafeNativeMethods.SafeDeleteFile(path)
				End If
			End If
	
			Return result
		End Function
	
		''' <summary>
		''' Deletes the specified alternate data stream if it exists.
		''' </summary>
		''' <param name="filePath">
		''' The path of the file to inspect.
		''' </param>
		''' <param name="streamName">
		''' The name of the stream to find.
		''' </param>
		''' <returns>
		''' <see langword="true"/> if the specified stream is deleted;
		''' otherwise, <see langword="false"/>.
		''' </returns>
		''' <exception cref="ArgumentNullException">
		''' <paramref name="filePath"/> is <see langword="null"/> or an empty string.
		''' </exception>
		''' <exception cref="ArgumentException">
		''' <para><paramref name="filePath"/> is not a valid file path.</para>
		''' <para>-or-</para>
		''' <para><paramref name="streamName"/> contains invalid characters.</para>
		''' </exception>
		''' <exception cref="SecurityException">
		''' The caller does not have the required permission. 
		''' </exception>
		''' <exception cref="UnauthorizedAccessException">
		''' The caller does not have the required permission, or the file is read-only.
		''' </exception>
		''' <exception cref="IOException">
		''' The specified file is in use. 
		''' </exception>
		Public Function DeleteAlternateDataStream(filePath As String, streamName As String) As Boolean
			If String.IsNullOrEmpty(filePath) Then
				Throw New ArgumentNullException("filePath")
			End If
			SafeNativeMethods.ValidateStreamName(streamName)
	
	#If NET35 Then
			Const permAccess As FileIOPermissionAccess = FileIOPermissionAccess.Write
			New FileIOPermission(permAccess, filePath).Demand()
	#End If
	
			Dim result = False
			If SafeNativeMethods.FileExists(filePath) Then
				Dim path As String = SafeNativeMethods.BuildStreamPath(filePath, streamName)
				If SafeNativeMethods.FileExists(path) Then
					result = SafeNativeMethods.SafeDeleteFile(path)
				End If
			End If
	
			Return result
		End Function
	
		#End Region
	End Module
End Namespace
