using System;
using System.Runtime.InteropServices;

namespace LightX.Classes
{
    public class LibrawClass
    {
        [DllImport("LibRawWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr processRawImage(IntPtr rawData, int dataLength);

        [DllImport("LibRawWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int extractThumb([In,Out] IntPtr rawData, int dataLength);

        [DllImport("LibRawWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int extractThumbFromFile([Out] IntPtr rawData, string path);
    }
}
