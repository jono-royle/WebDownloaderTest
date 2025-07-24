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
        Directory.CreateDirectory(outputFolder);

        using (HttpClient client = new HttpClient())
        {
            List<Task> tasks = [];
            int i = 0;
            foreach (var url in urls)
            {
                tasks.Add(DownloadWebpage(client, url, outputFolder, i));
                i++;
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("All downloads completed");
        }
    }

    private static async Task DownloadWebpage(HttpClient client, string url, string outputFolder, int urlNumber)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    string fileName = uri.Host + $"_{urlNumber}";
                    string filePath = Path.Combine(outputFolder, fileName);

                    FileStream fileStream = File.Create(filePath);
                    await fileStream.WriteAsync(data, 0, data.Length);
                    fileStream.Close();
                    Console.WriteLine($"Downloaded webpage {urlNumber}, url: {url} to {fileName}");
                }
                else
                {
                    Console.WriteLine($"Response code for webpage {urlNumber}, url: {url} - {response.StatusCode}");
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error: for {url} exception {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Error: {url} is not a valid URL");
        }


    }
}