public class WebpageDownloader
{

    private HttpClient _httpClient;
    private string _outputFolder;
    private int _retryMaximum;


    public WebpageDownloader(HttpClient client, string outputFolder, int retryMaximum)
    {
        _httpClient = client;
        _outputFolder = outputFolder;
        _retryMaximum = retryMaximum;
    }

    public async Task DownloadWebpage(string url,  int urlNumber)
    {
        int retryAttempts = 0;
        bool succeededConnection = false;
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            try
            {
                while (!succeededConnection && retryAttempts <= _retryMaximum)
                {
                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        succeededConnection = true;
                        await CreateOutputFileFromPage(url, urlNumber, uri, response);
                    }
                    else
                    {
                        Console.WriteLine($"Response code for webpage {urlNumber}, url: {url} - {response.StatusCode}");
                        retryAttempts++;
                        if (retryAttempts <= _retryMaximum)
                        {
                            Console.WriteLine($"Retrying connection for webpage {urlNumber}");
                        }
                    }
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

    private async Task CreateOutputFileFromPage(string url, int urlNumber, Uri uri, HttpResponseMessage response)
    {
        byte[] data = await response.Content.ReadAsByteArrayAsync();
        string fileName = uri.Host + $"_{urlNumber}";
        string filePath = Path.Combine(_outputFolder, fileName);

        FileStream fileStream = File.Create(filePath);
        await fileStream.WriteAsync(data, 0, data.Length);
        fileStream.Close();
        Console.WriteLine($"Downloaded webpage {urlNumber}, url: {url} to {fileName}");
    }
}