# WebDownloaderTest

This is a console application that asynchronously downloads webpages. The URLs to download and the output folder to save the results to are specified in the appsettings.json file in the project directory.
Also specified in the appsettings are the URLRetryAttempts - how many times the program will reattempt to download a URL if it does not return a successful status code and MaxConcurrentDownloads, the maximum number of webpages that can be downloaded at once. If the list of URLs is higher than the maximum, the rest will be queued for download and started whenever a download finishes. This will allow the program to scale to large numbers of URLs.

The program will output the results of each download to the console, and then output "All downloads completed" when it is finished.

There is also an attached unit test project which mocks the http client in order to test the functionality of the Webpage downloader.

Possible future extensions of the project:
- The input mechanism is currently a simple json file, but this could be replaced with a connection to a web API or a UI for user submission
- The retry mechanism currently just retries all non successful HTTP responses - this could be expanded with different responses depending on the reason for failure, or time delays if waiting for an internet reconnection, depending on the needs of the user



