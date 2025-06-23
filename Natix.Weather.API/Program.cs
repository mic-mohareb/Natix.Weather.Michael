using Natix.Weather.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Natix Weather API V1");
        options.RoutePrefix = string.Empty; // Optional: serves Swagger UI at root
    });
}

app.Run();
