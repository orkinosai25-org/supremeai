# Changelog

## v0.5.0-model-t

- **Full Product Rebrand** — Supreme Model T‑X naming system applied across all UI, API, and documentation
- **UI / Frontend** — Updated header, loading screen, and page titles to "Supreme Model T‑X"; added "T‑Series Autonomous Governance Models" tagline to top bar; Governance banner now reads "Powered by Model T‑101 Judgment Engine"; all user-visible "SupremeAI" text replaced with "Supreme Model T‑X" across Home, Pricing, Chat, and Brain pages
- **API layer** — Bumped `GovernanceMiddleware.ApiVersion` to `v0.5.0-model-t`; updated doc-comment to reference "Model T‑101 Judgment Engine"; `X-Api-Version: v0.5.0-model-t` on all responses
- **Documentation** — Replaced "SupremeAI" with "Supreme Model T‑X" in README, api-governance.md, demo-walkthrough.md, epics.md, user-stories.md, pitch-deck.md, and issues/README.md; added `docs/brand.md` explaining the full T‑Series hierarchy (T‑101 through T‑X)
- **Branding Assets** — Added `branding/model-t/` with four SVG logo variants: primary (dark-bg), monochrome, dark-mode, and T‑Series shield sub-brand logo
- **Zero breaking changes** — All API endpoints, routing paths, and .NET namespaces unchanged

## v0.4.0-chat-nav

- Add **Chat** page (`/chat`): model-selector dropdown, multi-turn conversation UI with typing indicator, per-message latency/token metadata, keyboard shortcut (Enter to send, Shift+Enter for newlines), and clear-conversation button; falls back to demo mode when API is unreachable
- Fix **NavMenu**: remove boilerplate Counter and Weather links; add Chat and Pricing navigation items
- Improve **CI/CD** (`main_supremeai.yml`): trigger on `pull_request` to `main` in addition to `push`; add `dotnet restore` step; add test discovery step; skip publish/upload/deploy on PRs
- Bump `GovernanceMiddleware.ApiVersion` to `v0.4.0-chat-nav`

## v0.3.3-supremeai-first-ui

- Add Blazor WebAssembly frontend: `Home` landing page with hero section, feature cards, live model showcase, and call-to-action navigation
- Add `Brain` page: multi-model Supreme Evaluation UI — model selector chips, prompt input, animated run button, supreme-answer card, and per-model result grid with score bars
- Add `Pricing` page: plan cards (Free, Gold, Emerald, Diamond, Enterprise) with feature lists, add-ons grid, and full comparison table; dark/light theme toggle
- Add `NotFound` page for unmatched routes
- Add `AiApiService` (Blazor service): typed HTTP client wrapping `POST /api/ai/supreme` with demo-mode fallback
- Add `SubscriptionService` (Blazor service): in-memory plan selection and tier management
- Add `ModelCatalogue` (shared models): chat-model metadata (provider, color, initial, tier) and demo responses
- Add `SubscriptionPlans` / `PlanTier` / `ModelTier` models for pricing and access-control data
- Bump `GovernanceMiddleware.ApiVersion` to `v0.3.3-supremeai-first-ui`

## v0.3.2-api-governance

- Add API Governance Layer: `GovernanceMiddleware` attaches `X-Request-Id` and `X-Api-Version` response headers and emits structured audit log entries (inbound + outbound with latency) for every request
- Add `GovernanceController` with `GET /health` (liveness probe returning status, version, and uptime) and `GET /version` (API release metadata) endpoints
- Add rate limiting: global fixed-window limit (100 req/60 s per IP) across all endpoints; stricter `ai-strict` policy (20 req/60 s per IP) applied to AI-generation endpoints (`POST /api/ai/*`)
- Update Swagger/OpenAPI description to include **API Governance** group and reflect the new release version

## v0.3.0-benchmarked

- Add Benchmark & Publishing Layer (v3): models, store, service, and controller
- Fix: remove unused CancellationToken in BenchmarkStore.ReadAllInner; sanitize benchmarkId before logging

## v0.1.0-judgment-engine

- Implement SupremeAI Judgment Engine v1
