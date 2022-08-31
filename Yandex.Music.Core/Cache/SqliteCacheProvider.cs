using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Text;
using Yandex.Api;
using Yandex.Api.Logging;
using Yandex.Music.Core.Cache.Database;
using Yandex.Music.Core.Compression;

namespace Yandex.Music.Core.Cache;

public class SqliteCacheProvider : ICleanableCacheProvider
{
    private readonly ILogger logger = LoggerService.Create<SqliteCacheProvider>();

    public SqliteCacheProvider(SqliteCacheProviderSettings settings) {
        Settings = settings;
        CacheTokenProvider = new Md5CacheTokenProvider();
        CompressionProvider = new GzipCompressionProvider();

        logger.LogTrace("Инициализация кэша");
        string connectionString = new SqliteConnectionStringBuilder() {
            DataSource = Settings.DatabasePath,
            Cache = SqliteCacheMode.Shared,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
        DbContextOptions<SqliteCacheDatabaseContext> options = new DbContextOptionsBuilder<SqliteCacheDatabaseContext>()
            .UseSqlite(connectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;
        DbContextPool = new PooledDbContextFactory<SqliteCacheDatabaseContext>(options);
        logger.LogDebug("Инициализация кэша - ok");

        logger.LogTrace("Инициализация БД кэша");
        using SqliteCacheDatabaseContext context = DbContextPool.CreateDbContext();
        context.Database.EnsureCreated();
        logger.LogDebug("Инициализация БД кэша - ok");
    }


    public SqliteCacheProviderSettings Settings { get; }

    public ICacheTokenProvider CacheTokenProvider { get; set; }

    public ICompressionProvider CompressionProvider { get; set; }

    public PooledDbContextFactory<SqliteCacheDatabaseContext> DbContextPool { get; }


    public async Task<string> GetStringAsync(RequestData requestData, CancellationToken cancellationToken) {
        byte[] content = await GetBytesAsync(requestData, cancellationToken).ConfigureAwait(false);
        return content == null ? null : Encoding.UTF8.GetString(content);
    }

    public async Task<byte[]> GetBytesAsync(RequestData requestData, CancellationToken cancellationToken) {
        string token = CacheTokenProvider.GetToken(requestData);

        await using SqliteCacheDatabaseContext context = await DbContextPool.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        CacheRecord cacheRecord = await context.Set<CacheRecord>()
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken).ConfigureAwait(false);

        if (cacheRecord == null) {
            logger.LogTrace($"Кэш '{token}' - не существует");
            return null;
        }
        else {
            if (cacheRecord.IsCompressed) {
                logger.LogTrace($"Кэш '{token}' - получен (сжатый)");
                return await CompressionProvider.DecompressAsync(cacheRecord.Data, cancellationToken).ConfigureAwait(false);
            }
            else {
                logger.LogTrace($"Кэш '{token}' - получен");
                return cacheRecord.Data;
            }
        }
    }

    public async Task SetStringAsync(RequestData requestData, string content, CancellationToken cancellationToken) {
        await SetBytesAsync(
            requestData,
            Encoding.UTF8.GetBytes(content),
            Settings.UseStringDataCompression,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task SetBytesAsync(RequestData requestData, byte[] content, CancellationToken cancellationToken) {
        await SetBytesAsync(
             requestData,
             content,
             false,
             cancellationToken).ConfigureAwait(false);
    }

    public async Task CleanAsync() {
        if (!Settings.DeleteCacheAfterSession) {
            return;
        }

        await using SqliteCacheDatabaseContext context = await DbContextPool.CreateDbContextAsync().ConfigureAwait(false);
        await context.Database.EnsureDeletedAsync().ConfigureAwait(false);
    }


    private async Task SetBytesAsync(RequestData requestData, byte[] content, bool needCompression, CancellationToken cancellationToken) {
        string token = CacheTokenProvider.GetToken(requestData);

        CacheRecord cacheRecord = new() {
            Token = token,
            CreationTime = DateTime.Now,
            Data = content,
            IsCompressed = false,
        };
        if (needCompression) {
            int decompressedLength = cacheRecord.Data.Length;

            cacheRecord.Data = await CompressionProvider.CompressAsync(cacheRecord.Data, cancellationToken).ConfigureAwait(false);
            cacheRecord.IsCompressed = true;

            int compressedLength = cacheRecord.Data.Length;

            logger.LogTrace($"Кэш '{token}' - сжатие перед сохранением {decompressedLength}->{compressedLength}" +
                $" ({Math.Round(compressedLength / (double)decompressedLength * 100.0, 2)}%)");
        }

        await using SqliteCacheDatabaseContext context = await DbContextPool.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.AddAsync(cacheRecord, cancellationToken).ConfigureAwait(false);
        try {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogTrace($"Кэш '{token}' - сохранён");
        }
        catch (Exception ex) when ((ex.InnerException as SqliteException)?.SqliteErrorCode == 19) {
            // UNIQUE constraint failed
            logger.LogDebug($"Кэш '{token}' - ошибка сохранения (UNIQUE constraint failed)");
            // ignore
        }
        catch (Exception ex) {
            logger.LogError(ex, $"Кэш '{token}' - ошибка сохранения");
            throw;
        }
    }
}
