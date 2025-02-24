using dZENcode_backend.Data;
using dZENcode_backend.Helpers;
using dZENcode_backend.Interfaces;
using dZENcode_backend.Middlewares;
using dZENcode_backend.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        Configure(app);

        app.Run();
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowPolicy", opts =>
            {
                opts
                    .WithOrigins("http://localhost:5173", "http://localhost:80", "http://13.53.64.232")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        builder.Services.AddDbContext<ApplicationContext>(opts =>
        {
            opts.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);
        });

        builder.Services.AddScoped<IComment, CommentRepository>();
        builder.Services.AddScoped<IUser, UserRepository>();
        builder.Services.AddSingleton<FileSaver>();
        builder.Services.AddScoped<ErrorHelper>();
    }

    public static async Task Configure(WebApplication app)
    {
        app.UseCors("AllowPolicy");

        using (var serviceScope = app.Services.CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationContext>();
            await Initializer.Initialize(context);
        }

        app.UseStaticFiles();
        app.UseMiddleware<XssProtectionMiddleware>();
        app.UseAuthorization();
        app.MapControllers();
    }
}