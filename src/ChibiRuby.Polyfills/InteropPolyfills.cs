using System;
using System.Text;

namespace ChibiRuby.Polyfills;

public static class MarshalEx
{
    public static unsafe string? PtrToStringUTF8(IntPtr ptr)
#if NETSTANDARD2_0
    {
        if (ptr == IntPtr.Zero) return null;
        var p = (byte*)ptr; var len = 0;
        while (p[len] != 0) len++;
        return Encoding.UTF8.GetString(p, len);
    }
#else
        => System.Runtime.InteropServices.Marshal.PtrToStringUTF8(ptr);
#endif
}

public static class TypeEx
{
    public static bool IsSZArray(this Type type)
#if NETSTANDARD2_0
        => type.IsArray && type.GetArrayRank() == 1 && type == type.GetElementType()!.MakeArrayType();
#else
        => type.IsSZArray;
#endif
}
