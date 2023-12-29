namespace user_management.Data.Logics;

public class Filter
{
    private string _operation = null!;
    public string Operation
    {
        get { return _operation; }
        set
        {
            if (!AllOperations.Exists(v => v == value))
                throw new ArgumentException("Operation property only accepts following values: " + string.Join(", ", AllOperations));

            _operation = value;
        }
    }

    private string? _type = null;
    public string? Type
    {
        get { return _type; }
        set
        {
            if (value is not null && !Types.AllTypes.Contains(value))
                throw new Exception("Unacceptable value provided for property Type. Following values are acceptable: " + string.Join(", ", Types.AllTypes));
            _type = value;
        }
    }

    public string? Field { get; set; }

    private dynamic? _value = null;
    public dynamic? Value
    {
        get { return _value; }
        set
        {
            if (_type != Types.NULL && value is null)
                _value = null;
            else if (value is null)
                _value = null;
            else
                _value = _type switch
                {
                    null => null,
                    Types.NULL => null,
                    Types.STRING => value.ToString()!,
                    Types.INT => int.Parse(value.ToString()!),
                    Types.LONG => long.Parse(value.ToString()!),
                    Types.FLOAT => float.Parse(value.ToString()!),
                    Types.DOUBLE => double.Parse(value.ToString()!),
                    Types.DECIMAL => decimal.Parse(value.ToString()!),
                    Types.BOOL => bool.Parse(value.ToString()!),
                    Types.DATETIME => DateTime.Parse(value.ToString()!),
                    Types.STRING_ARRAY => value.ToString()!.Split(","),
                    Types.INT_ARRAY => (value.ToString()!.Split(",") as IEnumerable<string>)!.Select(int.Parse).ToArray(),
                    Types.LONG_ARRAY => (value.ToString()!.Split(",") as IEnumerable<string>)!.Select(long.Parse).ToArray(),
                    Types.FLOAT_ARRAY => (value.ToString()!.Split(",") as IEnumerable<string>)!.Select(float.Parse).ToArray(),
                    Types.DOUBLE_ARRAY => (value.ToString()!.Split(",") as IEnumerable<string>)!.Select(double.Parse).ToArray(),
                    Types.DECIMAL_ARRAY => (value.ToString()!.Split(",") as IEnumerable<string>)!.Select(decimal.Parse).ToArray(),
                    Types.BOOL_ARRAY => (value.ToString()!.Split(",") as IEnumerable<string>)!.Select(bool.Parse).ToArray(),
                    Types.DATETIME_ARRAY => (value.ToString()!.Split(",") as IEnumerable<string>)!.Select(DateTime.Parse).ToArray(),
                    _ => throw new ArgumentException($"Invalid value type provided: {_type}"),
                };
        }
    }

    public IEnumerable<Filter>? Filters { get; set; }

    public IEnumerable<string> GetFields()
    {
        if (Filters is null)
            return new string[] { Field! };

        IEnumerable<string> fields = Array.Empty<string>();

        foreach (Filter filter in Filters)
            fields = fields.Concat(filter.GetFields()).Distinct();

        return fields;
    }

    public IEnumerable<string> GetTypes()
    {
        if (Filters is null)
            return new string[] { Type! };

        IEnumerable<string> types = Array.Empty<string>();

        foreach (Filter filter in Filters)
            types = types.Concat(filter.GetTypes()).Distinct();

        return types;
    }

    public IEnumerable<string> GetOperations()
    {
        if (Filters is null)
            return new string[] { Operation! };

        IEnumerable<string> operations = Array.Empty<string>();

        foreach (Filter filter in Filters)
            operations = operations.Concat(filter.GetOperations()).Distinct();

        return operations;
    }

    public readonly static List<string> AllOperations = new() { AND, OR, ALL, IN, REGEX, EXISTS, EQ, NE, GT, LT, GTE, LTE, ANYEQ, ANYNE, ANYGT, ANYLT, ANYGTE, ANYLTE, SIZEEQ, SIZEGT, SIZELT, SIZEGTE, SIZELTE };
    public const string AND = "&&";
    public const string OR = "||";
    public const string ALL = "All";
    public const string IN = "In";
    public const string REGEX = "Regex";
    public const string EXISTS = "Exists";
    public const string EQ = "Eq";
    public const string NE = "Ne";
    public const string GT = "Gt";
    public const string LT = "Lt";
    public const string GTE = "Gte";
    public const string LTE = "Lte";
    public const string ANYEQ = "AnyEq";
    public const string ANYNE = "AnyNe";
    public const string ANYGT = "AnyGt";
    public const string ANYLT = "AnyLt";
    public const string ANYGTE = "AnyGte";
    public const string ANYLTE = "AnyLte";
    public const string SIZEEQ = "SizeEq";
    public const string SIZEGT = "SizeGt";
    public const string SIZELT = "SizeLt";
    public const string SIZEGTE = "SizeGte";
    public const string SIZELTE = "SizeLte";
}
