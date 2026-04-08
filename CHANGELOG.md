# Changelog

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
