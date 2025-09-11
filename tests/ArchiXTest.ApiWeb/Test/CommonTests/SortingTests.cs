using ArchiX.Library.Sorting;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.CommonTests
{
    /// <summary>
    /// SortingExtensions için birim testler.
    /// </summary>
    public sealed class SortingTests
    {
        /// <summary>
        /// Test verisi için basit bir model.
        /// </summary>
        private sealed class Dummy
        {
            /// <summary>İsim.</summary>
            public string Name { get; init; } = string.Empty;

            /// <summary>Yaş.</summary>
            public int Age { get; init; }
        }

        /// <summary>
        /// Tek alan ile artan sıralama yapılabilmeli.
        /// </summary>
        [Fact]
        public void ApplySorting_SortsBySingleField_Ascending()
        {
            var data = new List<Dummy>
            {
                new() { Name = "C", Age = 30 },
                new() { Name = "A", Age = 10 },
                new() { Name = "B", Age = 20 }
            }.AsQueryable();

            var sorts = new[]
            {
                new SortItem { Field = nameof(Dummy.Name), Direction = SortDirection.Ascending }
            };

            var result = data.ApplySorting(sorts).ToList();

            Assert.Equal(new[] { "A", "B", "C" }, result.Select(x => x.Name));
        }

        /// <summary>
        /// Tek alan ile azalan sıralama yapılabilmeli.
        /// </summary>
        [Fact]
        public void ApplySorting_SortsBySingleField_Descending()
        {
            var data = new List<Dummy>
            {
                new() { Name = "C", Age = 30 },
                new() { Name = "A", Age = 10 },
                new() { Name = "B", Age = 20 }
            }.AsQueryable();

            var sorts = new[]
            {
                new SortItem { Field = nameof(Dummy.Age), Direction = SortDirection.Descending }
            };

            var result = data.ApplySorting(sorts).ToList();

            Assert.Equal(new[] { 30, 20, 10 }, result.Select(x => x.Age));
        }

        /// <summary>
        /// Birden fazla alan ile (Age sonra Name) sıralama çalışmalı.
        /// </summary>
        [Fact]
        public void ApplySorting_SortsByMultipleFields()
        {
            var data = new List<Dummy>
            {
                new() { Name = "C", Age = 10 },
                new() { Name = "B", Age = 10 },
                new() { Name = "A", Age = 20 }
            }.AsQueryable();

            var sorts = new[]
            {
                new SortItem { Field = nameof(Dummy.Age),  Direction = SortDirection.Ascending },
                new SortItem { Field = nameof(Dummy.Name), Direction = SortDirection.Ascending }
            };

            var result = data.ApplySorting(sorts).ToList();

            Assert.Equal(new[] { "B", "C", "A" }, result.Select(x => x.Name));
        }

        /// <summary>
        /// sorts null olduğunda orijinal sıra korunmalı.
        /// </summary>
        [Fact]
        public void ApplySorting_ReturnsUnchanged_WhenSortsNull()
        {
            var data = new List<Dummy>
            {
                new() { Name = "X", Age = 1 },
                new() { Name = "Y", Age = 2 }
            }.AsQueryable();

            var result = data.ApplySorting(null).ToList();

            Assert.Equal(new[] { "X", "Y" }, result.Select(x => x.Name));
        }

        /// <summary>
        /// Boş/whitespace alan adı verilen SortItem yok sayılmalı.
        /// </summary>
        [Fact]
        public void ApplySorting_IgnoresBlankField()
        {
            var data = new List<Dummy>
            {
                new() { Name = "B", Age = 2 },
                new() { Name = "A", Age = 1 }
            }.AsQueryable();

            var sorts = new[]
            {
                new SortItem { Field = "   ", Direction = SortDirection.Ascending },
                new SortItem { Field = nameof(Dummy.Name), Direction = SortDirection.Ascending }
            };

            var result = data.ApplySorting(sorts).ToList();

            Assert.Equal(new[] { "A", "B" }, result.Select(x => x.Name));
        }
    }
}
