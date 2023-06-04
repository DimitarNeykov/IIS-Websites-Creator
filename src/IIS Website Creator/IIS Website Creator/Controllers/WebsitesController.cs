using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using System.Xml.Linq;

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

        [HttpPost("GitServer")]
        // Defines a method named "CreateWebsiteWithScriptMap" that takes a string parameter "siteName"
        public void CreateWebsiteWithScriptMap(string siteName)
        {
            // Creates a new instance of the ServerManager class and assigns it to the variable "serverManager"
            using (ServerManager serverManager = new())
            {
                // Calls the "GetNextAvailablePort" method and assigns the result to the variable "nextAvailablePort"
                int nextAvailablePort = GetNextAvailablePort();
                // Creates a directory with the specified path
                Directory.CreateDirectory($"C:\\inetpub\\sites\\{siteName}");
                // Adds a new site to the server with the specified parameters and assigns it to the variable "newSite"
                Site newSite = serverManager.Sites.Add(siteName, $"C:\\inetpub\\sites\\{siteName}", nextAvailablePort);
                // Saves the changes made to the server manager
                serverManager.CommitChanges();
                // Gets the web configuration for the new site and assigns it to the variable "config"
                Configuration config = serverManager.GetWebConfiguration(newSite.Name);
                // Gets the "handlers" section from the configuration and assigns it to the variable "handlersSection"
                Microsoft.Web.Administration.ConfigurationSection handlersSection = config.GetSection("system.webServer/handlers");
                // Gets the collection of elements in the "handlers" section and assigns it to the variable "handlersCollection"
                ConfigurationElementCollection handlersCollection = handlersSection.GetCollection();

                // Creates a new element with the name "add" in the "handlers" collection and assigns it to the variable "scriptMapElement"
                ConfigurationElement scriptMapElement = handlersCollection.CreateElement("add");
                // Sets the attributes and values for the "scriptMapElement"
                scriptMapElement["name"] = "Git Smart HTTP";
                scriptMapElement["path"] = "*";
                scriptMapElement["verb"] = "*";
                scriptMapElement["modules"] = "CgiModule";
                scriptMapElement["scriptProcessor"] = "C:\\Program Files\\Git\\mingw64\\libexec\\git-core\\git-http-backend.exe";
                scriptMapElement["resourceType"] = "Unspecified";
                scriptMapElement["requireAccess"] = "None";
                scriptMapElement["preCondition"] = "bitness64";

                // Adds the "scriptMapElement" to the "handlersCollection"
                handlersCollection.Add(scriptMapElement);
                // Saves the changes made to the server manager
                serverManager.CommitChanges();

                // Specifies the path to the applicationHost.config file
                string configFilePath = @"C:\Windows\System32\inetsrv\config\applicationHost.config";

                // Loads the XML document from the specified config file
                XDocument doc = XDocument.Load(configFilePath);

                // Creates an XML element structure representing the desired configuration for a specific location
                XElement locationElement = new XElement("location",
                    new XAttribute("path", siteName),
                    new XElement("system.webServer",
                        new XElement("security",
                            new XElement("authentication",
                                new XElement("anonymousAuthentication",
                                    new XAttribute("enabled", "false")
                                ),
                                new XElement("basicAuthentication",
                                    new XAttribute("enabled", "true")
                                )
                            )
                        )
                    )
                );

                // Gets the root element of the XML document
                XElement rootElement = doc.Root;

                // Adds the "locationElement" to the root element of the XML document
                rootElement.Add(locationElement);
                // Saves the changes made to the XML document
                doc.Save(configFilePath);

                // Specifies the name of the application pool
                string poolName = "git";
                // Specifies the project root path
                string projectRoot = $@"C:\inetpub\sites\{siteName}";

                // Gets the "applicationPools" element from the XML document
                XElement appPoolsElement = doc.Descendants("applicationPools").FirstOrDefault();

                // Gets the specific "add" element within the "applicationPools" element based on the pool name
                XElement appPoolElement = appPoolsElement.Elements("add")
                    .FirstOrDefault(e => e.Attribute("name")?.Value == poolName);

                // Checks if the app pool element exists
                if (appPoolElement != null)
                {
                    // Gets the "environmentVariables" element within the app pool element
                    XElement environmentVariablesElement = appPoolElement.Element("environmentVariables");

                    // Checks if the environment variables element exists
                    if (environmentVariablesElement != null)
                    {
                        // Creates a new environment variable element and adds it to the environment variables element
                        XElement newEnvironmentVariableElement = new("add",
                            new XAttribute("name", "GIT_PROJECT_ROOT"),
                            new XAttribute("value", projectRoot)
                        );

                        environmentVariablesElement.Add(newEnvironmentVariableElement);
                        // Saves the changes made to the XML document
                        doc.Save(configFilePath);
                    }
                    else
                    {
                        // Handle case when the environment variables element is not found
                    }
                }
                else
                {
                    // Handle case when the app pool element is not found
                }
            }
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
