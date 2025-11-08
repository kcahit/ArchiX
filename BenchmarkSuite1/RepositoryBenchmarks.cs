#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Infrastructure.EfCore;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Abstractions.Caching;

namespace BenchmarkSuite1
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class RepositoryBenchmarks
    {
        [Params(100, 1000)]
        public int N;

        private AppDbContext? _db;
        private Repository<Statu>? _repo;
        private RepositoryCacheDecorator<Statu>? _repoCached;
        private int _existingId;
        private ICacheService? _memSvc;

        [GlobalSetup]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"bench_{Guid.NewGuid():N}")
                .Options;

            _db = new AppDbContext(options);
            _db.Database.EnsureCreated();

            for (int i = 1; i <= N; i++)
            {
                _db.Set<Statu>().Add(new Statu { Code = "C" + i, Name = "Name" + i, Description = "desc" });
            }

            _db.SaveChanges();

            _repo = new Repository<Statu>(_db);
            _existingId = _db.Set<Statu>().AsNoTracking().Select(s => s.Id).First();
            var mem = new MemoryCache(new MemoryCacheOptions());
            _memSvc = new MemoryCacheService(mem);
            _repoCached = new RepositoryCacheDecorator<Statu>(_repo, _memSvc, TimeSpan.FromMinutes(5));
            // warm cache for id + all
            var _ = _repoCached.GetByIdAsync(_existingId).GetAwaiter().GetResult();
            var _allWarm = _repoCached.GetAllAsync().GetAwaiter().GetResult();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _db?.Dispose();
            _db = null;
            _repo = null;
            _repoCached = null;
        }

        [Benchmark]
        public async Task<List<Statu>> GetAllAsync_Bench()
        {
            var r = (await _repo!.GetAllAsync()).ToList();
            return r;
        }

        [Benchmark]
        public async Task<List<Statu>> GetAllAsync_Cached_Bench()
        {
            var r = (await _repoCached!.GetAllAsync()).ToList();
            return r;
        }

        [Benchmark]
        public async Task<Statu?> GetByIdAsync_Bench()
        {
            return await _repo!.GetByIdAsync(_existingId);
        }

        [Benchmark]
        public async Task<List<int>> GetPage_Bench()
        {
            var pageSize = Math.Min(100, N);
            var r = await _repo!.GetPageAsync(s => s.Id, pageNumber: 1, pageSize: pageSize);
            return r;
        }

        [Benchmark]
        public async Task<Statu?> GetById_Cached_Bench()
        {
            return await _repoCached!.GetByIdAsync(_existingId);
        }
    }
}
