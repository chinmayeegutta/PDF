using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Read JSON data
            string json = File.ReadAllText("invoice.json");
            var jsonDoc = JsonDocument.Parse(json);
            var rootElement = jsonDoc.RootElement;
            
            var dataDict = JsonExtensions.JsonElementToObject(rootElement) as Dictionary<string, object>;

if (dataDict != null && dataDict.ContainsKey("items") && dataDict["items"] is List<object> items)
{
    for (int i = 0; i < items.Count; i++)
    {
        if (items[i] is Dictionary<string, object> itemDict)
        {
            itemDict["index"] = i + 1; // 1-based index
        }
    }
}

            // Convert logo image to base64 dynamically
            string logoImagePath = "logo.jpg"; // Make sure this file exists in your project folder
            string logoBase64 = "";
            if (File.Exists(logoImagePath))
            {
                logoBase64 = ConvertImageToBase64(logoImagePath);
                dataDict["logoImageHtml"] = $"<img src=\"data:image/jpg;base64,{logoBase64}\" alt=\"Logo\" style=\"max-height: 80px;\" />";
            }
            else
            {
                Console.WriteLine("⚠️ Logo image not found, skipping...");
                dataDict["logoImageHtml"] = "";
            }

            // Read HTML template with placeholders
            string templateHtml = File.ReadAllText("template.html");

            // Replace placeholders using custom template engine
            var engine = new SimpleTemplateEngine();
            string renderedHtml = engine.Parse(templateHtml, dataDict);

            File.WriteAllText("invoice-rendered.html", renderedHtml); // for debugging

            // Ensure Chromium is available
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            // Generate PDF
            await GeneratePdf(renderedHtml, "invoice.pdf");

            Console.WriteLine("✅ PDF invoice generated successfully: invoice.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    // Converts image file to base64 string
    public static string ConvertImageToBase64(string imagePath)
    {
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        return Convert.ToBase64String(imageBytes);
    }

    // Generate PDF from HTML using PuppeteerSharp
    static async Task GeneratePdf(string htmlContent, string outputPdfPath)
    {
        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });

        using var page = await browser.NewPageAsync();

        await page.SetContentAsync(htmlContent, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        await page.PdfAsync(outputPdfPath, new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "40px",
                Bottom = "120px",
                Left = "40px",
                Right = "40px"
            },
            DisplayHeaderFooter = true,
            HeaderTemplate = "<div></div>",
            FooterTemplate = @"<div style='font-size:10px; width:100%; text-align:center; color:#999; padding-bottom:5px;'>
                                Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                               </div>"
        });

        await browser.CloseAsync();
    }
}
