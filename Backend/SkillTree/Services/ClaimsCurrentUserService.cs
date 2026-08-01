using System.Security.Claims;

namespace skill_tree.Services;

public class ClaimsCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public ClaimsCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string? GetUserId()
    {
        string? id = _httpContextAccessor.HttpContext?.User.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            
        return string.IsNullOrEmpty(id) ? null : id;
    }
}