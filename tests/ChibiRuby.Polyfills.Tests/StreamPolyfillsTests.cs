namespace ChibiRuby.Polyfills.Tests;

/// <summary>
/// Span/Memory-based <see cref="Stream"/> operations. On net472 these are the netstandard2.0
/// polyfill extensions (TryGetArray fast-path + ArrayPool fallback); on net9.0 they are the BCL
/// instance methods.
/// </summary>
[TestFixture]
public class StreamPolyfillsTests
{
    static readonly byte[] Payload = Enumerable.Range(0, 777).Select(i => (byte)i).ToArray();

    [Test]
    public void Write_ReadOnlySpan_WritesAllBytes()
    {
        using var ms = new MemoryStream();

        ms.Write((ReadOnlySpan<byte>)Payload);

        Assert.That(ms.ToArray(), Is.EqualTo(Payload));
    }

    [Test]
    public async Task WriteAsync_ReadOnlyMemory_Then_ReadAsync_Memory_RoundTrips()
    {
        using var ms = new MemoryStream();

        await ms.WriteAsync((ReadOnlyMemory<byte>)Payload);
        ms.Position = 0;

        var buffer = new byte[Payload.Length];
        var total = 0;
        while (total < buffer.Length)
        {
            var n = await ms.ReadAsync(((Memory<byte>)buffer).Slice(total));
            if (n == 0) break;
            total += n;
        }

        Assert.That(total, Is.EqualTo(Payload.Length));
        Assert.That(buffer, Is.EqualTo(Payload));
    }
}
