using System.Text.Json;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace E2ETests;

[TestFixture]
public class TrainingPlansApiTests : PlaywrightTest
{
    private IAPIRequestContext _request = null!;
    private const string BaseUrl = "http://localhost:5228";
    private readonly List<int> _createdPlanIds = new();
    private readonly List<int> _createdTrainingIds = new();
    private readonly List<int> _createdTrainerIds = new();
    private readonly List<int> _createdMemberIds = new();

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
        foreach (var id in _createdPlanIds)
            await _request.DeleteAsync($"/api/trainingplans/{id}");
        foreach (var id in _createdTrainingIds)
            await _request.DeleteAsync($"/api/trainings/{id}");
        foreach (var id in _createdTrainerIds)
            await _request.DeleteAsync($"/api/trainers/{id}");
        foreach (var id in _createdMemberIds)
            await _request.DeleteAsync($"/api/members/{id}");
        _createdPlanIds.Clear();
        _createdTrainingIds.Clear();
        _createdTrainerIds.Clear();
        _createdMemberIds.Clear();
        await _request.DisposeAsync();
    }

    private async Task<int> CreateMemberAndTrack()
    {
        var response = await _request.PostAsync("/api/members", new APIRequestContextOptions
        {
            DataObject = new { firstName = "Test", lastName = "Member", email = $"member_{Guid.NewGuid():N}@test.com", joinDate = DateTime.UtcNow }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        _createdMemberIds.Add(id);
        return id;
    }

    private async Task<int> CreateTrainingAndTrack()
    {
        var trainerResponse = await _request.PostAsync("/api/trainers", new APIRequestContextOptions
        {
            DataObject = new { firstName = "Test", lastName = "Trainer", specialization = "Strength" }
        });
        var trainerId = JsonDocument.Parse(await trainerResponse.TextAsync()).RootElement.GetProperty("id").GetInt32();
        _createdTrainerIds.Add(trainerId);

        var trainingResponse = await _request.PostAsync("/api/trainings", new APIRequestContextOptions
        {
            DataObject = new { name = "Test Training", description = "Desc", durationInMinutes = 60, trainerId }
        });
        var trainingId = JsonDocument.Parse(await trainingResponse.TextAsync()).RootElement.GetProperty("id").GetInt32();
        _createdTrainingIds.Add(trainingId);
        return trainingId;
    }

    private async Task<JsonElement> CreatePlanAndTrack(int memberId, int trainingId)
    {
        var response = await _request.PostAsync("/api/trainingplans", new APIRequestContextOptions
        {
            DataObject = new { memberId, trainingId, startDate = DateTime.UtcNow, endDate = DateTime.UtcNow.AddMonths(1) }
        });
        var json = JsonDocument.Parse(await response.TextAsync()).RootElement;
        _createdPlanIds.Add(json.GetProperty("id").GetInt32());
        return json;
    }

    // ==================== GET ALL ====================

    [Test]
    public async Task GetAll_ReturnsStatusOk()
    {
        var response = await _request.GetAsync("/api/trainingplans");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAll_ReturnsJsonArray()
    {
        var response = await _request.GetAsync("/api/trainingplans");
        var json = JsonDocument.Parse(await response.TextAsync()).RootElement;
        Assert.That(json.ValueKind, Is.EqualTo(JsonValueKind.Array));
    }

    [Test]
    public async Task GetAll_AfterCreate_ContainsNewPlan()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var created = await CreatePlanAndTrack(memberId, trainingId);
        var response = await _request.GetAsync("/api/trainingplans");
        var plans = JsonDocument.Parse(await response.TextAsync()).RootElement;
        var found = plans.EnumerateArray().Any(p => p.GetProperty("id").GetInt32() == created.GetProperty("id").GetInt32());
        Assert.That(found, Is.True);
    }

    // ==================== GET BY ID ====================

    [Test]
    public async Task GetById_ExistingPlan_ReturnsOk()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var created = await CreatePlanAndTrack(memberId, trainingId);
        var response = await _request.GetAsync($"/api/trainingplans/{created.GetProperty("id").GetInt32()}");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await _request.GetAsync("/api/trainingplans/999999");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task GetById_ReturnsCorrectPlanData()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var created = await CreatePlanAndTrack(memberId, trainingId);
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.GetAsync($"/api/trainingplans/{id}");
        var plan = JsonDocument.Parse(await response.TextAsync()).RootElement;
        Assert.That(plan.GetProperty("memberId").GetInt32(), Is.EqualTo(memberId));
        Assert.That(plan.GetProperty("trainingId").GetInt32(), Is.EqualTo(trainingId));
    }

    // ==================== CREATE ====================

    [Test]
    public async Task Create_ValidPlan_Returns201()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var response = await _request.PostAsync("/api/trainingplans", new APIRequestContextOptions
        {
            DataObject = new { memberId, trainingId, startDate = DateTime.UtcNow, endDate = DateTime.UtcNow.AddMonths(1) }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        _createdPlanIds.Add(id);
        Assert.That(response.Status, Is.EqualTo(201));
    }

    [Test]
    public async Task Create_ValidPlan_ReturnsCreatedObjectWithId()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var created = await CreatePlanAndTrack(memberId, trainingId);
        Assert.That(created.GetProperty("id").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task Create_ValidPlan_CanBeFoundAfterward()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var created = await CreatePlanAndTrack(memberId, trainingId);
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.GetAsync($"/api/trainingplans/{id}");
        Assert.That(response.Status, Is.EqualTo(200));
    }

    // ==================== UPDATE ====================

    [Test]
    public async Task Update_ExistingPlan_Returns204()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var created = await CreatePlanAndTrack(memberId, trainingId);
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.PutAsync($"/api/trainingplans/{id}", new APIRequestContextOptions
        {
            DataObject = new { id, memberId, trainingId, startDate = DateTime.UtcNow, endDate = DateTime.UtcNow.AddMonths(3) }
        });
        Assert.That(response.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task Update_NonExistingPlan_Returns404()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var response = await _request.PutAsync("/api/trainingplans/999999", new APIRequestContextOptions
        {
            DataObject = new { id = 999999, memberId, trainingId, startDate = DateTime.UtcNow, endDate = DateTime.UtcNow.AddMonths(1) }
        });
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task Update_WithIdMismatch_Returns400()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var created = await CreatePlanAndTrack(memberId, trainingId);
        var id = created.GetProperty("id").GetInt32();
        var response = await _request.PutAsync($"/api/trainingplans/{id + 1}", new APIRequestContextOptions
        {
            DataObject = new { id, memberId, trainingId, startDate = DateTime.UtcNow, endDate = DateTime.UtcNow.AddMonths(1) }
        });
        Assert.That(response.Status, Is.EqualTo(400));
    }

    // ==================== DELETE ====================

    [Test]
    public async Task Delete_ExistingPlan_Returns204()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var response = await _request.PostAsync("/api/trainingplans", new APIRequestContextOptions
        {
            DataObject = new { memberId, trainingId, startDate = DateTime.UtcNow, endDate = DateTime.UtcNow.AddMonths(1) }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        var deleteResponse = await _request.DeleteAsync($"/api/trainingplans/{id}");
        Assert.That(deleteResponse.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task Delete_NonExistingPlan_Returns404()
    {
        var response = await _request.DeleteAsync("/api/trainingplans/999999");
        Assert.That(response.Status, Is.EqualTo(404));
    }

    [Test]
    public async Task Delete_ExistingPlan_CannotBeFoundAfterward()
    {
        var memberId = await CreateMemberAndTrack();
        var trainingId = await CreateTrainingAndTrack();
        var response = await _request.PostAsync("/api/trainingplans", new APIRequestContextOptions
        {
            DataObject = new { memberId, trainingId, startDate = DateTime.UtcNow, endDate = DateTime.UtcNow.AddMonths(1) }
        });
        var id = JsonDocument.Parse(await response.TextAsync()).RootElement.GetProperty("id").GetInt32();
        await _request.DeleteAsync($"/api/trainingplans/{id}");
        var getResponse = await _request.GetAsync($"/api/trainingplans/{id}");
        Assert.That(getResponse.Status, Is.EqualTo(404));
    }
}
