using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace E2ETests;

[TestFixture]
public class TrainingsApiTests : PlaywrightTest
{
    private IAPIRequestContext _request = null!;
    private const string BaseUrl = "http://localhost:5228";
    private readonly List<int> _createdTrainingIds = new();
    private readonly List<int> _createdTrainerIds = new();

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
        foreach (var id in _createdTrainingIds)
            await _request.DeleteAsync($"/api/trainings/{id}");
        foreach (var id in _createdTrainerIds)
            await _request.DeleteAsync($"/api/trainers/{id}");
        _createdTrainingIds.Clear();
        _createdTrainerIds.Clear();
        await _request.DisposeAsync();
    }

    private async Task<int> CreateTrainerAndTrack()
    {
        var response = await _request.PostAsync("/api/trainers", new APIRequestContextOptions
        {
            DataObject = new { firstName = "Test", lastName = "Trainer", specialization = "Strength" }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        _createdTrainerIds.Add(id);
        return id;
    }

    private async Task<JsonElement> CreateTrainingAndTrack(int trainerId, string name = "Test Training", string description = "Desc", int duration = 60)
    {
        var response = await _request.PostAsync("/api/trainings", new APIRequestContextOptions
        {
            DataObject = new { name, description, durationInMinutes = duration, trainerId }
        });
        var json = JsonDocument.Parse(await response.TextAsync()).RootElement;
        _createdTrainingIds.Add(json.GetProperty("id").GetInt32());
        return json;
    }


    [Test]
    public async Task GetAll_ReturnsStatusOk()
    {
        var response = await _request.GetAsync("/api/trainings");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAll_ReturnsJsonArray()
    {
        var response = await _request.GetAsync("/api/trainings");
        var json = JsonDocument.Parse(await response.TextAsync()).RootElement;
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
    }

    [Test]
    public async Task GetAll_AfterCreate_ContainsNewTraining()
    {
        var trainerId = await CreateTrainerAndTrack();
        var created = await CreateTrainingAndTrack(trainerId, "Unique Training");
        var response = await _request.GetAsync("/api/trainings");
        var trainings = JsonDocument.Parse(await response.TextAsync()).RootElement;
        var found = trainings.EnumerateArray().Any(t => t.GetProperty("id").GetInt32() == created.GetProperty("id").GetInt32());
        Assert.That(found, Is.True);
    }


    [Test]
    public async Task GetById_ExistingTraining_ReturnsOk()
    {
        var trainerId = await CreateTrainerAndTrack();
        var created = await CreateTrainingAndTrack(trainerId);
        var response = await _request.GetAsync($"/api/trainings/{created.GetProperty("id").GetInt32()}");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _request.GetAsync("/api/trainings/999999");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task GetById_ReturnsCorrectTrainingData()
    {
        var trainerId = await CreateTrainerAndTrack();
        var created = await CreateTrainingAndTrack(trainerId, "Cardio Blast", "Intense", 45);
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.GetAsync($"/api/trainings/{id}");
        var training = JsonDocument.Parse(await response.TextAsync()).RootElement;
        Assert.That(training.GetProperty("name").GetString(), Is.EqualTo("Cardio Blast"));
        Assert.That(training.GetProperty("durationInMinutes").GetInt32(), Is.EqualTo(45));
    }


    [Test]
    public async Task Create_ValidTraining_Returns201()
    {
        var trainerId = await CreateTrainerAndTrack();
        var response = await _request.PostAsync("/api/trainings", new APIRequestContextOptions
        {
            DataObject = new { name = "New Training", description = "Desc", durationInMinutes = 30, trainerId }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        _createdTrainingIds.Add(id);
        Assert.That(response.Status, Is.EqualTo(201));
    }

    [Test]
    public async Task Create_ValidTraining_ReturnsCreatedObjectWithId()
    {
        var trainerId = await CreateTrainerAndTrack();
        var created = await CreateTrainingAndTrack(trainerId, "Created Training");
        Assert.That(created.GetProperty("id").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task Create_ValidTraining_CanBeFoundAfterward()
    {
        var trainerId = await CreateTrainerAndTrack();
        var created = await CreateTrainingAndTrack(trainerId, "Persistent Training");
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.GetAsync($"/api/trainings/{id}");
        Assert.That(response.Status, Is.EqualTo(200));
    }


    [Test]
    public async Task Update_ExistingTraining_Returns204()
    {
        var trainerId = await CreateTrainerAndTrack();
        var created = await CreateTrainingAndTrack(trainerId);
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.PutAsync($"/api/trainings/{id}", new APIRequestContextOptions
        {
            DataObject = new { id, name = "Updated", description = "Desc", durationInMinutes = 90, trainerId }
        });
        Assert.That(response.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task Update_NonExistingTraining_Returns404()
    {
        var trainerId = await CreateTrainerAndTrack();
        var response = await _request.PutAsync("/api/trainings/999999", new APIRequestContextOptions
        {
            DataObject = new { id = 999999, name = "Ghost", description = "None", durationInMinutes = 0, trainerId }
        });
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task Update_WithIdMismatch_Returns400()
    {
        var trainerId = await CreateTrainerAndTrack();
        var created = await CreateTrainingAndTrack(trainerId);
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.PutAsync($"/api/trainings/{id + 1}", new APIRequestContextOptions
        {
            DataObject = new { id, name = "Mismatch", description = "Desc", durationInMinutes = 60, trainerId }
        });
        Assert.That(response.Status, Is.EqualTo(400));
    }


    [Test]
    public async Task Delete_ExistingTraining_Returns204()
    {
        var trainerId = await CreateTrainerAndTrack();
        var response = await _request.PostAsync("/api/trainings", new APIRequestContextOptions
        {
            DataObject = new { name = "ToDelete", description = "Desc", durationInMinutes = 30, trainerId }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        var deleteResponse = await _request.DeleteAsync($"/api/trainings/{id}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task Delete_NonExistingTraining_Returns404()
    {
        var response = await _request.DeleteAsync("/api/trainings/999999");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task Delete_ExistingTraining_CannotBeFoundAfterward()
    {
        var trainerId = await CreateTrainerAndTrack();
        var response = await _request.PostAsync("/api/trainings", new APIRequestContextOptions
        {
            DataObject = new { name = "Gone", description = "Desc", durationInMinutes = 30, trainerId }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        await _request.DeleteAsync($"/api/trainings/{id}");
        var getResponse = await _request.GetAsync($"/api/trainings/{id}");
        Assert.That(getResponse.Status, Is.EqualTo(404));
    }
}
