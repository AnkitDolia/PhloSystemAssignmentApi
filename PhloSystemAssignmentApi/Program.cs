using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using PhloSystemAssignmentApi;
using PhloSystemAssignmentApi.Services;
using PhloSystemAssignmentApi.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddMemoryCache();

// Add basic Security & Regulation support
builder.Services.AddCors(options =>
{
    options.AddPolicy(ConstantsVariables.CorsPolicyName, builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(ConstantsVariables.SwaggerEndpointVersion,
        new OpenApiInfo { Title = ConstantsVariables.SwaggerEndpointName, Version = ConstantsVariables.SwaggerEndpointVersion });
    c.EnableAnnotations();
});

builder.Services.AddSingleton<IProductManage, ProductManage>();
builder.Services.AddSingleton<IProductService, ProductService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
