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


Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices

''' <summary>
''' A <see cref="SafeHandle"/> for a global memory allocation.
''' </summary>
Friend NotInheritable Class SafeHGlobalHandle
	Inherits SafeHandle
	#Region "Constructor"

	''' <summary>
	''' Initializes a new instance of the <see cref="SafeHGlobalHandle"/> class.
	''' </summary>
	''' <param name="toManage">
	''' The initial handle value.
	''' </param>
	''' <param name="size">
	''' The size of this memory block, in bytes.
	''' </param>
	<ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)> _
	Private Sub New(toManage As IntPtr, size As Integer)
		MyBase.New(IntPtr.Zero, True)
		_Size = size
		SetHandle(toManage)
	End Sub

	''' <summary>
	''' Initializes a new instance of the <see cref="SafeHGlobalHandle"/> class.
	''' </summary>
	Private Sub New()
		MyBase.New(IntPtr.Zero, True)
	End Sub

	#End Region

	#Region "Properties"

	''' <summary>
	''' Gets a value indicating whether the handle value is invalid.
	''' </summary>
	''' <value>
	''' <see langword="true"/> if the handle value is invalid;
	''' otherwise, <see langword="false"/>.
	''' </value>
	Public Overrides ReadOnly Property IsInvalid() As Boolean
		Get
			Return IntPtr.Zero = handle
		End Get
	End Property

	''' <summary>
	''' Returns the size of this memory block.
	''' </summary>
	''' <value>
	''' The size of this memory block, in bytes.
	''' </value>
	Public Property Size() As Integer

	#End Region

	#Region "Methods"

	''' <summary>
	''' Allocates memory from the unmanaged memory of the process using GlobalAlloc.
	''' </summary>
	''' <param name="bytes">
	''' The number of bytes in memory required.
	''' </param>
	''' <returns>
	''' A <see cref="SafeHGlobalHandle"/> representing the memory.
	''' </returns>
	''' <exception cref="OutOfMemoryException">
	''' There is insufficient memory to satisfy the request.
	''' </exception>
	Public Shared Function Allocate(bytes As Integer) As SafeHGlobalHandle
		Return New SafeHGlobalHandle(Marshal.AllocHGlobal(bytes), bytes)
	End Function

	''' <summary>
	''' Returns an invalid handle.
	''' </summary>
	''' <returns>
	''' An invalid <see cref="SafeHGlobalHandle"/>.
	''' </returns>
	Public Shared Function Invalid() As SafeHGlobalHandle
		Return New SafeHGlobalHandle()
	End Function

	''' <summary>
	''' Executes the code required to free the handle.
	''' </summary>
	''' <returns>
	''' <see langword="true"/> if the handle is released successfully;
	''' otherwise, in the event of a catastrophic failure, <see langword="false"/>.
	''' In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.
	''' </returns>
	Protected Overrides Function ReleaseHandle() As Boolean
		Marshal.FreeHGlobal(handle)
		Return True
	End Function

	#End Region
End Class
