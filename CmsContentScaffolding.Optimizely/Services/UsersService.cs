using EPiServer.Shell.Security;

namespace CmsContentScaffolding.Optimizely.Services;

public class UsersService : IUsersService
{
    private readonly UIUserProvider _uIUserProvider;
    private readonly UIRoleProvider _uIRoleProvider;

    public UsersService(UIUserProvider uIUserProvider, UIRoleProvider uIRoleProvider)
    {
        _uIUserProvider = uIUserProvider;
        _uIRoleProvider = uIRoleProvider;
    }

    public async Task CreateUser(string username, string email, string password, IEnumerable<string> roles)
    {
        var usersCount = await _uIUserProvider.GetAllUsersAsync(0, 1).CountAsync();

        if (usersCount > 0)
            return;

        var result = await _uIUserProvider.CreateUserAsync(username, password, email, null, null, true);
        if (result.Status == UIUserCreateStatus.Success)
        {
            foreach (var role in roles)
            {
                var exists = await _uIRoleProvider.RoleExistsAsync(role);
                if (!exists)
                {
                    await _uIRoleProvider.CreateRoleAsync(role);
                }
            }

            var res = await _uIRoleProvider.AddUserToRolesAsync(result.User.Username, roles);

            if (!res.Succeeded)
                throw new Exception("Error occured while creating user!");
        }
    }
}
