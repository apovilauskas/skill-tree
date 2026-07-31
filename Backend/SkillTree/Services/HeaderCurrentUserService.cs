namespace skill_tree.Services;

public class HeaderCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public HeaderCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string? GetUserId()
    {
        string? id = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].ToString();
        return string.IsNullOrEmpty(id) ? null : id;
    }
}