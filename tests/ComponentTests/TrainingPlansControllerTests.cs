using System.Net;
using System.Net.Http.Json;
using backend.Models;

namespace ComponentTests;

[TestFixture]
public class TrainingPlansControllerTests
{
    private CustomWebAppFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebAppFactory();
        _client = _factory.CreateClient();
        _factory.ResetDatabase();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<Member> CreateMemberAsync()
    {
        var member = new Member { FirstName = "Marko", LastName = "Jovanovic", Email = "marko.jovanovic@example.com", JoinDate = DateTime.UtcNow };
        var response = await _client.PostAsJsonAsync("/api/members", member);
        return (await response.Content.ReadFromJsonAsync<Member>())!;
    }

    private async Task<Training> CreateTrainingAsync()
    {
        var trainer = new Trainer { FirstName = "Dejan", LastName = "Stankovic", Specialization = "Snaga" };
        var trainerResponse = await _client.PostAsJsonAsync("/api/trainers", trainer);
        var createdTrainer = (await trainerResponse.Content.ReadFromJsonAsync<Trainer>())!;

        var training = new Training { Name = "Funkcionalni trening", Description = "Opis", DurationInMinutes = 60, TrainerId = createdTrainer.Id };
        var trainingResponse = await _client.PostAsJsonAsync("/api/trainings", training);
        return (await trainingResponse.Content.ReadFromJsonAsync<Training>())!;
    }

    private async Task<TrainingPlan> CreateTrainingPlanAsync(int memberId, int trainingId)
    {
        var plan = new TrainingPlan
        {
            MemberId = memberId,
            TrainingId = trainingId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };
        var response = await _client.PostAsJsonAsync("/api/trainingplans", plan);
        return (await response.Content.ReadFromJsonAsync<TrainingPlan>())!;
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/trainingplans");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/trainingplans");
        var plans = await response.Content.ReadFromJsonAsync<List<TrainingPlan>>();
        Assert.That(plans, Is.Empty);
    }

    [Test]
    public async Task GetAll_WithTwoPlans_ReturnsCorrectCount()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        await CreateTrainingPlanAsync(member.Id, training.Id);
        await CreateTrainingPlanAsync(member.Id, training.Id);

        var response = await _client.GetAsync("/api/trainingplans");
        var plans = await response.Content.ReadFromJsonAsync<List<TrainingPlan>>();

        Assert.That(plans!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_ExistingPlan_ReturnsOk()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var created = await CreateTrainingPlanAsync(member.Id, training.Id);

        var response = await _client.GetAsync($"/api/trainingplans/{created.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetById_NonExistingPlan_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/trainingplans/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetById_ExistingPlan_ReturnsCorrectData()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var created = await CreateTrainingPlanAsync(member.Id, training.Id);

        var response = await _client.GetAsync($"/api/trainingplans/{created.Id}");
        var plan = await response.Content.ReadFromJsonAsync<TrainingPlan>();

        Assert.That(plan!.MemberId, Is.EqualTo(member.Id));
        Assert.That(plan.TrainingId, Is.EqualTo(training.Id));
    }

    [Test]
    public async Task Create_ValidPlan_ReturnsCreated()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var plan = new TrainingPlan { MemberId = member.Id, TrainingId = training.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) };

        var response = await _client.PostAsJsonAsync("/api/trainingplans", plan);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Create_ValidPlan_ReturnsCreatedPlanWithId()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var plan = new TrainingPlan { MemberId = member.Id, TrainingId = training.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) };

        var response = await _client.PostAsJsonAsync("/api/trainingplans", plan);
        var created = await response.Content.ReadFromJsonAsync<TrainingPlan>();

        Assert.That(created!.Id, Is.GreaterThan(0));
        Assert.That(created.MemberId, Is.EqualTo(member.Id));
    }

    [Test]
    public async Task Create_ValidPlan_CanBeRetrievedAfterward()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var plan = new TrainingPlan { MemberId = member.Id, TrainingId = training.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) };

        var createResponse = await _client.PostAsJsonAsync("/api/trainingplans", plan);
        var created = await createResponse.Content.ReadFromJsonAsync<TrainingPlan>();

        var getResponse = await _client.GetAsync($"/api/trainingplans/{created!.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Update_ExistingPlan_ReturnsNoContent()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var created = await CreateTrainingPlanAsync(member.Id, training.Id);

        created.EndDate = DateTime.UtcNow.AddMonths(3);
        var response = await _client.PutAsJsonAsync($"/api/trainingplans/{created.Id}", created);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Update_NonExistingPlan_ReturnsNotFound()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var plan = new TrainingPlan { Id = 9999, MemberId = member.Id, TrainingId = training.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) };

        var response = await _client.PutAsJsonAsync("/api/trainingplans/9999", plan);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var created = await CreateTrainingPlanAsync(member.Id, training.Id);

        var response = await _client.PutAsJsonAsync($"/api/trainingplans/{created.Id + 1}", created);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Delete_ExistingPlan_ReturnsNoContent()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var created = await CreateTrainingPlanAsync(member.Id, training.Id);

        var response = await _client.DeleteAsync($"/api/trainingplans/{created.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_NonExistingPlan_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/trainingplans/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ExistingPlan_IsActuallyRemoved()
    {
        var member = await CreateMemberAsync();
        var training = await CreateTrainingAsync();
        var created = await CreateTrainingPlanAsync(member.Id, training.Id);

        await _client.DeleteAsync($"/api/trainingplans/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/trainingplans/{created.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}