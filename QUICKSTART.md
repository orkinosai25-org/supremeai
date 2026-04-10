# SupremeAI – Quick Start Guide

Run **one file** to set everything up and launch the app.

---

## Prerequisites (install these first)

| Tool | Why | Install |
|---|---|---|
| **.NET 9 SDK** | Runs the backend API and Blazor frontend | https://dotnet.microsoft.com/download |
| **Azure CLI** | Provisions cloud resources *(optional – skip if you only want to run locally with your own keys)* | https://aka.ms/installazurecliwindows |

---

## Option A – One-File Quickstart (recommended)

> **This single script does everything**: checks requirements, collects your API keys, optionally provisions Azure, builds both projects, and starts the app.

### Linux / macOS / WSL

```bash
git clone https://github.com/orkinosai25-org/supremeai
cd supremeai
chmod +x quickstart.sh
./quickstart.sh
```

### Windows (PowerShell)

```powershell
git clone https://github.com/orkinosai25-org/supremeai
cd supremeai
# Run each part manually – see Option B below
```

---

## Option B – Step by Step (Windows / manual)

### Step 1 – Get your API keys

You need at least **one** of these to get real AI responses. The app works in demo mode without any keys.

| Key | Where to get it | What it unlocks |
|---|---|---|
| `AZURE_OPENAI_ENDPOINT` + `AZURE_OPENAI_API_KEY` | [Azure Portal → Azure OpenAI](https://portal.azure.com/#create/Microsoft.CognitiveServicesOpenAI) | GPT-4o, GPT-4o-mini, o1-preview, DALL-E 3 |
| `ANTHROPIC_API_KEY` | https://console.anthropic.com | Claude 3.5 Sonnet |
| `GOOGLE_GEMINI_API_KEY` | https://aistudio.google.com/app/apikey | Gemini 1.5 Pro |
| `XAI_GROK_API_KEY` | https://console.x.ai | Grok-2 |

### Step 2 – Configure the backend API

Create a file at `src/SupremeAI.Api/.env` (or set environment variables):

```env
AZURE_OPENAI_ENDPOINT=https://YOUR-RESOURCE.openai.azure.com/
AZURE_OPENAI_API_KEY=your-azure-openai-key

ANTHROPIC_API_KEY=sk-ant-...
GOOGLE_GEMINI_API_KEY=AIza...
XAI_GROK_API_KEY=xai-...
```

> **Tip**: Leave any key blank if you don't have it – those models will return a "not configured" message but won't crash the app.

### Step 3 – Start the backend API

```bash
cd src/SupremeAI.Api
dotnet run
# → Listening on http://localhost:5100
# → Swagger UI at http://localhost:5100/swagger
```

### Step 4 – Start the frontend

Open a **second terminal**:

```bash
cd src
dotnet run
# → Open http://localhost:5095 in your browser
```

---

## Option C – Provision Azure resources automatically

> Skip this if you already have API keys or want to use other providers.

### Linux / macOS / WSL

```bash
cd scripts/azure
chmod +x setup.sh
./setup.sh
```

### Windows (PowerShell)

```powershell
cd scripts\azure
.\setup.ps1
```

**What gets created in Azure (~10-20 min):**

| Resource | Models |
|---|---|
| Azure OpenAI | GPT-4o, GPT-4o-mini, o1-preview, DALL-E 3 |
| Azure AI Foundry Hub + Project | Phi-3.5 Mini, Phi-3 Medium, Llama 3.1 70B, Mistral Large, Command R+, Jais 30B |
| Azure Key Vault | Stores all endpoints and keys automatically |

After provisioning, run the API pointing to your Key Vault:

```bash
export AZURE_KEYVAULT_URI="https://supremeaikv.vault.azure.net/"
cd src/SupremeAI.Api && dotnet run
```

---

## URLs once everything is running

| URL | What it is |
|---|---|
| http://localhost:5095 | SupremeAI web app (Blazor frontend) |
| http://localhost:5100 | Backend API |
| http://localhost:5100/swagger | API documentation / test interface |
| http://localhost:5100/health | Liveness probe — returns status, version, and uptime |
| http://localhost:5100/version | Release metadata — confirms the deployed API version |
| http://localhost:5100/api/ai/models | JSON list of all supported models |

---

## Troubleshooting

**App works but all models show errors?**  
→ Check your API keys in `src/SupremeAI.Api/.env`

**"AZURE_OPENAI_ENDPOINT not set" error?**  
→ You need to either set the environment variable or run the Azure setup script (Option C)

**App shows demo responses even with keys?**  
→ The backend API might not be running. Make sure `dotnet run` is running in `src/SupremeAI.Api/`

**Azure setup fails?**  
→ Make sure you have `Contributor` + `User Access Administrator` roles on your Azure subscription

---

## Directory structure (what was added)

```
supremeai/
├── quickstart.sh               ← Run this to do everything at once
├── QUICKSTART.md               ← This file
├── scripts/azure/
│   ├── setup.sh                ← Azure provisioning (Bash)
│   ├── setup.ps1               ← Azure provisioning (PowerShell)
│   ├── main.bicep              ← Bicep root template
│   └── modules/
│       ├── key-vault.bicep
│       ├── openai.bicep
│       └── ai-foundry.bicep
└── src/
    ├── SupremeAI.Api/          ← Backend API (ASP.NET Core 9)
    │   ├── appsettings.json    ← App settings
    │   ├── .env                ← Your API keys (create this file)
    │   └── ...
    └── ...                     ← Blazor frontend (unchanged)
```
