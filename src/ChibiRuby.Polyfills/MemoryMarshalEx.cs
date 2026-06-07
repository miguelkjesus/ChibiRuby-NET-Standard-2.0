#if !NET7_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace ChibiRuby.Polyfills;
public static class MemoryMarshalEx
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetArrayDataReference<T>(T[] array) => ref MemoryMarshal.GetReference(array.AsSpan());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] AllocateUninitializedArray<T>(int length, bool pinned = false) => new T[length];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpan<T>(scoped ref T reference, int length) => new Span<T>(Unsafe.AsPointer(ref reference), length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(scoped ref T reference, int length) => new ReadOnlySpan<T>(Unsafe.AsPointer(ref reference), length);
}
#endif
