using ArchiX.Library.Entities;

using Xunit;

namespace ArchiX.Library.Tests.Tests.CommonTests
{
    public sealed class EntitiesTests
    {
        private sealed class TestEntity : BaseEntity
        {
            public new static bool MapToDb = false;   // <-- tabloya çevrilmeyecek
        }

        [Fact]
        public void NewEntity_ShouldHaveValidDefaults()
        {
            // Act
            var entity = new TestEntity();

            // Assert
            Assert.Equal(0, entity.Id);                             // Varsayılan Id = 0
            Assert.Equal(Guid.Empty, entity.RowId);                 // Varsayılan RowId = Guid.Empty
            Assert.True(entity.CreatedAt <= DateTimeOffset.UtcNow); // CreatedAt atanmalı
            Assert.Equal(0, entity.CreatedBy);                      // Varsayılan CreatedBy = 0
            Assert.Null(entity.UpdatedAt);                          // Varsayılan UpdatedAt = null
            Assert.Null(entity.UpdatedBy);                          // Varsayılan UpdatedBy = null
            Assert.Equal(3, entity.StatusId);                       // Varsayılan durum Approved (3)
            Assert.Null(entity.LastStatusAt);                       // Varsayılan LastStatusAt = null
            Assert.Equal(0, entity.LastStatusBy);                   // Varsayılan LastStatusBy = 0
        }
    }
}
