using System.ComponentModel.DataAnnotations;

namespace dZENcode_backend.ModelsDTO;

public class CreateCommentDTO
{
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Имя пользователя должно содержать только буквы и цифры.")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "Неверный формат электронной почты")]
    [Required(ErrorMessage = "Адрес электронной обязателен")]
    public string? Email { get; set; }
    
    [Url(ErrorMessage = "Некорректный формат ссылки.")]
    public string? HomePage { get; set; }
    
    [Required(ErrorMessage = "Комментарий обязателен")]
    public string? Content { get; set; }
    public string? ParentId { get; set; }
    
    public IFormFile? DocFile { get; set; }
    
    public IFormFile? ImageFile { get; set; }
}