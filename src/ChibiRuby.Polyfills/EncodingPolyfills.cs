#if NETSTANDARD2_0
using System;
namespace System.Text;
public static class EncodingPolyfills
{
    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty) return string.Empty;
        fixed (byte* p = bytes) return encoding.GetString(p, bytes.Length);
    }
    public static unsafe int GetCharCount(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty) return 0;
        fixed (byte* p = bytes) return encoding.GetCharCount(p, bytes.Length);
    }
    public static int GetBytes(this Encoding encoding, string chars, Span<byte> bytes)
        => encoding.GetBytes(chars.AsSpan(), bytes);

    public static unsafe int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
    {
        if (chars.IsEmpty) return 0;
        fixed (char* c = chars) fixed (byte* b = bytes)
            return encoding.GetBytes(c, chars.Length, b, bytes.Length);
    }
}
#endif
