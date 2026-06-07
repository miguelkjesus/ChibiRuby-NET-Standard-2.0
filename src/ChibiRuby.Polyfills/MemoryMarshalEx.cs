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
    // WARNING: unlike the BCL MemoryMarshal.CreateSpan, the returned span is built from a raw pointer and is
    // NOT GC-tracked. Only use when 'reference' points at fixed memory (stack locals, or a pinned/fixed array).
    // Do NOT use over a movable managed-heap array whose contents may be read across an allocation/GC.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpan<T>(scoped ref T reference, int length) => new Span<T>(Unsafe.AsPointer(ref reference), length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(scoped ref T reference, int length) => new ReadOnlySpan<T>(Unsafe.AsPointer(ref reference), length);
}
#endif
