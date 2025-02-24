using System.ComponentModel.DataAnnotations;

namespace dZENcode_backend.ModelsDTO;

public class CreateCommentDTO
{
    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Имя пользователя должно содержать только буквы и цифры.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Адрес электронной обязателен.")]
    [EmailAddress(ErrorMessage = "Неверный формат электронной почты.")]
    public string? Email { get; set; }
    
    [Url(ErrorMessage = "Некорректный формат ссылки.")]
    public string? HomePage { get; set; }
    
    [Required(ErrorMessage = "Комментарий обязателен.")]
    public string? Content { get; set; }
    
    public string? ParentId { get; set; }
    
    public IFormFile? Document { get; set; }
    
    public IFormFile? Image { get; set; }
}