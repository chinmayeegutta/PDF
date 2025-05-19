using System.Threading.Tasks;
using PuppeteerSharp;

namespace PdfGenerator
{
    public static class PdfHelper
    {
        public static async Task GeneratePdfFromHtml(string htmlContent, string outputPath)
        {
            // Launch headless browser using manually downloaded Chromium
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = @"C:\Users\cgutt\Downloads\chrome-headless-shell-win64\chrome-headless-shell-win64"
            });

            using var page = await browser.NewPageAsync();

            // Set the page content to the HTML we have
            await page.SetContentAsync(htmlContent);

            // Generate PDF file at output path
            await page.PdfAsync(outputPath);

            await browser.CloseAsync();
        }
    }
}
