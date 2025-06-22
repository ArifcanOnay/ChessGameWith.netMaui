namespace SatrancApi.Services
{
    public interface ICurrentUserService
    {
        string? GetCurrentUserEmail();
        Guid? GetCurrentUserId();
        string? GetCurrentUserName(); // 
        bool IsAuthenticated();
        void SetCurrentUser(string email, Guid userId, string? userName = null); 
        void ClearCurrentUser();
    }
}