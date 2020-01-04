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
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic

Namespace Trinet.Core.IO.Ntfs
	Friend NotInheritable Class StreamName
		Implements IDisposable
		Private Shared ReadOnly InvalidBlock As SafeHGlobalHandle = SafeHGlobalHandle.Invalid()
	
		Private m_memoryBlock As SafeHGlobalHandle = InvalidBlock
	
		''' <summary>
		''' Returns the handle to the block of memory.
		''' </summary>
		''' <value>
		''' The <see cref="SafeHGlobalHandle"/> representing the block of memory.
		''' </value>
		Public Property MemoryBlock() As SafeHGlobalHandle
			Get
				Return m_memoryBlock
			End Get
			Private Set
				m_memoryBlock = value
			End Set
		End Property
	
		''' <summary>
		''' Performs application-defined tasks associated with freeing, 
		''' releasing, or resetting unmanaged resources.
		''' </summary>
		Public Sub Dispose() Implements IDisposable.Dispose
			If Not MemoryBlock.IsInvalid Then
				MemoryBlock.Dispose()
				MemoryBlock = InvalidBlock
			End If
		End Sub
	
		''' <summary>
		''' Ensures that there is sufficient memory allocated.
		''' </summary>
		''' <param name="capacity">
		''' The required capacity of the block, in bytes.
		''' </param>
		''' <exception cref="OutOfMemoryException">
		''' There is insufficient memory to satisfy the request.
		''' </exception>
		Public Sub EnsureCapacity(capacity As Integer)
			Dim currentSize As Integer = If(MemoryBlock.IsInvalid, 0, MemoryBlock.Size)
			If capacity > currentSize Then
				If 0 <> currentSize Then
					currentSize <<= 1
				End If
				If capacity > currentSize Then
					currentSize = capacity
				End If
	
				If Not MemoryBlock.IsInvalid Then
					MemoryBlock.Dispose()
				End If
				MemoryBlock = SafeHGlobalHandle.Allocate(currentSize)
			End If
		End Sub
	
		''' <summary>
		''' Reads the Unicode string from the memory block.
		''' </summary>
		''' <param name="length">
		''' The length of the string to read, in characters.
		''' </param>
		''' <returns>
		''' The string read from the memory block.
		''' </returns>
		Public Function ReadString(length As Integer) As String
			If 0 >= length OrElse MemoryBlock.IsInvalid Then
				Return Nothing
			End If
			If length > MemoryBlock.Size Then
				length = MemoryBlock.Size
			End If
			Return Marshal.PtrToStringUni(MemoryBlock.DangerousGetHandle(), length)
		End Function
	
		''' <summary>
		''' Reads the string, and extracts the stream name.
		''' </summary>
		''' <param name="length">
		''' The length of the string to read, in characters.
		''' </param>
		''' <returns>
		''' The stream name.
		''' </returns>
		Public Function ReadStreamName(length As Integer) As String
			Dim name As String = ReadString(length)
			If Not String.IsNullOrEmpty(name) Then
				' Name is of the format ":NAME:$DATA\0"
				Dim separatorIndex As Integer = name.IndexOf(SafeNativeMethods.StreamSeparator, 1)
				If -1 <> separatorIndex Then
					name = name.Substring(1, separatorIndex - 1)
				Else
					' Should never happen!
					separatorIndex = name.IndexOf(ControlChars.NullChar)
					If 1 < separatorIndex Then
						name = name.Substring(1, separatorIndex - 1)
					Else
						name = Nothing
					End If
				End If
			End If
	
			Return name
		End Function
	End Class
End Namespace
