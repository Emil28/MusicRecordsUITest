using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace MusicRecordsUITest;

[TestClass]
[DoNotParallelize]
public sealed class MusicRecordsUiTests
{
    [TestMethod]
    public void GetAllRecords_DisplaysAllSixRecords()
    {
        // Arrange
        const int pauseMilliseconds = 1000;
        var options = new EdgeOptions();
       // options.AddArgument("--headless=new");
        var driver = new EdgeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        var recordSelector = By.CssSelector("#app ul li.list-group-item");

        try
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");
            Thread.Sleep(pauseMilliseconds);
            // Act
            driver.FindElement(By.Id("button1")).Click();
            wait.Until(d => d.FindElements(recordSelector).Count == 6);
            Thread.Sleep(pauseMilliseconds);



            var toggleButton = driver.FindElement(By.Id("button2"));
            Assert.AreEqual("Hide posts", toggleButton.Text.Trim());

            toggleButton.Click();
            Thread.Sleep(pauseMilliseconds);
            Assert.IsEmpty(driver.FindElements(recordSelector));
            Assert.AreEqual("Show posts", driver.FindElement(By.Id("button2")).Text.Trim());

            driver.FindElement(By.Id("button2")).Click();
            wait.Until(d => d.FindElements(recordSelector).Count == 6);
            Thread.Sleep(pauseMilliseconds);
            Assert.AreEqual("Hide posts", driver.FindElement(By.Id("button2")).Text.Trim());

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
        const int pauseMilliseconds = 1000;
        var options = new EdgeOptions();
        var driver = new EdgeDriver(options);
        // options.AddArgument("--headless=new");
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var title = $"UI Test {Guid.NewGuid():N}";
        var updatedTitle = $"{title} Updated";

        try
        {
            driver.Navigate().GoToUrl("http://127.0.0.1:5500/index.html");
            Thread.Sleep(pauseMilliseconds);

            // Act

            driver.FindElement(By.Id("recordTitle")).SendKeys(title);
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("recordArtist")).SendKeys("UI Test Artist");
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("recordDuration")).SendKeys("210");
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("recordPublicationYear")).SendKeys("2024");
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("addRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='status']"))
           .Any(e => e.Text == "Record added."));
            Thread.Sleep(pauseMilliseconds);

            var row = wait.Until(d => d.FindElements(
               By.CssSelector("#app ul li.list-group-item"))
           .FirstOrDefault(e => e.Text.Contains($"title: {title}")));

            if (row is null)
                Assert.Fail("The added record was not displayed.");

            var recordId = row.Text.Split(" - ")[0].Replace("id: ", "").Trim();

            driver.FindElement(By.Id("recordId")).SendKeys(recordId);
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("recordTitle")).SendKeys(updatedTitle);
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("recordArtist")).SendKeys("Updated UI Test Artist");
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("recordDuration")).SendKeys("240");
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("recordPublicationYear")).SendKeys("2025");
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("updateRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='status']"))
                .Any(e => e.Text == "Record updated."));
            wait.Until(d => d.FindElements(
                    By.CssSelector("#app ul li.list-group-item"))
                .Any(e => e.Text.Contains($"title: {updatedTitle}")));
            Thread.Sleep(pauseMilliseconds);

            driver.FindElement(By.Id("recordId")).SendKeys(recordId);
            Thread.Sleep(pauseMilliseconds);
            driver.FindElement(By.Id("deleteRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='status']"))
                .Any(e => e.Text == "Record deleted."));
            wait.Until(d => d.FindElements(
                    By.XPath($"//li[starts-with(normalize-space(.), 'id: {recordId} -')]"))
                .Count == 0);
            Thread.Sleep(pauseMilliseconds);
        }
        finally
        {
            driver.Quit();
        }
    }


}
    

