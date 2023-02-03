namespace CmsContentBuilder.Optimizely.Services;

public interface IUsersService
{
    Task CreateUser(string username, string email, string password, IEnumerable<string> roles);
}