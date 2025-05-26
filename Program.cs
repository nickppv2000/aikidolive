using Microsoft.Azure.Cosmos;
using AikidoLive;
using AikidoLive.Services.DBConnector;
using AikidoLive.Services.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Require authentication for Library and Playlists pages
    options.Conventions.AuthorizePage("/LibraryView");
    options.Conventions.AuthorizePage("/Playlists");
    options.Conventions.AuthorizePage("/library-titles");
});

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Add authorization
builder.Services.AddAuthorization();

// Add DBServiceConnector
builder.Services.AddScoped<DBServiceConnector>();
builder.Services.AddSingleton(x => 
{
    var cosmosDbConfig = builder.Configuration.GetSection("CosmosDb");
    var account = cosmosDbConfig["Account"];
    var key = cosmosDbConfig["Key"];
    return new CosmosClient(account, key);
});

// Add Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
