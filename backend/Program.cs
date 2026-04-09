using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "gym.db");
dbPath = Path.GetFullPath(dbPath);
builder.Services.AddDbContext<GymContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3002")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GymContext>();
    db.Database.EnsureCreated();
    SeedData(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReact");
app.MapControllers();
app.Run();

void SeedData(GymContext db)
{
    if (db.Trainers.Any()) return;

    var trainers = new List<Trainer>
    {
        new() { FirstName = "Marko",   LastName = "Petrovic",  Specialization = "Strength Training" },
        new() { FirstName = "Ana",     LastName = "Jovanovic", Specialization = "Cardio & Endurance" },
        new() { FirstName = "Stefan",  LastName = "Nikolic",   Specialization = "CrossFit" },
    };
    db.Trainers.AddRange(trainers);
    db.SaveChanges();

    var trainings = new List<Training>
    {
        new() { Name = "Morning Strength",  Description = "Full body strength workout",         DurationInMinutes = 60,  TrainerId = trainers[0].Id },
        new() { Name = "HIIT Cardio",       Description = "High intensity interval training",   DurationInMinutes = 45,  TrainerId = trainers[1].Id },
        new() { Name = "CrossFit WOD",      Description = "Workout of the day",                 DurationInMinutes = 50,  TrainerId = trainers[2].Id },
        new() { Name = "Upper Body Focus",  Description = "Chest, back and arms",               DurationInMinutes = 55,  TrainerId = trainers[0].Id },
        new() { Name = "Endurance Run",     Description = "Long distance cardio session",       DurationInMinutes = 90,  TrainerId = trainers[1].Id },
    };
    db.Trainings.AddRange(trainings);
    db.SaveChanges();

    var members = new List<Member>
    {
        new() { FirstName = "Nikola",   LastName = "Stojanovic", Email = "nikola.s@gmail.com",  JoinDate = new DateTime(2024, 1, 10) },
        new() { FirstName = "Jovana",   LastName = "Markovic",   Email = "jovana.m@gmail.com",  JoinDate = new DateTime(2024, 2, 5)  },
        new() { FirstName = "Petar",    LastName = "Ilic",       Email = "petar.i@gmail.com",   JoinDate = new DateTime(2024, 3, 15) },
        new() { FirstName = "Milica",   LastName = "Djordjevic", Email = "milica.d@gmail.com",  JoinDate = new DateTime(2024, 4, 20) },
        new() { FirstName = "Aleksa",   LastName = "Todorovic",  Email = "aleksa.t@gmail.com",  JoinDate = new DateTime(2024, 5, 8)  },
        new() { FirstName = "Jelena",   LastName = "Pavlovic",   Email = "jelena.p@gmail.com",  JoinDate = new DateTime(2024, 6, 1)  },
    };
    db.Members.AddRange(members);
    db.SaveChanges();

    var plans = new List<TrainingPlan>
    {
        new() { MemberId = members[0].Id, TrainingId = trainings[0].Id, StartDate = new DateTime(2025, 1, 1),  EndDate = new DateTime(2025, 6, 30) },
        new() { MemberId = members[0].Id, TrainingId = trainings[2].Id, StartDate = new DateTime(2025, 2, 1),  EndDate = new DateTime(2025, 8, 31) },
        new() { MemberId = members[1].Id, TrainingId = trainings[1].Id, StartDate = new DateTime(2025, 1, 15), EndDate = new DateTime(2025, 7, 15) },
        new() { MemberId = members[2].Id, TrainingId = trainings[0].Id, StartDate = new DateTime(2025, 3, 1),  EndDate = new DateTime(2025, 9, 1)  },
        new() { MemberId = members[3].Id, TrainingId = trainings[3].Id, StartDate = new DateTime(2025, 4, 1),  EndDate = new DateTime(2025, 12, 31)},
        new() { MemberId = members[4].Id, TrainingId = trainings[4].Id, StartDate = new DateTime(2025, 5, 1),  EndDate = new DateTime(2026, 5, 1)  },
        new() { MemberId = members[5].Id, TrainingId = trainings[2].Id, StartDate = new DateTime(2025, 6, 1),  EndDate = new DateTime(2026, 6, 1)  },
    };
    db.TrainingPlans.AddRange(plans);
    db.SaveChanges();
}

public partial class Program { }
