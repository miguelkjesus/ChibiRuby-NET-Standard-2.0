namespace ChibiRuby.Polyfills.Tests;

/// <summary>
/// Span-based <see cref="Encoding"/> operations. On net472 these resolve to the
/// netstandard2.0 polyfill extensions (the .NET Framework BCL has no span overloads);
/// on net9.0 they resolve to the BCL instance methods.
/// </summary>
[TestFixture]
public class EncodingPolyfillsTests
{
    const string Sample = "Hëllo, 世界! 🎉";

    [Test]
    public void GetString_FromReadOnlySpan_RoundTrips()
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(Sample);
        ReadOnlySpan<byte> span = utf8;

        Assert.That(Encoding.UTF8.GetString(span), Is.EqualTo(Sample));
    }

    [Test]
    public void GetString_EmptySpan_ReturnsEmpty()
    {
        Assert.That(Encoding.UTF8.GetString(ReadOnlySpan<byte>.Empty), Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetCharCount_FromReadOnlySpan_MatchesDecodedLength()
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(Sample);

        Assert.That(Encoding.UTF8.GetCharCount((ReadOnlySpan<byte>)utf8), Is.EqualTo(Sample.Length));
    }

    [Test]
    public void GetBytes_FromString_IntoSpan_RoundTrips()
    {
        Span<byte> dest = new byte[Encoding.UTF8.GetByteCount(Sample)];

        int written = Encoding.UTF8.GetBytes(Sample, dest);

        Assert.That(written, Is.EqualTo(dest.Length));
        Assert.That(Encoding.UTF8.GetString((ReadOnlySpan<byte>)dest.ToArray()), Is.EqualTo(Sample));
    }

    [Test]
    public void GetBytes_FromCharSpan_IntoSpan_RoundTrips()
    {
        ReadOnlySpan<char> chars = Sample.ToCharArray();
        Span<byte> dest = new byte[Encoding.UTF8.GetByteCount(Sample)];

        int written = Encoding.UTF8.GetBytes(chars, dest);

        Assert.That(written, Is.EqualTo(dest.Length));
        Assert.That(Encoding.UTF8.GetString((ReadOnlySpan<byte>)dest.ToArray()), Is.EqualTo(Sample));
    }
}
