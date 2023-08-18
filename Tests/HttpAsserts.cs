using Microsoft.AspNetCore.Mvc;
using Xunit;

public static class HttpAsserts
{
    public static void IsOk(IActionResult actionResult) => Assert.Equal<int>(200, (int)(actionResult as StatusCodeResult)!.StatusCode!);
    public static void IsProblem(IActionResult actionResult, int code = 500) => Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
    public static void IsProblem(IActionResult actionResult, string? value, int code = 500)
    {
        Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
        Assert.Equal<string>(value!, (string)((actionResult as ObjectResult)!.Value as ProblemDetails)!.Detail!);
    }
    public static void IsBadRequest(IActionResult actionResult, int code = 400) => Assert.Equal<int>(code, (int)(actionResult as StatusCodeResult)!.StatusCode!);
    public static void IsNotFound(IActionResult actionResult, int code = 404) => Assert.Equal<int>(code, (int)(actionResult as StatusCodeResult)!.StatusCode!);
    public static void IsUnauthorized(IActionResult actionResult, int code = 403) => Assert.Equal<int>(code, (int)(actionResult as StatusCodeResult)!.StatusCode!);
    public static void IsUnauthenticated(IActionResult actionResult, int code = 401) => Assert.Equal<int>(code, (int)(actionResult as StatusCodeResult)!.StatusCode!);
}

public static class HttpAsserts<T>
{
    public static void IsOk(IActionResult actionResult, T? value)
    {
        Assert.Equal<int>(200, (int)(actionResult as ObjectResult)!.StatusCode!);
        Assert.Equal<T>(value!, (T)(actionResult as ObjectResult)!.Value!);
    }
    public static void IsBadRequest(IActionResult actionResult, T? value, int code = 400)
    {
        Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
        Assert.Equal<T>(value!, (T)(actionResult as ObjectResult)!.Value!);
    }
    public static void IsNotFound(IActionResult actionResult, T? value, int code = 404)
    {
        Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
        Assert.Equal<T>(value!, (T)(actionResult as ObjectResult)!.Value!);
    }
    public static void IsUnauthorized(IActionResult actionResult, T? value, int code = 403)
    {
        Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
        Assert.Equal<T>(value!, (T)(actionResult as ObjectResult)!.Value!);
    }
}