using Microsoft.AspNetCore.Mvc;

namespace user_management_unit_tests;

public static class HttpAsserts
{
    public static void IsOk(IActionResult actionResult) => Assert.Equal<int>(200, (int)(actionResult as OkResult)!.StatusCode!);
    public static void IsProblem(IActionResult actionResult, int code = 500) => Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
    public static void IsProblem(IActionResult actionResult, string? value, int code = 500)
    {
        Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
        Assert.Equal(value!, (string)((actionResult as ObjectResult)!.Value as ProblemDetails)!.Detail!);
    }
    public static void IsBadRequest(IActionResult actionResult, int code = 400) => Assert.Equal<int>(code, (int)(actionResult as BadRequestResult)!.StatusCode!);
    public static void IsNotFound(IActionResult actionResult, int code = 404) => Assert.Equal<int>(code, (int)(actionResult as NotFoundResult)!.StatusCode!);
    public static void IsUnauthorized(IActionResult actionResult, int code = 403) => Assert.Equal<int>(code, (int)(actionResult as StatusCodeResult)!.StatusCode!);
    public static void IsUnauthenticated(IActionResult actionResult, int code = 401) => Assert.Equal<int>(code, (int)(actionResult as UnauthorizedResult)!.StatusCode!);
    public static void IsRedirectResult(IActionResult actionResult, string? value) => Assert.Equal(value!, (actionResult as RedirectResult)!.Url);
}

public static class HttpAsserts<T>
{
    public static void IsOk(IActionResult actionResult, T? expectedValue)
    {
        Assert.Equal<int>(200, (int)(actionResult as OkObjectResult)!.StatusCode!);
        Assert.Equal<T?>(expectedValue, (T?)(actionResult as OkObjectResult)!.Value);
    }
    public static void IsBadRequest(IActionResult actionResult, T? expectedValue, int code = 400)
    {
        Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
        Assert.Equal<T?>(expectedValue, (T?)(actionResult as ObjectResult)!.Value);
    }
    public static void IsNotFound(IActionResult actionResult, T? expectedValue, int code = 404)
    {
        Assert.Equal<int>(code, (int)(actionResult as NotFoundObjectResult)!.StatusCode!);
        Assert.Equal<T?>(expectedValue, (T?)(actionResult as NotFoundObjectResult)!.Value);
    }
    public static void IsUnauthorized(IActionResult actionResult, T? expectedValue, int code = 403)
    {
        Assert.Equal<int>(code, (int)(actionResult as ObjectResult)!.StatusCode!);
        Assert.Equal<T?>(expectedValue, (T?)(actionResult as ObjectResult)!.Value);
    }
    public static void IsUnauthenticated(IActionResult actionResult, T? expectedValue, int code = 401)
    {
        Assert.Equal<int>(code, (int)(actionResult as UnauthorizedObjectResult)!.StatusCode!);
        Assert.Equal<T?>(expectedValue, (T?)(actionResult as UnauthorizedObjectResult)!.Value);
    }
    public static T? GetValue(IActionResult actionResult) => (T?)(actionResult as ObjectResult)!.Value;
}
