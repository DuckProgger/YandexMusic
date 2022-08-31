namespace Yandex.Music.Core.Compression;

public interface ICompressionProvider
{
    public ValueTask<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken);

    public ValueTask<byte[]> DecompressAsync(byte[] data, CancellationToken cancellationToken);
}
