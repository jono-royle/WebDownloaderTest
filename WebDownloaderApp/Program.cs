using System;
using Microsoft.Extensions.Configuration;


class Program
{
    static async Task Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var urls = config.GetSection("Urls").Get<string[]>();
        var outputFolder = config["OutputFolder"];
        var retryMaximumString = config["URLRetryAttempts"];
        int retryMaximum = 0;

        if (urls == null || urls.Length < 1)
        {
            Console.WriteLine("No URLs found in appsettings file");
            return;
        }
        if (string.IsNullOrEmpty(outputFolder)) 
        {
            Console.WriteLine("No output folder found in appsettings file");
            return;
        }
        if(!string.IsNullOrEmpty(retryMaximumString))
        {
            int.TryParse(retryMaximumString, out retryMaximum);
            if (retryMaximum < 0)
            {
                retryMaximum = 0;
            }
        }
        Directory.CreateDirectory(outputFolder);

        using (HttpClient client = new HttpClient())
        {
            var webpageDownloader = new WebpageDownloader(client, outputFolder, retryMaximum);
            List<Task> tasks = [];
            int i = 0;
            foreach (var url in urls)
            {
                tasks.Add(webpageDownloader.DownloadWebpage(url, i));
                i++;
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("All downloads completed");
        }
    }
}