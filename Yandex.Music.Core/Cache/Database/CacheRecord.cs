using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yandex.Music.Core.Cache.Database;

[Table("cache")]
public record CacheRecord
{
    [Key, Column("token"), Required]
    public string Token { get; set; }

    [Column("is_compressed"), Required]
    public bool IsCompressed { get; set; }

    [Column("creation_time"), Required]
    public DateTime CreationTime { get; set; }

    [Column("data"), Required]
    public byte[] Data { get; set; }
}