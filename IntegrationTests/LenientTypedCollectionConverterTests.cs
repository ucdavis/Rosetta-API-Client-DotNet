using System.Text.Json;
using Shouldly;
using UCD.Rosetta.Client.Core.Converters;

namespace IntegrationTests;

public class LenientTypedCollectionConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new LenientTypedCollectionConverterFactory() }
    };

    [Fact]
    public void Deserialize_SkipsInvalidArrayElementsAndKeepsValidElements()
    {
        var json = """
            {
              "numbers": [1, {"bad": true}, 2, "bad", 3],
              "nullableNumbers": [4, "bad", 5],
              "ids": ["11111111-1111-1111-1111-111111111111", 7, "22222222-2222-2222-2222-222222222222"],
              "dates": ["2024-01-02T00:00:00Z", false, "2024-01-03T00:00:00Z"],
              "nestedNumbers": [[1, 2], "bad", [3]],
              "objects": [1, "two", {"three": 3}, [4]]
            }
            """;

        var result = JsonSerializer.Deserialize<CollectionSample>(json, Options);

        result.ShouldNotBeNull();
        result.Numbers.ShouldBe([1, 2, 3]);
        result.NullableNumbers.ShouldBe([4, 5]);
        result.Ids.ShouldBe([
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222")
        ]);
        result.Dates.ShouldBe([
            new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc)
        ]);
        result.NestedNumbers.Select(numbers => numbers.ToArray()).ShouldBe(new[] { new[] { 1, 2 }, [3] });
        result.Objects.Count.ShouldBe(4);
        result.Objects.ShouldAllBe(value => value is JsonElement);
    }

    [Fact]
    public void Deserialize_WrapsScalarValuesInCollections()
    {
        var json = """
            {
              "numbers": 1,
              "nullableNumbers": 2,
              "strings": "alpha",
              "ids": "11111111-1111-1111-1111-111111111111",
              "dates": "2024-01-02T00:00:00Z"
            }
            """;

        var result = JsonSerializer.Deserialize<CollectionSample>(json, Options);

        result.ShouldNotBeNull();
        result.Numbers.ShouldBe([1]);
        result.NullableNumbers.ShouldBe([2]);
        result.Strings.ShouldBe(["alpha"]);
        result.Ids.ShouldBe([Guid.Parse("11111111-1111-1111-1111-111111111111")]);
        result.Dates.ShouldBe([
            new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        ]);
    }

    private sealed class CollectionSample
    {
        public ICollection<int> Numbers { get; set; } = [];
        public ICollection<int?> NullableNumbers { get; set; } = [];
        public ICollection<string> Strings { get; set; } = [];
        public ICollection<Guid> Ids { get; set; } = [];
        public ICollection<DateTime> Dates { get; set; } = [];
        public ICollection<int[]> NestedNumbers { get; set; } = [];
        public ICollection<object> Objects { get; set; } = [];
    }
}
