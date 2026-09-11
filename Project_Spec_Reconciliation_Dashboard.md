# Customer Account Reconciliation & Billing Dashboard
## Complete Project Specification (v2 — versions verified, AI Engineering track added)

---

## 1. What This Project Is

A web application that manages a portfolio of customer accounts, detects data mismatches between linked records, applies rules-based billing corrections, surfaces flagged/at-risk accounts on a reporting dashboard, and uses an AI layer to summarize flagged cases and surface similar past cases.

The project is deliberately built to mirror real work already done — split-meter/item-classification investigation, the TSYS interest-suppression Excel tool, the Octopus portfolio risk dashboard, and the real TradeStone SOP/knowledge-base documentation — so every feature has a genuine story behind it, not an invented one. It is not a copy of any employer's actual system; it's an original build themed around the same category of problem.

---

## 2. Why This Specific Project

Across the job search conducted in this conversation, the two most frequently repeated gaps across 60+ postings were **.NET Core** and **Angular**, followed by **Azure** and **Docker**. A newer, fast-growing category — AI-assisted development and LLM/RAG integration — also appeared repeatedly (Worldpay/Payrix named Claude Sonnet and LangChain directly; Deutsche Bank, BNY, ACI, and UKG all referenced Copilot, agents, or LLM concepts). This project is built to close the core .NET/Angular/Azure gaps **and** give a real, honest entry point into AI engineering — directly supporting the goal of moving toward AI engineering work over time, not just patching resume gaps.

---

## 3. Technology Stack (versions verified current as of September 2026)

**Backend**
- ASP.NET Core — **.NET 10** (current LTS, released November 2025, supported through November 2028. .NET 8 and .NET 9 both reach end-of-support November 10, 2026 — do not build on either.)
- C#
- RESTful Web API

**Frontend**
- **Angular 22** (current stable major version). Angular remains the dominant framework specifically in enterprise .NET hiring — the target market for this project — even though React leads in overall global market share.

**Database**
- SQL Server
- **Entity Framework Core 10** (EF Core's major version tracks .NET's — EF Core 10 is the correct pairing for .NET 10)

**Cloud & Hosting**
- Microsoft Azure
  - Azure App Service (hosting the API and frontend)
  - Azure SQL Database (hosting the database)
  - **Azure OpenAI Service** (the AI layer — see Section 4, Feature 4)

**DevOps**
- Azure DevOps (CI/CD pipeline, source control integration)
- Git (version control)

**Testing**
- **xUnit v3** (current major version — replaces the older xUnit v2 branch; supports .NET 8+)

**Stretch additions (Week 3–4, if time allows)**
- JWT/OAuth2 authentication (real login system)
- Docker (containerize the finished app)
- Basic Application Insights logging (observability, ties into Azure)
- One async messaging feature (e.g., risk-flag event triggers a logged notification) to genuinely touch the messaging/event-driven pattern

---

## 4. Core Features, Mapped to Real Experience

### Feature 1: Account & Data-Mismatch Detection
Users can look up a customer account and view its linked records. The system checks for mismatches between related data points (e.g., two records that should reference the same underlying entity but don't) and flags them automatically.

**Real story this echoes:** The TradeStone item-classification investigation (a product set up under the wrong department broke downstream approval logic) and the Octopus split-meter case (a physically split property still had its meter mapped to the wrong unit).

### Feature 2: Rules-Based Billing Correction Engine
A configurable rules module takes raw account data and applies a defined set of business rules (e.g., interest-suppression logic, compliance-driven formatting) to produce a corrected, audit-ready record.

**Real story this echoes:** The TSYS structured Excel tool that converted raw financial data into compliant account records — except here, it's real code you wrote yourself, not a tool you used.

### Feature 3: Risk & Casework Dashboard
A reporting view listing accounts by risk/flag status, allowing prioritization of casework — similar to how a real portfolio would be triaged.

**Real story this echoes:** The Octopus Energy 1,000+ account portfolio management, using dashboards to prioritize casework by financial risk.

### Feature 4: AI Case Insight (the AI Engineering feature)
When an account is flagged, an integrated LLM (via Azure OpenAI Service) generates a short, plain-language summary explaining why it was flagged and what the underlying data mismatch actually is — the same kind of translation you'd do manually when explaining a technical issue to a non-technical stakeholder.

Separately, past case notes (a knowledge-base table you populate, mirroring your real TradeStone SOP documentation) are converted into vector embeddings and stored for semantic search. When a new case is flagged, the system searches this knowledge base for similar historical cases and surfaces them — a genuine, if small-scale, Retrieval-Augmented Generation (RAG) pattern.

**Real story this echoes:** The actual documentation and knowledge-transfer work done during the TradeStone ownership period — except this feature automates the "search past cases for a similar pattern" step that used to be manual.

**What this genuinely teaches, honestly:** LLM API integration, prompt engineering (writing the prompt that turns raw flagged-account data into a clear summary), and the basic RAG pattern (embeddings + similarity search + retrieval before generation) — the three concepts named most often across the AI-related postings in this job search.

### Feature 5 (stretch): Authentication
Real login/JWT-based auth protecting the dashboard and API endpoints — closes the security gap named in several postings (Pearson, Deutsche Bank context) and is a normal, expected feature of any real application regardless.

### Feature 6 (stretch): Event Logging
When an account gets flagged, the system publishes an internal event that a separate listener picks up and logs — a small, honest touch of the event-driven/messaging pattern (Kafka/RabbitMQ concept) without needing a full message broker for a portfolio-scale project.

---

## 5. Architecture Overview

```
[Angular 22 Frontend]
        |
        | REST API calls
        v
[ASP.NET Core (.NET 10) Web API] ------> [Azure OpenAI Service]
        |                                  (case summaries + embeddings)
        | Entity Framework Core 10
        v
[SQL Server / Azure SQL Database]
   (includes a knowledge-base table
    storing case notes + their vector
    embeddings for similarity search)
```

- The frontend (Angular) communicates with the backend exclusively through REST endpoints — no direct database access from the client.
- The backend applies business logic (mismatch detection, rules engine) before touching the database.
- The AI layer sits behind its own service class in the backend — the rest of the application only knows it can ask "summarize this case" or "find similar cases" and gets an answer back, so the AI provider could be swapped later without touching the rest of the app.
- Deployed via Azure App Service, with the database hosted on Azure SQL Database.
- Azure DevOps handles build and deployment pipelines from the Git repository.

---

## 6. What You Can Honestly Claim Once It's Built

- Hands-on ASP.NET Core (.NET 10) development
- Entity Framework Core 10
- Angular 22 frontend development
- SQL Server schema design and querying
- Azure deployment (App Service, Azure SQL)
- Azure DevOps CI/CD pipeline configuration
- xUnit v3 testing
- REST API design
- **LLM API integration and prompt engineering (Azure OpenAI Service)**
- **Basic RAG pattern: vector embeddings, semantic search, retrieval before generation**
- (If stretch features completed) JWT/OAuth2 authentication, Docker containerization, basic observability, event-driven pattern

This directly addresses the core requirement of a large share of the postings evaluated in this job search, including several that scored lower specifically because of these exact gaps — and gives a genuine, defensible starting point for pursuing AI engineering roles going forward, not just a line on a resume.

---

## 7. Build Timeline (4 Weeks)

**Week 1:** C#/.NET 10 fundamentals refresh, SQL practice, project scaffolding, one working API endpoint.

**Week 2:** Full CRUD functionality for account records, daily C# coding practice, SQL Server connected and populated with realistic sample data.

**Week 3:** Angular 22 frontend built out, connected to the API; build the AI Case Insight feature (summary generation, then basic RAG search); start using the project in mock interview practice.

**Week 4:** Deploy to Azure (including Azure OpenAI Service), add remaining stretch features if time allows, add the live project link to resume and LinkedIn, run full mock interviews walking through the architecture and decisions — including the AI feature specifically, since that's likely to draw follow-up questions.

---

## 8. Ground Rule for This Project

Every feature built should be something you can explain in detail — what it does, why it's built that way, what trade-offs were made. This project exists specifically to give you real, current, defensible experience to speak from, not just a resume line. This applies especially to the AI feature — be ready to explain what a "prompt" actually contains, why RAG retrieves before generating rather than just asking the model directly, and what happens if the AI service is unavailable.
