using Verbum.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IndexingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// Get the instance of IndexingService and build the index on startup
using (var scope = app.Services.CreateScope())
{
    var indexingService = scope.ServiceProvider.GetRequiredService<IndexingService>();
    indexingService.BuildIndex();
}

await app.RunAsync();