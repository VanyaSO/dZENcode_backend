using dZENcode_backend.Helpers;
using dZENcode_backend.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace dZENcode_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CaptchaController : ControllerBase
{
    private readonly string _sessionKey = "Captcha";
    private readonly Random _random = new Random();
    private readonly ErrorHelper _errors;
    private readonly FontCollection _fonts;

    public CaptchaController(ErrorHelper errors)
    {
        _errors = errors;
        _fonts = new FontCollection();
        _fonts.AddSystemFonts();
    }

    [HttpGet("generate")]
    public IActionResult Generate()
    {
        string captchaCode = GenerateCaptchaCode();
        return Ok(new Captcha
        {
            Code = captchaCode,
            Image = GenerateCaptchaImage(captchaCode)
        });
    }

    private string GenerateCaptchaImage(string captchaCode)
    {
        var fontFamily = _fonts.Families.FirstOrDefault();

        var font = fontFamily.CreateFont(18);

        using (var image = new Image<Rgba32>(120, 30))
        {
            image.Mutate(x => x
                .BackgroundColor(Color.White)
                .DrawText(captchaCode, font, Color.Black, new PointF(5, 5))
            );

            for (int i = 0; i < 100; i++)
            {
                int x = _random.Next(image.Width);
                int y = _random.Next(image.Height);
                image[x, y] = new Rgba32((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256));
            }

            using (var ms = new MemoryStream())
            {
                image.SaveAsPng(ms);
                ms.Position = 0;
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string GenerateCaptchaCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(_ => chars[_random.Next(chars.Length)]).ToArray());
    }
}