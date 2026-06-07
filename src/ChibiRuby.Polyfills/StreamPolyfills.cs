#if NETSTANDARD2_0
using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace System.IO;
public static class StreamPolyfills
{
    public static ValueTask<int> ReadAsync(this Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out var seg))
            return new ValueTask<int>(stream.ReadAsync(seg.Array!, seg.Offset, seg.Count, cancellationToken));
        return ReadFallback(stream, buffer, cancellationToken);
    }
    static async ValueTask<int> ReadFallback(Stream stream, Memory<byte> buffer, CancellationToken ct)
    {
        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
        try { var n = await stream.ReadAsync(rented, 0, buffer.Length, ct).ConfigureAwait(false);
              new ReadOnlySpan<byte>(rented, 0, n).CopyTo(buffer.Span); return n; }
        finally { ArrayPool<byte>.Shared.Return(rented); }
    }
    public static ValueTask WriteAsync(this Stream stream, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (MemoryMarshal.TryGetArray(buffer, out var seg))
            return new ValueTask(stream.WriteAsync(seg.Array!, seg.Offset, seg.Count, cancellationToken));
        return WriteFallback(stream, buffer, cancellationToken);
    }
    static async ValueTask WriteFallback(Stream stream, ReadOnlyMemory<byte> buffer, CancellationToken ct)
    {
        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
        try { buffer.Span.CopyTo(rented); await stream.WriteAsync(rented, 0, buffer.Length, ct).ConfigureAwait(false); }
        finally { ArrayPool<byte>.Shared.Return(rented); }
    }
    public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
    {
        var rented = ArrayPool<byte>.Shared.Rent(buffer.Length);
        try { buffer.CopyTo(rented); stream.Write(rented, 0, buffer.Length); }
        finally { ArrayPool<byte>.Shared.Return(rented); }
    }
}
#endif
