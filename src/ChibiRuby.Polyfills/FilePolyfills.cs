using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace ChibiRuby.Polyfills;
public static class FileEx
{
    public static Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
#if NET6_0_OR_GREATER
        => File.ReadAllBytesAsync(path, cancellationToken);
#else
        => Task.Run(() => File.ReadAllBytes(path), cancellationToken);
#endif
}
