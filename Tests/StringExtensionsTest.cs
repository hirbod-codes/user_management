using user_management.Utilities;
using Xunit;

namespace user_management.Tests;

public class StringExtensionsTest
{
    [Fact]
    public void test_to_pascal_case()
    {
        string str = "is_verified";
        string result = StringExtentions.ToPascalCase(str);

        Assert.Equal("IsVerified", result);
    }

    [Fact]
    public void test_to_snake_case()
    {
        string str = "IsVerified";
        string result = StringExtentions.ToSnakeCase(str);

        Assert.Equal("is_verified", result);
    }
}