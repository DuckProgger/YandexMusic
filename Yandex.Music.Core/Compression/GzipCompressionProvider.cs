using System.IO;
using System.IO.Compression;

namespace Yandex.Music.Core.Compression;

public class GzipCompressionProvider : ICompressionProvider
{
    public async ValueTask<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken) {
        await using MemoryStream compressedStream = new();
        await using (GZipStream gzipStream = new(compressedStream, CompressionMode.Compress, true)) {
            await gzipStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        }
        return compressedStream.ToArray();
    }

    public async ValueTask<byte[]> DecompressAsync(byte[] data, CancellationToken cancellationToken) {
        await using MemoryStream compressedStream = new(data);
        await using MemoryStream decompressedStream = new();
        await using (GZipStream gzipStream = new(compressedStream, CompressionMode.Decompress, true)) {
            await gzipStream.CopyToAsync(decompressedStream, cancellationToken).ConfigureAwait(false);
        }
        return decompressedStream.ToArray();
    }
}
