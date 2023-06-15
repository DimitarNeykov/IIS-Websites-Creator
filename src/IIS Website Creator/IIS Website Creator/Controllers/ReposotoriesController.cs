using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IIS_Website_Creator.Controllers
{
    public class ReposotoriesController : BaseController
    {
        [HttpPost]
        public IActionResult Create(string repositoryName, string gitServer)
        {
            // Create a new process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c git init --bare {repositoryName}.git",
                WorkingDirectory = $"C:\\inetpub\\sites\\{gitServer}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Create a new process
            Process process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            // Event handler for capturing the output
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine(e.Data);
                }
            };

            // Event handler for capturing the error output
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine(e.Data);
                }
            };

            // Start the process
            process.Start();

            // Begin reading the output and error asynchronously
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for the process to exit
            process.WaitForExit();

            // Close the process
            process.Close();

            return this.StatusCode(201);
        }
    }
}
