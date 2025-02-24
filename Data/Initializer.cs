using dZENcode_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace dZENcode_backend.Data;

public static class Initializer
{
    public static async Task Initialize(ApplicationContext context)
    {
        context.Database.Migrate();
    }
}
