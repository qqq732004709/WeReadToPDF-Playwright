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
            await loginBtn.ClickAsync();

            var waitTurns = 15;
            for (int i = 0; i < waitTurns; i++)
            {
                Console.WriteLine($"Wait for QRCode Scan...{i}/{waitTurns}turns");
                var content = await page.QuerySelectorAsync(".menu_container");
                if (content != null)
                {
                    Console.WriteLine("Login Succeed.");
                    break;
                }

                await page.WaitForTimeoutAsync(1000);
            }
        }
        catch (Exception)
        {
            throw new Exception("Login Time Out!");
        }
    }
}
