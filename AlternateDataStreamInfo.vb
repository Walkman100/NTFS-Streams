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


Imports System.ComponentModel
Imports System.Diagnostics
Imports System.IO
Imports System.Security
Imports System.Security.Permissions

''' <summary>
''' Represents the details of an alternative data stream.
''' </summary>
<DebuggerDisplay("{FullPath}")> _
Public NotInheritable Class AlternateDataStreamInfo
	Implements IEquatable(Of AlternateDataStreamInfo)
	#Region "Constructor"

	''' <summary>
	''' Initializes a new instance of the <see cref="AlternateDataStreamInfo"/> class.
	''' </summary>
	''' <param name="filePath">
	''' The full path of the file.
	''' This argument must not be <see langword="null"/>.
	''' </param>
	''' <param name="info">
	''' The <see cref="SafeNativeMethods.Win32StreamInfo"/> containing the stream information.
	''' </param>
	Friend Sub New(filePath__1 As String, info As SafeNativeMethods.Win32StreamInfo)
		FilePath = filePath__1
		Name = info.StreamName
		StreamType = info.StreamType
		Attributes = info.StreamAttributes
		Size = info.StreamSize
		Exists = True

		FullPath = SafeNativeMethods.BuildStreamPath(FilePath, Name)
	End Sub

	''' <summary>
	''' Initializes a new instance of the <see cref="AlternateDataStreamInfo"/> class.
	''' </summary>
	''' <param name="filePath">
	''' The full path of the file.
	''' This argument must not be <see langword="null"/>.
	''' </param>
	''' <param name="streamName">
	''' The name of the stream
	''' This argument must not be <see langword="null"/>.
	''' </param>
	''' <param name="fullPath">
	''' The full path of the stream.
	''' If this argument is <see langword="null"/>, it will be generated from the 
	''' <paramref name="filePath"/> and <paramref name="streamName"/> arguments.
	''' </param>
	''' <param name="exists">
	''' <see langword="true"/> if the stream exists;
	''' otherwise, <see langword="false"/>.
	''' </param>
	Friend Sub New(filePath__1 As String, streamName As String, fullPath__2 As String, exists__3 As Boolean)
		If String.IsNullOrEmpty(fullPath__2) Then
			fullPath__2 = SafeNativeMethods.BuildStreamPath(filePath__1, streamName)
		End If
		StreamType = FileStreamType.AlternateDataStream

		FilePath = filePath__1
		Name = streamName
		FullPath = fullPath__2
		Exists = exists__3

		If Exists Then
			Size = SafeNativeMethods.GetFileSize(FullPath)
		End If
	End Sub

	#End Region

	#Region "Properties"

	''' <summary>
	''' Returns the full path of this stream.
	''' </summary>
	''' <value>
	''' The full path of this stream.
	''' </value>
	Public Property FullPath() As String
		Get
			Return FullPath
		End Get
		Private Set
			FullPath = value
		End Set
	End Property

	''' <summary>
	''' Returns the full path of the file which contains the stream.
	''' </summary>
	''' <value>
	''' The full file-system path of the file which contains the stream.
	''' </value>
	Public Property FilePath() As String
		Get
			Return FilePath
		End Get
		Private Set
			FilePath = value
		End Set
	End Property

	''' <summary>
	''' Returns the name of the stream.
	''' </summary>
	''' <value>
	''' The name of the stream.
	''' </value>
	Public Property Name() As String
		Get
			Return Name
		End Get
		Private Set
			Name = value
		End Set
	End Property

	''' <summary>
	''' Returns a flag indicating whether the specified stream exists.
	''' </summary>
	''' <value>
	''' <see langword="true"/> if the stream exists;
	''' otherwise, <see langword="false"/>.
	''' </value>
	Public Property Exists() As Boolean
		Get
			Return Exists
		End Get
		Private Set
			Exists = value
		End Set
	End Property

	''' <summary>
	''' Returns the size of the stream, in bytes.
	''' </summary>
	''' <value>
	''' The size of the stream, in bytes.
	''' </value>
	Public Property Size() As Long
		Get
			Return Size
		End Get
		Private Set
			Size = value
		End Set
	End Property

	''' <summary>
	''' Returns the type of data.
	''' </summary>
	''' <value>
	''' One of the <see cref="FileStreamType"/> values.
	''' </value>
	<EditorBrowsable(EditorBrowsableState.Advanced)> _
	Public Property StreamType() As FileStreamType
		Get
			Return StreamType
		End Get
		Private Set
			StreamType = value
		End Set
	End Property

	''' <summary>
	''' Returns attributes of the data stream.
	''' </summary>
	''' <value>
	''' A combination of <see cref="FileStreamAttributes"/> values.
	''' </value>
	<EditorBrowsable(EditorBrowsableState.Advanced)> _
	Public Property Attributes() As FileStreamAttributes
		Get
			Return Attributes
		End Get
		Private Set
			Attributes = value
		End Set
	End Property

	#End Region

	#Region "Methods"

	#Region "-IEquatable"

	''' <summary>
	''' Returns a <see cref="String"/> that represents the current instance.
	''' </summary>
	''' <returns>
	''' A <see cref="String"/> that represents the current instance.
	''' </returns>
	Public Overrides Function ToString() As String
		Return FullPath
	End Function

	''' <summary>
	''' Serves as a hash function for a particular type.
	''' </summary>
	''' <returns>
	''' A hash code for the current <see cref="Object"/>.
	''' </returns>
	Public Overrides Function GetHashCode() As Integer
		Dim comparer = StringComparer.OrdinalIgnoreCase
		Return comparer.GetHashCode(If(FilePath, String.Empty)) Xor comparer.GetHashCode(If(Name, String.Empty))
	End Function

	''' <summary>
	''' Indicates whether the current object is equal to another object of the same type.
	''' </summary>
	''' <param name="obj">
	''' An object to compare with this object.
	''' </param>
	''' <returns>
	''' <see langword="true"/> if the current object is equal to the <paramref name="obj"/> parameter;
	''' otherwise, <see langword="false"/>.
	''' </returns>
	Public Overrides Function Equals(obj As Object) As Boolean
		Return Equals(TryCast(obj, AlternateDataStreamInfo))
	End Function

	''' <summary>
	''' Returns a value indicating whether
	''' this instance is equal to another instance.
	''' </summary>
	''' <param name="other">
	''' The instance to compare to.
	''' </param>
	''' <returns>
	''' <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter;
	''' otherwise, <see langword="false"/>.
	''' </returns>
	Public Overloads Function Equals(other As AlternateDataStreamInfo) As Boolean
		If ReferenceEquals(Nothing, other) Then
			Return False
		End If
		If ReferenceEquals(Me, other) Then
			Return True
		End If

		Dim comparer = StringComparer.OrdinalIgnoreCase
		Return comparer.Equals(If(FilePath, String.Empty), If(other.FilePath, String.Empty)) AndAlso comparer.Equals(If(Name, String.Empty), If(other.Name, String.Empty))
	End Function

	''' <summary>
	''' The equality operator.
	''' </summary>
	''' <param name="first">
	''' The first object.
	''' </param>
	''' <param name="second">
	''' The second object.
	''' </param>
	''' <returns>
	''' <see langword="true"/> if the two objects are equal;
	''' otherwise, <see langword="false"/>.
	''' </returns>
	Public Shared Operator =(first As AlternateDataStreamInfo, second As AlternateDataStreamInfo) As Boolean
		Return Equals(first, second)
	End Operator

	''' <summary>
	''' The inequality operator.
	''' </summary>
	''' <param name="first">
	''' The first object.
	''' </param>
	''' <param name="second">
	''' The second object.
	''' </param>
	''' <returns>
	''' <see langword="true"/> if the two objects are not equal;
	''' otherwise, <see langword="false"/>.
	''' </returns>
	Public Shared Operator <>(first As AlternateDataStreamInfo, second As AlternateDataStreamInfo) As Boolean
		Return Not Equals(first, second)
	End Operator

	#End Region

	#Region "-Delete"

	''' <summary>
	''' Deletes this stream from the parent file.
	''' </summary>
	''' <returns>
	''' <see langword="true"/> if the stream was deleted;
	''' otherwise, <see langword="false"/>.
	''' </returns>
	''' <exception cref="SecurityException">
	''' The caller does not have the required permission. 
	''' </exception>
	''' <exception cref="UnauthorizedAccessException">
	''' The caller does not have the required permission, or the file is read-only.
	''' </exception>
	''' <exception cref="IOException">
	''' The specified file is in use. 
	''' </exception>
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	Public Function Delete() As Boolean
		#If NET35 Then
		Const  permAccess As FileIOPermissionAccess = FileIOPermissionAccess.Write
		New FileIOPermission(permAccess, FilePath).Demand()
		#End If

		Return SafeNativeMethods.SafeDeleteFile(FullPath)
	End Function

	#End Region

	#Region "-Open"

	#If NETFULL Then
	''' <summary>
	''' Calculates the access to demand.
	''' </summary>
	''' <param name="mode">
	''' The <see cref="FileMode"/>.
	''' </param>
	''' <param name="access">
	''' The <see cref="FileAccess"/>.
	''' </param>
	''' <returns>
	''' The <see cref="FileIOPermissionAccess"/>.
	''' </returns>
	Private Shared Function CalculateAccess(mode As FileMode, access As FileAccess) As FileIOPermissionAccess
		Dim permAccess = FileIOPermissionAccess.NoAccess
		Select Case mode
			Case FileMode.Append
				permAccess = FileIOPermissionAccess.Append
				Exit Select

			Case FileMode.Create, FileMode.CreateNew, FileMode.OpenOrCreate, FileMode.Truncate
				permAccess = FileIOPermissionAccess.Write
				Exit Select

			Case FileMode.Open
				permAccess = FileIOPermissionAccess.Read
				Exit Select
		End Select
		Select Case access
			Case FileAccess.ReadWrite
				permAccess = permAccess Or FileIOPermissionAccess.Write
				permAccess = permAccess Or FileIOPermissionAccess.Read
				Exit Select

			Case FileAccess.Write
				permAccess = permAccess Or FileIOPermissionAccess.Write
				Exit Select

			Case FileAccess.Read
				permAccess = permAccess Or FileIOPermissionAccess.Read
				Exit Select
		End Select

		Return permAccess
	End Function
	#End If

	''' <summary>
	''' Opens this alternate data stream.
	''' </summary>
	''' <param name="mode">
	''' A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
	''' and determines whether the contents of existing streams are retained or overwritten.
	''' </param>
	''' <param name="access">
	''' A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
	''' </param>
	''' <param name="share">
	''' A <see cref="FileShare"/> value specifying the type of access other threads have to the file. 
	''' </param>
	''' <param name="bufferSize">
	''' The size of the buffer to use.
	''' </param>
	''' <param name="useAsync">
	''' <see langword="true"/> to enable async-IO;
	''' otherwise, <see langword="false"/>.
	''' </param>
	''' <returns>
	''' A <see cref="FileStream"/> for this alternate data stream.
	''' </returns>
	''' <exception cref="ArgumentOutOfRangeException">
	''' <paramref name="bufferSize"/> is less than or equal to zero.
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
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	''' <exception cref="Win32Exception">
	''' There was an error opening the stream.
	''' </exception>
	Public Function Open(mode As FileMode, access As FileAccess, share As FileShare, bufferSize As Integer, useAsync As Boolean) As FileStream
		If 0 >= bufferSize Then
			Throw New ArgumentOutOfRangeException("bufferSize", bufferSize, Nothing)
		End If

		#If NET35 Then
		Dim permAccess As FileIOPermissionAccess = CalculateAccess(mode, access)
		New FileIOPermission(permAccess, FilePath).Demand()
		#End If

		Dim flags As SafeNativeMethods.NativeFileFlags = If(useAsync, SafeNativeMethods.NativeFileFlags.Overlapped, 0)
		Dim handle = SafeNativeMethods.SafeCreateFile(FullPath, access.ToNative(), share, IntPtr.Zero, mode, flags, _
			IntPtr.Zero)
		If handle.IsInvalid Then
			SafeNativeMethods.ThrowLastIOError(FullPath)
		End If
		Return New FileStream(handle, access, bufferSize, useAsync)
	End Function

	''' <summary>
	''' Opens this alternate data stream.
	''' </summary>
	''' <param name="mode">
	''' A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
	''' and determines whether the contents of existing streams are retained or overwritten.
	''' </param>
	''' <param name="access">
	''' A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
	''' </param>
	''' <param name="share">
	''' A <see cref="FileShare"/> value specifying the type of access other threads have to the file. 
	''' </param>
	''' <param name="bufferSize">
	''' The size of the buffer to use.
	''' </param>
	''' <returns>
	''' A <see cref="FileStream"/> for this alternate data stream.
	''' </returns>
	''' <exception cref="ArgumentOutOfRangeException">
	''' <paramref name="bufferSize"/> is less than or equal to zero.
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
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	''' <exception cref="Win32Exception">
	''' There was an error opening the stream.
	''' </exception>
	Public Function Open(mode As FileMode, access As FileAccess, share As FileShare, bufferSize As Integer) As FileStream
		Return Open(mode, access, share, bufferSize, False)
	End Function

	''' <summary>
	''' Opens this alternate data stream.
	''' </summary>
	''' <param name="mode">
	''' A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
	''' and determines whether the contents of existing streams are retained or overwritten.
	''' </param>
	''' <param name="access">
	''' A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
	''' </param>
	''' <param name="share">
	''' A <see cref="FileShare"/> value specifying the type of access other threads have to the file. 
	''' </param>
	''' <returns>
	''' A <see cref="FileStream"/> for this alternate data stream.
	''' </returns>
	''' <exception cref="SecurityException">
	''' The caller does not have the required permission. 
	''' </exception>
	''' <exception cref="UnauthorizedAccessException">
	''' The caller does not have the required permission, or the file is read-only.
	''' </exception>
	''' <exception cref="IOException">
	''' The specified file is in use. 
	''' </exception>
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	''' <exception cref="Win32Exception">
	''' There was an error opening the stream.
	''' </exception>
	Public Function Open(mode As FileMode, access As FileAccess, share As FileShare) As FileStream
		Return Open(mode, access, share, SafeNativeMethods.DefaultBufferSize, False)
	End Function

	''' <summary>
	''' Opens this alternate data stream.
	''' </summary>
	''' <param name="mode">
	''' A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
	''' and determines whether the contents of existing streams are retained or overwritten.
	''' </param>
	''' <param name="access">
	''' A <see cref="FileAccess"/> value that specifies the operations that can be performed on the stream. 
	''' </param>
	''' <returns>
	''' A <see cref="FileStream"/> for this alternate data stream.
	''' </returns>
	''' <exception cref="SecurityException">
	''' The caller does not have the required permission. 
	''' </exception>
	''' <exception cref="UnauthorizedAccessException">
	''' The caller does not have the required permission, or the file is read-only.
	''' </exception>
	''' <exception cref="IOException">
	''' The specified file is in use. 
	''' </exception>
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	''' <exception cref="Win32Exception">
	''' There was an error opening the stream.
	''' </exception>
	Public Function Open(mode As FileMode, access As FileAccess) As FileStream
		Return Open(mode, access, FileShare.None, SafeNativeMethods.DefaultBufferSize, False)
	End Function

	''' <summary>
	''' Opens this alternate data stream.
	''' </summary>
	''' <param name="mode">
	''' A <see cref="FileMode"/> value that specifies whether a stream is created if one does not exist, 
	''' and determines whether the contents of existing streams are retained or overwritten.
	''' </param>
	''' <returns>
	''' A <see cref="FileStream"/> for this alternate data stream.
	''' </returns>
	''' <exception cref="SecurityException">
	''' The caller does not have the required permission. 
	''' </exception>
	''' <exception cref="UnauthorizedAccessException">
	''' The caller does not have the required permission, or the file is read-only.
	''' </exception>
	''' <exception cref="IOException">
	''' The specified file is in use. 
	''' </exception>
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	''' <exception cref="Win32Exception">
	''' There was an error opening the stream.
	''' </exception>
	Public Function Open(mode As FileMode) As FileStream
		Dim access As FileAccess = If((FileMode.Append = mode), FileAccess.Write, FileAccess.ReadWrite)
		Return Open(mode, access, FileShare.None, SafeNativeMethods.DefaultBufferSize, False)
	End Function

	#End Region

	#Region "-OpenRead / OpenWrite / OpenText"

	''' <summary>
	''' Opens this stream for reading.
	''' </summary>
	''' <returns>
	''' A read-only <see cref="FileStream"/> for this stream.
	''' </returns>
	''' <exception cref="SecurityException">
	''' The caller does not have the required permission. 
	''' </exception>
	''' <exception cref="UnauthorizedAccessException">
	''' The caller does not have the required permission, or the file is read-only.
	''' </exception>
	''' <exception cref="IOException">
	''' The specified file is in use. 
	''' </exception>
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	''' <exception cref="Win32Exception">
	''' There was an error opening the stream.
	''' </exception>
	Public Function OpenRead() As FileStream
		Return Open(FileMode.Open, FileAccess.Read, FileShare.Read)
	End Function

	''' <summary>
	''' Opens this stream for writing.
	''' </summary>
	''' <returns>
	''' A write-only <see cref="FileStream"/> for this stream.
	''' </returns>
	''' <exception cref="SecurityException">
	''' The caller does not have the required permission. 
	''' </exception>
	''' <exception cref="UnauthorizedAccessException">
	''' The caller does not have the required permission, or the file is read-only.
	''' </exception>
	''' <exception cref="IOException">
	''' The specified file is in use. 
	''' </exception>
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	''' <exception cref="Win32Exception">
	''' There was an error opening the stream.
	''' </exception>
	Public Function OpenWrite() As FileStream
		Return Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)
	End Function

	''' <summary>
	''' Opens this stream as a text file.
	''' </summary>
	''' <returns>
	''' A <see cref="StreamReader"/> which can be used to read the contents of this stream.
	''' </returns>
	''' <exception cref="SecurityException">
	''' The caller does not have the required permission. 
	''' </exception>
	''' <exception cref="UnauthorizedAccessException">
	''' The caller does not have the required permission, or the file is read-only.
	''' </exception>
	''' <exception cref="IOException">
	''' The specified file is in use. 
	''' </exception>
	''' <exception cref="ArgumentException">
	''' The path of the stream is invalid.
	''' </exception>
	''' <exception cref="Win32Exception">
	''' There was an error opening the stream.
	''' </exception>
	Public Function OpenText() As StreamReader
		Dim fileStream As Stream = Open(FileMode.Open, FileAccess.Read, FileShare.Read)
		Return New StreamReader(fileStream)
	End Function

	#End Region

	#End Region
End Class
