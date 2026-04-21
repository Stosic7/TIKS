using System.Net;
using System.Net.Http.Json;
using backend.Models;

namespace ComponentTests;

[TestFixture]
public class MembersControllerTests
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

    private async Task<Member> CreateMemberAsync(string firstName = "Marko", string lastName = "Jovanovic", string email = "marko@example.com")
    {
        var member = new Member
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            JoinDate = DateTime.UtcNow
        };
        var response = await _client.PostAsJsonAsync("/api/members", member);
        return (await response.Content.ReadFromJsonAsync<Member>())!;
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/members");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/members");
        var members = await response.Content.ReadFromJsonAsync<List<Member>>();
        Assert.That(members, Is.Empty);
    }

    [Test]
    public async Task GetAll_WithTwoMembers_ReturnsCorrectCount()
    {
        await CreateMemberAsync("Jovana", "Mitic", "jovana.mitic@gmail.com");
        await CreateMemberAsync("Tara", "Petrovic", "tara.petrovic@gmail.com");

        var response = await _client.GetAsync("/api/members");
        var members = await response.Content.ReadFromJsonAsync<List<Member>>();

        Assert.That(members!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_ExistingMember_ReturnsOk()
    {
        var created = await CreateMemberAsync();
        var response = await _client.GetAsync($"/api/members/{created.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetById_NonExistingMember_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/members/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetById_ExistingMember_ReturnsCorrectData()
    {
        var created = await CreateMemberAsync("Milica", "Nikolic", "milica.nikolic@example.com");
        var response = await _client.GetAsync($"/api/members/{created.Id}");
        var member = await response.Content.ReadFromJsonAsync<Member>();

        Assert.That(member!.FirstName, Is.EqualTo("Milica"));
        Assert.That(member.LastName, Is.EqualTo("Nikolic"));
        Assert.That(member.Email, Is.EqualTo("milica.nikolic@example.com"));
    }

    [Test]
    public async Task Create_ValidMember_ReturnsCreated()
    {
        var member = new Member { FirstName = "Stefan", LastName = "Ilic", Email = "stefan.ilic@example.com", JoinDate = DateTime.UtcNow };
        var response = await _client.PostAsJsonAsync("/api/members", member);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Create_ValidMember_ReturnsCreatedMemberWithId()
    {
        var member = new Member { FirstName = "Stefan", LastName = "Ilic", Email = "stefan.ilic@example.com", JoinDate = DateTime.UtcNow };
        var response = await _client.PostAsJsonAsync("/api/members", member);
        var created = await response.Content.ReadFromJsonAsync<Member>();

        Assert.That(created!.Id, Is.GreaterThan(0));
        Assert.That(created.FirstName, Is.EqualTo("Stefan"));
    }

    [Test]
    public async Task Create_ValidMember_CanBeRetrievedAfterward()
    {
        var member = new Member { FirstName = "Nikola", LastName = "Djordjevic", Email = "nikola.djordjevic@example.com", JoinDate = DateTime.UtcNow };
        var createResponse = await _client.PostAsJsonAsync("/api/members", member);
        var created = await createResponse.Content.ReadFromJsonAsync<Member>();

        var getResponse = await _client.GetAsync($"/api/members/{created!.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Update_ExistingMember_ReturnsNoContent()
    {
        var created = await CreateMemberAsync();
        created.FirstName = "Dusan";
        var response = await _client.PutAsJsonAsync($"/api/members/{created.Id}", created);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Update_NonExistingMember_ReturnsNotFound()
    {
        var member = new Member { Id = 9999, FirstName = "Lazar", LastName = "Stanisic", Email = "lazar.stanisic@example.com", JoinDate = DateTime.UtcNow };
        var response = await _client.PutAsJsonAsync("/api/members/9999", member);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var created = await CreateMemberAsync();
        var response = await _client.PutAsJsonAsync($"/api/members/{created.Id + 1}", created);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Delete_ExistingMember_ReturnsNoContent()
    {
        var created = await CreateMemberAsync();
        var response = await _client.DeleteAsync($"/api/members/{created.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Delete_NonExistingMember_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/members/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ExistingMember_IsActuallyRemoved()
    {
        var created = await CreateMemberAsync();
        await _client.DeleteAsync($"/api/members/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/members/{created.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}