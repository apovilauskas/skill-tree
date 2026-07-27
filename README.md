# Skill Tree

A backend API for tracking personal skill development as a **prerequisite-gated tree**, inspired by RPG skill trees. Skills stay locked until their prerequisites are completed, progress is tracked through logged practice sessions, and the system recommends what to focus on next based on inactivity and unlock impact.

## Description

Skill Tree is an ASP.NET Core Web API that models learning and practice as a directed graph of skills. Each skill has a target (e.g. "50 matches" or "20 hours"), and users log sessions against it over time. Progress isn't a simple ratio — it factors in consistency and streaks, rewarding steady practice over last-minute cramming. Skills can require other skills as prerequisites, and the API enforces a valid, acyclic dependency graph before any relationship is created.

The project was built to practice a clean, layered .NET architecture: Controllers → Services → Repositories, backed by EF Core and PostgreSQL, with manual DTO mapping and unit tests around the core business logic.

## Features

- **Skill CRUD** — create and retrieve skills with custom metrics (matches, hours, chapters, etc.) and targets
- **Prerequisite system** — link skills together with automatic circular-dependency detection before insertion
- **Practice logging** — log dated, quantified sessions against a skill (`SkillLog`)
- **Progress calculation** — a weighted formula combining raw completion, consistency, and streaks (see below)
- **Unlock gating** — `CanStart` endpoint checks whether a skill's prerequisites are satisfied
- **Unlocked / Completed views** — quickly list skills by status
- **Smart recommendations** — surfaces the top 3 skills worth focusing on next, based on how long they've been neglected and how many other skills they unlock
- **Repository Pattern + Service Layer** — clean separation between data access, business logic, and API surface
- **Manual DTO mapping** — explicit mapping via extension methods, no AutoMapper magic
- **Async EF Core + PostgreSQL** — fully async data access throughout
- **xUnit tests** — coverage for the progress formula, plus service-layer logic like `CanStart` gating and circular-dependency detection at multiple graph depths, using Moq to mock the repository layer

### API Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/skills` | Get all skills |
| POST | `/api/skills` | Create a new skill |
| POST | `/api/skills/{skillId}/prerequisites` | Add a prerequisite to a skill |
| GET | `/api/skills/{skillId}/logs` | Get logs for a skill |
| POST | `/api/skills/{skillId}/logs` | Add a practice log to a skill |
| GET | `/api/skills/canStart/{skillId}` | Check if a skill's prerequisites are met |
| GET | `/api/skills/unlocked` | Get all unlocked (in-progress) skills |
| GET | `/api/skills/completed` | Get all completed skills |
| GET | `/api/skills/recommended` | Get the top 3 recommended skills |

## Getting Started
 
### Setup
 
1. Clone the repository
```
   git clone https://github.com/apovilauskas/skill-tree.git
```
2. Configure the database connection
   Open `appsettings.json` and update the connection string:
```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=skill_tree_db;Username=postgres;Password=your_password"
   }
```
3. Apply migrations to create the database schema
```
   dotnet ef database update
```
4. Run the application
   - **Rider**: press the green Run button
   - **CLI**:
```
     dotnet run
```
5. Navigate to `https://localhost:PORT` (check your terminal output for the exact port)
6. Use the included `Requests/test-skills.http` file to try out the endpoints directly from your IDE

## Under the Hood: The Algorithms & Data Model

Four pieces of this project were worth designing carefully rather than reaching for a naive default. Here's how each works.

### 1. The Relationships

Two EF Core relationships do the heavy lifting, and one of them is self-referencing:

- **`Skill` → `SkillLog` (one-to-many)** — a skill has many logs, each log belongs to exactly one skill.
- **`Skill` → `SkillPrerequisite` (self-referencing many-to-many)** — `SkillPrerequisite` is a join entity connecting `Skill` to itself: `SkillId` is the skill being gated, `PrerequisiteId` is the skill gating it. A skill can have many prerequisites, *and* the same skill can be listed as a prerequisite on many other skills.

### 2. Progress Calculation

Progress isn't just "logged amount ÷ target." It's weighted by a **consistency multiplier** so that steady, frequent practice is rewarded over sporadic bursts:

```
Progress = min(100, (v / t) * c * 100)
```

Where:
- **v** — total amount logged across all sessions (sum of `SkillLog.Amount`)
- **t** — the skill's target
- **c** — the consistency multiplier:

```
c = 0.8 + 0.4 * (0.5 * (d1 / d2) + 0.5 * min(1, s / 30))
```

- **d1** — number of distinct days the skill was practiced
- **d2** — number of days since the skill was created (minimum 1, to avoid divide-by-zero on day one)
- **s** — current streak: consecutive days practiced, counted backward from today (or yesterday, so a single missed day doesn't immediately zero it out)

The multiplier **c** ranges from **0.8 to 1.2**. This means:
- Inconsistent practice caps your effective progress at 80% of the raw ratio.
- Practicing most days *and* holding a long streak (30+ days) can boost effective progress up to 20% above the raw ratio.

The result is a progress bar that reflects *how* you practiced, not just how much — two people who log the same total amount can end up with meaningfully different progress if one was consistent and the other wasn't.

### 3. Circular Dependency Detection

Before a prerequisite relationship is created, the service builds an in-memory adjacency graph of all existing skill-to-prerequisite relationships and runs a recursive depth-first search to confirm the new edge won't introduce a cycle:

```csharp
private bool IsValidPrerequisite(int skillId, int prerequisiteId, Dictionary<int, List<int>> graph)
{
    if (!graph.TryGetValue(prerequisiteId, out var prereq)) return true;
    if (prereq.Count < 1) return true;

    foreach (int pId in prereq)
    {
        if (pId == skillId) return false;
        if (!IsValidPrerequisite(skillId, pId, graph)) return false;
    }
    return true;
}
```

Starting from the proposed prerequisite, it walks that skill's own prerequisite chain. If it ever encounters the original skill again, adding the edge would create a loop (e.g. Skill A requires B, B requires C, C requires A), so the request is rejected before it touches the database. Self-referencing prerequisites (`skillId == prerequisiteId`) are also short-circuited before the graph walk even begins.

### 4. Recommendation Scoring

The `/recommended` endpoint doesn't just return the oldest or newest skills — it scores every eligible skill and returns the top 3:

```
score = min(inactiveDays, 15) + (unlockCount * 10)
```

- **Inactivity** — days since the skill was last logged (capped at 15 points, so an abandoned skill doesn't dominate forever)
- **Unlock impact** — +10 points for every other skill that has this one as a prerequisite

This balances two competing signals: skills you've been neglecting, and skills that are "keys" to unlocking more of the tree.

## Tech Stack

**Technologies**
- ASP.NET Core Web API (Controllers)
- Entity Framework Core
- PostgreSQL
- xUnit
- Moq

**Architecture & Patterns**
- Repository Pattern for data access abstraction
- Service Layer for business logic
- async/await throughout the data access and service layers
- EF Core relationships & navigation properties (skill ↔ prerequisites ↔ logs)
- Manual DTO mapping via extension methods

---
Thanks for checking out the project!
