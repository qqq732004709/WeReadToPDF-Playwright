global using Microsoft.Playwright;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Playwright_WeReadToPDF;
public class Helper
{
    private readonly IPage page;

    public Helper(IPage page)
    {
        this.page = page;
    }

    public async Task Login()
    {
        await page.GotoAsync("https://weread.qq.com/");

        try
        {
            var loginBtn = await page.WaitForSelectorAsync("button.navBar_link_Login", new() { State = WaitForSelectorState.Visible });
            await loginBtn?.ClickAsync();

            var waitTurns = 15;
            for (int i = 0; i < waitTurns; i++)
            {
                Console.WriteLine($"Wait for QRCode Scan...{i}/{waitTurns}turns");
                await page.WaitForTimeoutAsync(3000);
                var content = await page.QuerySelectorAsync(".menu_container");
                if (content != null)
                {
                    Console.WriteLine("Login Succeed.");
                    break;
                }

            }
        }
        catch (Exception)
        {
            throw new Exception("Login Time Out!");
        }
    }

    public async Task SaveAsPdf(string bookUrl, string saveAt = ".")
    {
        if (bookUrl.IndexOf("weread.qq.com/web/reader", StringComparison.Ordinal) < 0)
        {
            throw new Exception("Wrong Url");
        }

        await page.GotoAsync(bookUrl);

        //打开日间模式
        await TurnOnLight();

        await page.ClickAsync("button.catalog");
        await page.ClickAsync("li.chapterItem:nth-child(2)");

        var bookName = await page.TextContentAsync("span.readerTopBar_title_link");
        Console.WriteLine($"ready to scan {bookName}");

        var pageIndex = 1;
        var pngNameList = new List<string>();
        while (true)
        {
            Thread.Sleep(500);

            var chapter = await page.TextContentAsync("span.readerTopBar_title_chapter");
            Console.WriteLine($"scanning chapter{chapter}");

            await CheckAllImageLoaded();

            var pngName = $"{bookName.Trim()}/{chapter.Trim()}_{pageIndex}";
            await ScreenShotFullContent(pngName);
            pngNameList.Add(pngName);
            Console.WriteLine($"save chapter scan {pngName}");

            try
            {
                var readerFooter = await page.WaitForSelectorAsync(".readerFooter_button,.readerFooter_ending");
                var readerFooterClass = await readerFooter.GetAttributeAsync("class");
                if (readerFooterClass.Contains("ending"))
                {
                    break;
                }

                var nextBtnText = (await readerFooter.TextContentAsync()).Trim();

                if (nextBtnText == "下一页")
                {
                    Console.WriteLine("go to next page");
                    pageIndex++;
                }
                else if (nextBtnText == "下一章")
                {
                    Console.WriteLine("go to next chapter");
                    pageIndex = 1;
                }
                else
                {
                    throw new Exception("Unexpected Exception");
                }

                await readerFooter.ClickAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        Console.WriteLine("pdf converting");
        ConvertImgToPdf($"{saveAt}/{bookName}", pngNameList);
    }

    public async Task TurnOnLight()
    {
        await page.WaitForTimeoutAsync(300);
        await page.ClickAsync("button.readerControls_item.white");
        await page.WaitForTimeoutAsync(300);
    }

    public async Task<bool> CheckAllImageLoaded()
    {
        var unCheckedImg = await page.QuerySelectorAllAsync("img.wr_absolute");
        var unCheckImgList = unCheckedImg.ToList();
        if (unCheckedImg.Count == 0)
        {
            Console.WriteLine("all img loaded");
            return true;
        }

        for (int i = 0; i < 100; i++)
        {
            await page.WaitForTimeoutAsync(50);
            foreach (var img in unCheckImgList)
            {
                var prop = img.GetPropertyAsync("complete");
                unCheckImgList.Remove(img);
            }
            if (unCheckImgList.Count == 0)
            {
                Console.WriteLine("all img loaded");
                return true;
            }
        }

        Console.WriteLine("all img not loaded");
        return false;
    }

    public async Task ScreenShotFullContent(string pngName)
    {
        var renderTargetContainer = page.Locator(".renderTargetContainer");
        int height = Convert.ToInt32(await renderTargetContainer.GetAttributeAsync("offsetHeight"));
        height += Convert.ToInt32(await renderTargetContainer.GetAttributeAsync("offsetTop"));
        var width = await page.EvaluateAsync<int>("window.outerWidth");
        await page.SetViewportSizeAsync(width, height);
        await page.WaitForTimeoutAsync(1000);

        var content = await page.WaitForSelectorAsync(".app_content", new() { Timeout = 1000 * 3 });
        Directory.CreateDirectory(Path.GetDirectoryName($"{pngName}.png"));
        await content.ScreenshotAsync(new() {  Path = $"{pngName}.png" });
    }

    public void ConvertImgToPdf(string fileName, List<string> imgList)
    {
        Document document = new Document();
        PdfWriter.GetInstance(document, new FileStream($"{fileName}.pdf", FileMode.Create));
        document.Open();

        foreach (var img in imgList)
        {
            var image = Image.GetInstance(img);
            document.Add(image);
        }

        document.Close();
    }
}
