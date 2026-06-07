namespace ChibiRuby.Polyfills.Tests;

/// <summary>
/// Exercises <see cref="SequenceReader{T}"/> the way <c>MRubyDapMessageHandler</c> uses it
/// (forward reads, <c>TryReadTo</c> with advance-past semantics) over both single- and
/// multi-segment sequences. On net472 this is the netstandard2.0 polyfill; on net9.0 it is the
/// BCL reader, so the assertions double as a parity check.
/// </summary>
[TestFixture]
public class SequenceReaderTests
{
    /// <summary>Builds a multi-segment <see cref="ReadOnlySequence{T}"/> from the given chunks.</summary>
    static ReadOnlySequence<byte> MultiSegment(params byte[][] chunks)
    {
        var first = new Segment(chunks[0]);
        var last = first;
        for (var i = 1; i < chunks.Length; i++)
            last = last.Append(chunks[i]);
        return new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);
    }

    sealed class Segment : ReadOnlySequenceSegment<byte>
    {
        public Segment(ReadOnlyMemory<byte> memory) => Memory = memory;

        public Segment Append(ReadOnlyMemory<byte> memory)
        {
            var next = new Segment(memory) { RunningIndex = RunningIndex + Memory.Length };
            Next = next;
            return next;
        }
    }

    static byte[] Bytes(string s) => Encoding.ASCII.GetBytes(s);

    static string Str(ReadOnlySequence<byte> seq) => Encoding.ASCII.GetString(seq.ToArray());

    [Test]
    public void TryReadTo_SingleSegment_ReturnsPrefixAndAdvancesPastDelimiter()
    {
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(Bytes("ab\r\ncd")));

        Assert.That(reader.TryReadTo(out ReadOnlySequence<byte> line, (byte)'\n', advancePastDelimiter: true), Is.True);
        Assert.That(Str(line), Is.EqualTo("ab\r"));
        Assert.That(reader.Consumed, Is.EqualTo(4));
        Assert.That(reader.Remaining, Is.EqualTo(2));
        Assert.That(reader.End, Is.False);
    }

    [Test]
    public void TryReadTo_DelimiterSpansSegments_IsFound()
    {
        // "ab\r" | "\ncd" — the '\n' delimiter is the first byte of the second segment.
        var reader = new SequenceReader<byte>(MultiSegment(Bytes("ab\r"), Bytes("\ncd")));

        Assert.That(reader.TryReadTo(out ReadOnlySequence<byte> line, (byte)'\n', advancePastDelimiter: true), Is.True);
        Assert.That(Str(line), Is.EqualTo("ab\r"));
        Assert.That(reader.Consumed, Is.EqualTo(4));

        Assert.That(reader.TryReadTo(out ReadOnlySequence<byte> _, (byte)'\n'), Is.False); // no more delimiters
        Assert.That(reader.Remaining, Is.EqualTo(2));               // "cd" still unread
    }

    [Test]
    public void TryReadTo_NoAdvance_LeavesPositionAtPrefixEnd()
    {
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(Bytes("key=val\n")));

        Assert.That(reader.TryReadTo(out ReadOnlySequence<byte> key, (byte)'=', advancePastDelimiter: false), Is.True);
        Assert.That(Str(key), Is.EqualTo("key"));
        Assert.That(reader.Consumed, Is.EqualTo(3)); // stopped before '=', not past it
    }

    [Test]
    public void TryReadTo_NotFound_ReturnsFalseAndDoesNotAdvance()
    {
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(Bytes("abc")));

        Assert.That(reader.TryReadTo(out ReadOnlySequence<byte> _, (byte)'\n'), Is.False);
        Assert.That(reader.Consumed, Is.EqualTo(0));
    }

    [Test]
    public void TryRead_And_Advance_WalkAcrossSegments()
    {
        var reader = new SequenceReader<byte>(MultiSegment(Bytes("AB"), Bytes("C")));

        Assert.That(reader.TryRead(out var b0), Is.True);
        Assert.That(b0, Is.EqualTo((byte)'A'));

        reader.Advance(1); // skip 'B'
        Assert.That(reader.Consumed, Is.EqualTo(2));

        Assert.That(reader.TryRead(out var b2), Is.True);
        Assert.That(b2, Is.EqualTo((byte)'C')); // crossed the segment boundary
        Assert.That(reader.End, Is.True);
        Assert.That(reader.TryRead(out _), Is.False);
    }
}
