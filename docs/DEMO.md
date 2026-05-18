# DeskFlow AI Demo Guide

This guide is a short presenter script for showing DeskFlow AI in a clear order.

## One-Minute Explanation

DeskFlow AI is an internal operations desktop app. It helps a company manage customers, projects, tasks, teams, documents, and AI review from one place.

The main value is not only CRUD screens. The app shows an operations workflow:

- customers have projects,
- projects have tasks and documents,
- tasks move through a lifecycle and kanban board,
- project notes notify the team,
- documents go through file check, text extraction, AI analysis, approval, and review,
- overview metrics show delivery health.

For demos, Development mode uses Mock AI so the AI analysis flow works without an API key or OpenAI quota.

## Recommended Setup

Run the app in Development mode:

```powershell
$env:DOTNET_ENVIRONMENT="Development"
dotnet run
```

Use the seeded admin account:

```text
admin@deskflow.ai
Admin123
```

Development mode should show AI results with:

- Provider: `MockAI`
- UsedFallback: `false`
- Risk: `Low`, `Medium`, or `High`

## Demo Flow

### 1. Sign In

Sign in as Admin and briefly explain the dashboard:

- open tasks,
- active projects,
- overdue work,
- AI queue,
- notifications,
- AI policy summary.

Presenter line:

> This is the operations dashboard. It gives managers a quick view of project workload, document AI status, and items that need attention.

### 2. Select A Customer

Go to `Customers`, select a customer such as `BluePeak Logistics` or `Northwind Consulting`.

Show:

- customer list,
- search field,
- audit log.

Presenter line:

> Customers are the starting point. Selecting a customer filters the project workspace and keeps the workflow focused.

### 3. Select A Project

Open `Projects`, select an active project.

Show:

- project list,
- selected customer,
- project status,
- due date,
- project team,
- project note field,
- timeline.

Presenter line:

> A project connects tasks, team members, documents, notes, and timeline activity in one operational context.

### 4. Show Tasks

Open `Tasks`.

Show:

- task status,
- priority,
- assignee,
- due date,
- blocked/dependency information,
- filters.

Presenter line:

> Tasks are not just records. They carry workflow state, priority, ownership, due dates, and dependencies.

### 5. Show Kanban Board

Open `Board`.

Show columns such as:

- To Do,
- In Progress,
- Review,
- Blocked,
- Done.

Presenter line:

> The board makes the task lifecycle easier to understand visually, especially for project managers.

### 6. Add A Project Note

Return to the project detail area and add a short note, for example:

```text
Please review the delivery risks before the next customer update.
```

Then show the notification area.

Presenter line:

> Project managers can leave notes for the selected project team. The app creates notifications for the people working on that project.

### 7. Analyze A Document

Open `Documents`.

For a reliable demo, select one of:

- `low-risk-handoff.txt`
- `medium-risk-approval.txt`
- `high-risk-delivery.txt`

Run:

1. file check,
2. text extraction,
3. analyze.

Show:

- provider,
- risk level,
- risk score,
- compliance status,
- policy violations,
- summary,
- recommendations,
- detected issues.

Presenter line:

> The AI flow reviews project documents and produces risk, compliance, summary, and recommendation fields. In Development mode this uses Mock AI, so the demo does not depend on quota or secrets.

### 8. Mark AI Review

If the document analysis is ready, mark it reviewed.

Presenter line:

> AI output is treated as a review step. The system keeps a distinction between analysis being ready and a human actually reviewing it.

### 9. Show Overview

Open `Overview`.

Show:

- project progress,
- task completion,
- AI usage,
- workload,
- overdue heatmap,
- team productivity,
- AI usage metrics.

Presenter line:

> Overview turns the operational data into management-level signals: delivery progress, workload, overdue risk, team productivity, and AI usage.

## Screenshot Checklist

Capture these screens for GitHub:

1. Dashboard with notifications.
2. Customers and audit log.
3. Project details with team and timeline.
4. Tasks list.
5. Kanban board.
6. Document AI analysis result.
7. Overview metrics.

Save screenshots in:

```text
docs/screenshots/
```

Use clear file names:

```text
dashboard-customers.png
project-details.png
kanban-board.png
document-ai-analysis.png
overview-dashboard.png
```

