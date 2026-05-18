# DeskFlow AI

DeskFlow AI is a WPF desktop application for internal operations teams. It brings customer records, project delivery, task workflow, kanban tracking, team workload, notifications, audit logs, and AI-assisted document review into one desktop workspace.

The goal is to show how a company can manage delivery work and review project documents with a controlled AI workflow. The app can run with real OpenAI integration in production-like mode, or with realistic Mock AI responses in development/demo mode without using an API key.

## What This Project Does

DeskFlow AI helps an operations team answer these questions:

- Which customers and projects are active?
- Which tasks are open, blocked, overdue, or ready for review?
- Who is working on each project?
- Which project documents need file checks, text extraction, AI analysis, approval, or review?
- What risks, compliance issues, and recommendations did AI detect?
- What changed recently in the workspace?
- What does the overall workload and delivery health look like?

The project includes a document AI analysis pipeline with three modes:

- `OpenAI`: uses the real OpenAI Responses API.
- `MockAI`: returns realistic demo analysis in Development without calling OpenAI.
- `RuleBasedFallback`: keeps the app usable when OpenAI is unavailable, not configured, or quota-limited.

## GitHub Description

Suggested repository description:

```text
WPF desktop operations dashboard with customer/project/task workflow, kanban board, notifications, audit logs, and OpenAI/Mock AI document risk analysis.
```

Suggested topics:

```text
wpf dotnet entity-framework sql-server openai desktop-app operations-dashboard document-analysis kanban
```

## Screenshots

Add screenshots under `docs/screenshots/` and reference them here:

| Screen | Suggested file |
| --- | --- |
| Dashboard + customers | `docs/screenshots/dashboard-customers.png` |
| Project details + team/timeline | `docs/screenshots/project-details.png` |
| Kanban board | `docs/screenshots/kanban-board.png` |
| Document AI analysis | `docs/screenshots/document-ai-analysis.png` |
| Overview metrics | `docs/screenshots/overview-dashboard.png` |

## Features

- Customer management with search and audit tracking.
- Project tracking with due dates, status updates, team notes, timeline activity, and due-soon overview.
- Task workflow with status, priority, due date, employee assignment, filters, dependencies, and a kanban board.
- Team management with availability, leave dates, backup employee, and workload summary.
- User management with roles and permission-based UI access.
- Dashboard notifications for overdue work, AI review readiness, approval needs, and project notes.
- Overview insights for project progress, task completion, workload, overdue heatmap, team productivity, and AI usage metrics.
- Document management with file checks, text extraction, AI processing policy, and AI analysis.
- Document AI metadata: provider, fallback flag, risk level, risk score, compliance status, policy violations, confidence score, detected issues, recommendations, summary, and risk notes.
- Seeded demo text documents for Low, Medium, and High risk Mock AI analysis.

## Tech Stack

- .NET 8
- WPF
- Entity Framework Core
- SQL Server
- OpenAI Responses API
- PdfPig for PDF text extraction

## Getting Started

### Requirements

- Windows
- .NET 8 SDK
- SQL Server running locally
- A database connection that matches `appsettings.json`

Default connection string:

```json
"DeskFlowDb": "Server=localhost;Database=DeskFlowAI;Trusted_Connection=True;TrustServerCertificate=True;"
```

The app runs EF Core migrations automatically on startup through `DatabaseInitializer`.

### Build

```powershell
dotnet build
```

### Run Tests

The repository includes a lightweight test runner that does not require external test NuGet packages.

```powershell
dotnet run --project tests\DeskFlowAI.Tests\DeskFlowAI.Tests.csproj
```

Current coverage focuses on:

- role-to-permission mapping,
- Admin/Manager/Staff permission boundaries,
- Mock AI high-risk and medium-risk keyword behavior,
- Mock AI provider/fallback behavior.

### Run In Development Mock AI Mode

Use this mode for demos and local development when you do not want to call the real OpenAI API.

```powershell
$env:DOTNET_ENVIRONMENT="Development"
dotnet run
```

Development loads `appsettings.Development.json`:

```json
{
  "AI": {
    "Provider": "Mock"
  }
}
```

Expected document analysis result:

- Provider: `MockAI`
- UsedFallback: `false`
- Risk: `Low`, `Medium`, or `High`
- No OpenAI quota or API key required

This is the recommended mode for GitHub demos, school/project presentations, and local testing because it produces realistic AI-looking output without sending data to an external service.

### Run With OpenAI

Default and Production mode use OpenAI:

```json
"AI": {
  "Provider": "OpenAI"
}
```

Set the API key as a Windows user environment variable. Do not put secrets in source files, `appsettings` files, screenshots, commits, or issue comments.

```powershell
[Environment]::SetEnvironmentVariable("OPENAI_API_KEY", "your-api-key-here", "User")
```

Restart your terminal or IDE after setting the variable.

Then run:

```powershell
dotnet run
```

If OpenAI returns an error, for example quota or network failure, DeskFlow AI stores the analysis using `RuleBasedFallback` with `UsedFallback=true`.

### AI Provider Behavior

| Mode | Provider shown in result | Uses API key | UsedFallback |
| --- | --- | --- | --- |
| Development Mock | `MockAI` | No | `false` |
| Production/OpenAI success | `OpenAI` | Yes | `false` |
| OpenAI unavailable/quota error | `RuleBasedFallback` | Attempted | `true` |

Mock AI is intentionally not marked as fallback. It is a controlled demo provider.

## Demo Login Accounts

Seeded accounts:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@deskflow.ai` | `Admin123` |
| Manager | `manager@deskflow.ai` | `Admin123` |
| Staff | `staff@deskflow.ai` | `Admin123` |

## Recommended Demo Flow

1. Sign in as Admin.
2. Select a customer.
3. Select a project.
4. Open `Tasks` and show status, priority, due date, assignee, filters, and dependencies.
5. Open `Board` and show the kanban lifecycle.
6. Open `Project details`, add a project note, and show how it creates a notification for the project team.
7. Open `Documents`, select one of the seeded `.txt` files, run file check, extract text, and analyze.
8. Review provider, risk score, compliance status, policy violations, summary, and recommendations.
9. Open `Overview` to show progress, workload, overdue heatmap, productivity, and AI usage.

For a more detailed presenter script, see [`docs/DEMO.md`](docs/DEMO.md).

## Document AI Flow

1. Sign in as Admin or Manager.
2. Select a customer and project.
3. Add or select a document.
4. Set AI processing policy:
   - `Internal Only`: internal rule-based analysis only.
   - `External AI Allowed`: OpenAI or MockAI can analyze the document.
   - `Needs Approval`: requires approval before external AI.
   - `Blocked`: AI pipeline is blocked.
5. Run file check.
6. Extract text.
7. Click Analyze.

The analysis result shows:

- AI status
- Provider
- Used fallback indicator
- Risk level
- Confidence score
- Summary
- Risk notes
- Recommendations
- Detected issues

The seed data includes demo `.txt` documents that can be analyzed immediately after startup:

| Demo File | Expected Mock Risk |
| --- | --- |
| `low-risk-handoff.txt` | Low |
| `medium-risk-approval.txt` | Medium |
| `high-risk-delivery.txt` | High |

## Mock AI Risk Rules

Development Mock AI detects risk from document text and notes:

| Risk | Keywords |
| --- | --- |
| High | `delay`, `penalty`, `breach`, `termination`, `overdue`, `missing`, `failed` |
| Medium | `refund`, `revision`, `pending`, `approval`, `dependency` |
| Low | No configured risk keywords detected |

Mock AI is not a fallback. It is a controlled demo provider, so `UsedFallback=false`.

## Security Notes

- Never commit API keys.
- Keep `OPENAI_API_KEY` in environment variables.
- `appsettings.json` only stores the environment variable name and provider settings.
- Test document output is ignored by Git through `TestDocuments/`.

## Current Status

The current implementation supports local SQL Server persistence, role-based desktop workflows, document text extraction, configurable AI providers, OpenAI integration, Mock AI demos, and fallback-safe analysis.
