using MongoDB.Driver;
using user_management.Utilities;

namespace user_management.Data.MongoDB.Logics;

public class Filter<T> : Data.Logics.Filter
{
    public static FilterDefinition<T> BuildDefinition(Data.Logics.Filter filter)
    {
        if (filter.Filters is null || !filter.Filters.Any())
            return Build(filter);

        FilterDefinition<T>[] filterDefinitions = Array.Empty<FilterDefinition<T>>();
        foreach (var f in filter.Filters)
            filterDefinitions = filterDefinitions.Append(BuildDefinition(f)).ToArray();

        return filter.Operation switch
        {
            AND => Builders<T>.Filter.And(filterDefinitions),
            OR => Builders<T>.Filter.Or(filterDefinitions),
            _ => throw new ArgumentException("Invalid operator property.")
        };
    }

    public static FilterDefinition<T> Build(Data.Logics.Filter filter) => filter.Operation switch
    {
        ALL => Builders<T>.Filter.All<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        IN => Builders<T>.Filter.In<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        REGEX => Builders<T>.Filter.Regex(filter.Field!.ToSnakeCase(), filter.Value),
        EXISTS => Builders<T>.Filter.Exists(filter.Field!.ToSnakeCase(), filter.Value),
        EQ => Builders<T>.Filter.Eq<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        NE => Builders<T>.Filter.Ne<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        GT => Builders<T>.Filter.Gt<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        LT => Builders<T>.Filter.Lt<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        GTE => Builders<T>.Filter.Gte<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        LTE => Builders<T>.Filter.Lte<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        ANYEQ => Builders<T>.Filter.AnyEq<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        ANYNE => Builders<T>.Filter.AnyNe<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        ANYGT => Builders<T>.Filter.AnyGt<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        ANYLT => Builders<T>.Filter.AnyLt<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        ANYGTE => Builders<T>.Filter.AnyGte<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        ANYLTE => Builders<T>.Filter.AnyLte<dynamic>(filter.Field!.ToSnakeCase(), filter.Value),
        SIZEEQ => Builders<T>.Filter.Size(filter.Field!.ToSnakeCase(), filter.Value),
        SIZEGT => Builders<T>.Filter.SizeGt(filter.Field!.ToSnakeCase(), filter.Value),
        SIZELT => Builders<T>.Filter.SizeLt(filter.Field!.ToSnakeCase(), filter.Value),
        SIZEGTE => Builders<T>.Filter.SizeGte(filter.Field!.ToSnakeCase(), filter.Value),
        SIZELTE => Builders<T>.Filter.SizeLte(filter.Field!.ToSnakeCase(), filter.Value),
        _ => throw new ArgumentException("Invalid Operation provided"),
    };
}
