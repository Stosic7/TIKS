using System.Net;
using System.Net.Http.Json;
using backend.Models;

namespace ComponentTests;

[TestFixture]
public class TrainingsControllerTests
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

    private async Task<Trainer> CreateTrainerAsync()
    {
        var trainer = new Trainer { FirstName = "Zoran", LastName = "Pavlovic", Specialization = "Snaga" };
        var response = await _client.PostAsJsonAsync("/api/trainers", trainer);
        return (await response.Content.ReadFromJsonAsync<Trainer>())!;
    }

    private async Task<Training> CreateTrainingAsync(int trainerId, string name = "Jutarnji trening", string description = "Osnovni trening", int duration = 60)
    {
        var training = new Training
        {
            Name = name,
            Description = description,
            DurationInMinutes = duration,
            TrainerId = trainerId
        };
        var response = await _client.PostAsJsonAsync("/api/trainings", training);
        return (await response.Content.ReadFromJsonAsync<Training>())!;
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/trainings");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/trainings");
        var trainings = await response.Content.ReadFromJsonAsync<List<Training>>();
        Assert.That(trainings, Is.Empty);
    }

    [Test]
    public async Task GetAll_WithTwoTrainings_ReturnsCorrectCount()
    {
        var trainer = await CreateTrainerAsync();
        await CreateTrainingAsync(trainer.Id, "Trening A");
        await CreateTrainingAsync(trainer.Id, "Trening B");

        var response = await _client.GetAsync("/api/trainings");
        var trainings = await response.Content.ReadFromJsonAsync<List<Training>>();

        Assert.That(trainings!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_ExistingTraining_ReturnsOk()
    {
        var trainer = await CreateTrainerAsync();
        var created = await CreateTrainingAsync(trainer.Id);
        var response = await _client.GetAsync($"/api/trainings/{created.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetById_NonExistingTraining_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/trainings/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetById_ExistingTraining_ReturnsCorrectData()
    {
        var trainer = await CreateTrainerAsync();
        var created = await CreateTrainingAsync(trainer.Id, "Kardio udar", "Intenzivan kardio", 45);

        var response = await _client.GetAsync($"/api/trainings/{created.Id}");
        var training = await response.Content.ReadFromJsonAsync<Training>();

        Assert.That(training!.Name, Is.EqualTo("Kardio udar"));
        Assert.That(training.DurationInMinutes, Is.EqualTo(45));
        Assert.That(training.TrainerId, Is.EqualTo(trainer.Id));
    }

    [Test]
    public async Task Create_ValidTraining_ReturnsCreated()
    {
        var trainer = await CreateTrainerAsync();
        var training = new Training { Name = "Novi trening", Description = "Opis", DurationInMinutes = 30, TrainerId = trainer.Id };
        var response = await _client.PostAsJsonAsync("/api/trainings", training);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Create_ValidTraining_ReturnsCreatedTrainingWithId()
    {
        var trainer = await CreateTrainerAsync();
        var training = new Training { Name = "Novi trening", Description = "Opis", DurationInMinutes = 30, TrainerId = trainer.Id };
        var response = await _client.PostAsJsonAsync("/api/trainings", training);
        var created = await response.Content.ReadFromJsonAsync<Training>();

        Assert.That(created!.Id, Is.GreaterThan(0));
        Assert.That(created.Name, Is.EqualTo("Novi trening"));
    }

    [Test]
    public async Task Create_ValidTraining_CanBeRetrievedAfterward()
    {
        var trainer = await CreateTrainerAsync();
        var training = new Training { Name = "Uporni trening", Description = "Opis", DurationInMinutes = 50, TrainerId = trainer.Id };
        var createResponse = await _client.PostAsJsonAsync("/api/trainings", training);
        var created = await createResponse.Content.ReadFromJsonAsync<Training>();

        var getResponse = await _client.GetAsync($"/api/trainings/{created!.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Update_ExistingTraining_ReturnsNoContent()
    {
        var trainer = await CreateTrainerAsync();
        var created = await CreateTrainingAsync(trainer.Id);
        created.Name = "Azuriran naziv";
        var response = await _client.PutAsJsonAsync($"/api/trainings/{created.Id}", created);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Update_NonExistingTraining_ReturnsNotFound()
    {
        var trainer = await CreateTrainerAsync();
        var training = new Training { Id = 9999, Name = "Nepostojeci", Description = "Nema", DurationInMinutes = 0, TrainerId = trainer.Id };
        var response = await _client.PutAsJsonAsync("/api/trainings/9999", training);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var trainer = await CreateTrainerAsync();
        var created = await CreateTrainingAsync(trainer.Id);
        var response = await _client.PutAsJsonAsync($"/api/trainings/{created.Id + 1}", created);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Delete_ExistingTraining_ReturnsNoContent()
    {
        var trainer = await CreateTrainerAsync();
        var created = await CreateTrainingAsync(trainer.Id);
        var response = await _client.DeleteAsync($"/api/trainings/{created.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_NonExistingTraining_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/trainings/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ExistingTraining_IsActuallyRemoved()
    {
        var trainer = await CreateTrainerAsync();
        var created = await CreateTrainingAsync(trainer.Id);
        await _client.DeleteAsync($"/api/trainings/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/trainings/{created.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}