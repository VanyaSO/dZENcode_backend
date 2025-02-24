using dZENcode_backend.Helpers;
using dZENcode_backend.Interfaces;
using dZENcode_backend.Models;
using dZENcode_backend.Models.Pages;
using dZENcode_backend.ModelsDTO;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using FileInfo = dZENcode_backend.Models.FileInfo;

namespace dZENcode_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly IComment _comments;
    private readonly IUser _users;
    private readonly ErrorHelper _errors;

    private readonly string[] _imageFormats = { ".jpg", ".png", ".gif" };

    public CommentController(IComment comments, IUser users, ErrorHelper errors)
    {
        _comments = comments;
        _users = users;
        _errors = errors;
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
                Comments = pagedList.Select(e => GetCommentDTO(e)).ToList(),
                TotalPages = pagedList.TotalPages,
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateComment([FromServices]FileSaver fileSaver, CreateCommentDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        User? currentUser = await _users.GetUserByEmailOrUsernameAsync(model.Email, model.Username);
        bool isNewUser = false;
        if (currentUser != null)
        {
            // Проверка данных пользователя
            if (!currentUser.Username.Equals(model.Username))
                _errors.AddError("email",
                    "Этот адрес электронной почты уже используется для другого имени пользователя");

            if (!currentUser.Email.Equals(model.Email))
                _errors.AddError("username",
                    "Это имя пользователя уже используется для другого адреса электронной почты.");
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
            isNewUser = true;
            currentUser = newUser;
        }

        // Проверка контента на допустимые теги и их закрытие
        (bool result, string? text) isValidContent = HtmlValidator.IsValid(model.Content);
        if (!isValidContent.result)
            _errors.AddError("content", isValidContent.text ?? "Произошла ошибка из-за html тегов");

        ValidateFiles(model.Image, model.Document);

        Guid currentParentId = Guid.Empty;
        if (!string.IsNullOrEmpty(model.ParentId))
        {
            if (Guid.TryParse(model.ParentId, out currentParentId) && !_errors.HasErrors())
            {
                Comment? parentComment = await _comments.GetCommentByIdAsync(currentParentId);
                if (parentComment != null)
                    await _comments.UpdateRepliesCountAsync(parentComment, 1);
            }
            else
                _errors.AddError("general", "Не удалось ответить на коментарий");
        }


        if (!_errors.HasErrors())
        {
            if (isNewUser)
                await _users.AddUserAsync(currentUser);
            else if (!string.IsNullOrEmpty(model.HomePage) && !model.HomePage.Equals(currentUser.HomePage)) // Eсли пользователь обновил "HomePage" обновляем в бд
            {
                currentUser.HomePage = model.HomePage;
                await _users.UpdateUserAsync(currentUser);
            }

            FileInfo? documentInfo = null, imageInfo = null;
            if (model.Document != null)
                documentInfo = await fileSaver.SaveFileAsync(model.Document, "uploads");

            if (model.Image != null)
            {
                Image resizeImage = await ResizeImageAsync(model.Image, 320, 240);
                imageInfo = await fileSaver.SaveFileAsync(resizeImage, "uploads", model.Image.FileName);
                resizeImage.Dispose();
            }
                            
            Comment newComment = new Comment()
            {
                UserId = currentUser.Id,
                Content = model.Content,
                ParentId = currentParentId == Guid.Empty ? null : currentParentId,
                Document = documentInfo,
                Image = imageInfo,
            };
                    
            await _comments.AddComment(newComment);
            return Ok(GetCommentDTO(newComment));
        }

        return BadRequest(new { errors = _errors.GetErrors() });
    }

    [HttpGet("replies/{id}")]
    public async Task<ActionResult> GetReplies(string id)
    {
        if (Guid.TryParse(id, out Guid parsedId))
        {
            // Получаем коментарий
            Comment? currentComment = await _comments.GetCommentByIdAsync(parsedId);
            if (currentComment == null)
                return NotFound(_errors.GetNewErrorResponse("general", "Коментарий не найден"));

            // Возвращаем ответы на найденный коментарий
            IEnumerable<Comment> comments = await _comments.GetRepliesWithUserByIdAsync(parsedId);
            return Ok(comments.Select(e => GetCommentDTO(e)));
        }

        return BadRequest(_errors.GetNewErrorResponse("general", "Неверный формат Id"));
    }

    [HttpGet("download-file")]
    public IActionResult DownloadFile([FromServices] IWebHostEnvironment appEnvironment, [FromQuery] string path)
    {
        Uri uri = new Uri(path);

        string filePath = appEnvironment.WebRootPath + uri.AbsolutePath;
        string fileName = Path.GetFileName(path).Substring(36);
        
        if (!System.IO.File.Exists(filePath))
            return NotFound(_errors.GetNewErrorResponse("general", "Файл не найден."));

        string fileType = fileName.GetMimeType();

        try
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            return File(fs, fileType, fileName);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Нет доступа к файлу.");
        }
    }
    
    private CommentDTO GetCommentDTO(Comment comment) => new CommentDTO()
    {
        Id = comment.Id.ToString(),
        Username = comment.User?.Username ?? "",
        Email = comment.User?.Email ?? "",
        HomePage = comment.User?.HomePage ?? "",
        CreatedAt = $"{comment.CreatedAt.ToShortDateString()} в {comment.CreatedAt.ToShortTimeString()} UTC",
        Content = comment.Content,
        Parent = comment.ParentId != null
            ? new CommentParentDTO() { Username = comment.Parent?.User?.Username, Content = comment.Parent?.Content }
            : null,
        Image =  comment.Image?.BuildFullPath(HttpContext.Request),
        Document = comment.Document?.BuildFullPath(HttpContext.Request),
        RepliesCount = comment.RepliesCount
    };

    private void ValidateFiles(IFormFile? imageFile, IFormFile? docFile)
    {
        // Валидация текстового файла
        if (docFile != null)
        {
            string fileExtension = Path.GetExtension(docFile.FileName).ToLower();
            if (fileExtension != ".txt")
                _errors.AddError("document", "Файл должен иметь расширение .txt");

            if (docFile.Length >= (long)100 * 1024)
                _errors.AddError("document", "Максимальный размер текстового файла 100кб");
        }

        // Валидация/ресайз картинки
        if (imageFile != null)
        {
            string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!_imageFormats.Contains(fileExtension))
                _errors.AddError("image", "Файл должен иметь расширение .jpg, .gif, .png");
        }
    }

    private async Task<Image> ResizeImageAsync(IFormFile imageFile, int width, int height)
    {
        using (var stream = imageFile.OpenReadStream())
        {
            Image image = await Image.LoadAsync(stream);
            if (image.Bounds.Width > width || image.Bounds.Height > height)
            {
                float widthRatio = (float)width / image.Bounds.Width;
                float heightRatio = (float)height / image.Bounds.Height;

                float scaleRatio = Math.Min(widthRatio, heightRatio);

                int newWidth = (int)(image.Bounds.Width * scaleRatio);
                int newHeight = (int)(image.Bounds.Height * scaleRatio);

                image.Mutate(x => x.Resize(newWidth, newHeight));
            }

            return image;
        }
    }
}