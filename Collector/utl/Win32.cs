using System;
 using System.Collections.Generic;
 using System.Linq;
 using System.Text;
 using System.Runtime.InteropServices;
 using System.Security.Principal;

namespace utl
{
    public class Win32
    {
        public const int SECURITY_MAX_SID_SIZE = 68;
        public const int SDDL_REVISION_1 = 1;
        public const uint INVALID_HANDLE_VALUE = 0xffffffff;
        public const int PAGE_READWRITE = 0x04;
        public const int FILE_MAP_WRITE = 0X02;
        public const int FILE_MAP_READ = 0X01;

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateBoundaryDescriptor
        (
        [In] string Name,
        [In] int Flags
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CreateWellKnownSid
        (
        [In] WellKnownSidType WellKnownSidType,
        [In] [Optional] IntPtr DomainSid,
        [In] IntPtr pSid,
        [In][Out]ref int cbSid
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AddSIDToBoundaryDescriptor
        (
        [In][Out] ref IntPtr BoundaryDescriptor,
        [In] IntPtr RequiredSid
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor
        (
        [In] string StringSecurityDescriptor,
        [In] int StringSDRevision,
        [Out] out IntPtr SecurityDescriptor,
        [Out] IntPtr SecurityDescriptorSize
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree([In] IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreatePrivateNamespace(
        [In][Optional] ref SECURITY_ATTRIBUTES lpPrivateNamespaceAttributes,
        [In] IntPtr lpBoundaryDescriptor,
        [In] string lpAliasPrefix
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr OpenPrivateNamespace(
        [In] IntPtr lpBoundaryDescriptor,
        [In] string lpAliasPrefix
        );
        
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFileMapping(
        [In] uint hFile,
        [In][Optional] ref SECURITY_ATTRIBUTES lpAttributes,
        [In] int flProtect,
        [In] int dwMaximumSizeHigh,
        [In] int dwMaximumSizeLow,
        [In][Optional] string lpName
        );

        [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenFileMapping(
        [In] int dwDesiredAccess,
        [In] [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        [In] string lpName
        );

        
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
        [In] IntPtr hFileMappingObject,
        [In] int dwDesiredAccess,
        [In] int dwFileOffsetHigh,
        [In] int dwFileOffsetLow,
        [In] int dwNumberOfBytesToMap
        );

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemCopy(IntPtr dest, IntPtr src, uint count);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle
        (
        [In] IntPtr hObject
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void DeleteBoundaryDescriptor
        (
        [In] IntPtr BoundaryDescriptor
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ClosePrivateNamespace
        (
        [In] IntPtr Handle,
        [In] ulong Flags            // 1 - to destroy
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile
        (
        [In] IntPtr lpBaseAddress
        );

    }
}