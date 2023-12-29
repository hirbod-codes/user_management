using System.Text.RegularExpressions;
using user_management.Data.Logics;
using user_management.Utilities;

namespace user_management.Data.InMemory.Logics;

public class Filter<T> : Data.Logics.Filter
{
    public static Func<dynamic, bool> BuildDefinition(Data.Logics.Filter filter)
    {
        if (filter.Filters is null || !filter.Filters.Any())
            return Build(filter);

        Func<dynamic, bool>[] filterDefinitions = Array.Empty<Func<dynamic, bool>>();
        foreach (var f in filter.Filters)
            filterDefinitions = filterDefinitions.Append(BuildDefinition(f)).ToArray();

        return filter.Operation switch
        {
            AND => record =>
            {
                foreach (var filter in filterDefinitions)
                    if (!filter(record))
                        return false;
                return true;
            }
            ,
            OR => record =>
            {
                foreach (var filter in filterDefinitions)
                    if (filter(record))
                        return true;
                return false;
            }
            ,
            _ => throw new ArgumentException("Invalid operator property."),
        };
    }

    public static Func<dynamic, bool> Build(Filter filter) => filter.Operation switch
    {
        ALL => (object record) => filter.Type switch
        {
            Types.INT_ARRAY => (filter.Value as IEnumerable<int>)!.Where(v => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Where(vv => v == vv).Any()).Any(),
            Types.FLOAT_ARRAY => (filter.Value as IEnumerable<float>)!.Where(v => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Where(vv => v == vv).Any()).Any(),
            Types.DECIMAL_ARRAY => (filter.Value as IEnumerable<decimal>)!.Where(v => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Where(vv => v == vv).Any()).Any(),
            Types.DOUBLE_ARRAY => (filter.Value as IEnumerable<double>)!.Where(v => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Where(vv => v == vv).Any()).Any(),
            Types.LONG_ARRAY => (filter.Value as IEnumerable<long>)!.Where(v => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Where(vv => v == vv).Any()).Any(),
            Types.BOOL_ARRAY => (filter.Value as IEnumerable<bool>)!.Where(v => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<bool>)!.Where(vv => v == vv).Any()).Any(),
            Types.DATETIME_ARRAY => (filter.Value as IEnumerable<DateTime>)!.Where(v => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Where(vv => v == vv).Any()).Any(),
            Types.STRING_ARRAY => (filter.Value as IEnumerable<string>)!.Where(v => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Where(vv => v == vv).Any()).Any(),
            _ => throw new Exception("Invalid type provided")
        },
        IN => (object record) => filter.Type switch
        {
            Types.INT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Where(v => v == (int)filter.Value).Any(),
            Types.FLOAT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Where(v => v == (float)filter.Value).Any(),
            Types.DECIMAL_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Where(v => v == (decimal)filter.Value).Any(),
            Types.DOUBLE_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Where(v => v == (double)filter.Value).Any(),
            Types.LONG_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Where(v => v == (long)filter.Value).Any(),
            Types.BOOL_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<bool>)!.Where(v => v == (bool)filter.Value).Any(),
            Types.DATETIME_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Where(v => v == (DateTime)filter.Value).Any(),
            Types.STRING_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Where(v => v == (string)filter.Value!).Any(),
            _ => throw new Exception("Invalid type provided")
        },
        REGEX => (object record) =>
        {
            if (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as string is null || filter.Type != Types.STRING)
                throw new Exception($"Invalid type provided for this operation: {filter.Operation}.");
            return Regex.IsMatch(record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as string, filter.Value);
        }
        ,
        EXISTS => (object record) => record.GetType().GetProperty(filter.Field!.ToPascalCase()) is not null,
        EQ => (object record) => filter.Type switch
        {
            Types.NULL => (object?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (object?)filter.Value,
            Types.INT => (int?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (int?)filter.Value,
            Types.FLOAT => (float?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (float?)filter.Value,
            Types.DECIMAL => (decimal?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (decimal?)filter.Value,
            Types.DOUBLE => (double?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (double?)filter.Value,
            Types.LONG => (long?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (long?)filter.Value,
            Types.BOOL => (bool?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (bool?)filter.Value,
            Types.DATETIME => (DateTime?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (DateTime?)filter.Value,
            Types.STRING => (string?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) == (string?)filter.Value,
            _ => throw new Exception("Invalid type provided")
        },
        NE => (object record) => filter.Type switch
        {
            Types.NULL => (object?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) is not null,
            Types.INT => (int?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) != (int?)filter.Value,
            Types.FLOAT => (float?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) != (float?)filter.Value,
            Types.DECIMAL => (decimal?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) != (decimal?)filter.Value,
            Types.DOUBLE => (double?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) != (double?)filter.Value,
            Types.LONG => (long?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) != (long?)filter.Value,
            Types.BOOL => (bool?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) != (bool?)filter.Value,
            Types.DATETIME => (DateTime?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) != (DateTime?)filter.Value,
            Types.STRING => (string?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) != (string?)filter.Value,
            _ => throw new Exception("Invalid type provided")
        },
        GT => (object record) => filter.Type switch
        {
            Types.INT => (int?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) > (int?)filter.Value,
            Types.FLOAT => (float?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) > (float?)filter.Value,
            Types.DECIMAL => (decimal?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) > (decimal?)filter.Value,
            Types.DOUBLE => (double?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) > (double?)filter.Value,
            Types.LONG => (long?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) > (long?)filter.Value,
            Types.DATETIME => (DateTime?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) > (DateTime?)filter.Value,
            Types.STRING => ((string)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record)!).Length > ((string)filter.Value!).Length,
            _ => throw new Exception("Invalid type provided")
        },
        LT => (object record) => filter.Type switch
        {
            Types.INT => (int?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) < (int?)filter.Value,
            Types.FLOAT => (float?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) < (float?)filter.Value,
            Types.DECIMAL => (decimal?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) < (decimal?)filter.Value,
            Types.DOUBLE => (double?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) < (double?)filter.Value,
            Types.LONG => (long?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) < (long?)filter.Value,
            Types.DATETIME => (DateTime?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) < (DateTime?)filter.Value,
            Types.STRING => ((string)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record)!).Length < ((string)filter.Value!).Length,
            _ => throw new Exception("Invalid type provided")
        },
        GTE => (object record) => filter.Type switch
        {
            Types.INT => (int?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) >= (int?)filter.Value,
            Types.FLOAT => (float?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) >= (float?)filter.Value,
            Types.DECIMAL => (decimal?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) >= (decimal?)filter.Value,
            Types.DOUBLE => (double?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) >= (double?)filter.Value,
            Types.LONG => (long?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) >= (long?)filter.Value,
            Types.DATETIME => (DateTime?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) >= (DateTime?)filter.Value,
            Types.STRING => ((string)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record)!).Length >= ((string)filter.Value!).Length,
            _ => throw new Exception("Invalid type provided")
        },
        LTE => (object record) => filter.Type switch
        {
            Types.INT => (int?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) <= (int?)filter.Value,
            Types.FLOAT => (float?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) <= (float?)filter.Value,
            Types.DECIMAL => (decimal?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) <= (decimal?)filter.Value,
            Types.DOUBLE => (double?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) <= (double?)filter.Value,
            Types.LONG => (long?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) <= (long?)filter.Value,
            Types.DATETIME => (DateTime?)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) <= (DateTime?)filter.Value,
            Types.STRING => ((string)record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record)!).Length <= ((string)filter.Value!).Length,
            _ => throw new Exception("Invalid type provided")
        },
        ANYEQ => (object record) => filter.Type switch
        {
            Types.INT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Where(v => Equals(v, (int)filter.Value)).Any(),
            Types.FLOAT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Where(v => Equals(v, (float)filter.Value)).Any(),
            Types.DECIMAL => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Where(v => Equals(v, (decimal)filter.Value)).Any(),
            Types.DOUBLE => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Where(v => Equals(v, (double)filter.Value)).Any(),
            Types.LONG => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Where(v => Equals(v, (long)filter.Value)).Any(),
            Types.DATETIME => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Where(v => Equals(v, (DateTime)filter.Value)).Any(),
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Where(v => Equals(v, (string)filter.Value!)).Any(),
            _ => throw new Exception("Invalid type provided")
        },
        ANYNE => (object record) => filter.Type switch
        {
            Types.INT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Where(v => !Equals(v, (int)filter.Value)).Any(),
            Types.FLOAT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Where(v => !Equals(v, (float)filter.Value)).Any(),
            Types.DECIMAL => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Where(v => !Equals(v, (decimal)filter.Value)).Any(),
            Types.DOUBLE => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Where(v => !Equals(v, (double)filter.Value)).Any(),
            Types.LONG => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Where(v => !Equals(v, (long)filter.Value)).Any(),
            Types.DATETIME => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Where(v => !Equals(v, (DateTime)filter.Value)).Any(),
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Where(v => !Equals(v, (string)filter.Value!)).Any(),
            _ => throw new Exception("Invalid type provided")
        },
        ANYGT => (object record) => filter.Type switch
        {
            Types.INT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Where(v => v > (int)filter.Value).Any(),
            Types.FLOAT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Where(v => v > (float)filter.Value).Any(),
            Types.DECIMAL => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Where(v => v > (decimal)filter.Value).Any(),
            Types.DOUBLE => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Where(v => v > (double)filter.Value).Any(),
            Types.LONG => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Where(v => v > (long)filter.Value).Any(),
            Types.DATETIME => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Where(v => v > (DateTime)filter.Value).Any(),
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Where(v => v.Length > ((string)filter.Value!).Length).Any(),
            _ => throw new Exception("Invalid type provided")
        },
        ANYLT => (object record) => filter.Type switch
        {
            Types.INT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Where(v => v < (int)filter.Value).Any(),
            Types.FLOAT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Where(v => v < (float)filter.Value).Any(),
            Types.DECIMAL => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Where(v => v < (decimal)filter.Value).Any(),
            Types.DOUBLE => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Where(v => v < (double)filter.Value).Any(),
            Types.LONG => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Where(v => v < (long)filter.Value).Any(),
            Types.DATETIME => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Where(v => v < (DateTime)filter.Value).Any(),
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Where(v => v.Length < ((string)filter.Value!).Length).Any(),
            _ => throw new Exception("Invalid type provided")
        },
        ANYGTE => (object record) => filter.Type switch
        {
            Types.INT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Where(v => v >= (int)filter.Value).Any(),
            Types.FLOAT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Where(v => v >= (float)filter.Value).Any(),
            Types.DECIMAL => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Where(v => v >= (decimal)filter.Value).Any(),
            Types.DOUBLE => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Where(v => v >= (double)filter.Value).Any(),
            Types.LONG => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Where(v => v >= (long)filter.Value).Any(),
            Types.DATETIME => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Where(v => v >= (DateTime)filter.Value).Any(),
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Where(v => v.Length >= ((string)filter.Value!).Length).Any(),
            _ => throw new Exception("Invalid type provided")
        },
        ANYLTE => (object record) => filter.Type switch
        {
            Types.INT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Where(v => v <= (int)filter.Value).Any(),
            Types.FLOAT => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Where(v => v <= (float)filter.Value).Any(),
            Types.DECIMAL => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Where(v => v <= (decimal)filter.Value).Any(),
            Types.DOUBLE => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Where(v => v <= (double)filter.Value).Any(),
            Types.LONG => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Where(v => v <= (long)filter.Value).Any(),
            Types.DATETIME => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Where(v => v <= (DateTime)filter.Value).Any(),
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Where(v => v.Length <= ((string)filter.Value!).Length).Any(),
            _ => throw new Exception("Invalid type provided")
        },
        SIZEEQ => (object record) => filter.Type switch
        {
            Types.INT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Count() == (int)filter.Value,
            Types.FLOAT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Count() == (int)filter.Value,
            Types.DECIMAL_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Count() == (int)filter.Value,
            Types.DOUBLE_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Count() == (int)filter.Value,
            Types.LONG_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Count() == (int)filter.Value,
            Types.DATETIME_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Count() == (int)filter.Value,
            Types.STRING_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Count() == (int)filter.Value,
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as string)!.Length == (int)filter.Value,
            _ => throw new Exception("Invalid type provided")
        },
        SIZEGT => (object record) => filter.Type switch
        {
            Types.INT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Count() > (int)filter.Value,
            Types.FLOAT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Count() > (int)filter.Value,
            Types.DECIMAL_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Count() > (int)filter.Value,
            Types.DOUBLE_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Count() > (int)filter.Value,
            Types.LONG_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Count() > (int)filter.Value,
            Types.DATETIME_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Count() > (int)filter.Value,
            Types.STRING_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Count() > (int)filter.Value,
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as string)!.Length > (int)filter.Value,
            _ => throw new Exception("Invalid type provided")
        },
        SIZELT => (object record) => filter.Type switch
        {
            Types.INT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Count() < (int)filter.Value,
            Types.FLOAT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Count() < (int)filter.Value,
            Types.DECIMAL_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Count() < (int)filter.Value,
            Types.DOUBLE_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Count() < (int)filter.Value,
            Types.LONG_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Count() < (int)filter.Value,
            Types.DATETIME_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Count() < (int)filter.Value,
            Types.STRING_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Count() < (int)filter.Value,
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as string)!.Length < (int)filter.Value,
            _ => throw new Exception("Invalid type provided")
        },
        SIZEGTE => (object record) => filter.Type switch
        {
            Types.INT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Count() >= (int)filter.Value,
            Types.FLOAT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Count() >= (int)filter.Value,
            Types.DECIMAL_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Count() >= (int)filter.Value,
            Types.DOUBLE_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Count() >= (int)filter.Value,
            Types.LONG_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Count() >= (int)filter.Value,
            Types.DATETIME_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Count() >= (int)filter.Value,
            Types.STRING_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Count() >= (int)filter.Value,
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as string)!.Length >= (int)filter.Value,
            _ => throw new Exception("Invalid type provided")
        },
        SIZELTE => (object record) => filter.Type switch
        {
            Types.INT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<int>)!.Count() <= (int)filter.Value,
            Types.FLOAT_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<float>)!.Count() <= (int)filter.Value,
            Types.DECIMAL_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<decimal>)!.Count() <= (int)filter.Value,
            Types.DOUBLE_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<double>)!.Count() <= (int)filter.Value,
            Types.LONG_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<long>)!.Count() <= (int)filter.Value,
            Types.DATETIME_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<DateTime>)!.Count() <= (int)filter.Value,
            Types.STRING_ARRAY => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as IEnumerable<string>)!.Count() <= (int)filter.Value,
            Types.STRING => (record.GetType().GetProperty(filter.Field!.ToPascalCase())!.GetValue(record) as string)!.Length <= (int)filter.Value,
            _ => throw new Exception("Invalid type provided")
        },
        _ => throw new ArgumentException("Invalid Operation provided."),
    };
}
