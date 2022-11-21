using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";

                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.Cookie.Name = "client1";
                            options.Cookie.SameSite = SameSiteMode.Strict;
                            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        })
                    .AddOpenIdConnect("oidc", options =>
                    {
                        options.Authority = "https://identityserver...de.com";
                        options.RequireHttpsMetadata = true;
                        options.ClientId = "client1";
                        options.ClientSecret = "SuperSecretPassword";
                        options.ResponseType = "code";

                        options.Scope.Clear();

                        options.Scope.Add("openid");
                        options.Scope.Add("profile");


                        options.ClaimActions.MapJsonKey("role", "role", "role");

                        options.SaveTokens = true;

                        options.GetClaimsFromUserInfoEndpoint = true;

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            NameClaimType = "name",
                            RoleClaimType = "role"
                        };

                        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                        options.Events = new OpenIdConnectEvents
                        {
                            OnMessageReceived = context => OnMessageReceived(context),
                            OnRedirectToIdentityProvider = context => OnRedirectToIdentityProvider(context)
                        };
                    });



builder.Services.AddRazorPages();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();

app.Run();





static Task OnMessageReceived(MessageReceivedContext context)
{
    context.Properties.IsPersistent = true;
    context.Properties.ExpiresUtc = new DateTimeOffset(DateTime.Now.AddHours(12));

    return Task.CompletedTask;
}
static Task OnRedirectToIdentityProvider(RedirectContext context)
{
    context.ProtocolMessage.RedirectUri = "https://client...de.com/signin-oidc";
    return Task.CompletedTask;
}

static void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        if (!httpContext.Request.IsHttps || DisallowsSameSiteNone(userAgent))
        {
            // For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
            options.SameSite = SameSiteMode.Unspecified;
        }
    }
}
static bool DisallowsSameSiteNone(string userAgent)
{
    // Cover all iOS based browsers here. This includes:
    // - Safari on iOS 12 for iPhone, iPod Touch, iPad
    // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
    // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
    // All of which are broken by SameSite=None, because they use the iOS networking stack
    if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
    {
        return true;
    }

    // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
    // - Safari on Mac OS X.
    // This does not include:
    // - Chrome on Mac OS X
    // Because they do not use the Mac OS networking stack.
    if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
        userAgent.Contains("Version/") && userAgent.Contains("Safari"))
    {
        return true;
    }

    // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
    // and none in this range require it.
    // Note: this covers some pre-Chromium Edge versions, 
    // but pre-Chromium Edge does not require SameSite=None.
    if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
    {
        return true;
    }

    return false;
}