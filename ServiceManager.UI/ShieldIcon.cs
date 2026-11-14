using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace ServiceManager.UI
{
    /// <summary>
    /// The shield Windows puts in front of an entry that asks for elevated rights. It is the icon of the
    /// operating system rather than a picture of the project, so it reads the same as in the Explorer. A menu
    /// needs a picture rather than an icon, and the icon of the operating system comes in its own size, which
    /// is why it is drawn into the small one and kept.
    /// </summary>
    internal static class ShieldIcon
    {
        private const int IDI_SHIELD = 32518;
        private const int SM_CXSMICON = 49;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr instance, IntPtr name);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);

        private static Image m_Shield;

        /// <summary>
        /// The shield in the small icon size the menus use, made once. Null when the operating system has
        /// none: an entry without an image simply reads as it did before, a missing hint is better than a
        /// broken menu entry.
        /// </summary>
        public static Image Get()
        {
            if (m_Shield != null)
            {
                return m_Shield;
            }

            int size = GetSystemMetrics(SM_CXSMICON);

            if (size <= 0)
            {
                size = 16;
            }

            //The handle belongs to the operating system, so it is only borrowed while the picture is made
            IntPtr shared = LoadIcon(IntPtr.Zero, (IntPtr)IDI_SHIELD);

            if (shared == IntPtr.Zero)
            {
                return null;
            }

            using (Icon icon = Icon.FromHandle(shared))
            using (Image full = icon.ToBitmap())
            {
                m_Shield = DrawSmall(full, size);
            }

            return m_Shield;
        }

        static Image DrawSmall(Image full, int size)
        {
            Bitmap small = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(small))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(full, new Rectangle(0, 0, size, size));
            }

            return small;
        }
    }
}
