using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace MusicRecordsUITest;

[TestClass]
public sealed class MusicRecordsUiTests
{
    private IWebDriver? _driver;

    [TestMethod]
    public void GetAllRecords_DisplaysAllSixRecords()
    {
        var options = new EdgeOptions();
        options.AddArgument("--headless=new");

        var driver = new EdgeDriver(options);
        _driver = driver;

        var indexPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "RestMusicFrontEnd",
            "index.html"));

        driver.Navigate().GoToUrl(new Uri(indexPath).AbsoluteUri);
        driver.FindElement(By.Id("button1")).Click();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        wait.Until(d => d.FindElements(By.CssSelector("#app ul li.list-group-item")).Count > 0);

        var records = driver.FindElements(By.CssSelector("#app ul li.list-group-item"));
        Assert.HasCount(6, records);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}
