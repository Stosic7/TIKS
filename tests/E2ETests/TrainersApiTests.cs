using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace E2ETests;

[TestFixture]
public class TrainersApiTests : PlaywrightTest
{
    private IAPIRequestContext _request = null!;
    private const string BaseUrl = "http://localhost:5228";
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
            await _request.DeleteAsync($"/api/trainers/{id}");
        _createdIds.Clear();
        await _request.DisposeAsync();
    }

    private async Task<JsonElement> CreateTrainerAndTrack(string firstName = "Test", string lastName = "Trainer", string specialization = "Strength")
    {
        var response = await _request.PostAsync("/api/trainers", new APIRequestContextOptions
        {
            DataObject = new { firstName, lastName, specialization }
        });
        var json = JsonDocument.Parse(await response.TextAsync()).RootElement;
        _createdIds.Add(json.GetProperty("id").GetInt32());
        return json;
    }

    // ==================== GET ALL ====================

    [Test]
    public async Task GetAll_ReturnsStatusOk()
    {
        var response = await _request.GetAsync("/api/trainers");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAll_ReturnsJsonArray()
    {
        var response = await _request.GetAsync("/api/trainers");
        var json = JsonDocument.Parse(await response.TextAsync()).RootElement;
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
    }

    [Test]
    public async Task GetAll_AfterCreate_ContainsNewTrainer()
    {
        var created = await CreateTrainerAndTrack("Playwright", "Trainer", "Yoga");
        var response = await _request.GetAsync("/api/trainers");
        var trainers = JsonDocument.Parse(await response.TextAsync()).RootElement;
        var found = trainers.EnumerateArray().Any(t => t.GetProperty("id").GetInt32() == created.GetProperty("id").GetInt32());
        Assert.That(found, Is.True);
    }

    // ==================== GET BY ID ====================

    [Test]
    public async Task GetById_ExistingTrainer_ReturnsOk()
    {
        var created = await CreateTrainerAndTrack();
        var response = await _request.GetAsync($"/api/trainers/{created.GetProperty("id").GetInt32()}");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _request.GetAsync("/api/trainers/999999");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task GetById_ReturnsCorrectTrainerData()
    {
        var created = await CreateTrainerAndTrack("Unique", "Coach", "Boxing");
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.GetAsync($"/api/trainers/{id}");
        var trainer = JsonDocument.Parse(await response.TextAsync()).RootElement;
        Assert.That(trainer.GetProperty("firstName").GetString(), Is.EqualTo("Unique"));
        Assert.That(trainer.GetProperty("specialization").GetString(), Is.EqualTo("Boxing"));
    }

    // ==================== CREATE ====================

    [Test]
    public async Task Create_ValidTrainer_Returns201()
    {
        var response = await _request.PostAsync("/api/trainers", new APIRequestContextOptions
        {
            DataObject = new { firstName = "New", lastName = "Trainer", specialization = "Pilates" }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        _createdIds.Add(id);
        Assert.That(response.Status, Is.EqualTo(201));
    }

    [Test]
    public async Task Create_ValidTrainer_ReturnsCreatedObjectWithId()
    {
        var created = await CreateTrainerAndTrack("Created", "Trainer", "CrossFit");
        Assert.That(created.GetProperty("id").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task Create_ValidTrainer_CanBeFoundAfterward()
    {
        var created = await CreateTrainerAndTrack("Findable", "Trainer", "Swimming");
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.GetAsync($"/api/trainers/{id}");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    // ==================== UPDATE ====================

    [Test]
    public async Task Update_ExistingTrainer_Returns204()
    {
        var created = await CreateTrainerAndTrack();
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.PutAsync($"/api/trainers/{id}", new APIRequestContextOptions
        {
            DataObject = new { id, firstName = "Updated", lastName = "Trainer", specialization = "Cardio" }
        });
        Assert.That(response.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task Update_NonExistingTrainer_Returns404()
    {
        var response = await _request.PutAsync("/api/trainers/999999", new APIRequestContextOptions
        {
            DataObject = new { id = 999999, firstName = "Ghost", lastName = "Trainer", specialization = "None" }
        });
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task Update_WithIdMismatch_Returns400()
    {
        var created = await CreateTrainerAndTrack();
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.PutAsync($"/api/trainers/{id + 1}", new APIRequestContextOptions
        {
            DataObject = new { id, firstName = "Mismatch", lastName = "Trainer", specialization = "None" }
        });
        Assert.That(response.Status, Is.EqualTo(400));
    }

    // ==================== DELETE ====================

    [Test]
    public async Task Delete_ExistingTrainer_Returns204()
    {
        var response = await _request.PostAsync("/api/trainers", new APIRequestContextOptions
        {
            DataObject = new { firstName = "ToDelete", lastName = "Trainer", specialization = "None" }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        var deleteResponse = await _request.DeleteAsync($"/api/trainers/{id}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task Delete_NonExistingTrainer_Returns404()
    {
        var response = await _request.DeleteAsync("/api/trainers/999999");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task Delete_ExistingTrainer_CannotBeFoundAfterward()
    {
        var response = await _request.PostAsync("/api/trainers", new APIRequestContextOptions
        {
            DataObject = new { firstName = "Gone", lastName = "Trainer", specialization = "None" }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        await _request.DeleteAsync($"/api/trainers/{id}");
        var getResponse = await _request.GetAsync($"/api/trainers/{id}");
        Assert.That(getResponse.Status, Is.EqualTo(404));
    }
}
