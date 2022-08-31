using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;
using System.Text;
using Yandex.Api;
using Yandex.Api.Passport.Entities;
using Yandex.Benchmarks;
using Yandex.Music.Core.Cache;

public class Program
{
    public static void Main() {
        Console.OutputEncoding = Encoding.Unicode;
        BenchmarkRunner.Run(typeof(Program).Assembly);
    }

}