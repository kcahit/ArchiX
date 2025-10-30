
using System.Linq.Expressions;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.DiagnosticsTests
{
    /// <summary>
    /// 12. Test – Performans/Grid · Filtering
    /// - FilterItem seed kodlarının enum karşılıkları
    /// - Tüm operatörlerin davranış testleri
    /// </summary>
    public sealed class GridFiltering_SeedAndBehavior_Tests
    {
        #region Enum & Map
        public enum FilterOperator
        {
            Equals, NotEquals,
            StartsWith, NotStartsWith,
            EndsWith, NotEndsWith,
            Contains, NotContains,
            GreaterThan, GreaterThanOrEqual,
            LessThan, LessThanOrEqual,
            Between, NotBetween,
            In, NotIn,
            IsNull, IsNotNull
        }

        private static readonly Dictionary<string, FilterOperator> CodeToOperator = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Equals"] = FilterOperator.Equals,
            ["NotEquals"] = FilterOperator.NotEquals,
            ["StartsWith"] = FilterOperator.StartsWith,
            ["NotStartsWith"] = FilterOperator.NotStartsWith,
            ["EndsWith"] = FilterOperator.EndsWith,
            ["NotEndsWith"] = FilterOperator.NotEndsWith,
            ["Contains"] = FilterOperator.Contains,
            ["NotContains"] = FilterOperator.NotContains,
            ["GreaterThan"] = FilterOperator.GreaterThan,
            ["GreaterThanOrEqual"] = FilterOperator.GreaterThanOrEqual,
            ["LessThan"] = FilterOperator.LessThan,
            ["LessThanOrEqual"] = FilterOperator.LessThanOrEqual,
            ["Between"] = FilterOperator.Between,
            ["NotBetween"] = FilterOperator.NotBetween,
            ["In"] = FilterOperator.In,
            ["NotIn"] = FilterOperator.NotIn,
            ["IsNull"] = FilterOperator.IsNull,
            ["IsNotNull"] = FilterOperator.IsNotNull
        };
        #endregion

        #region DTO & Engine
        public sealed class FilterRule
        {
            public string Field { get; init; } = default!;
            public FilterOperator Operator { get; init; }
            public object? Value { get; init; }
            public object? Value2 { get; init; }
            public bool IgnoreCase { get; init; } = true;
        }

        public static class FilterEngine
        {
            public static IQueryable<T> Apply<T>(IQueryable<T> source, IEnumerable<FilterRule> rules)
            {
                foreach (var r in rules)
                    source = source.Where(BuildPredicate<T>(r));
                return source;
            }

            private static Expression<Func<T, bool>> BuildPredicate<T>(FilterRule rule)
            {
                var p = Expression.Parameter(typeof(T), "x");
                var member = rule.Field.Split('.').Aggregate<string, Expression>(p, Expression.PropertyOrField);
                var mType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;

                Expression Coerce(Expression e, Type t)
                {
                    var et = Nullable.GetUnderlyingType(e.Type) ?? e.Type;
                    return et == t ? e : Expression.Convert(e, t);
                }

                Expression C(object? v, Type t) => Expression.Convert(Expression.Constant(v, v?.GetType() ?? t), t);

                Expression body;

                switch (rule.Operator)
                {
                    case FilterOperator.IsNull:
                        body = Expression.Equal(member, Expression.Constant(null));
                        break;

                    case FilterOperator.IsNotNull:
                        body = Expression.NotEqual(member, Expression.Constant(null));
                        break;

                    case FilterOperator.Equals:
                    case FilterOperator.NotEquals:
                        {
                            var eq = Expression.Equal(Coerce(member, mType), C(rule.Value, mType));
                            body = rule.Operator == FilterOperator.Equals ? eq : Expression.Not(eq);
                            break;
                        }

                    case FilterOperator.GreaterThan:
                        body = Expression.GreaterThan(Coerce(member, mType), C(rule.Value, mType));
                        break;

                    case FilterOperator.GreaterThanOrEqual:
                        body = Expression.GreaterThanOrEqual(Coerce(member, mType), C(rule.Value, mType));
                        break;

                    case FilterOperator.LessThan:
                        body = Expression.LessThan(Coerce(member, mType), C(rule.Value, mType));
                        break;

                    case FilterOperator.LessThanOrEqual:
                        body = Expression.LessThanOrEqual(Coerce(member, mType), C(rule.Value, mType));
                        break;

                    case FilterOperator.Between:
                    case FilterOperator.NotBetween:
                        {
                            var ge = Expression.GreaterThanOrEqual(Coerce(member, mType), C(rule.Value, mType));
                            var le = Expression.LessThanOrEqual(Coerce(member, mType), C(rule.Value2, mType));
                            var between = Expression.AndAlso(ge, le);
                            body = rule.Operator == FilterOperator.Between ? between : Expression.Not(between);
                            break;
                        }

                    case FilterOperator.In:
                    case FilterOperator.NotIn:
                        {
                            // Value: IEnumerable<object?> beklenir. Bunu member türüne (mType) cast ederek T[] oluştur.
                            var raw = (rule.Value as IEnumerable<object?>) ?? Array.Empty<object?>();
                            var arr = Array.CreateInstance(mType, raw.Count());
                            int i = 0;
                            foreach (var v in raw)
                            {
                                var val = v is null ? null : Convert.ChangeType(v, mType);
                                arr.SetValue(val, i++);
                            }

                            var contains = typeof(Enumerable)
                                .GetMethods()
                                .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
                                .MakeGenericMethod(mType);

                            var call = Expression.Call(null, contains, Expression.Constant(arr, arr.GetType()), Coerce(member, mType));
                            body = rule.Operator == FilterOperator.In ? call : Expression.Not(call);
                            break;
                        }

                    case FilterOperator.StartsWith:
                    case FilterOperator.NotStartsWith:
                    case FilterOperator.EndsWith:
                    case FilterOperator.NotEndsWith:
                    case FilterOperator.Contains:
                    case FilterOperator.NotContains:
                        {
                            // string yolu: null-safe + opsiyonel ignore-case
                            Expression strExpr =
                                mType == typeof(string)
                                    ? Coerce(member, typeof(string))
                                    : Expression.Call(Coerce(member, mType), mType.GetMethod("ToString", Type.EmptyTypes)!);

                            // null -> "" (null güvenli)
                            strExpr = Expression.Coalesce(strExpr, Expression.Constant(string.Empty));

                            Expression left = strExpr;
                            Expression right = Expression.Constant(rule.Value?.ToString() ?? "");

                            if (rule.IgnoreCase)
                            {
                                left = Expression.Call(left, typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!);
                                right = Expression.Call(right, typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!);
                            }

                            string method = rule.Operator switch
                            {
                                FilterOperator.StartsWith or FilterOperator.NotStartsWith => nameof(string.StartsWith),
                                FilterOperator.EndsWith or FilterOperator.NotEndsWith => nameof(string.EndsWith),
                                _ => nameof(string.Contains)
                            };

                            var call = Expression.Call(left, typeof(string).GetMethod(method, new[] { typeof(string) })!, right);
                            bool negate = rule.Operator is FilterOperator.NotStartsWith or FilterOperator.NotEndsWith or FilterOperator.NotContains;
                            body = negate ? Expression.Not(call) : call;
                            break;
                        }

                    default:
                        throw new NotSupportedException(rule.Operator.ToString());
                }

                return Expression.Lambda<Func<T, bool>>(body, p);
            }
        }
        #endregion

        #region Sample data
        private static IQueryable<ProductRow> Sample()
        {
            var now = new DateTime(2025, 09, 01, 12, 00, 00, DateTimeKind.Utc);
            return new List<ProductRow>
            {
                new() { Id = 1,  Name = "Kalem",      Price = 10.5m,  CreatedAt = now.AddDays(-10), Description = "Mavi tükenmez" },
                new() { Id = 2,  Name = "Defter",     Price = 20m,    CreatedAt = now.AddDays(-8),  Description = "A4 çizgili" },
                new() { Id = 3,  Name = "Silgi",      Price = 5.5m,   CreatedAt = now.AddDays(-6),  Description = null },
                new() { Id = 4,  Name = "Kitap",      Price = 50m,    CreatedAt = now.AddDays(-4),  Description = "Roman" },
                new() { Id = 5,  Name = "Kalemtıraş", Price = 12m,    CreatedAt = now.AddDays(-2),  Description = "Metal" },
                new() { Id = 6,  Name = "Makas",      Price = 25m,    CreatedAt = now.AddDays(-1),  Description = "Paslanmaz" },
            }.AsQueryable();
        }

        private sealed class ProductRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
            public decimal Price { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? Description { get; set; }
        }
        #endregion

        #region Tests – Seed code mapping

        [Theory]
        [InlineData("Equals", nameof(FilterOperator.Equals))]
        [InlineData("NotEquals", nameof(FilterOperator.NotEquals))]
        [InlineData("StartsWith", nameof(FilterOperator.StartsWith))]
        [InlineData("NotStartsWith", nameof(FilterOperator.NotStartsWith))]
        [InlineData("EndsWith", nameof(FilterOperator.EndsWith))]
        [InlineData("NotEndsWith", nameof(FilterOperator.NotEndsWith))]
        [InlineData("Contains", nameof(FilterOperator.Contains))]
        [InlineData("NotContains", nameof(FilterOperator.NotContains))]
        [InlineData("GreaterThan", nameof(FilterOperator.GreaterThan))]
        [InlineData("GreaterThanOrEqual", nameof(FilterOperator.GreaterThanOrEqual))]
        [InlineData("LessThan", nameof(FilterOperator.LessThan))]
        [InlineData("LessThanOrEqual", nameof(FilterOperator.LessThanOrEqual))]
        [InlineData("Between", nameof(FilterOperator.Between))]
        [InlineData("NotBetween", nameof(FilterOperator.NotBetween))]
        [InlineData("In", nameof(FilterOperator.In))]
        [InlineData("NotIn", nameof(FilterOperator.NotIn))]
        [InlineData("IsNull", nameof(FilterOperator.IsNull))]
        [InlineData("IsNotNull", nameof(FilterOperator.IsNotNull))]
        public void Seed_code_should_map_to_enum(string code, string expectedEnumName)
        {
            Assert.True(CodeToOperator.TryGetValue(code, out var op));
            Assert.Equal(expectedEnumName, op.ToString());
        }

        #endregion

        #region Tests – Behavior

        [Fact]
        public void Equals_and_NotEquals()
        {
            var q = Sample();
            var eq = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Price), Operator = FilterOperator.Equals, Value = 20m }
            }).Select(x => x.Id).ToList();
            Assert.Single(eq);
            Assert.Equal(2, eq[0]);

            var neq = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Price), Operator = FilterOperator.NotEquals, Value = 20m }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 3, 4, 5, 6 }, neq);
        }

        [Fact]
        public void StartsWith_and_NotStartsWith()
        {
            var q = Sample();
            var s = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Name), Operator = FilterOperator.StartsWith, Value = "Ka" }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 5 }, s);

            var ns = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Name), Operator = FilterOperator.NotStartsWith, Value = "Ka" }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 2, 3, 4, 6 }, ns);
        }

        [Fact]
        public void EndsWith_and_NotEndsWith()
        {
            var q = Sample();
            var e = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Name), Operator = FilterOperator.EndsWith, Value = "as" }
            }).Select(x => x.Id).ToList();
            Assert.Equal(new[] { 6 }, e);

            var ne = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Name), Operator = FilterOperator.NotEndsWith, Value = "as" }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, ne);
        }

        [Fact]
        public void Contains_and_NotContains()
        {
            var q = Sample();
            var c = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Description), Operator = FilterOperator.Contains, Value = "paslan" }
            }).Select(x => x.Id).ToList();
            Assert.Equal(new[] { 6 }, c);

            var nc = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Description), Operator = FilterOperator.NotContains, Value = "paslan" }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, nc);
        }

        [Fact]
        public void Greater_Less_Between_NotBetween()
        {
            var q = Sample();

            var gt = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Price), Operator = FilterOperator.GreaterThan, Value = 20m }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 4, 6 }, gt);

            var le = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Price), Operator = FilterOperator.LessThanOrEqual, Value = 12m }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 3, 5 }, le);

            var btw = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Price), Operator = FilterOperator.Between, Value = 10m, Value2 = 20m }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 2, 5 }, btw);

            var nbtw = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Price), Operator = FilterOperator.NotBetween, Value = 10m, Value2 = 20m }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 3, 4, 6 }, nbtw);
        }

        [Fact]
        public void In_and_NotIn()
        {
            var q = Sample();
            var In = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Id), Operator = FilterOperator.In, Value = new object[] {2,4,6} }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 2, 4, 6 }, In);

            var notIn = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Id), Operator = FilterOperator.NotIn, Value = new object[] {2,4,6} }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 3, 5 }, notIn);
        }

        [Fact]
        public void Null_checks()
        {
            var q = Sample();

            var onlyNull = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Description), Operator = FilterOperator.IsNull }
            }).Select(x => x.Id).ToList();
            Assert.Equal(new[] { 3 }, onlyNull);

            var notNull = FilterEngine.Apply(q, new[]
            {
                new FilterRule { Field = nameof(ProductRow.Description), Operator = FilterOperator.IsNotNull }
            }).Select(x => x.Id).OrderBy(x => x).ToList();
            Assert.Equal(new[] { 1, 2, 4, 5, 6 }, notNull);
        }

        #endregion
    }
}
