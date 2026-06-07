#if NETSTANDARD2_0
namespace System.Collections.Generic;
public static class KeyValuePairPolyfills
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
    { key = kvp.Key; value = kvp.Value; }
}
#endif
