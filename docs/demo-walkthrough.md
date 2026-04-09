# Supreme Model T‑X Demo Walkthrough

This walkthrough shows how to exercise the key platform capabilities end-to-end in roughly **10 minutes**. It is suitable for live demonstrations, recorded screencasts, and self-guided technical evaluations.

---

## Prerequisites

- .NET 9 SDK installed
- At least one AI provider key (Azure OpenAI, Anthropic, Google, or xAI) — or run in demo mode without any keys
- The Supreme Model T‑X API running locally (see [QUICKSTART.md](../QUICKSTART.md))

Start the API:

```bash
cd src/SupremeAI.Api
dotnet run
# → Listening on http://localhost:5100
```

Start the frontend (separate terminal):

```bash
cd src
dotnet run
# → Open http://localhost:5095
```

All `curl` commands below target `http://localhost:5100`. Replace the base URL if your API is hosted elsewhere.

---

## Step 1 — Confirm the API is alive

```bash
curl -s http://localhost:5100/health | jq .
```

Expected response:

```json
{
  "version": "v0.5.0-model-t",
  "uptime": "0.00:00:12",
  "timestamp": "2026-04-09T23:00:00Z"
}
```

**What to highlight:**  
The `status`, `version`, and `uptime` fields confirm the API is running and identify the exact release. This is the endpoint referenced by Kubernetes probes and Azure App Service health checks.

---

## Step 2 — Check the release version

```bash
curl -s http://localhost:5100/version | jq .
```

Expected response:

```json
{
  "version": "v0.5.0-model-t",
  "api": "Supreme Model T‑X API",
  "description": "Powered by Model T‑101 Judgment Engine. Evaluates multiple AI models, estimates confidence, and provides explainable, auditable decisions."
}
```

**What to highlight:**  
Auditors, change-control boards, and procurement teams can hit this endpoint to confirm which release is deployed without requiring a login.

---

## Step 3 — Inspect governance response headers

```bash
curl -sI http://localhost:5100/health
```

Look for these headers in the response:

```
X-Request-Id: 9f4a2b1c3e5d6f7a8b0c1d2e3f4a5b6c
X-Api-Version: v0.5.0-model-t
```

**What to highlight:**  
`X-Request-Id` is a UUID generated server-side for every request. It appears in every log line for that request, enabling complete audit trail reconstruction by filtering on a single value. `X-Api-Version` lets clients detect version mismatches at the header level without calling `GET /version`.

---

## Step 4 — List available AI models

```bash
curl -s http://localhost:5100/api/ai/models | jq '[.[] | {id, provider}]'
```

Expected output (excerpt):

```json
[
  { "id": "gpt-4o",           "provider": "Azure OpenAI" },
  { "id": "claude-3-5-sonnet","provider": "Anthropic"    },
  { "id": "gemini-1-5-pro",   "provider": "Google"       },
  { "id": "grok-2",           "provider": "xAI"          }
]
```

**What to highlight:**  
Supreme Model T‑X supports 13 models across six providers. The Model T‑101 Judgment Engine fans a prompt out to all selected models in parallel and ranks the responses.

---

## Step 5 — Run the Model T‑101 Judgment Engine

This is the core capability. Replace `YOUR_QUERY` with the question you want evaluated.

```bash
curl -s -X POST http://localhost:5100/api/ai/supreme \
  -H "Content-Type: application/json" \
  -d '{
    "query": "What are the key principles of responsible AI deployment in a public-sector context?",
    "modelIds": []
  }' | jq '{winner: .winner, confidence: .confidence, rationale: .rationale}'
```

Expected response structure:

```json
{
  "winner": "gpt-4o",
  "confidence": 0.87,
  "rationale": "GPT-4o produced the most structured and comprehensive response, covering transparency, accountability, and human oversight. It scored highest on clarity and completeness."
}
```

**What to highlight:**
- `winner` — the model that produced the best response for this prompt
- `confidence` — a 0–1 score indicating how clearly one model outperformed the others
- `rationale` — a plain-English explanation of why the winning model was selected

This is what makes Supreme Model T‑X an _auditable_ AI platform: every decision includes a reason.

---

## Step 6 — Observe audit logs

Switch to the terminal where `dotnet run` is running and look at the log output. For each request from Steps 1–5 you will see a pair of structured log lines:

```
info: SupremeAI.Api.Middleware.GovernanceMiddleware[0]
      → POST /api/ai/supreme [9f4a2b1c3e5d6f7a8b0c1d2e3f4a5b6c]

info: SupremeAI.Api.Middleware.GovernanceMiddleware[0]
      ← 200 POST /api/ai/supreme [9f4a2b1c3e5d6f7a8b0c1d2e3f4a5b6c] 1243ms
```

**What to highlight:**  
Every request is logged with method, path, request ID, status code, and latency. The same request ID appears in both lines, making it trivial to correlate any response with its full processing history.

---

## Step 7 — Demonstrate rate limiting

Send more than 20 requests to an AI endpoint within 60 seconds to trigger the `ai-strict` policy:

```bash
for i in $(seq 1 22); do
  curl -s -o /dev/null -w "%{http_code}\n" -X POST http://localhost:5100/api/ai/supreme \
    -H "Content-Type: application/json" \
    -d '{"query":"test","modelIds":[]}'
done
```

After the 21st request you will see:

```
429
```

And the response body:

```json
{"error": "Rate limit exceeded. Please retry after 60 seconds."}
```

The response also includes a `Retry-After: 60` header.

**What to highlight:**  
Rate limiting prevents runaway costs and resource exhaustion. The `Retry-After` header allows well-behaved clients to back off automatically.

---

## Step 8 — Open the Swagger UI

Navigate to [http://localhost:5100/swagger](http://localhost:5100/swagger) in a browser.

**What to highlight:**
- The **API Governance** group at the top contains `GET /health` and `GET /version`
- The **Supreme Model T‑X — Judgment & Governance** group contains the production endpoints
- The **Legacy — Direct Access** group is clearly labelled as not recommended for production use
- Every endpoint has an `X-Request-Id` and `X-Api-Version` header on responses, visible in the Swagger UI

---

## Step 9 — Use the Blazor Frontend

Open [http://localhost:5095](http://localhost:5095).

1. Enter a question in the input field
2. Select one or more models (or leave blank to use the default panel)
3. Click **Run Supreme Evaluation**
4. Observe the ranked model responses, confidence score, and rationale

**What to highlight:**  
The frontend calls `POST /api/ai/supreme` — the same governed endpoint demonstrated in Step 5. Every response it displays is backed by the full judgment engine and audit trail.

---

## Summary — What You Have Demonstrated

| Capability | Evidence |
|------------|----------|
| Liveness / readiness | `GET /health` returns structured status and uptime |
| Version auditability | `GET /version` returns release tag without authentication |
| Request correlation | `X-Request-Id` header on every response; matches log lines |
| Version tracing | `X-Api-Version` header on every response |
| Structured audit logging | Inbound + outbound log lines per request with latency |
| Rate limiting | `ai-strict` policy enforced; `Retry-After` header returned on `429` |
| Multi-model judgment | `POST /api/ai/supreme` fans prompt across models and ranks responses |
| Explainability | `rationale` field explains every judgment decision |
| Swagger documentation | API groups clearly labelled for governance and legacy access |
