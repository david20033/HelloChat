namespace HelloChat.Services.IServices
{
    public interface IUserStatusService
    {
        Task SetUserActive(string userId);
        Task SetUserExitActive(string userId);
        Task<string> GetUserActiveString(string userId);
    }
}
