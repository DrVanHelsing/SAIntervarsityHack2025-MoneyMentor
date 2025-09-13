# MoneyMentor / FinanceBuddy Multi-App AI Platform

Comprehensive developer & operator guide for the MoneyMentor / FinanceBuddy repository.

This repository contains a .NET MAUI client (FinanceBuddy), an ASP.NET Core API orchestrator (MoneyMentor.ApiOrchestrator), and a shared DTO/domain library (MoneyMentor.Shared). The project demonstrates an LLM-backed personal finance assistant with expense tracking, two-way translation and automatic language detection.

This README is intentionally detailed and organized so new contributors and operators can get productive quickly.

---
CONTENTS
- Project overview
- High-level architecture
- What changed (recent refactor summary)
- Detailed component map (client, server, shared)
- Models & DTOs (concrete shapes and examples)
- Developer quick start (local dev, emulator tips)
- Configuration & secrets
- Running, debugging & common gotchas
- Translation & detection flow (how it works)
- Advice service & prompt engineering notes
- Extending & re-enabling removed speech as an opt-in feature
- Tests, CI and release guidance
- Troubleshooting and logs
- Contribution guidelines
- Changelog (summary of current branch changes)
- License

---
PROJECT OVERVIEW

- FinanceBuddy (Client): .NET MAUI app (targeting .NET 9). Primary responsibilities: show expense list, collect new expenses, and provide a chat UI that sends questions to the Advice API and displays replies.

- MoneyMentor.ApiOrchestrator (Backend): ASP.NET Core (targeting .NET 8). Responsibility: receive advice requests, build contextual prompts (optionally summarizing recent expenses), call Azure OpenAI (via Azure.AI.OpenAI client), and return structured responses. Also provides expense CRUD endpoints.

- MoneyMentor.Shared: DTOs and domain models shared between client and server.

High-level goals:
- Deliver a friendly, contextual, South African-focused personal finance assistant.
- Keep the client simple (typed chat) while providing accurate translated and localized responses.
- Provide deterministic fallbacks when AI is unavailable.

---
WHAT CHANGED (RECENT REFRACTOR SUMMARY)

1. Removed on-device speech recording / SpeechService. Rationale: reduces platform complexity, permissions, and maintenance surface while focusing on translation and AI experience.
2. Added automatic language detection and improved translation pipeline:
   - DetectLanguageWithScoreAsync on the TranslationService returns detected language and confidence
   - Chat UI includes an 'Auto' toggle to detect language and optionally update the UI language picker
3. Replacement of Application.MainPage assignment with CreateWindow override in `App.xaml.cs` to comply with latest MAUI patterns.
4. Added ServiceHelper static to expose built IServiceProvider to support parameterless XAML page constructors used by Shell DataTemplates.
5. ChatPage updated to translate user text to English before sending to backend and translate responses back to user language.

Why this matters: simplified client, consistent translation flow, and better startup/DI compatibility for XAML-created pages.

---
DETAILED COMPONENT MAP

Client (FinanceBuddy):
- App.xaml / App.xaml.cs
  - App.xaml registers resources.
  - App.xaml.cs overrides CreateWindow(Window) to return the AppShell Window.

- MauiProgram.cs
  - Registers DI services, including ApiClient (HttpClient factory) and TranslationService.
  - Registers pages for DI: AppShell (singleton), ExpensesPage, ChatPage (transient).
  - Calls ServiceHelper.Initialize(app.Services) so parameterless ctor resolutions work in XAML scenarios.

- ServiceHelper.cs
  - Lightweight static wrapper around IServiceProvider used only for resolving services in parameterless constructors for XAML DataTemplates.

- Pages/
  - ChatPage.xaml / ChatPage.xaml.cs
    - Chat UI with language picker, Auto toggle, detected language label, and simple chat bubbles.
    - Uses TranslationService to detect language (with score) and to translate text as necessary.
    - Uses ApiClient to call POST /api/chat/message.

  - ExpensesPage.xaml / ExpensesPage.xaml.cs
    - Expense list, add panel and runtime sample seeding (idempotent).
    - Seeds sample expenses on first load (configurable behavior in code).

- Services/
  - ApiClient.cs: typed HttpClient wrapper for calls to the orchestrator (get/add expenses, ask advice).
  - TranslationService.cs: wrapper around Azure Cognitive Services Translator endpoints.
    - TranslateTextAsync(text, from, to)
    - DetectLanguageWithScoreAsync(text) -> (language, score)

Shared (MoneyMentor.Shared):
- DTOs and models (ExpenseEntryDto, AdviceRequestDto, AdviceResponseDto, Expense model, etc.). These are the ground truth for client-server contracts.

Backend (MoneyMentor.ApiOrchestrator):
- Program.cs: configures EF Core (SQL or InMemory fallback), migrations and seeding.
- Services/AdviceService.cs
  - Builds expense context summary, composes system prompt, and calls Azure OpenAI ChatClient.
  - Returns AdviceResponseDto and handles fallbacks.
- Controllers/ChatController.cs: exposes POST /api/chat/message that accepts AdviceRequestDto.
- Controllers/ExpensesController.cs: CRUD endpoints for expenses.
- Data/AppDbContext.cs: models, precision for decimal Amount and seeds.

---
MODELS & DTOs (concrete shapes)

Key DTOs (examples; definitive shapes live in MoneyMentor.Shared):

AdviceRequestDto
- Guid UserId
- string Question
- string Language (two-letter translate code, e.g. "en", "zu")
- List<ExpenseEntryDto>? Expenses

AdviceResponseDto
- string Answer
- string Language

ExpenseEntryDto
- Guid ExpenseId
- Guid UserId
- int CategoryId
- decimal Amount
- string Currency
- string Note
- DateTime ExpenseDate

Example AdviceRequest JSON

```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "question": "Hoe bespaar ek op my maandlike begroting?",
  "language": "af",
  "expenses": [ /* optional expense entries */ ]
}
```

---
DEVELOPER QUICK START (LOCAL)

Prereqs:
- .NET 8 SDK (for API)
- .NET 9 SDK (for MAUI)
- Visual Studio 2022/2023 with MAUI workload (or VS 2022/VS 2023 Preview with MAUI support)
- Android emulator or device for mobile testing (or target Windows)

Steps:
1. Clone the repo: `git clone https://github.com/DrVanHelsing/MoneyMentor` (or your fork)
2. Open solution in Visual Studio.
3. Configure backend secrets (see next section).
4. Start the API (MoneyMentor.ApiOrchestrator) first:
   - `dotnet run --project MoneyMentor.ApiOrchestrator` or run from the IDE.
   - Verify `Now listening on: https://localhost:7001` in logs.
5. Start the MAUI client (FinanceBuddy) in your chosen emulator.
   - If using Android emulator and hosting API locally, the client will attempt local URL `http://10.0.2.2:7000` by default (see ApiClient for base URL resolution logic).

Notes on debugging:
- Clean bin/obj and uninstall the app from the emulator if you see stale FastDev deployment issues.
- If client pages are created by Shell DataTemplates, XAML needs parameterless constructors (we use ServiceHelper to resolve DI there).

---
CONFIGURATION & SECRETS (ENV VARS & appsettings)

Keys & settings are read from `MoneyMentor.ApiOrchestrator/appsettings.json` (and environment-specific files) or environment variables.

Important values:
- `AzureOpenAI:Endpoint` - e.g. `https://<your-resource>.openai.azure.com/`
- `AzureOpenAI:DeploymentName` - model/deployment id (e.g. `gpt-4o-mini`)
- `AzureOpenAI:ApiKey` - API key for Azure OpenAI
- `ConnectionStrings:DefaultConnection` - SQL Server connection (optional; InMemory fallback used when absent)

Recommended practices:
- Use `dotnet user-secrets` locally or environment variables for keys.
- For production, use Azure App Configuration, Key Vault, or environment variables in the hosting environment.

Example `appsettings.Development.json` snippet:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://<your-resource>.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini",
    "ApiKey": "<YOUR_KEY>"
  }
}
```

---
TRANSLATION & AUTO-DETECTION FLOW (DETAILED)

1. The chat UI contains a language Picker and an Auto switch.
2. When Auto is ON and the user sends a message:
   a. The client calls `TranslationService.DetectLanguageWithScoreAsync(userText)`.
   b. Service returns language code (e.g. `zu`) and confidence score (0..1).
   c. If confidence >= threshold (default in current code: 0.5) the client updates the language picker and records `currentLanguage`.
3. Before sending to API, if detected language != `en`, the client translates the user text to English: `TranslateTextAsync(text, detected, "en")`.
4. The client sends AdviceRequestDto with `Language` set to the user's two-letter translate code (the LLM is instructed via system message to reply in that language).
5. Backend calls Azure OpenAI and returns `Answer` in English (if the model responds in English) or in the requested language. The client will perform a return translation to the user's language if necessary.

Edge cases handled:
- If detection score is low, the client will not forcibly change the selected language but will still show a detection attempt and score.
- If translation calls fail, the original text is used as a fallback to maintain UX.

---
ADVICE SERVICE & PROMPT NOTES

- AdviceService constructs a system-level prompt that enforces form, tone, and localization. It also builds a small expense summary if expense context is provided.
- Temperature is tuned low (0.3) for consistency.
- Max tokens currently set to ~700 to avoid runaway responses.
- Deterministic fallback messages exist per language to present the user with core principles when the AI service is not configured or fails.

System prompt guidance highlights:
- South African financial context (TFSA, RA, SARS, UIF, JSE, etc.)
- Structured response sections (Summary, Key Considerations, Steps, Example, Risks, Disclaimer)
- Always include an educational disclaimer that the assistant is not a tax or licensed financial advisor
- Respectful handling of culturally sensitive topics such as "Black Tax"
- Avoid special invalid characters in responses; friendly conversational tone

Security note: Avoid logging or persistently storing raw prompts that contain PII.

---
RE-ENABLING DEVICE SPEECH (OPTIONAL)

If you want to reintroduce on-device speech later, consider implementing as an opt-in feature behind a feature flag and DI registration. Suggested approach:

1. Create an ISpeechService interface and a platform-specific implementation (Android/iOS) using the native speech SDK or a cross-platform wrapper.
2. Register speech implementation in MauiProgram conditionally:
   ```csharp
   if (config.GetValue<bool>("Features:EnableDeviceSpeech"))
       builder.Services.AddSingleton<ISpeechService, PlatformSpeechService>();
   else
       builder.Services.AddSingleton<ISpeechService, NoOpSpeechService>();
   ```
3. Guard UI: only show mic button when `ISpeechService` is present and the feature flag is enabled.
4. Ensure microphone runtime permission logic is in place before starting recognition.

This keeps speech optional and avoids shipping audio permissions by default.

---
TESTING, CI & RELEASE

Testing recommendations:
- Unit tests for AdviceService prompt builder and fallback behavior.
- Tests for TranslationService request/response mapping (mock HttpClient using HttpMessageHandler).
- Integration test for ExpensesController using InMemory provider.

CI Suggestions (GitHub Actions):
- Build matrix for net8.0 (API) and net9.0 (MAUI) as applicable. Note: MAUI app builds generally require macOS for iOS targets.
- Run unit tests, run static analysis and create artifacts for API image.

Release notes:
- API can be containerized (Linux container on Azure App Service or Azure Container Instances). Set secrets as environment variables during deployment.

---
TROUBLESHOOTING & COMMON LOG MESSAGES

- "Azure OpenAI not configured" — check env vars or appsettings for `AzureOpenAI:Endpoint` and `AzureOpenAI:ApiKey`.
- "Failed to initialize Azure OpenAI chat client" — inspect logs for thrown exceptions during AdviceService ctor; ensure network access and correct endpoint format.
- Stalled splash or app exit on device — clean `bin/obj`, uninstall app from device/emulator, rebuild and redeploy.
- FastDeploy / Hot Reload stale assembly issues — fully stop debugging, uninstall the app from device, and rebuild.
- Language detection not updating picker — ensure Auto is toggled on and the message text is long enough to detect; detection uses Azure Translator `/detect` endpoint and can return low confidence for very short phrases.

Logs to review:
- MAUI app console output (device logs)
- API logs (AdviceService initialization, request timing)
- Translation API request/response logs (TranslationService writes debug info on exceptions only)

---
EXTENSION POINTS (WHERE TO ADD NEW FEATURES)
- Add per-user authentication and separate DB tenancy (AppDbContext -> scoped context with user ID lookup).
- Add conversation persistence to provide multi-turn context to AdviceService.
- Add SignalR streaming for partial/streamed responses.
- Add offline caching (SQLite) and sync logic for expense collection when offline.

---
CONTRIBUTION GUIDELINES
- Fork -> feature branch naming: `feature/<short-desc>` or `fix/<short-desc>`
- Keep commits small and focused with clear messages (use the commit message template included in repo).
- Open PR: include testing steps, what to validate in both API and client.
- Maintain code style: C# 10/11 conventions in server (net8) and C# 13 on MAUI where used.

---
CHANGELOG / CURRENT BRANCH NOTES
- Removed SpeechService + mic UI and related platform DI.
- Added auto-detect language feature in ChatPage with confidence-based picker sync.
- TranslationService extended to return detection confidence.
- Added ServiceHelper and parameterless ctors to support Shell DataTemplate instantiation with DI.
- Replaced Application.MainPage setter with CreateWindow override to address MAUI obsolescence warnings.
- Updated AdviceService prompt to enforce style and fallbacks.

---
LICENSE
MIT (see root LICENSE file if present).

---
CONTACT / HELP
- For repo questions, open an issue. Tag with `area:client` or `area:server` as appropriate.
- For quick troubleshooting, include:
  - Platform (Windows / macOS), SDK versions (`dotnet --info`)
  - Steps to reproduce
  - Relevant logs (attach device logcat output for Android issues)

---

Thank you — this README is intended as a living document. Please request updates if you want more detail in any section.

