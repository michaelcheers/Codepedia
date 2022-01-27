using Codepedia;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiTest
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            Process process = Process.Start(new ProcessStartInfo("dotnet", "help")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false
            });

            process.ErrorDataReceived += (sendingProcess, dataLine) => Console.WriteLine(dataLine.Data);
            process.OutputDataReceived += (sendingProcess, dataLine) => Console.WriteLine(dataLine.Data);

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();

            while (Console.ReadKey(false).Key != ConsoleKey.Enter) ;
            //RestClient client = new RestClient("https://localhost:5001");
            //Console.WriteLine((await CodepediaApi.Search("MergePDFs", default))[0].Markdown);
        }
    }
}
