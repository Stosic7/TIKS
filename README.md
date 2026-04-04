# GymOS — Gym Management System

A fullstack web application for managing a gym. Provides complete management of trainers, trainings, members, and training plans through a modern web interface.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET 10) — REST API |
| Database | SQLite (`gym.db`) via Entity Framework Core |
| Frontend | React 18 |
| Component Tests | NUnit + WebApplicationFactory (in-memory SQLite) |
| API & E2E Tests | Playwright for .NET |

---

## Prerequisites

Make sure you have the following installed before running the project:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [SQLite](https://www.sqlite.org/) *(optional, for manual database inspection)*

Verify your versions:

```bash
dotnet --version   # must be 10.x
node --version     # must be 18+
npm --version
```

---

## Project Structure

```
TIKS/
├── backend/                        # ASP.NET Core REST API
│   ├── Controllers/
│   │   ├── MembersController.cs
│   │   ├── TrainersController.cs
│   │   ├── TrainingsController.cs
│   │   └── TrainingPlansController.cs
│   ├── Models/
│   │   ├── Member.cs
│   │   ├── Trainer.cs
│   │   ├── Training.cs
│   │   └── TrainingPlan.cs
│   ├── Data/
│   │   └── GymContext.cs
│   ├── Program.cs                  # Startup + Seed data
│   ├── backend.csproj
│   └── gym.db                      # SQLite database (auto-created on first run)
│
├── frontend/                       # React application
│   ├── src/
│   │   ├── components/
│   │   │   ├── Dashboard.js
│   │   │   ├── Members.js
│   │   │   ├── Trainers.js
│   │   │   ├── Trainings.js
│   │   │   └── TrainingPlans.js
│   │   ├── services/
│   │   │   └── api.js              # HTTP client for API calls
│   │   └── App.js
│   └── package.json
│
└── tests/
    ├── ComponentTests/             # NUnit component tests (in-memory)
    │   ├── CustomWebAppFactory.cs
    │   ├── MembersControllerTests.cs
    │   ├── TrainersControllerTests.cs
    │   ├── TrainingsControllerTests.cs
    │   └── TrainingPlansControllerTests.cs
    └── E2ETests/                   # Playwright API + browser tests
        ├── MembersApiTests.cs
        ├── TrainersApiTests.cs
        ├── TrainingsApiTests.cs
        ├── TrainingPlansApiTests.cs
        └── GymOSE2ETests.cs
```

---

## Running the Application

### 1. Clone the repository

```bash
git clone <repository-url>
cd TIKS
```

### 2. Start the backend

```bash
cd backend
dotnet run
```

Backend runs at: **`http://localhost:5228`**

Swagger UI (API documentation) available at: **`http://localhost:5228/swagger`**

> **Note:** On the first run with an empty database, demo seed data is automatically inserted:
> - 3 trainers, 5 trainings, 6 members, 7 training plans

### 3. Start the frontend

Open a new terminal:

```bash
cd frontend
npm install
npm start
```

Frontend runs at: **`http://localhost:3000`**

> The backend must be running for the frontend to work.

---

## API Endpoints

Base URL: `http://localhost:5228/api`

### Members

| Method | Endpoint | Description |
|---|---|---|
| GET | `/Members` | Get all members |
| GET | `/Members/{id}` | Get member by ID |
| POST | `/Members` | Create a new member |
| PUT | `/Members/{id}` | Update a member |
| DELETE | `/Members/{id}` | Delete a member |

### Trainers

| Method | Endpoint | Description |
|---|---|---|
| GET | `/Trainers` | Get all trainers |
| GET | `/Trainers/{id}` | Get trainer by ID |
| POST | `/Trainers` | Create a new trainer |
| PUT | `/Trainers/{id}` | Update a trainer |
| DELETE | `/Trainers/{id}` | Delete a trainer |

### Trainings

| Method | Endpoint | Description |
|---|---|---|
| GET | `/Trainings` | Get all trainings |
| GET | `/Trainings/{id}` | Get training by ID |
| POST | `/Trainings` | Create a new training |
| PUT | `/Trainings/{id}` | Update a training |
| DELETE | `/Trainings/{id}` | Delete a training |

### TrainingPlans

| Method | Endpoint | Description |
|---|---|---|
| GET | `/TrainingPlans` | Get all training plans |
| GET | `/TrainingPlans/{id}` | Get training plan by ID |
| POST | `/TrainingPlans` | Create a new training plan |
| PUT | `/TrainingPlans/{id}` | Update a training plan |
| DELETE | `/TrainingPlans/{id}` | Delete a training plan |

### Example POST request

```json
POST /api/Members
{
  "firstName": "Nikola",
  "lastName": "Stojanovic",
  "email": "nikola@gmail.com",
  "joinDate": "2024-01-10T00:00:00"
}
```

---

## Running the Tests

### Component Tests (NUnit)

Component tests use **WebApplicationFactory** with an in-memory SQLite database — **no running backend or frontend required**.

```bash
dotnet test tests/ComponentTests/ComponentTests.csproj
```

**60 tests** — 3 tests per CRUD operation (GetAll, GetById, Create, Update, Delete) for each of the 4 controllers.

```
MembersControllerTests       — 15 tests ✅
TrainersControllerTests      — 15 tests ✅
TrainingsControllerTests     — 15 tests ✅
TrainingPlansControllerTests — 15 tests ✅
```

---

### API Tests (Playwright)

API tests send HTTP requests directly to the backend. **Backend must be running** (`dotnet run`).

```bash
# All API tests
dotnet test tests/E2ETests/E2ETests.csproj --filter "FullyQualifiedName~ApiTests"

# Single controller
dotnet test tests/E2ETests/E2ETests.csproj --filter "FullyQualifiedName~MembersApiTests"
dotnet test tests/E2ETests/E2ETests.csproj --filter "FullyQualifiedName~TrainersApiTests"
dotnet test tests/E2ETests/E2ETests.csproj --filter "FullyQualifiedName~TrainingsApiTests"
dotnet test tests/E2ETests/E2ETests.csproj --filter "FullyQualifiedName~TrainingPlansApiTests"
```

**60 tests** — 3 tests per CRUD operation for each of the 4 controllers.

```
MembersApiTests        — 15 tests ✅
TrainersApiTests       — 15 tests ✅
TrainingsApiTests      — 15 tests ✅
TrainingPlansApiTests  — 15 tests ✅
```

---

### E2E Browser Tests (Playwright)

E2E tests launch a real Chromium browser and simulate user interactions through the UI. **Both backend and frontend must be running**.

#### Install Playwright browser (first time only)

```bash
cd tests/E2ETests
dotnet tool install --global Microsoft.Playwright.CLI
playwright install chromium
```

#### Run E2E tests

```bash
dotnet test tests/E2ETests/E2ETests.csproj --filter "FullyQualifiedName~GymOSE2ETests"
```

**12 tests** covering:

```
App_LoadsDashboardByDefault                     ✅
Navigation_ClickMembers_ShowsMembersPage        ✅
Navigation_ClickTrainers_ShowsTrainersPage      ✅
Navigation_ClickTrainings_ShowsTrainingsPage    ✅
Navigation_ClickPlans_ShowsPlansPage            ✅
Members_AddNewMember_AppearsInTable             ✅
Members_EditMember_UpdatesInTable               ✅
Members_DeleteMember_RemovedFromTable           ✅
Trainers_AddNewTrainer_AppearsInTable           ✅
Trainers_DeleteTrainer_RemovedFromTable         ✅
Dashboard_ShowsStatCards                        ✅
Dashboard_AfterAddingMember_UpdatesMemberCount  ✅
```

#### Run all E2E tests at once

```bash
dotnet test tests/E2ETests/E2ETests.csproj
```

---

### Test Summary

| Project | Type | Tests | Requires |
|---|---|---|---|
| ComponentTests | NUnit (in-memory) | 60 | Nothing |
| E2ETests (API) | Playwright HTTP | 60 | Backend running |
| E2ETests (Browser) | Playwright Chromium | 12 | Backend + Frontend running |
| **Total** | | **132** | |

---

## Domain Models

### Member
```json
{
  "id": 1,
  "firstName": "Nikola",
  "lastName": "Stojanovic",
  "email": "nikola@gmail.com",
  "joinDate": "2024-01-10T00:00:00",
  "trainingPlans": []
}
```

### Trainer
```json
{
  "id": 1,
  "firstName": "Marko",
  "lastName": "Petrovic",
  "specialization": "Strength Training",
  "trainings": []
}
```

### Training
```json
{
  "id": 1,
  "name": "Morning Strength",
  "description": "Full body strength workout",
  "durationInMinutes": 60,
  "trainerId": 1
}
```

### TrainingPlan
```json
{
  "id": 1,
  "memberId": 1,
  "trainingId": 1,
  "startDate": "2025-01-01T00:00:00",
  "endDate": "2025-06-30T00:00:00"
}
```

---

## Seed Data

On the first run with an empty database, the following demo data is automatically inserted:

**Trainers:**
- Marko Petrovic — Strength Training
- Ana Jovanovic — Cardio & Endurance
- Stefan Nikolic — CrossFit

**Trainings:**
- Morning Strength (60 min)
- HIIT Cardio (45 min)
- CrossFit WOD (50 min)
- Upper Body Focus (55 min)
- Endurance Run (90 min)

**Members:** Nikola Stojanovic, Jovana Markovic, Petar Ilic, Milica Djordjevic, Aleksa Todorovic, Jelena Pavlovic

**Training Plans:** 7 plans linking the above members and trainings

---

## Quick Start (TL;DR)

```bash
# Terminal 1 — Backend
cd backend && dotnet run

# Terminal 2 — Frontend
cd frontend && npm install && npm start

# Terminal 3 — Component tests (no app needed)
dotnet test tests/ComponentTests/ComponentTests.csproj

# Terminal 3 — API tests (backend required)
dotnet test tests/E2ETests/E2ETests.csproj --filter "FullyQualifiedName~ApiTests"

# Terminal 3 — E2E browser tests (backend + frontend required)
dotnet test tests/E2ETests/E2ETests.csproj --filter "FullyQualifiedName~GymOSE2ETests"
```

---

## Authors

Project developed as part of the **TIKS** course.
