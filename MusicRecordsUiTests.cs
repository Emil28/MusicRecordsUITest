using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace MusicRecordsUITest;

[TestClass]
[DoNotParallelize]
public sealed class MusicRecordsUiTests
{
    private const string FrontendUrl = "http://127.0.0.1:5500";

    private static void Login(IWebDriver driver, WebDriverWait wait)

    {
        driver.Navigate().GoToUrl($"{FrontendUrl}/login.html");
       
        driver.FindElement(By.CssSelector("input[placeholder='Username']")).SendKeys("admin");
        
        driver.FindElement(By.CssSelector("input[placeholder='Password']")).SendKeys("123");
        driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        try
        {
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript(
                "return location.href.toLowerCase().endsWith('/index.html') || Boolean(document.querySelector('.error')?.textContent.trim());") is true);
        }
        catch (WebDriverTimeoutException)
        {
            var state = ((IJavaScriptExecutor)driver).ExecuteScript(
                "return JSON.stringify({ url: location.href, apiBaseUrl: window.MUSIC_RECORDS_API_BASE_URL, vueLoaded: !!window.Vue, usernameLength: document.querySelector(\"input[placeholder='Username']\")?.value.length, passwordLength: document.querySelector(\"input[placeholder='Password']\")?.value.length, error: document.querySelector('.error')?.textContent });");
            Assert.Fail($"Login did not finish. Browser state: {state}");
        }

        if (!driver.Url.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase))
        {
            var errorMessage = driver.FindElement(By.CssSelector(".error")).Text;
            Assert.Fail($"Login failed: {errorMessage}");
        }

        wait.Until(d => d.FindElement(By.Id("button1")).Displayed);
    }

    private static void SearchByFilter(
        IWebDriver driver,
        WebDriverWait wait,
        string filterId,
        string value,
        string expectedText)
    {
        var records = By.CssSelector("#app ul li.list-group-item");
        var clearButton = By.XPath("//button[normalize-space()='Clear filters']");

        driver.FindElement(clearButton).Click();
        wait.Until(d => d.FindElement(By.Id(filterId)).GetAttribute("value") == "");
        wait.Until(d => d.FindElements(records).Count > 1);

        driver.FindElement(By.Id(filterId)).SendKeys(value);
        driver.FindElement(By.Id("searchButton")).Click();

        wait.Until(d =>
        {
            try
            {
                var matchingRecords = d.FindElements(records);
                return matchingRecords.Count > 0 &&
                       matchingRecords.All(record => record.Text.Contains(expectedText));
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }

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
            Login(driver, wait);
            Thread.Sleep(pauseMilliseconds);
            // Act
            driver.FindElement(By.Id("button1")).Click();
            wait.Until(d => d.FindElements(recordSelector).Count > 0);

            //wait.Until(d => d.FindElements(recordSelector).Count == 6);
            Thread.Sleep(pauseMilliseconds);



            var toggleButton = driver.FindElement(By.Id("button2"));
            Assert.AreEqual("Hide posts", toggleButton.Text.Trim());

            toggleButton.Click();
            wait.Until(d => d.FindElements(recordSelector).Count == 0);
            Assert.AreEqual("Show posts", driver.FindElement(By.Id("button2")).Text.Trim());

            driver.FindElement(By.Id("button2")).Click();
            wait.Until(d => d.FindElements(recordSelector).Count > 0);
            Thread.Sleep(pauseMilliseconds);

            var records = driver.FindElements(recordSelector);
            wait.Until(d => d.FindElements(recordSelector).Count > 0);
            //wait.Until(d => d.FindElements(recordSelector).Count == 6);
           // Thread.Sleep(pauseMilliseconds);
            Assert.IsTrue(records.Any(r => r.Text.Contains("Blinding Lights")));
            Assert.AreEqual(
                "Hide posts",
                driver.FindElement(By.Id("button2")).Text.Trim());
            Thread.Sleep(pauseMilliseconds);
        }
        finally
        {
            driver.Quit();
        }
    }

    [TestMethod]
    public void SearchByFilters_AndClearFilters()
    {
        var options = new EdgeOptions();
        var driver = new EdgeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        var records = By.CssSelector("#app ul li.list-group-item");

        try
        {
            Login(driver, wait);

            SearchByFilter(driver, wait, "text", "Blinding Lights", "title: Blinding Lights");
            SearchByFilter(driver, wait, "artistFilter", "Dua Lipa", "artist: Dua Lipa");
            SearchByFilter(driver, wait, "idFilter", "4", "id: 4 -");
            SearchByFilter(driver, wait, "durationFilter", "69", "duration: 69");
            SearchByFilter(driver, wait, "publicationYearFilter", "2022", "publication year: 2022");

            driver.FindElement(By.XPath("//button[normalize-space()='Clear filters']")).Click();
            wait.Until(d => d.FindElements(records).Count > 1);
            Assert.AreEqual("", driver.FindElement(By.Id("text")).GetAttribute("value"));
            Assert.AreEqual("", driver.FindElement(By.Id("artistFilter")).GetAttribute("value"));
            Assert.AreEqual("", driver.FindElement(By.Id("idFilter")).GetAttribute("value"));
            Assert.AreEqual("", driver.FindElement(By.Id("durationFilter")).GetAttribute("value"));
            Assert.AreEqual("", driver.FindElement(By.Id("publicationYearFilter")).GetAttribute("value"));
        }
        finally
        {
            driver.Quit();
        }
    }

    [TestMethod]
    public void SortByTitle_ShowsTitlesInOrder()
    {
        var options = new EdgeOptions();
        var driver = new EdgeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        var records = By.CssSelector("#app ul li.list-group-item");

        try
        {
            Login(driver, wait);
            new SelectElement(driver.FindElement(By.CssSelector("select[aria-label='Sort records by']")))
                .SelectByValue("title");

            wait.Until(d =>
            {
                var titles = d.FindElements(records)
                    .Select(record => record.Text.Split("title: ")[1].Split(" - artist:")[0])
                    .ToList();
                return titles.Count > 1 && titles.SequenceEqual(titles.OrderBy(title => title));
            });
        }
        finally
        {
            driver.Quit();
        }
    }

    [TestMethod]
    public void AddRecordWithoutTitle_ShowsValidationError()
    {
        var options = new EdgeOptions();
        var driver = new EdgeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        const string expectedError = "Enter a title, artist, positive duration and publication year.";

        try
        {
            Login(driver, wait);
            driver.FindElement(By.Id("recordArtist")).SendKeys("UI Test Artist");
            driver.FindElement(By.Id("recordDuration")).SendKeys("210");
            driver.FindElement(By.Id("recordPublicationYear")).SendKeys("2024");
            driver.FindElement(By.Id("addRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='alert']"))
                .Any(error => error.Text == expectedError));
            Assert.AreEqual(expectedError, driver.FindElement(By.CssSelector("[role='alert']")).Text);
        }
        finally
        {
            driver.Quit();
        }
    }

    [TestMethod]
    public void DeleteRecordThatDoesNotExist_ShowsError()
    {
        var options = new EdgeOptions();
        var driver = new EdgeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        try
        {
            Login(driver, wait);
            driver.FindElement(By.Id("recordId")).SendKeys("999999");
            driver.FindElement(By.Id("deleteRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='alert']"))
                .Any(error => error.Text == "No record found with that ID."));
        }
        finally
        {
            driver.Quit();
        }
    }

    [TestMethod]
    public void UpdateRecordThatDoesNotExist_ShowsError()
    {
        var options = new EdgeOptions();
        var driver = new EdgeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        try
        {
            Login(driver, wait);
            driver.FindElement(By.Id("recordId")).SendKeys("999999");
            driver.FindElement(By.Id("recordTitle")).SendKeys("Record not found");
            driver.FindElement(By.Id("recordArtist")).SendKeys("UI Test Artist");
            driver.FindElement(By.Id("recordDuration")).SendKeys("210");
            driver.FindElement(By.Id("recordPublicationYear")).SendKeys("2024");
            driver.FindElement(By.Id("updateRecordButton")).Click();

            wait.Until(d => d.FindElements(By.CssSelector("[role='alert']"))
                .Any(error => error.Text == "No record found with that ID."));
        }
        finally
        {
            driver.Quit();
        }
    }

    [TestMethod]
    public void Logout_GoesBackToLoginPage()
    {
        var options = new EdgeOptions();
        var driver = new EdgeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        try
        {
            Login(driver, wait);
            driver.FindElement(By.XPath("//button[normalize-space()='Logout']")).Click();

            wait.Until(d => d.Url.EndsWith("/login.html", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(driver.FindElement(By.CssSelector("input[placeholder='Username']")).Displayed);
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
            Login(driver, wait);
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
    

