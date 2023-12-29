namespace user_management.Data.Logics;

public class Update
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

    public string Field { get; set; } = null!;

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

    public readonly static List<string> AllOperations = new() { INC, MAX, MIN, MUL, POPFIRST, POPLAST, PULL, PULLALL, PUSH, PUSHEACH, SET };
    public const string INC = "Inc";
    public const string MAX = "Max";
    public const string MIN = "Min";
    public const string MUL = "Mul";
    public const string POPFIRST = "Popfirst";
    public const string POPLAST = "Poplast";
    public const string PULL = "Pull";
    public const string PULLALL = "Pullall";
    public const string PUSH = "Push";
    public const string PUSHEACH = "Pusheach";
    public const string SET = "Set";
}
