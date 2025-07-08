using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoApi.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.MapPost("/todos",async (Todo todo, AppDbContext dbContext) =>
{
    dbContext.Todos.Add(todo);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapGet("/todos", async (AppDbContext dbContext) =>
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

