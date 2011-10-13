using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MultiAgentLibrary
{
    public class Native
    {
        [DllImport("MultiAgentNative.dll", CallingConvention=CallingConvention.Cdecl)]
        public static extern int DecreaseValues(int arSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] uint[] ar, int value);
    }
}
