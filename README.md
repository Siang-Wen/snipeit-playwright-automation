# SNIPE-IT Asset Automation with Playwright (.NET)

This project automates the creation and verification of an asset in the [Snipe-IT demo](https://demo.snipeitapp.com) using **Playwright** with **.NET 8** and **NUnit**.

## Features
- Logs into the Snipe-IT demo instance
- Creates a new **Macbook Pro 13"** asset:
  - Uploads a photo
  - Assigns status as **"Ready to Deploy"**
  - Checks out the asset to a random user
- Searches for the asset using the asset tag
- Validates:
  - The asset appears in the list
  - The assigned user and company match
  - The **History** tab logs the correct creation date

## Tech Stack
- .NET 8
- Microsoft.Playwright
- NUnit
- Visual Studio 2022

## Setup Instructions
1. **Clone or download** this repo
2. Open the solution (`.csproj`) in **Visual Studio 2022**
3. Install Playwright

You can run the project using Visual Studio Test Explorer or via command line: dotnet test

## Future Improvements
- Add more detailed assertions (e.g., verifying additional metadata)
- Improve robustness of element selectors
- Parameterize asset details for broader testing scenarios
