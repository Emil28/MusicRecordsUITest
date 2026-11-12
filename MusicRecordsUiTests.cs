using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace MusicRecordsUITest;

[TestClass]
public sealed class MusicRecordsUiTests
{
    [TestMethod]
    public void GetAllRecords_DisplaysAllSixRecords()
    {
        // Arrange
        var options = new EdgeOptions();
        options.AddArgument("--headless=new");
        var driver = new EdgeDriver(options);

        var indexPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "RestMusicFrontEnd",
            "index.html"));

        try
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");

            // Act
            driver.FindElement(By.Id("button1")).Click();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            wait.Until(d => d.FindElements(By.CssSelector("#app ul li.list-group-item")).Count > 0);

            // Assert
            var records = driver.FindElements(By.CssSelector("#app ul li.list-group-item"));
            Assert.HasCount(6, records);
        }
        finally
        {
            driver.Quit();
        }
    }
}
