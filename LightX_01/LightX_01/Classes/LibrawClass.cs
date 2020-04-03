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
        [DllImport("LibRawTester.dll")]
        public static extern int processRawImage(int a);
    }
}
