using Playwright_WeReadToPDF;

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new() { Channel = "msedge", Headless = false });
var page = await browser.NewPageAsync();

var helper = new Helper(page);
await helper.Login();
await helper.SaveAsPDF("https://weread.qq.com/web/reader/a3d3227071db5702a3d9c37");
await browser.CloseAsync();