using Moq.Protected;
using Moq;
using System.Net;
using System;

namespace WebDownloaderUnitTests
{

    [TestClass]
    public sealed class WebDownloaderTests
    {
        private WebpageDownloader _webPageDownloader;
        private string _outputFolder;
        private Mock<HttpMessageHandler> _handler;
        private readonly string _workingUrl = "https://example.com";
        private readonly string _workingUrlName = "example.com";
        private readonly string _notFoundUrl = "https://notfound.com";
        private readonly int _retryAttempts = 2;

        [TestInitialize]
        public void Setup()
        {
            _outputFolder = Path.GetTempPath();
            var expectedContent = "<html>Hello World</html>";

            _handler = new Mock<HttpMessageHandler>();

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri != null && Uri.Compare(req.RequestUri, new Uri(_workingUrl), UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedContent)
                })
                .Verifiable();

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri != null && Uri.Compare(req.RequestUri, new Uri(_notFoundUrl), UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                })
                .Verifiable();

            var httpClient = new HttpClient(_handler.Object);
            _webPageDownloader = new WebpageDownloader(httpClient, _outputFolder, _retryAttempts);
        }

        [TestMethod]
        public async Task TestSuccessfulDownload_SendsAsyncAndCreatesFile()
        {
            var urlNumber = 1;
            string fileName = _workingUrlName + $"_{urlNumber}";
            string filePath = Path.Combine(_outputFolder, fileName);
            try
            {
                await _webPageDownloader.DownloadWebpage(_workingUrl, urlNumber);
                _handler.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
                Assert.IsTrue(File.Exists(filePath));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [TestMethod]
        public async Task TestIncorrectUriFormat_DoesNotAttemptDownload()
        {
            await _webPageDownloader.DownloadWebpage("nonsense", 1);
            _handler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task TestNotFoundUrl_AttemptsUpToRetryAttemptsTimes()
        {
            await _webPageDownloader.DownloadWebpage(_notFoundUrl, 1);
            _handler.Protected().Verify(
                "SendAsync",
                Times.Exactly(_retryAttempts + 1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }


    }
}
