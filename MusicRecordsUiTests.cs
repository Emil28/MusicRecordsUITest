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


    [TestMethod]
    public void AddUpdateDeleteRecord_ThroughTheUI()
    {
        // Arrange
        var options = new EdgeOptions();
        options.AddArgument("--headless=new");
        var driver = new EdgeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var title = $"UI Test {Guid.NewGuid():N}";
        var updatedTitle = $"{title} Updated";

        try
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");

            // Act

            driver.FindElement(By.Id("recordTitle")).SendKeys(title);
            driver.FindElement(By.Id("recordArtist")).SendKeys("UI Test Artist");
            driver.FindElement(By.Id("recordDuration")).SendKeys("210");
            driver.FindElement(By.Id("recordPublicationYear")).SendKeys("2024");
            driver.FindElement(By.Id("addRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='status']"))
           .Any(e => e.Text == "Record added."));

            var row = wait.Until(d => d.FindElements(
               By.CssSelector("#app ul li.list-group-item"))
           .FirstOrDefault(e => e.Text.Contains($"title: {title}")));

            if (row is null)
                Assert.Fail("The added record was not displayed.");

            var recordId = row.Text.Split(" - ")[0].Replace("id: ", "").Trim();

            driver.FindElement(By.Id("recordId")).SendKeys(recordId);
            driver.FindElement(By.Id("recordTitle")).SendKeys(updatedTitle);
            driver.FindElement(By.Id("recordArtist")).SendKeys("Updated UI Test Artist");
            driver.FindElement(By.Id("recordDuration")).SendKeys("240");
            driver.FindElement(By.Id("recordPublicationYear")).SendKeys("2025");
            driver.FindElement(By.Id("updateRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='status']"))
                .Any(e => e.Text == "Record updated."));
            wait.Until(d => d.FindElements(
                    By.CssSelector("#app ul li.list-group-item"))
                .Any(e => e.Text.Contains($"title: {updatedTitle}")));

            driver.FindElement(By.Id("recordId")).SendKeys(recordId);
            driver.FindElement(By.Id("deleteRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='status']"))
                .Any(e => e.Text == "Record deleted."));
            wait.Until(d => d.FindElements(
                    By.CssSelector("#app ul li.list-group-item"))
                .All(e => !e.Text.StartsWith($"id: {recordId} -")));
        }
        finally
        {
            driver.Quit();
        }
    }


}
    

