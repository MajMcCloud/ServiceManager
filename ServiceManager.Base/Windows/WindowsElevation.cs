using System;
using System.Runtime.InteropServices;

namespace ServiceManager.Base.Windows
{
    /// <summary>
    /// Reads the elevation of the own process from its access token. The token is asked rather than the
    /// group membership, because a member of the administrators group who did not confirm the prompt holds
    /// the group but not the rights, and the operations of the interface still ask him for them.
    /// </summary>
    public class WindowsElevation : IProcessElevation
    {
        private const int TOKEN_QUERY = 0x0008;
        private const int TokenElevation = 20;

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_ELEVATION
        {
            public int TokenIsElevated;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr tokenHandle, int informationClass,
            out TOKEN_ELEVATION information, int informationLength, out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// True when the own process holds elevated rights. A token that cannot be read counts as not
        /// elevated: an operation that turns out to need more rights than the process has is worse for the
        /// user than a shield that was shown once too often.
        /// </summary>
        public bool IsElevated()
        {
            IntPtr token = IntPtr.Zero;

            try
            {
                if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, out token))
                {
                    return false;
                }

                TOKEN_ELEVATION value = new TOKEN_ELEVATION();
                int returnLength;

                if (!GetTokenInformation(token, TokenElevation, out value, Marshal.SizeOf(typeof(TOKEN_ELEVATION)), out returnLength))
                {
                    return false;
                }

                return value.TokenIsElevated != 0;
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }
        }
    }
}
