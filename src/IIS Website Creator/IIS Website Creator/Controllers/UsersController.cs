using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.AccountManagement;

namespace IIS_Website_Creator.Controllers
{
    public class UsersController : BaseController
    {
        [HttpPost]
        public IActionResult Create(string username, string password)
        {
            try
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                {
                    UserPrincipal user = new UserPrincipal(context);
                    user.SamAccountName = username;
                    user.SetPassword(password);
                    user.Enabled = true;
                    user.Save();
                }
            }
            catch (Exception ex)
            {
            }

            return this.StatusCode(201);
        }
    }
}
