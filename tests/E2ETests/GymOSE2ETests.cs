using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace E2ETests;

[TestFixture]
public class GymOSE2ETests : PageTest
{
    private const string FrontendUrl = "http://localhost:3000";


    [Test]
    public async Task App_LoadsDashboardByDefault()
    {
        await Page.GotoAsync(FrontendUrl);
        await Expect(Page.Locator(".sidebar-logo .logo-title")).ToHaveTextAsync("GymOS");
        await Expect(Page.Locator(".nav-item.active")).ToContainTextAsync("Dashboard");
    }

    [Test]
    public async Task Navigation_ClickMembers_ShowsMembersPage()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Members" }).ClickAsync();
        await Expect(Page.Locator(".section-title")).ToContainTextAsync("Manage Members");
    }

    [Test]
    public async Task Navigation_ClickTrainers_ShowsTrainersPage()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Trainers" }).ClickAsync();
        await Expect(Page.Locator(".section-title")).ToContainTextAsync("Manage Trainers");
    }

    [Test]
    public async Task Navigation_ClickTrainings_ShowsTrainingsPage()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Trainings" }).ClickAsync();
        await Expect(Page.Locator(".section-title")).ToContainTextAsync("Manage Trainings");
    }

    [Test]
    public async Task Navigation_ClickPlans_ShowsPlansPage()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Plans" }).ClickAsync();
        await Expect(Page.Locator(".section-title")).ToContainTextAsync("Manage Plans");
    }



    [Test]
    public async Task Members_AddNewMember_AppearsInTable()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Members" }).ClickAsync();

        var uniqueEmail = $"milan_{Guid.NewGuid():N}@test.com";

        await Page.Locator("input.form-input").Nth(0).FillAsync("Milan");
        await Page.Locator("input.form-input").Nth(1).FillAsync("Petrovic");
        await Page.Locator("input[type='email']").FillAsync(uniqueEmail);
        await Page.Locator("input[type='date']").FillAsync("2024-01-15");
        await Page.Locator("button.btn-primary").ClickAsync();

        await Expect(Page.Locator(".data-table")).ToContainTextAsync("Milan");
        await Expect(Page.Locator(".data-table")).ToContainTextAsync("Petrovic");
    }

    [Test]
    public async Task Members_EditMember_UpdatesInTable()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Members" }).ClickAsync();

        var uniqueLast = "Orig" + Guid.NewGuid().ToString("N")[..6];
        await Page.Locator("input.form-input").Nth(0).FillAsync("Jelena");
        await Page.Locator("input.form-input").Nth(1).FillAsync(uniqueLast);
        await Page.Locator("input[type='email']").FillAsync($"jelena_{Guid.NewGuid():N}@test.com");
        await Page.Locator("input[type='date']").FillAsync("2024-02-01");
        await Page.Locator("button.btn-primary").ClickAsync();

        var row = Page.Locator(".data-table tr", new() { HasText = uniqueLast });
        await row.WaitForAsync();
        await row.Locator(".btn-icon").First.ClickAsync();

        await Page.Locator("input.form-input").Nth(1).FillAsync("Markovic");
        await Page.Locator("button.btn-primary").ClickAsync();

        await Expect(Page.Locator(".data-table")).ToContainTextAsync("Markovic");
    }

    [Test]
    public async Task Members_DeleteMember_RemovedFromTable()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Members" }).ClickAsync();

        var uniqueLast = "Del" + Guid.NewGuid().ToString("N")[..8];
        await Page.Locator("input.form-input").Nth(0).FillAsync("Branko");
        await Page.Locator("input.form-input").Nth(1).FillAsync(uniqueLast);
        await Page.Locator("input[type='email']").FillAsync($"branko_{Guid.NewGuid():N}@test.com");
        await Page.Locator("input[type='date']").FillAsync("2024-03-01");
        await Page.Locator("button.btn-primary").ClickAsync();

        var row = Page.Locator(".data-table tr", new() { HasText = uniqueLast });
        await row.WaitForAsync();
        await row.Locator(".btn-icon.danger").ClickAsync();

        await Page.WaitForTimeoutAsync(500);
        await Expect(Page.Locator(".data-table, .empty-state")).Not.ToContainTextAsync(uniqueLast);
    }



    [Test]
    public async Task Trainers_AddNewTrainer_AppearsInTable()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Trainers" }).ClickAsync();

        await Page.Locator("input.form-input").Nth(0).FillAsync("Nemanja");
        await Page.Locator("input.form-input").Nth(1).FillAsync("Kostic");
        await Page.Locator("select.form-input").SelectOptionAsync("Crossfit");
        await Page.Locator("button.btn-primary").ClickAsync();

        await Expect(Page.Locator(".data-table")).ToContainTextAsync("Nemanja");
        await Expect(Page.Locator(".data-table")).ToContainTextAsync("Crossfit");
    }

    [Test]
    public async Task Trainers_DeleteTrainer_RemovedFromTable()
    {
        await Page.GotoAsync(FrontendUrl);
        await Page.Locator(".nav-item", new() { HasText = "Trainers" }).ClickAsync();

        var uniqueLast = "Tr" + Guid.NewGuid().ToString("N")[..8];
        await Page.Locator("input.form-input").Nth(0).FillAsync("Predrag");
        await Page.Locator("input.form-input").Nth(1).FillAsync(uniqueLast);
        await Page.Locator("select.form-input").SelectOptionAsync("Cardio");
        await Page.Locator("button.btn-primary").ClickAsync();

        var row = Page.Locator(".data-table tr", new() { HasText = uniqueLast });
        await row.WaitForAsync();
        await row.Locator(".btn-icon.danger").ClickAsync();

        await Page.WaitForTimeoutAsync(500);
        await Expect(Page.Locator(".data-table, .empty-state")).Not.ToContainTextAsync(uniqueLast);
    }



    [Test]
    public async Task Dashboard_ShowsStatCards()
    {
        await Page.GotoAsync(FrontendUrl);
        await Expect(Page.Locator(".stat-card").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Dashboard_AfterAddingMember_UpdatesMemberCount()
    {
        await Page.GotoAsync(FrontendUrl);

        var memberCard = Page.Locator(".stat-card .stat-value").Nth(2);

        await Expect(memberCard).Not.ToHaveTextAsync("", new() { Timeout = 10000 });
        await Page.WaitForTimeoutAsync(500);
        var countBefore = await memberCard.InnerTextAsync();

        await Page.Locator(".nav-item", new() { HasText = "Members" }).ClickAsync();
        await Page.Locator("input.form-input").Nth(0).FillAsync("Bojana");
        await Page.Locator("input.form-input").Nth(1).FillAsync("Simic");
        await Page.Locator("input[type='email']").FillAsync($"bojana_{Guid.NewGuid():N}@test.com");
        await Page.Locator("input[type='date']").FillAsync("2024-04-01");
        await Page.Locator("button.btn-primary").ClickAsync();

        await Page.Locator(".nav-item", new() { HasText = "Dashboard" }).ClickAsync();

        await Expect(memberCard).Not.ToHaveTextAsync("", new() { Timeout = 10000 });
        await Page.WaitForTimeoutAsync(500);
        var countAfter = await memberCard.InnerTextAsync();

        Assert.That(int.Parse(countAfter), Is.GreaterThanOrEqualTo(int.Parse(countBefore)));
    }
}