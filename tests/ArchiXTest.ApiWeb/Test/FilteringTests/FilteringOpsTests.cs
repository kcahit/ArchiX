using System.Globalization;
using System.Linq.Expressions;

using ArchiX.Library.Filtering;

using Xunit;

namespace ArchiXTest.ApiWeb.Tests.FilteringTests
{
    /// <summary>
    /// 9. Test – Filtering Ops
    /// FilterItem üzerinden StartsWith, EndsWith, Between gibi filtrelerin çalışmasını doğrular.
    /// Bu test, in-memory veri üzerinde Expression tabanlı predicate üretir.
    /// </summary>
    public class FilteringOpsTests
    {
        // Basit örnek varlık
        private sealed class Person
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
            public int Age { get; set; }
            public decimal Salary { get; set; }
            public DateTime Joined { get; set; }
            public string? Note { get; set; }
        }

        // Test verisi
        private static List<Person> Seed() => new()
        {
            new Person { Id = 1, Name = "Alice",  Age = 28, Salary = 7000m, Joined = new DateTime(2024, 1, 10), Note = null },
            new Person { Id = 2, Name = "Alan",   Age = 33, Salary = 5000m, Joined = new DateTime(2023, 12, 1),  Note = "ok" },
            new Person { Id = 3, Name = "Bob",    Age = 20, Salary = 4000m, Joined = new DateTime(2022, 5, 6),   Note = null },
            new Person { Id = 4, Name = "Carla",  Age = 25, Salary = 6000m, Joined = new DateTime(2023, 2, 15),  Note = "x" },
            new Person { Id = 5, Name = "Daniel", Age = 40, Salary = 8000m, Joined = new DateTime(2021, 8, 20),  Note = null },
            new Person { Id = 6, Name = "Megan",  Age = 29, Salary = 7500m, Joined = new DateTime(2024, 7, 10),  Note = null },
            new Person { Id = 7, Name = "Ethan",  Age = 31, Salary = 6200m, Joined = new DateTime(2025, 3, 2),   Note = "note" },
            new Person { Id = 8, Name = "Zoe",    Age = 19, Salary = 3000m, Joined = new DateTime(2025, 6, 18),  Note = "" },
        };

        // ------------------------------------------------------------
        // STARTSWITH / ENDSWITH
        // ------------------------------------------------------------
        [Fact(DisplayName = "StartsWith: Name 'A' ile başlayanlar")]
        public void StartsWith_Name_A()
        {
            var people = Seed();
            var filter = new FilterItem { Code = "StartsWith", ItemType = "Operator" };

            var result = Apply(people, nameof(Person.Name), filter, "A")
                .Select(p => p.Name)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(new[] { "Alan", "Alice" }, result);
        }

        [Fact(DisplayName = "EndsWith: Name 'n' ile bitenler")]
        public void EndsWith_Name_n()
        {
            var people = Seed();
            var filter = new FilterItem { Code = "EndsWith", ItemType = "Operator" };

            var result = Apply(people, nameof(Person.Name), filter, "n")
                .Select(p => p.Name)
                .OrderBy(x => x)
                .ToList();

            // Alan, Ethan, Megan -> 'n' ile biter
            Assert.Equal(new[] { "Alan", "Ethan", "Megan" }, result);
        }

        // ------------------------------------------------------------
        // BETWEEN (int / DateTime)
        // ------------------------------------------------------------
        [Fact(DisplayName = "Between: Age 20..30 (dahil)")]
        public void Between_Age_20_30()
        {
            var people = Seed();
            var filter = new FilterItem { Code = "Between", ItemType = "Operator" };

            var result = Apply(people, nameof(Person.Age), filter, 20, 30)
                .Select(p => p.Name)
                .OrderBy(x => x)
                .ToList();

            // 20..30: Alice(28), Bob(20), Carla(25), Megan(29)
            Assert.Equal(new[] { "Alice", "Bob", "Carla", "Megan" }, result);
        }

        [Fact(DisplayName = "Between: Joined 2023-01-01..2024-12-31")]
        public void Between_Joined_DateRange()
        {
            var people = Seed();
            var filter = new FilterItem { Code = "Between", ItemType = "Operator" };

            var result = Apply(people, nameof(Person.Joined), filter,
                               new DateTime(2023, 1, 1),
                               new DateTime(2024, 12, 31))
                .Select(p => p.Name)
                .OrderBy(x => x)
                .ToList();

            // Alan(2023-12-01), Carla(2023-02-15), Alice(2024-01-10), Megan(2024-07-10)
            Assert.Equal(new[] { "Alan", "Alice", "Carla", "Megan" }, result);
        }

        // Ek doğrulamalar (kapsayıcılık)
        [Fact(DisplayName = "NotBetween: Age 20..30 dışında kalanlar")]
        public void NotBetween_Age_20_30()
        {
            var people = Seed();
            var filter = new FilterItem { Code = "NotBetween", ItemType = "Operator" };

            var result = Apply(people, nameof(Person.Age), filter, 20, 30)
                .Select(p => p.Name)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(new[] { "Alan", "Daniel", "Ethan", "Zoe" }, result);
        }

        [Fact(DisplayName = "In: Age ∈ {19, 28, 31}")]
        public void In_Age_Set()
        {
            var people = Seed();
            var filter = new FilterItem { Code = "In", ItemType = "Operator" };

            var result = Apply(people, nameof(Person.Age), filter, 19, 28, 31)
                .Select(p => p.Name)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(new[] { "Alice", "Ethan", "Zoe" }, result);
        }

        [Fact(DisplayName = "IsNull: Note == null")]
        public void IsNull_Note()
        {
            var people = Seed();
            var filter = new FilterItem { Code = "IsNull", ItemType = "Operator" };

            var result = Apply(people, nameof(Person.Note), filter)
                .Select(p => p.Name)
                .OrderBy(x => x)
                .ToList();

            Assert.Equal(new[] { "Alice", "Daniel", "Megan", "Bob" }.OrderBy(x => x), result);
        }

        // ============================================================
        // Yardımcı: FilterItem.Code -> Expression<Func<T,bool>>
        // ============================================================
        private static IEnumerable<T> Apply<T>(IEnumerable<T> source, string propertyName, FilterItem filter, params object?[] values)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var member = BuildMemberExpression(param, propertyName); // x.Property[.SubProperty]
            var body = BuildBody(member, filter.Code, values);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);

            return source.AsQueryable().Where(lambda);
        }

        private static MemberExpression BuildMemberExpression(ParameterExpression param, string propertyPath)
        {
            Expression current = param;
            foreach (var segment in propertyPath.Split('.'))
                current = Expression.PropertyOrField(current, segment);

            return (MemberExpression)current;
        }

        private static Expression BuildBody(MemberExpression member, string opCode, object?[] values)
        {
            var memberType = member.Type;
            var underlying = Nullable.GetUnderlyingType(memberType) ?? memberType;

            // ---- Değer sayısı kontrolleri (önerilen sağlamlık) ----
            if (opCode is "Between" or "NotBetween")
            {
                if (values.Length < 2)
                    throw new ArgumentException("Between/NotBetween iki değer ister (min ve max).");
            }
            else if (opCode is "Equals" or "NotEquals" or "GreaterThan" or "GreaterThanOrEqual"
                               or "LessThan" or "LessThanOrEqual"
                               or "StartsWith" or "EndsWith" or "Contains"
                               or "NotStartsWith" or "NotEndsWith" or "NotContains"
                               or "In" or "NotIn")
            {
                if (values.Length < 1)
                    throw new ArgumentException($"{opCode} en az bir değer ister.");
            }

            // ---- STRING OPERATÖRLERİ ----
            if (opCode is "StartsWith" or "NotStartsWith" or "EndsWith" or "NotEndsWith" or "Contains" or "NotContains")
            {
                if (underlying != typeof(string))
                    throw new NotSupportedException($"{opCode} yalnızca string alanlarda desteklenir.");

                var value = Expression.Constant(values[0]?.ToString() ?? string.Empty, typeof(string));

                // switch sonucu Expression olmalı (CS8506 fix)
                Expression call = opCode switch
                {
                    "StartsWith" => Expression.Call(member, nameof(string.StartsWith), Type.EmptyTypes, value),
                    "EndsWith" => Expression.Call(member, nameof(string.EndsWith), Type.EmptyTypes, value),
                    "Contains" => Expression.Call(member, nameof(string.Contains), Type.EmptyTypes, value),
                    "NotStartsWith" => Expression.Not(Expression.Call(member, nameof(string.StartsWith), Type.EmptyTypes, value)),
                    "NotEndsWith" => Expression.Not(Expression.Call(member, nameof(string.EndsWith), Type.EmptyTypes, value)),
                    "NotContains" => Expression.Not(Expression.Call(member, nameof(string.Contains), Type.EmptyTypes, value)),
                    _ => throw new NotSupportedException(opCode)
                };
                return call;
            }

            // ---- NULL OPERATÖRLERİ (önerilen sağlamlık) ----
            if (opCode is "IsNull" or "IsNotNull")
            {
                // Sadece referans tipler veya Nullable<T> desteklensin
                if (member.Type.IsValueType && Nullable.GetUnderlyingType(member.Type) is null)
                    throw new NotSupportedException($"{opCode} yalnızca referans veya Nullable<T> alanlarda desteklenir.");

                var nullConst = Expression.Constant(null, member.Type);
                return opCode == "IsNull"
                    ? Expression.Equal(member, nullConst)
                    : Expression.NotEqual(member, nullConst);
            }

            // ---- KARŞILAŞTIRMALAR / BETWEEN / IN ----
            var left = underlying != member.Type ? Expression.Convert(member, underlying) : (Expression)member;

            ConstantExpression C(object? v) => Expression.Constant(Coerce(v, underlying), underlying);

            // switch sonucu Expression olmalı (CS8506 fix)
            Expression expr = opCode switch
            {
                "Equals" => Expression.Equal(left, C(values[0])),
                "NotEquals" => Expression.NotEqual(left, C(values[0])),
                "GreaterThan" => Expression.GreaterThan(left, C(values[0])),
                "GreaterThanOrEqual" => Expression.GreaterThanOrEqual(left, C(values[0])),
                "LessThan" => Expression.LessThan(left, C(values[0])),
                "LessThanOrEqual" => Expression.LessThanOrEqual(left, C(values[0])),

                "Between" => Expression.AndAlso(
                                               Expression.GreaterThanOrEqual(left, C(values[0])),
                                               Expression.LessThanOrEqual(left, C(values[1]))),

                "NotBetween" => Expression.Not(
                                               Expression.AndAlso(
                                                   Expression.GreaterThanOrEqual(left, C(values[0])),
                                                   Expression.LessThanOrEqual(left, C(values[1])))),

                "In" => BuildIn(left, underlying, values, positive: true),
                "NotIn" => BuildIn(left, underlying, values, positive: false),

                _ => throw new NotSupportedException($"Desteklenmeyen operator: {opCode}")
            };

            return expr;
        }



        private static object? Coerce(object? value, Type target)
        {
            if (value is null) return null;
            var t = Nullable.GetUnderlyingType(target) ?? target;

            if (t.IsEnum)
                return Enum.Parse(t, value.ToString()!, ignoreCase: true);

            if (t == typeof(Guid))
                return value is Guid g ? g : Guid.Parse(value.ToString()!);

            if (t == typeof(DateTime))
                return value is DateTime dt ? dt : DateTime.Parse(value.ToString()!, CultureInfo.InvariantCulture);

            if (t == typeof(decimal))
                return value is decimal d ? d : Convert.ChangeType(value, t, CultureInfo.InvariantCulture);

            return Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
        }

        private static Expression BuildIn(Expression left, Type underlying, object?[] values, bool positive)
        {
            // T[] oluştur ve Contains(T) uygula
            var arr = Array.CreateInstance(underlying, values.Length);
            for (int i = 0; i < values.Length; i++)
                arr.SetValue(Coerce(values[i], underlying), i);

            var arrConst = Expression.Constant(arr, underlying.MakeArrayType());

            var containsMethod = typeof(Enumerable)
                .GetMethods()
                .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
                .MakeGenericMethod(underlying);

            var containsCall = Expression.Call(containsMethod, arrConst, left);
            return positive ? containsCall : Expression.Not(containsCall);
        }
    }
}
