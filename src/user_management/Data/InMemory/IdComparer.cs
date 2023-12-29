namespace user_management.Data.InMemory;

public class IdComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x is null || y is null) throw new Exception();

        if (long.Parse(x) > long.Parse(y)) return 1;
        if (long.Parse(x) == long.Parse(y)) return 0;
        if (long.Parse(x) < long.Parse(y)) return -1;

        throw new Exception();
    }
}
