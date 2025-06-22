
using SatrancApi.Services;

namespace SatrancAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        //  STATIK DEĞERLER 
        private static string? _currentUserEmail;
        private static Guid? _currentUserId;
        private static string? _currentUserName;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUserEmail()
        {
            // Önce statik değeri kontrol et
            if (!string.IsNullOrEmpty(_currentUserEmail))
                return _currentUserEmail;

            // HTTP context'ten al (eğer varsa)
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        }

        public Guid? GetCurrentUserId()
        {
            return _currentUserId;
        }

        //  Kullanıcı adını döndür
        public string? GetCurrentUserName()
        {
            return _currentUserName ?? _currentUserEmail;
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(_currentUserEmail) && _currentUserId.HasValue;
        }

        // Kullanıcı adını da set et
        public void SetCurrentUser(string email, Guid userId, string? userName = null)
        {
            _currentUserEmail = email;
            _currentUserId = userId;
            _currentUserName = userName;

            Console.WriteLine($"✅ Current User Set: {userName} ({email}) - ID: {userId}");
        }

        public void ClearCurrentUser()
        {
            _currentUserEmail = null;
            _currentUserId = null;
            _currentUserName = null;

            Console.WriteLine("❌ Current User Cleared");
        }
    }
}