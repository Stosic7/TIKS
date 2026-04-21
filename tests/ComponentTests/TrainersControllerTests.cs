using System.Net;
using System.Net.Http.Json;
using backend.Models;

namespace ComponentTests;

[TestFixture]
public class TrainersControllerTests
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

    private async Task<Trainer> CreateTrainerAsync(string firstName = "Milos", string lastName = "Obradovic", string specialization = "Snaga")
    {
        var trainer = new Trainer
        {
            FirstName = firstName,
            LastName = lastName,
            Specialization = specialization
        };
        var response = await _client.PostAsJsonAsync("/api/trainers", trainer);
        return (await response.Content.ReadFromJsonAsync<Trainer>())!;
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/trainers");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/trainers");
        var trainers = await response.Content.ReadFromJsonAsync<List<Trainer>>();
        Assert.That(trainers, Is.Empty);
    }

    [Test]
    public async Task GetAll_WithTwoTrainers_ReturnsCorrectCount()
    {
        await CreateTrainerAsync("Milos", "Obradovic", "Snaga");
        await CreateTrainerAsync("Sara", "Lazarevic", "Kardio");

        var response = await _client.GetAsync("/api/trainers");
        var trainers = await response.Content.ReadFromJsonAsync<List<Trainer>>();

        Assert.That(trainers!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_ExistingTrainer_ReturnsOk()
    {
        var created = await CreateTrainerAsync();
        var response = await _client.GetAsync($"/api/trainers/{created.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetById_NonExistingTrainer_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/trainers/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetById_ExistingTrainer_ReturnsCorrectData()
    {
        var created = await CreateTrainerAsync("Dragan", "Todorovic", "Joga");
        var response = await _client.GetAsync($"/api/trainers/{created.Id}");
        var trainer = await response.Content.ReadFromJsonAsync<Trainer>();

        Assert.That(trainer!.FirstName, Is.EqualTo("Dragan"));
        Assert.That(trainer.LastName, Is.EqualTo("Todorovic"));
        Assert.That(trainer.Specialization, Is.EqualTo("Joga"));
    }

    [Test]
    public async Task Create_ValidTrainer_ReturnsCreated()
    {
        var trainer = new Trainer { FirstName = "Petar", LastName = "Savic", Specialization = "Boks" };
        var response = await _client.PostAsJsonAsync("/api/trainers", trainer);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Create_ValidTrainer_ReturnsCreatedTrainerWithId()
    {
        var trainer = new Trainer { FirstName = "Petar", LastName = "Savic", Specialization = "Boks" };
        var response = await _client.PostAsJsonAsync("/api/trainers", trainer);
        var created = await response.Content.ReadFromJsonAsync<Trainer>();

        Assert.That(created!.Id, Is.GreaterThan(0));
        Assert.That(created.Specialization, Is.EqualTo("Boks"));
    }

    [Test]
    public async Task Create_ValidTrainer_CanBeRetrievedAfterward()
    {
        var trainer = new Trainer { FirstName = "Aleksandar", LastName = "Popovic", Specialization = "CrossFit" };
        var createResponse = await _client.PostAsJsonAsync("/api/trainers", trainer);
        var created = await createResponse.Content.ReadFromJsonAsync<Trainer>();

        var getResponse = await _client.GetAsync($"/api/trainers/{created!.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Update_ExistingTrainer_ReturnsNoContent()
    {
        var created = await CreateTrainerAsync();
        created.Specialization = "Pilates";
        var response = await _client.PutAsJsonAsync($"/api/trainers/{created.Id}", created);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Update_NonExistingTrainer_ReturnsNotFound()
    {
        var trainer = new Trainer { Id = 9999, FirstName = "Vladan", LastName = "Ristic", Specialization = "Nema" };
        var response = await _client.PutAsJsonAsync("/api/trainers/9999", trainer);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var created = await CreateTrainerAsync();
        var response = await _client.PutAsJsonAsync($"/api/trainers/{created.Id + 1}", created);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Delete_ExistingTrainer_ReturnsNoContent()
    {
        var created = await CreateTrainerAsync();
        var response = await _client.DeleteAsync($"/api/trainers/{created.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_NonExistingTrainer_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/trainers/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ExistingTrainer_IsActuallyRemoved()
    {
        var created = await CreateTrainerAsync();
        await _client.DeleteAsync($"/api/trainers/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/trainers/{created.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}