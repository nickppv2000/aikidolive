using Microsoft.Azure.Cosmos;
using AikidoLive;
using AikidoLive.Services.DBConnector;
using AikidoLive.Services;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<DBServiceConnector>();
builder.Services.AddSingleton(x => 
{
    var cosmosDbConfig = builder.Configuration.GetSection("CosmosDb");
    var account = cosmosDbConfig["Account"];
    var key = cosmosDbConfig["Key"];
    return new CosmosClient(account, key);
});

builder.Services.AddLogging();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<IUserStore<IdentityUser>>(serviceProvider =>
    new DBUserStore(serviceProvider.GetRequiredService<ILogger<UserService>>(), 
                    serviceProvider.GetRequiredService<DBServiceConnector>()));

builder.Services.AddScoped<IRoleStore<IdentityRole>>(serviceProvider =>
    new DBRoleStore(serviceProvider.GetRequiredService<ILogger<RoleService>>(),
            serviceProvider.GetRequiredService<DBServiceConnector>()));


builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AikidoLibraryIdentityContext>();    

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
