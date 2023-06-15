using Microsoft.AspNetCore.Mvc;

namespace IIS_Website_Creator.Controllers
{
    public class GroupsController : BaseController
    {
        [HttpPost]
        public IActionResult Create(string path, string groupName)
        {
            string directoryPath = $@"{path}\{groupName}";

            Directory.CreateDirectory(directoryPath);

            return this.StatusCode(201);
        }
    }
}
