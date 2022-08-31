using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Perfolizer.Horology;
using Yandex.Api;
using Yandex.Music.Core.Cache;

namespace Yandex.Benchmarks;

[MemoryDiagnoser]
//[RankColumn]
//[Config(typeof(Config))]
public class CacheBenchmarks
{
    //private class Config : ManualConfig
    //{
    //    public Config() {
    //        AddJob(Job.Dry
    //            .WithIterationTime(TimeInterval.Millisecond * 100));
    //    }
    //}

    private readonly byte[] data = new byte[100000];
    private readonly SqliteCacheProvider sqliteCacheProvider;
    private readonly FileSystemCacheProvider fileSystemCacheProvider;
    //private const int iterations = 100;
    private readonly RequestData requestData;
    //private readonly Random random = new();
    //private readonly RequestData[] requestDatas = new RequestData[iterations];


    public CacheBenchmarks() {
        requestData = new() {
            RequestUrl = "https://music.yandex.ru/handlers/playlist.jsx?owner=music-blog&kinds=2240"
        };

        //for (int i = 0; i < iterations; i++) {
        //    requestDatas[i] = new RequestData { RequestUrl = random.NextInt64().ToString() };
        //}

        sqliteCacheProvider = new SqliteCacheProvider(new SqliteCacheProviderSettings() {
            DatabasePath = "bench.db3",
        });

        fileSystemCacheProvider = new();
    }

    [Benchmark]
    //[MinIterationTime(100)]
    public async Task SqliteSetCacheOneIteration() {
        await sqliteCacheProvider.SetBytesAsync(requestData, data, default).ConfigureAwait(false);
    }

    [Benchmark]
    public async Task FileSetCacheOneIteration() {
        await fileSystemCacheProvider.SetBytesAsync(requestData, data, default).ConfigureAwait(false);
    }

    [Benchmark]
    //[MinIterationTime(100)]
    public async Task SqliteGetCacheOneIteration() {
        await sqliteCacheProvider.GetBytesAsync(requestData, default).ConfigureAwait(false);
    }

    [Benchmark]
    public async Task FileGetCacheOneIteration() {
        await fileSystemCacheProvider.GetBytesAsync(requestData, default).ConfigureAwait(false);
    }
}




