# DeskFlow AI

DeskFlow AI is a WPF desktop application for managing customers, projects, tasks, team workload, user accounts, audit logs, and project documents in an internal operations workflow.

The project includes a document AI analysis pipeline with three modes:

- `OpenAI`: uses the real OpenAI Responses API.
- `MockAI`: returns realistic demo analysis in Development without calling OpenAI.
- `RuleBasedFallback`: keeps the app usable when OpenAI is unavailable, not configured, or quota-limited.

## Features

- Customer management with search and audit tracking.
- Project tracking with due dates, status updates, and due-soon overview.
- Task workflow with status, priority, due date, employee assignment, and filters.
- Team management with availability, leave dates, backup employee, and workload summary.
- User management with roles and permission-based UI access.
- Document management with file checks, text extraction, AI processing policy, and AI analysis.
- Document AI metadata: provider, fallback flag, risk level, confidence score, detected issues, recommendations, summary, and risk notes.
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

### Run With OpenAI

Default and Production mode use OpenAI:

```json
"AI": {
  "Provider": "OpenAI"
}
```

Set the API key as a Windows user environment variable. Do not put secrets in source files.

```powershell
[Environment]::SetEnvironmentVariable("OPENAI_API_KEY", "your-api-key-here", "User")
```

Restart your terminal or IDE after setting the variable.

Then run:

```powershell
dotnet run
```

If OpenAI returns an error, for example quota or network failure, DeskFlow AI stores the analysis using `RuleBasedFallback` with `UsedFallback=true`.

## Demo Login Accounts

Seeded accounts:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@deskflow.ai` | `Admin123` |
| Manager | `manager@deskflow.ai` | `Admin123` |
| Staff | `staff@deskflow.ai` | `Admin123` |

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
