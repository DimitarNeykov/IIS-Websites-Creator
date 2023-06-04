using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;

namespace IIS_Website_Creator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebsitesController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateWebsiteOnNextAvailablePort(string siteName, string physicalPath)
        {
            using (ServerManager serverManager = new())
            {
                int nextAvailablePort = GetNextAvailablePort();

                Site site = serverManager.Sites.Add(siteName, physicalPath, nextAvailablePort);

                serverManager.CommitChanges();
                Directory.CreateDirectory(physicalPath);
            }

            return this.StatusCode(201);
        }

        private int GetNextAvailablePort()
        {
            using (ServerManager serverManager = new ServerManager())
            {
                int startingPort = 9000;
                int maxPort = 65535;

                for (int port = startingPort; port <= maxPort; port++)
                {
                    Site site = serverManager.Sites.FirstOrDefault(s => s.Bindings.Any(b => b.EndPoint.Port == port));

                    if (site == null)
                    {
                        return port;
                    }
                }
            }

            throw new Exception("There is no free port available to create a site.");
        }
    }
}
