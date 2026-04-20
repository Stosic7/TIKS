using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace E2ETests;

[TestFixture]
public class MembersApiTests : PlaywrightTest
{
    private IAPIRequestContext _request = null!;
    private const string BaseUrl = "http://localhost:5229";
    private readonly List<int> _createdIds = new();

    [SetUp]
    public async Task SetupApiTesting()
    {
        _request = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = BaseUrl,
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        });
    }

    [TearDown]
    public async Task TeardownApiTesting()
    {
        foreach (var id in _createdIds)
            await _request.DeleteAsync($"/api/members/{id}");
        _createdIds.Clear();
        await _request.DisposeAsync();
    }

    private async Task<JsonElement> CreateMemberAndTrack(string firstName = "Test", string lastName = "User", string email = "test@playwright.com")
    {
        var response = await _request.PostAsync("/api/members", new APIRequestContextOptions
        {
            DataObject = new { firstName, lastName, email, joinDate = DateTime.UtcNow }
        });
        var json = JsonDocument.Parse(await response.TextAsync()).RootElement;
        _createdIds.Add(json.GetProperty("id").GetInt32());
        return json;
    }

    // ==================== GET ALL ====================

    [Test]
    public async Task GetAll_ReturnsStatusOk()
    {
        var response = await _request.GetAsync("/api/members");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAll_ReturnsJsonArray()
    {
        var response = await _request.GetAsync("/api/members");
        var body = await response.TextAsync();
        var json = JsonDocument.Parse(body).RootElement;
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
    }

    [Test]
    public async Task GetAll_AfterCreate_ContainsNewMember()
    {
        var created = await CreateMemberAndTrack("Playwright", "Member", "playwright@test.com");
        var response = await _request.GetAsync("/api/members");
        var members = JsonDocument.Parse(await response.TextAsync()).RootElement;
        var found = members.EnumerateArray().Any(m => m.GetProperty("id").GetInt32() == created.GetProperty("id").GetInt32());
        Assert.That(found, Is.True);
    }

    // ==================== GET BY ID ====================

    [Test]
    public async Task GetById_ExistingMember_ReturnsOk()
    {
        var created = await CreateMemberAndTrack();
        var response = await _request.GetAsync($"/api/members/{created.GetProperty("id").GetInt32()}");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _request.GetAsync("/api/members/999999");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task GetById_ReturnsCorrectMemberData()
    {
        var created = await CreateMemberAndTrack("Unique", "Name", "unique@test.com");
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.GetAsync($"/api/members/{id}");
        var member = JsonDocument.Parse(await response.TextAsync()).RootElement;
        Assert.That(member.GetProperty("firstName").GetString(), Is.EqualTo("Unique"));
        Assert.That(member.GetProperty("email").GetString(), Is.EqualTo("unique@test.com"));
    }

    // ==================== CREATE ====================

    [Test]
    public async Task Create_ValidMember_Returns201()
    {
        var response = await _request.PostAsync("/api/members", new APIRequestContextOptions
        {
            DataObject = new { firstName = "New", lastName = "Member", email = "new@test.com", joinDate = DateTime.UtcNow }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        _createdIds.Add(id);
        Assert.That(response.Status, Is.EqualTo(201));
    }

    [Test]
    public async Task Create_ValidMember_ReturnsCreatedObjectWithId()
    {
        var created = await CreateMemberAndTrack("Created", "User", "created@test.com");
        Assert.That(created.GetProperty("id").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task Create_ValidMember_CanBeFoundAfterward()
    {
        var created = await CreateMemberAndTrack("Findable", "User", "find@test.com");
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.GetAsync($"/api/members/{id}");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    // ==================== UPDATE ====================

    [Test]
    public async Task Update_ExistingMember_Returns204()
    {
        var created = await CreateMemberAndTrack();
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.PutAsync($"/api/members/{id}", new APIRequestContextOptions
        {
            DataObject = new { id, firstName = "Updated", lastName = "User", email = "updated@test.com", joinDate = DateTime.UtcNow }
        });
        Assert.That(response.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task Update_NonExistingMember_Returns404()
    {
        var response = await _request.PutAsync("/api/members/999999", new APIRequestContextOptions
        {
            DataObject = new { id = 999999, firstName = "Ghost", lastName = "User", email = "ghost@test.com", joinDate = DateTime.UtcNow }
        });
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task Update_WithIdMismatch_Returns400()
    {
        var created = await CreateMemberAndTrack();
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.PutAsync($"/api/members/{id + 1}", new APIRequestContextOptions
        {
            DataObject = new { id, firstName = "Mismatch", lastName = "User", email = "mis@test.com", joinDate = DateTime.UtcNow }
        });
        Assert.That(response.Status, Is.EqualTo(400));
    }

    // ==================== DELETE ====================

    [Test]
    public async Task Delete_ExistingMember_Returns204()
    {
        var response = await _request.PostAsync("/api/members", new APIRequestContextOptions
        {
            DataObject = new { firstName = "ToDelete", lastName = "User", email = "del@test.com", joinDate = DateTime.UtcNow }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        var deleteResponse = await _request.DeleteAsync($"/api/members/{id}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task Delete_NonExistingMember_Returns404()
    {
        var response = await _request.DeleteAsync("/api/members/999999");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task Delete_ExistingMember_CannotBeFoundAfterward()
    {
        var response = await _request.PostAsync("/api/members", new APIRequestContextOptions
        {
            DataObject = new { firstName = "Gone", lastName = "User", email = "gone@test.com", joinDate = DateTime.UtcNow }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        await _request.DeleteAsync($"/api/members/{id}");
        var getResponse = await _request.GetAsync($"/api/members/{id}");
        Assert.That(getResponse.Status, Is.EqualTo(404));
    }
}
