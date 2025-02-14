using dZENcode_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace dZENcode_backend.Data;

public static class Initializer
{
    public static async Task Initialize(ApplicationContext context)
    {
        context.Database.Migrate();

        if (!context.Users.Any() && !context.Comments.Any())
        {
            var user = new User
            {
                Username = "Ivan",
                Email = "ushachovg324@gmail.com"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                UserId = user.Id,
                Content = $"This is comment number 1.",
                CreatedAt = DateTime.Now,
                RepliesCount = 1,
                Replies = new List<Comment>()
                {
                    new Comment
                    {
                        UserId = user.Id,
                        Content = $"This is comment number 2/1.",
                        CreatedAt = DateTime.Now,
                        RepliesCount = 1,
                        Replies = new List<Comment>()
                        {
                            new Comment
                            {
                                UserId = user.Id,
                                Content = $"This is comment number 3/1.",
                                CreatedAt = DateTime.Now,
                            }
                        }
                    }
                }
            };

            await context.Comments.AddAsync(comment);
            await context.SaveChangesAsync();


            await context.SaveChangesAsync();
        }

    }
}
