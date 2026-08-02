namespace skill_tree.DTOs;

public class RegisterDto
{
    private string _username = string.Empty;

    public string Username
    {
        get => _username;
        set => _username = value?.Trim() ?? string.Empty;
    }

    public string Password { get; set; } = string.Empty;
}