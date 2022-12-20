global using Microsoft.Playwright;

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

    public async Task SaveAsPdf(string bookUrl)
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

        var bookName = page.TextContentAsync("span.readerTopBar_title_link");
        Console.WriteLine($"ready to scan {bookName.Result}");

        var pageIndex = 1;
        while (true)
        {
            Thread.Sleep(500);

            var chapter = await page.TextContentAsync("span.readerTopBar_title_chapter");
            Console.WriteLine($"scanning chapter{chapter}");

            await CheckAllImageLoaded();

            await page.Locator(".renderTargetContainer").ScreenshotAsync(new() { Path = "/screenShots/01.png",  });
            await page.PdfAsync(new() { Path = "page.pdf" });
            pageIndex++;
        }

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

    public async Task ScreenShotFullContent()
    {

    }

}
