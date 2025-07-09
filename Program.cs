using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoApi.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
Console.WriteLine("CORS Origin: " + builder.Configuration["Cors:DefaultOrigin"]);
var allowedOrigin = builder.Configuration["Cors:DefaultOrigin"];
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(allowedOrigin!).AllowAnyHeader().AllowAnyMethod();
                      });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/b50f048f-914d-406f-af34-ebd0524508ab";
        options.Audience = "api://99511933-a7a3-44b1-a6e5-0b2ee669bb7b";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/b50f048f-914d-406f-af34-ebd0524508ab",
                $"https://sts.windows.net/b50f048f-914d-406f-af34-ebd0524508ab"
            }
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);


app.UseAuthentication();
app.UseAuthorization();
app.MapPost("/todos",[Authorize] async (Todo todo, AppDbContext dbContext) =>
{
    dbContext.Todos.Add(todo);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapGet("/todos",[Authorize] async (AppDbContext dbContext) =>
{
    var todos = await dbContext.Todos.ToListAsync();
    return Results.Ok(todos);
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated(); // Creates DB/tables if not already there
    // OR use dbContext.Database.Migrate(); if using EF Migrations
}
app.Run();

