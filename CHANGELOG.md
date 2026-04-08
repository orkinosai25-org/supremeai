# Changelog

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
