using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Globalization;

namespace Global360Task.Tests
{
    public class PlaywrightTest
    {
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;
        private string _photoPath = "";

        [SetUp]
        public async Task Setup()
        {
            // Define the Path for Macbook Pro 13" inch Photo
            var projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\"));
            _photoPath = Path.Combine(projectRoot, "Fixtures", "macbookPro13.jpg");

            // Set the Playwright Test to be ran in Headed Mode and Slow Mo of 100
            var playwright = await Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 100
            });
            _context = await _browser.NewContextAsync();
            _page = await _context.NewPageAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            // Close context and browser
            if (_context != null) await _context.CloseAsync();
            if (_browser != null) await _browser.CloseAsync();
        }

        [Test]
        public async Task CreateAndVerifyAssetAsync()
        {
            // 1. Login to the snipeit demo at https://demo.snipeitapp.com/login
            await _page.GotoAsync("https://demo.snipeitapp.com/login");
            await _page.GetByLabel("username").FillAsync("admin");
            await _page.GetByLabel("password").FillAsync("password");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

            // 2. Create a new Macbook Pro 13" asset with the ready to deploy status and checked out to a random user
            await _page.WaitForURLAsync("https://demo.snipeitapp.com");
            // Create New Asset
            await _page.GetByText("Create New").ClickAsync();
            await _page.Locator("li.dropdown.open > ul > li:nth-child(1)").ClickAsync();
            // Select the second Company on the Select list
            await _page.WaitForURLAsync("https://demo.snipeitapp.com/hardware/create");
            await _page.GetByText("Select Company").Nth(1).ClickAsync();
            await _page.Locator("#select2-company_select-results > li:nth-child(2)").ClickAsync();
            // Retrieve the Name of the Company Selected
            var company = await _page.Locator("#select2-company_select-container").GetAttributeAsync("title");
            Console.WriteLine($"Company is: {company}");
            // Retrieve the Asset Tag assigned
            var asset_tag = await _page.Locator("#asset_tag").GetAttributeAsync("value");
            Console.WriteLine($"Value is: {asset_tag}");
            // Select the Macbook Pro 13 inch model by typing "Macbook Pro 13" in the search bar and select the first item in Select list
            await _page.GetByText("Select a Model").ClickAsync();
            await _page.Locator("input.select2-search__field").FillAsync("Macbook Pro 13");
            await _page.Locator("#select2-model_select_id-results > li").ClickAsync();
            // Select the Status as "Ready to Deploy" and checked out to a random user (Select the second user in Select list)
            await _page.GetByText("Select Status").Nth(1).ClickAsync();
            await _page.Locator("li.select2-results__option", new PageLocatorOptions { HasTextString = "Ready to Deploy" }).ClickAsync();
            await _page.GetByText("Select a User").Nth(1).ClickAsync();
            await _page.Locator("#select2-assigned_user_select-results > li:nth-child(2)").ClickAsync();
            // Retrieve the name of the user
            var user = await _page.Locator("#select2-assigned_user_select-container").GetAttributeAsync("title");
            Console.WriteLine($"User is: {user}");
            // Attach a photo of Macbook Pro to the asset and Save the asset creation
            await _page.SetInputFilesAsync("input[type='file']", _photoPath);
            await _page.GetByRole(AriaRole.Button, new() { Name = "Save" }).Nth(1).ClickAsync();
            await _page.WaitForURLAsync("https://demo.snipeitapp.com");

            // 3. Find the asset you just created in the assets list to verify it was created successfully
            // Go to Assets List
            await _page.Locator("a[data-title='Assets']").ClickAsync();
            await _page.WaitForURLAsync("https://demo.snipeitapp.com/hardware");
            // Search for Asset using Asset Tag
            await _page.GetByPlaceholder("Search").FillAsync(asset_tag);
            await _page.GetByPlaceholder("Search").PressAsync("Enter");
            var assetElement = _page.GetByText(asset_tag);
            var assetCheck = await assetElement.TextContentAsync();
            // Check if Asset was created successfully
            Assert.That(assetCheck, Is.EqualTo(asset_tag), "Asset Tage does not match.");

            // 4. Navigate to the asset page from the list and validate relevant details from the asset creation
            await _page.GetByText(asset_tag).ClickAsync();
            // Modify the user name to the format of "{firstName} {lastName}"/ "{firstName}"
            // Remove everything starting from the first '('
            int parenIndex = user.IndexOf('(');
            string fullName = "";
            if (parenIndex > 0)
            {
                // Back up one character to remove the space before '(' if it exists
                int trimIndex = user[parenIndex - 1] == ' ' ? parenIndex - 1 : parenIndex;
                user = user.Substring(0, trimIndex);
            }
            // Trim off trailing dashes or whitespace
            string namePart = user.TrimEnd('-', ' ').Trim();
            // Check if it’s "LastName, FirstName" format
            if (namePart.Contains(","))
            {
                string[] parts = namePart.Split(',');
                if (parts.Length == 2)
                {
                    string lastName = parts[0].Trim();
                    string firstName = parts[1].Trim();
                    fullName = $"{firstName} {lastName}";
                }
            }
            else
            {
                fullName = namePart;
            }

            // Check if user set on Asset Creation is the same as user in asset page
            var userCheck = await _page.Locator(".user-image-inline").GetAttributeAsync("alt");
            userCheck = userCheck.Trim();
            Assert.That(userCheck, Is.EqualTo(fullName), "User does not match.");
            // Check if company set on Asset Creation is the same as company in asset page
            var companyElement = _page.Locator("//*[@id=\"details\"]/div/div/div[2]/div/div[3]/div[2]/a");
            var companyCheck = await companyElement.TextContentAsync();
            Assert.That(companyCheck, Is.EqualTo(company), "Company does not match.");

            // 5. Validate the details in the "History" tab on the asset page
            await _page.GetByText("History").Nth(1).ClickAsync();
            var dateElement = _page.Locator("#assetHistory > tbody > tr:nth-child(1) > td:nth-child(2)");
            var dateCheck = await dateElement.TextContentAsync();
            Console.WriteLine(dateCheck);
            // Modify DateTime from History Tab to default format
            string format = "dd/MM/yyyy hh:mm";
            // Try parsing History Tab DateTime
            if (DateTime.TryParseExact(dateCheck, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                // Compare just the date
                DateTime today = DateTime.Today;
                // Check if Asset Creation Date is the same as today's date
                Assert.That(parsedDate.Date, Is.EqualTo(today), $"Expected date to be {today:yyyy-MM-dd}, but got {parsedDate:yyyy-MM-dd}");
            }
            else
            {
                Assert.Fail("Could not parse the date string: " + dateCheck);
            }
        }
    }
}
