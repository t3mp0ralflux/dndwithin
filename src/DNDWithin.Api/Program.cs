using Asp.Versioning;
using DNDWithin.Api.Mapping;
using DNDWithin.Application;
using DNDWithin.Application.Database;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager config = builder.Configuration;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1.0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
    x.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
}).AddMvc();

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddDatabase(config["ConnectionStrings:Database"]!);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("DND Within API")
            .WithTheme(ScalarTheme.Mars)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ValidationMappingMiddleware>();

app.MapControllers();

DbInitializer dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();