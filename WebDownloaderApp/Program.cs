using System;

class Program
{
    static async Task Main(string[] args)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync(args[0]);
            if (response.IsSuccessStatusCode)
            {
                byte[] data = await response.Content.ReadAsByteArrayAsync();

                FileStream fileStream = File.Create(args[1]);
                await fileStream.WriteAsync(data, 0, data.Length);
                fileStream.Close();
                Console.WriteLine("Downloaded webpage");
            }
            else
            {
                Console.WriteLine($"Response code {response.StatusCode}");
            }
        }

    }
}