using Playwright_WeReadToPDF;

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new() { Channel = "msedge", Headless = true });
var page = await browser.NewPageAsync();

var helper = new Helper(page);
await helper.Login();
await helper.SaveAsPdf("https://weread.qq.com/web/reader/3da32b505dd9f43da9a1aca");
await browser.CloseAsync();