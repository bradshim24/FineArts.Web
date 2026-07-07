using FineArts.Core;

var builder = WebApplication.CreateBuilder(args);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connStr))
    Database.ConnectionString = connStr;

builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".FineArts.Session";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Simple password-based auth: redirect to /Login if not authenticated
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "/";
    bool isLoginPage = path.Equals("/Login", StringComparison.OrdinalIgnoreCase)
                    || path.Equals("/", StringComparison.OrdinalIgnoreCase);

    if (!isLoginPage && string.IsNullOrEmpty(context.Session.GetString("Auth")))
    {
        context.Response.Redirect("/Login");
        return;
    }
    await next();
});

app.MapRazorPages();

app.Run();
