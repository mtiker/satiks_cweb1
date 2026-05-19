using Base.Domain;
using Base.Helpers;
using System.Security.Claims;

namespace WebApp.Tests.Unit;

public class LangStrTests
{
    [Fact]
    public void Constructor_WithValue_UsesCurrentCulture()
    {
        var ls = new LangStr("Hello");
        Assert.NotEmpty(ls);
    }

    [Fact]
    public void ImplicitToString_ReturnsTranslatedValue()
    {
        var ls = new LangStr("World", "en");
        string result = ls;
        Assert.Equal("World", result);
    }

    [Fact]
    public void ImplicitToLangStr_FromString_WrapsValue()
    {
        LangStr ls = "Foo";
        Assert.Equal("Foo", ls.ToString());
    }

    [Fact]
    public void ImplicitToString_NullLangStr_ReturnsNull()
    {
        LangStr? ls = null;
        string result = ls;
        Assert.Equal("null", result);
    }
}

public class IdentityHelpersTests
{
    private const string Key      = "super-secret-key-at-least-32-bytes-long!!";
    private const string Issuer   = "test-issuer";
    private const string Audience = "test-audience";

    [Fact]
    public void ValidateJWT_ValidToken_ReturnsTrue()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "user") };
        var jwt    = IdentityHelpers.GenerateJwt(claims, Key, Issuer, Audience, 60);

        Assert.True(IdentityHelpers.ValidateJWT(jwt, Key, Issuer, Audience));
    }

    [Fact]
    public void ValidateJWT_WrongKey_ReturnsFalse()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "user") };
        var jwt    = IdentityHelpers.GenerateJwt(claims, Key, Issuer, Audience, 60);

        Assert.False(IdentityHelpers.ValidateJWT(jwt, "wrong-key-that-is-also-32-bytes-lon!!", Issuer, Audience));
    }

    [Fact]
    public void ValidateJWT_GarbageToken_ReturnsFalse()
    {
        Assert.False(IdentityHelpers.ValidateJWT("not.a.jwt", Key, Issuer, Audience));
    }
}
