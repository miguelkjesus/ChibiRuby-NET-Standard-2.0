#if !NET7_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace ChibiRuby.Polyfills;
public static class MemoryMarshalEx
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetArrayDataReference<T>(T[] array) => ref MemoryMarshal.GetReference(array.AsSpan());
    // NOTE: 'pinned' is ignored - netstandard2.0 has no pinned-heap allocation. Callers that pass
    // pinned: true to safely take a pointer get a movable array here; pin explicitly via 'fixed' instead.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] AllocateUninitializedArray<T>(int length, bool pinned = false) => new T[length];
    // WARNING: CreateSpan builds the span from a raw pointer and is NOT GC-tracked. It only works for
    // *unmanaged* T — netstandard2.0's System.Memory Span<T>(void*, int) ctor throws
    // "Only value types without pointers or references are supported" for any T that contains managed
    // references. Only use over fixed memory (stack locals / pinned-or-fixed arrays) of unmanaged elements.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpan<T>(scoped ref T reference, int length) => new Span<T>(Unsafe.AsPointer(ref reference), length);

    // Read-only span over 'length' contiguous elements starting at 'reference'.
    // Unlike CreateSpan this is valid for *managed* T too: the slow-span ReadOnlySpan<T>(void*, int) ctor
    // rejects reference-containing T (e.g. MRubyValue), so instead of aliasing through a pointer we copy the
    // run into a GC-tracked array. Every caller consumes the result read-only, so the copy is observationally
    // equivalent. (net7+ uses the zero-copy BCL MemoryMarshal.CreateReadOnlySpan instead of this polyfill.)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> CreateReadOnlySpan<T>(scoped ref T reference, int length)
    {
        if (length == 0) return ReadOnlySpan<T>.Empty;
        var copy = new T[length];
        for (var i = 0; i < length; i++)
            copy[i] = Unsafe.Add(ref reference, i);
        return copy;
    }
}
#endif
