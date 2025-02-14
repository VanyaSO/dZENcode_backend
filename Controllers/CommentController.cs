using dZENcode_backend.Helpers;
using dZENcode_backend.Interfaces;
using dZENcode_backend.Models;
using dZENcode_backend.Models.Pages;
using dZENcode_backend.ModelsDTO;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace dZENcode_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly IComment _comments;
    private readonly IUser _users;
    private readonly IWebHostEnvironment _appEnvironment;

    private readonly string[] _imageFormats = { ".jpg", ".png", ".gif" };
    private readonly (int width, int height) _imageSizes = (320, 240);

    public CommentController(IComment comments, IUser users, IWebHostEnvironment appEnvironment)
    {
        ErrorHelper.ClearErrors();
        _comments = comments;
        _users = users;
        _appEnvironment = appEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMainComments([FromQuery] QueryOptions options)
    {
        options.OrderBy ??= "CreatedAt";

        try
        {
            // Получаем главные коментарии учитывая параметры запроса
            var pagedList = await _comments.GetAllMainCommentsAsync(options);

            return Ok(new
            {
                Comments = pagedList.Select(e => GetCommentDTO(e)),
                TotalPages = pagedList.TotalPages,
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("create-comment")]
    public async Task<ActionResult> CreateComment(CreateCommentDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        User? currentUser = await _users.GetUserByEmailOrUsernameAsync(model.Email, model.Username);
        bool isNewUser = false;
        if (currentUser != null)
        {
            // Проверка данных пользователя
            if (!currentUser.Username.Equals(model.Username))
                ErrorHelper.AddError("username", "Это имя пользователя уже используется для другого адреса электронной почты.");

            if (!currentUser.Email.Equals(model.Email))
                ErrorHelper.AddError("email", "Этот адрес электронной почты уже используется для другого имени пользователя");
        }
        else
        {
            // Создаем нового пользователя
            User newUser = new User()
            {
                Email = model.Email,
                Username = model.Username,
                HomePage = model.HomePage,
            };
            currentUser = newUser;
        }

        // Проверка контента на допустимые теги и их закрытие
        (bool result, string? text) isValidContent = HtmlValidator.IsValid(model.Content);
        if (!isValidContent.result)
            ErrorHelper.AddError("content", isValidContent.text ?? "Произошла ошибка из-за html тегов");
        

        Guid currentParentId = Guid.Empty;
        if (!string.IsNullOrEmpty(model.ParentId))
            if (!Guid.TryParse(model.ParentId, out currentParentId))
                ErrorHelper.AddError("general", "Не удалось ответить на коментарий");

        await ValidateFilesAsync(model.ImageFile, model.DocFile);
        
        if (!ErrorHelper.HasErrors())
        {
            if (currentParentId != Guid.Empty)
            {
                Comment? parentComment = await _comments.GetCommentByIdAsync(currentParentId);
                if (parentComment != null)
                    await _comments.UpdateRepliesCountAsync(parentComment, 1);
            }
            
            if (isNewUser)
                await _users.AddUserAsync(currentUser);
            else if (!string.IsNullOrEmpty(model.HomePage) && !model.HomePage.Equals(currentUser.HomePage)) // Eсли пользователь обновил "HomePage" обновляем в бд
            {
                currentUser.HomePage = model.HomePage;
                await _users.UpdateUserAsync(currentUser);
            }

            Comment newComment = new Comment()
            {
                UserId = currentUser.Id,
                Content = model.Content,
                ParentId = currentParentId == Guid.Empty ? null : currentParentId,
            };

            await _comments.AddComment(newComment);
            return Ok(GetCommentDTO(newComment));
        }

        return BadRequest(ErrorHelper.GetErrorsResponse());
    }

    [HttpGet("{id}/replies")]
    public async Task<ActionResult> GetReplies(string id)
    {
        if (Guid.TryParse(id, out Guid parsedId))
        {
            // Получаем коментарий
            Comment? currentComment = await _comments.GetCommentByIdAsync(parsedId);
            if (currentComment == null)
                return NotFound(ErrorHelper.GetNewErrorResponse("general", "Неверный формат Id"));

            // Возвращаем ответы на найденный коментарий
            IEnumerable<Comment> comments = await _comments.GetRepliesWithUserByIdAsync(parsedId);
            return Ok(comments.Select(e => GetCommentDTO(e)));
        }

        return BadRequest(ErrorHelper.GetNewErrorResponse("general", "Неверный формат Id"));
    }

    private CommentDTO GetCommentDTO(Comment comment) => new CommentDTO()
    {
        Id = comment.Id.ToString(),
        Username = comment.User?.Username ?? "",
        HomePage = comment.User?.HomePage ?? "",
        CreatedAt = $"{comment.CreatedAt.ToShortDateString()} в {comment.CreatedAt.ToShortTimeString()}",
        Content = comment.Content,
        ParentId = comment.ParentId.ToString(),
        RepliesCount = comment.RepliesCount
    };

    private async Task ValidateFilesAsync(IFormFile? imageFile, IFormFile? docFile)
    {
        var fileSaver = HttpContext.RequestServices.GetRequiredService<FileSaver>();
        // Валидация текстового файла
        if (docFile != null)
        {
            string fileExtension = Path.GetExtension(docFile.FileName).ToLower();
            if (fileExtension != ".txt")
                ErrorHelper.AddError("docFile", "Файл должен иметь расширение .txt");

            if (docFile.Length >= (long)100 * 1024)
                ErrorHelper.AddError("docFile", "Максимальный размер текстового файла 100кб");
        }

        // Валидация/ресайз картинки
        if (imageFile != null)
        {
            string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!_imageFormats.Contains(fileExtension))
                ErrorHelper.AddError("imageFile", "Файл должен иметь расширение .jpg, .gif, .png");
        }

        if (!ErrorHelper.HasErrors())
        {
            if (docFile != null) await fileSaver.SaveFileAsync(docFile, "/docs/");
            if (imageFile != null)
            {
                await fileSaver.SaveFileAsync(imageFile, "/images/");
                // using (var memoryStream = new MemoryStream())
                // {
                    // await imageFile.CopyToAsync(memoryStream);
                    // memoryStream.Position = 0;
                
                    // byte[] buffer = new byte[20];
                    // memoryStream.Read(buffer, 0, buffer.Length);
                    // memoryStream.Position = 0;
                    
                    // Console.WriteLine($"First 20 bytes: {BitConverter.ToString(buffer)}");
                
                    // using (var image = await Image.LoadAsync(memoryStream))
                    // {
                        // if (image.Bounds.Width > _imageSizes.width || image.Bounds.Height > _imageSizes.height)
                        // {
                            // float widthRatio = (float)_imageSizes.width / image.Bounds.Width;
                            // float heightRatio = (float)_imageSizes.height / image.Bounds.Height;
                
                            // float scaleRatio = Math.Min(widthRatio, heightRatio);
                
                            // int newWidth = (int)(image.Bounds.Width * scaleRatio);
                            // int newHeight = (int)(image.Bounds.Height * scaleRatio);
                
                            // image.Mutate(x => x.Resize(newWidth, newHeight));
                        // }   
                        // await fileSaver.SaveFileAsync(image, "/images/", imageFile.FileName);
                    // }
                // }
            } 
        }
    }
}