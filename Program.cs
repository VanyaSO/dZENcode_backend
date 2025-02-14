using dZENcode_backend.Data;
using dZENcode_backend.Helpers;
using dZENcode_backend.Interfaces;
using dZENcode_backend.Middlewares;
using dZENcode_backend.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});

builder.Services.AddScoped<IComment, CommentRepository>();
builder.Services.AddScoped<IUser, UserRepository>();
builder.Services.AddSingleton<FileSaver>();

var app = builder.Build();
app.UseCors(policy => policy.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

using (var serviceScope = app.Services.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationContext>();
    await Initializer.Initialize(context);
}

app.UseMiddleware<XssProtectionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();