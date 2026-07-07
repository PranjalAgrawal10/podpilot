using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PodPilot.Contracts.Auth;
using PodPilot.Contracts.Common;
using PodPilot.Contracts.Health;

namespace PodPilot.Api.Tests;

public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthyStatus()
    {
        var response = await client.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<HealthResponse>>();
        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.NotNull(content.Data);
        Assert.Equal("Healthy", content.Data.Status);
        Assert.Contains("api", content.Data.Checks.Keys);
    }
}

public class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_And_Login_Work_End_To_End()
    {
        var uniqueEmail = $"test_{Guid.NewGuid():N}@podpilot.test";

        var registerRequest = new RegisterRequest
        {
            Email = uniqueEmail,
            Password = "SecureP@ss1",
            FirstName = "Test",
            LastName = "User",
            OrganizationName = "Test Organization",
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registerContent = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        Assert.NotNull(registerContent);
        Assert.True(registerContent.Success);
        Assert.NotNull(registerContent.Data);
        Assert.False(string.IsNullOrEmpty(registerContent.Data.AccessToken));
        Assert.False(string.IsNullOrEmpty(registerContent.Data.RefreshToken));

        var loginRequest = new LoginRequest
        {
            Email = uniqueEmail,
            Password = "SecureP@ss1",
        };

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginContent = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        Assert.NotNull(loginContent);
        Assert.True(loginContent.Success);
        Assert.NotNull(loginContent.Data);
        Assert.Equal(uniqueEmail, loginContent.Data.User.Email);
    }

    [Fact]
    public async Task Login_With_Invalid_Credentials_Returns_Unauthorized()
    {
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@podpilot.test",
            Password = "WrongP@ss1",
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
