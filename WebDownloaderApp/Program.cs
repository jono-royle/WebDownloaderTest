using System;
using System.Collections.Concurrent;
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
        var maxConcurrentDownloadsString = config["MaxConcurrentDownloads"];
        int retryMaximum = 0;
        int maxConcurrent = 1;

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
        if (!string.IsNullOrEmpty(maxConcurrentDownloadsString))
        {
            int.TryParse(maxConcurrentDownloadsString, out maxConcurrent);
            if (maxConcurrent < 1)
            {
                maxConcurrent = 1;
            }
        }
        Directory.CreateDirectory(outputFolder);

        using (HttpClient client = new HttpClient())
        {
            var webpageDownloader = new WebpageDownloader(client, outputFolder, retryMaximum);
            ConcurrentQueue<(string url, int index)> queue = new ConcurrentQueue<(string, int)>();
            List<Task> activeTasks = new List<Task>();
            object lockObj = new object();
            int initialDownloadListLength = Math.Min(urls.Length, maxConcurrent);
            for (int i = 0; i < urls.Length; i++) 
            {
                if (i < initialDownloadListLength) 
                {
                    var task = StartDownload(webpageDownloader, urls[i], i, queue, activeTasks, lockObj);
                    lock (lockObj) activeTasks.Add(task);
                }
                else
                {
                    queue.Enqueue((urls[i], i));
                }
            }

            await Task.WhenAll(activeTasks);

            Console.WriteLine("All downloads completed");
        }

        static async Task StartDownload(WebpageDownloader downloader, string url, int index, ConcurrentQueue<(string url, int index)> queue, List<Task> activeTasks, object lockObj)
        {
            await downloader.DownloadWebpage(url, index);
            if (queue.TryDequeue(out var next))
            {
                Console.WriteLine($"Dequeued next url download: {next.url}");
                var nextTask = StartDownload(downloader, next.url, next.index, queue, activeTasks, lockObj);
                lock (lockObj) activeTasks.Add(nextTask);
            }
            lock (lockObj)
            {
                activeTasks.RemoveAll(t => t.IsCompleted);
            }
        }
    }
}