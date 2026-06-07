namespace ChibiRuby.Polyfills.Tests;

[TestFixture]
public class TypeExTests
{
    [Test]
    public void IsSZArray_SingleDimensionalZeroBased_IsTrue()
        => Assert.That(typeof(int[]).IsSZArray(), Is.True);

    [Test]
    public void IsSZArray_MultiDimensional_IsFalse()
        => Assert.That(typeof(int[,]).IsSZArray(), Is.False);

    [Test]
    public void IsSZArray_NonArray_IsFalse()
        => Assert.That(typeof(int).IsSZArray(), Is.False);
}

[TestFixture]
public class KeyValuePairPolyfillsTests
{
    [Test]
    public void Deconstruct_YieldsKeyAndValue()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

        var sum = 0;
        var keys = "";
        foreach (var (key, value) in dict) // uses Deconstruct (polyfill on net472, BCL on net9.0)
        {
            keys += key;
            sum += value;
        }

        Assert.That(sum, Is.EqualTo(3));
        Assert.That(keys.Length, Is.EqualTo(2));
    }
}

[TestFixture]
public class FileExTests
{
    [Test]
    public async Task ReadAllBytesAsync_ReturnsFileContents()
    {
        var path = Path.GetTempFileName();
        var expected = Encoding.UTF8.GetBytes("polyfill round-trip\n0123456789");
        try
        {
            File.WriteAllBytes(path, expected);

            var actual = await FileEx.ReadAllBytesAsync(path);

            Assert.That(actual, Is.EqualTo(expected));
        }
        finally
        {
            File.Delete(path);
        }
    }
}

[TestFixture]
public class MarshalExTests
{
    [Test]
    public unsafe void PtrToStringUTF8_DecodesNullTerminatedUtf8()
    {
        // "café" + NUL, encoded as UTF-8.
        byte[] buffer = Encoding.UTF8.GetBytes("café\0");
        fixed (byte* p = buffer)
        {
            Assert.That(MarshalEx.PtrToStringUTF8((IntPtr)p), Is.EqualTo("café"));
        }
    }

    [Test]
    public void PtrToStringUTF8_Zero_ReturnsNull()
        => Assert.That(MarshalEx.PtrToStringUTF8(IntPtr.Zero), Is.Null);
}

#if !NET7_0_OR_GREATER
// MemoryMarshalEx only exists on the polyfill TFMs (#if !NET7_0_OR_GREATER); on net9.0 the BCL
// MemoryMarshal is used directly, so these only compile/run on the net472 leg.
[TestFixture]
public class MemoryMarshalExTests
{
    [Test]
    public void CreateSpan_OverStackLocal_AliasesTheReference()
    {
        int value = 41;
        Span<int> span = MemoryMarshalEx.CreateSpan(ref value, 1);

        span[0] = 42;

        Assert.That(value, Is.EqualTo(42)); // span aliases the local
    }

    [Test]
    public void CreateReadOnlySpan_OverStackLocal_ReadsTheReference()
    {
        int value = 7;
        ReadOnlySpan<int> span = MemoryMarshalEx.CreateReadOnlySpan(ref value, 1);

        Assert.That(span[0], Is.EqualTo(7));
    }
}
#endif
