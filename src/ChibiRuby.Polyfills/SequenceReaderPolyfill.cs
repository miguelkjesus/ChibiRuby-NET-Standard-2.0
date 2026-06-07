#if NETSTANDARD2_0
using System;

namespace System.Buffers;

/// <summary>
/// Minimal netstandard2.0 polyfill of <see cref="System.Buffers.SequenceReader{T}"/>, supporting
/// the subset of the API used by ChibiRuby (sequential forward reads over a <see cref="ReadOnlySequence{T}"/>).
/// Implemented over absolute-offset slicing so behavior matches the BCL reader for these operations.
/// </summary>
public ref struct SequenceReader<T> where T : unmanaged, IEquatable<T>
{
    readonly ReadOnlySequence<T> _sequence;
    readonly long _length;
    long _consumed;

    public SequenceReader(ReadOnlySequence<T> sequence)
    {
        _sequence = sequence;
        _length = sequence.Length;
        _consumed = 0;
    }

    public readonly long Length => _length;
    public readonly long Consumed => _consumed;
    public readonly long Remaining => _length - _consumed;
    public readonly bool End => _consumed >= _length;
    public readonly SequencePosition Position => _sequence.GetPosition(_consumed);
    public readonly ReadOnlySequence<T> UnreadSequence => _sequence.Slice(_consumed);

    public void Advance(long count) => _consumed += count;

    public bool TryRead(out T value)
    {
        foreach (var memory in _sequence.Slice(_consumed))
        {
            if (memory.Length > 0)
            {
                value = memory.Span[0];
                _consumed++;
                return true;
            }
        }
        value = default;
        return false;
    }

    public bool TryReadTo(out ReadOnlySequence<T> sequence, T delimiter, bool advancePastDelimiter = true)
    {
        var remaining = _sequence.Slice(_consumed);
        long offset = 0;
        foreach (var memory in remaining)
        {
            var span = memory.Span;
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i].Equals(delimiter))
                {
                    sequence = remaining.Slice(0, offset + i);
                    Advance(offset + i + (advancePastDelimiter ? 1 : 0));
                    return true;
                }
            }
            offset += span.Length;
        }
        sequence = default;
        return false;
    }
}
#endif
