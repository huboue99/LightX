using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//#pragma comment( lib, "ws2_32.lib")

namespace LightX_01.Classes
{
    public class LibrawClass
    {
        [DllImport("LibRawTester.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr processRawImage(IntPtr rawData, int dataLength);

        [DllImport("LibRawTester.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int extractThumb([In,Out] IntPtr rawData, int dataLength);
    }
}
