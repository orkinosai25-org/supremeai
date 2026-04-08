# SupremeAI API Governance

**Release:** `v0.3.2-api-governance`

This document covers the API governance layer introduced in v0.3.2. It is intended for platform operators, security reviewers, and public-sector technical assessors who need to verify that SupremeAI meets production and regulatory standards.

---

## Overview

Every HTTP request processed by the SupremeAI API passes through the governance layer. The layer is responsible for:

- Assigning a unique correlation identifier to each request
- Stamping the API release version on each response
- Logging structured audit entries (inbound + outbound with latency)
- Enforcing rate limits to protect platform stability

Governance endpoints (`GET /health`, `GET /version`) are explicitly excluded from rate limiting so that monitoring and auditing infrastructure remains reliable even when the platform is under load.

---

## Governance Endpoints

### `GET /health`

Liveness and readiness probe. Returns API status, current release version, and process uptime.

**Typical use cases:**
- Kubernetes liveness / readiness probes
- Azure App Service health checks
- Ops dashboards and uptime monitors

**Example response:**

```json
{
  "status": "healthy",
  "version": "v0.3.2-api-governance",
  "uptime": "0.00:04:32",
  "timestamp": "2026-04-08T21:00:00Z"
}
```

**Status codes:**

| Code | Meaning |
|------|---------|
| `200 OK` | API is healthy and accepting requests |

---

### `GET /version`

Returns the current API release tag and a short description of the platform. Clients and auditors can use this endpoint to confirm which release they are talking to without requiring authentication.

**Typical use cases:**
- Change-control and audit evidence
- Procurement and security reviews
- CI/CD pipeline version verification

**Example response:**

```json
{
  "version": "v0.3.2-api-governance",
  "api": "SupremeAI API",
  "description": "Judgment, Benchmarking, and API Governance layer for SupremeAI. Evaluates multiple AI models, estimates confidence, and provides explainable, auditable decisions."
}
```

**Status codes:**

| Code | Meaning |
|------|---------|
| `200 OK` | Version metadata returned successfully |

---

## Governance Middleware

`GovernanceMiddleware` runs on every incoming request before the request reaches a controller.

### Request Tracing — `X-Request-Id`

A UUID is generated for each request and attached as the `X-Request-Id` response header. The same ID is written into every log entry produced for that request so that a complete audit trail can be reconstructed from logs by filtering on a single value.

```
X-Request-Id: 9f4a2b1c3e5d6f7a8b0c1d2e3f4a5b6c
```

### Version Stamping — `X-Api-Version`

The current API release tag is attached as the `X-Api-Version` response header on every response. This allows downstream clients to detect version mismatches without calling `GET /version`.

```
X-Api-Version: v0.3.2-api-governance
```

### Audit Logging

Two structured log entries are emitted for each request:

**Inbound (request received):**
```
→ POST /api/ai/supreme [9f4a2b1c3e5d6f7a8b0c1d2e3f4a5b6c]
```

**Outbound (response sent):**
```
← 200 POST /api/ai/supreme [9f4a2b1c3e5d6f7a8b0c1d2e3f4a5b6c] 312ms
```

Log entries include HTTP method, path, request ID, HTTP status code, and elapsed time in milliseconds. All values that originate from the HTTP request (method, path) are sanitised before writing to prevent log-forging attacks ([CWE-117](https://cwe.mitre.org/data/definitions/117.html)).

---

## Rate Limiting

Rate limiting is implemented using ASP.NET Core's built-in fixed-window rate limiter. All limits are applied per remote IP address.

### Global limit

Applied to every endpoint not covered by a named policy.

| Parameter | Value |
|-----------|-------|
| Window | 60 seconds |
| Permit limit | 100 requests |
| Queue limit | 0 (no queuing) |

### `ai-strict` policy

Applied to AI-generation endpoints (`POST /api/ai/*`). These endpoints invoke external AI providers and carry a higher per-call cost.

| Parameter | Value |
|-----------|-------|
| Window | 60 seconds |
| Permit limit | 20 requests |
| Queue limit | 0 (no queuing) |

### Rate-limit response

When a limit is exceeded the API responds with HTTP `429 Too Many Requests` and a `Retry-After` header:

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 60
Content-Type: application/json

{"error": "Rate limit exceeded. Please retry after 60 seconds."}
```

### Governance endpoints and rate limiting

`GET /health` and `GET /version` are **not** subject to the `ai-strict` policy. They fall under the global limit (100 req/60 s), which is intentionally generous so that monitoring infrastructure remains operational under load.

---

## API Groups

The Swagger/OpenAPI documentation groups endpoints as follows:

| Group | Endpoints | Notes |
|-------|-----------|-------|
| **API Governance** | `GET /health`, `GET /version` | Use for liveness probes and audit confirmation |
| **SupremeAI — Judgment & Governance** | `GET /supreme/models`, `POST /supreme/judge`, benchmark endpoints | Recommended for production and public-sector deployments |
| **SupremeAI — Primary Endpoint** | `POST /api/ai/supreme` | Default endpoint used by the SupremeAI frontend |
| **Legacy — Direct Access** | `POST /api/ai/chat`, `POST /api/ai/image`, `GET /api/ai/models` | Bypasses judgment; not recommended for production use |

---

## Security Notes

| Concern | Mitigation |
|---------|------------|
| Log forging (CWE-117) | All user-supplied values (method, path) are sanitised before writing to logs — ASCII control characters are replaced with spaces |
| Request spoofing | `X-Request-Id` is generated server-side; any client-supplied value is ignored |
| Resource exhaustion | Fixed-window rate limiting enforced per IP on all endpoints; stricter limit on AI-generation routes |
| Audit trail | Every request produces two log lines (inbound + outbound with latency) keyed by `X-Request-Id` |
