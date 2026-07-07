using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FineArts.Web.Pages;

public class LoginModel : PageModel
{
    private readonly IConfiguration _config;

    public LoginModel(IConfiguration config)
    {
        _config = config;
    }

    public string ErrorMessage { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Auth")))
            return RedirectToPage("/Evaluation");
        return Page();
    }

    public IActionResult OnPost(string password)
    {
        var correct = _config["AppSettings:Password"] ?? "FineArts2025";
        if (password == correct)
        {
            HttpContext.Session.SetString("Auth", "true");
            return RedirectToPage("/Evaluation");
        }
        ErrorMessage = "Incorrect password. Please try again.";
        return Page();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Login");
    }
}
