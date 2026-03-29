/**
 * SupremeAI — Multi-Model SaaS Application Logic
 *
 * Features:
 *  - Model list with All / Default / Custom selection modes
 *  - Multi-column response grid that adapts to selected model count
 *  - Simulated streaming responses for each selected model
 *  - Gemstone tier badges (Diamond, Emerald, Gold, Silver)
 *  - Dark / Light theme toggle
 *  - Auto-resizing prompt textarea
 */

/* ─── Model Catalogue ─────────────────────────────────────────────────────── */
const MODELS = [
  {
    id: 'gpt-4o',
    name: 'GPT-4o',
    provider: 'OpenAI',
    tier: 'diamond',
    color: '#10A37F',
    initial: 'G',
    defaultSelected: true,
  },
  {
    id: 'claude-3-5-sonnet',
    name: 'Claude 3.5 Sonnet',
    provider: 'Anthropic',
    tier: 'diamond',
    color: '#CC785C',
    initial: 'C',
    defaultSelected: true,
  },
  {
    id: 'gemini-1-5-pro',
    name: 'Gemini 1.5 Pro',
    provider: 'Google',
    tier: 'diamond',
    color: '#4285F4',
    initial: 'Ge',
    defaultSelected: true,
  },
  {
    id: 'llama-3-70b',
    name: 'Llama 3 70B',
    provider: 'Meta',
    tier: 'emerald',
    color: '#0668E1',
    initial: 'L',
    defaultSelected: false,
  },
  {
    id: 'mistral-large',
    name: 'Mistral Large',
    provider: 'Mistral AI',
    tier: 'emerald',
    color: '#FF7000',
    initial: 'M',
    defaultSelected: false,
  },
  {
    id: 'grok-2',
    name: 'Grok-2',
    provider: 'xAI',
    tier: 'gold',
    color: '#1DA1F2',
    initial: 'Gk',
    defaultSelected: false,
  },
  {
    id: 'command-r-plus',
    name: 'Command R+',
    provider: 'Cohere',
    tier: 'gold',
    color: '#D700D7',
    initial: 'Co',
    defaultSelected: false,
  },
  {
    id: 'supreme-llama',
    name: 'Supreme-Llama',
    provider: 'SupremeAI',
    tier: 'diamond',
    color: '#50C878',
    initial: 'S',
    defaultSelected: false,
    comingSoon: true,
  },
];

/* Tier label display names */
const TIER_LABELS = {
  diamond: 'Diamond',
  emerald: 'Emerald',
  gold:    'Gold',
  silver:  'Silver',
};

/* ─── Demo response fragments (simulated streaming) ───────────────────────── */
const DEMO_RESPONSES = [
  `Here's a comprehensive answer to your question. I've analysed the request carefully and identified the key concepts involved.\n\nThe primary considerations are:\n\n1. **Context Understanding** — Breaking down what you're asking into its core components.\n2. **Knowledge Synthesis** — Combining relevant information from my training data.\n3. **Structured Output** — Presenting the answer in a clear, actionable format.\n\nIn summary, the answer depends on several factors, but the most important thing is to approach this systematically and ensure each step is validated before proceeding.`,

  `Great question! Let me walk you through this step by step.\n\nFirst, it's important to understand the underlying principles at play here. The core mechanism involves a multi-layered approach where each component interacts with the others.\n\nFrom my analysis:\n- The primary factor accounts for roughly 60% of the outcome\n- Secondary considerations provide important nuance\n- Edge cases should always be handled explicitly\n\nI'd recommend starting with the fundamentals and building from there. Would you like me to go deeper on any specific aspect?`,

  `Excellent prompt! Here's my take:\n\nThe question touches on an interesting intersection of technology and human experience. At its core, we're looking at how intelligent systems can augment human capability without replacing human judgement.\n\n**Key insight**: The best AI tools are those that remain transparent about their reasoning and limitations.\n\nPractical next steps:\n1. Define your success criteria clearly\n2. Identify measurable outcomes\n3. Iterate based on real feedback\n\nLet me know if you'd like code examples or a deeper conceptual breakdown.`,

  `I've processed your request. Here's a direct, concise answer:\n\nThe solution involves three main components working in concert. Implementing them correctly will give you the results you're looking for.\n\n\`\`\`\ncomponent_a -> transforms input\ncomponent_b -> validates output\ncomponent_c -> handles edge cases\n\`\`\`\n\nThis approach is battle-tested and scales well. The main trade-off is setup complexity vs long-term maintainability — usually worth it for non-trivial systems.`,
];

/* ─── Application State ───────────────────────────────────────────────────── */
const state = {
  mode: 'all',            // 'all' | 'default' | 'custom'
  selectedIds: new Set(MODELS.map((m) => m.id)),
  conversations: [],      // { id, prompt, responses: [{modelId, text, status}] }
  currentConvId: null,
  isDark: true,
};

/* ─── DOM References ──────────────────────────────────────────────────────── */
const $ = (id) => document.getElementById(id);
const modelList    = $('modelList');
const selectedCount= $('selectedCount');
const btnAll       = $('btnAll');
const btnDefault   = $('btnDefault');
const btnCustom    = $('btnCustom');
const promptForm   = $('promptForm');
const promptInput  = $('promptInput');
const sendBtn      = $('sendBtn');
const charCount    = $('charCount');
const responseGrid = $('responseGrid');
const emptyState   = $('emptyState');
const responseHeader = $('responseHeader');
const responseTabs   = $('responseTabs');
const themeToggle  = $('themeToggle');

/* ─── Initialise ──────────────────────────────────────────────────────────── */
function init() {
  renderModelList();
  updateSelectionUI();
  bindEvents();
}

/* ─── Render model list ───────────────────────────────────────────────────── */
function renderModelList() {
  modelList.innerHTML = '';

  MODELS.forEach((model) => {
    const isSelected = state.selectedIds.has(model.id);

    const li = document.createElement('li');
    li.className = `model-item${isSelected ? ' selected' : ''}${model.comingSoon ? ' coming-soon' : ''}`;
    li.setAttribute('role', 'option');
    li.setAttribute('aria-selected', String(isSelected));
    li.setAttribute('tabindex', '0');
    li.dataset.id = model.id;

    li.innerHTML = `
      <div class="model-logo" style="background:${model.color}20;color:${model.color}">
        ${model.initial}
      </div>
      <div class="model-info">
        <div class="model-name">${escHtml(model.name)}${model.comingSoon ? ' <span style="font-size:0.65em;opacity:0.6">(soon)</span>' : ''}</div>
        <div class="model-provider">${escHtml(model.provider)}</div>
      </div>
      <span class="gem-badge gem-${model.tier}">${TIER_LABELS[model.tier]}</span>
      <div class="model-check" aria-hidden="true">
        <svg width="10" height="10" viewBox="0 0 12 12" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="1 6 5 10 11 2"/>
        </svg>
      </div>`;

    li.addEventListener('click', () => toggleModel(model.id));
    li.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); toggleModel(model.id); }
    });

    modelList.appendChild(li);
  });
}

/* ─── Toggle a single model ───────────────────────────────────────────────── */
function toggleModel(id) {
  if (state.selectedIds.has(id)) {
    if (state.selectedIds.size === 1) return; // keep at least one
    state.selectedIds.delete(id);
  } else {
    state.selectedIds.add(id);
  }
  state.mode = 'custom';
  setQuickBtn('custom');
  refreshModelItems();
  updateSelectionUI();
}

/* ─── Quick-select modes ──────────────────────────────────────────────────── */
function applyMode(mode) {
  state.mode = mode;
  setQuickBtn(mode);

  if (mode === 'all') {
    MODELS.forEach((m) => state.selectedIds.add(m.id));
  } else if (mode === 'default') {
    state.selectedIds.clear();
    MODELS.filter((m) => m.defaultSelected).forEach((m) => state.selectedIds.add(m.id));
  }
  // 'custom' — no change to selectedIds, user controls individually

  refreshModelItems();
  updateSelectionUI();
}

function setQuickBtn(mode) {
  [btnAll, btnDefault, btnCustom].forEach((b) => b.classList.remove('active'));
  if (mode === 'all')     btnAll.classList.add('active');
  if (mode === 'default') btnDefault.classList.add('active');
  if (mode === 'custom')  btnCustom.classList.add('active');
}

/* ─── Refresh model item visual state ────────────────────────────────────── */
function refreshModelItems() {
  modelList.querySelectorAll('.model-item').forEach((li) => {
    const sel = state.selectedIds.has(li.dataset.id);
    li.classList.toggle('selected', sel);
    li.setAttribute('aria-selected', String(sel));
  });
}

/* ─── Update badge count & send button state ─────────────────────────────── */
function updateSelectionUI() {
  const n = state.selectedIds.size;
  selectedCount.textContent = state.mode === 'all' ? 'All' : String(n);
  updateSendState();
}

function updateSendState() {
  sendBtn.disabled = promptInput.value.trim() === '' || state.selectedIds.size === 0;
}

/* ─── Bind events ─────────────────────────────────────────────────────────── */
function bindEvents() {
  btnAll.addEventListener('click',     () => applyMode('all'));
  btnDefault.addEventListener('click', () => applyMode('default'));
  btnCustom.addEventListener('click',  () => applyMode('custom'));

  promptInput.addEventListener('input', () => {
    autoResize(promptInput);
    const len = promptInput.value.length;
    charCount.textContent = `${len.toLocaleString()} / 8,000`;
    updateSendState();
  });

  promptInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      if (!sendBtn.disabled) submitPrompt();
    }
  });

  promptForm.addEventListener('submit', (e) => {
    e.preventDefault();
    if (!sendBtn.disabled) submitPrompt();
  });

  themeToggle.addEventListener('click', toggleTheme);
}

/* ─── Auto-resize textarea ────────────────────────────────────────────────── */
function autoResize(el) {
  el.style.height = 'auto';
  el.style.height = Math.min(el.scrollHeight, 200) + 'px';
}

/* ─── Submit prompt ───────────────────────────────────────────────────────── */
function submitPrompt() {
  const prompt = promptInput.value.trim();
  if (!prompt) return;

  const selectedModels = MODELS.filter((m) => state.selectedIds.has(m.id));
  const convId = Date.now().toString(36);

  const conv = {
    id: convId,
    prompt,
    responses: selectedModels.map((m) => ({
      modelId: m.id,
      text: '',
      status: 'loading', // 'loading' | 'done' | 'error'
      tokens: 0,
      ms: 0,
    })),
  };

  state.conversations.push(conv);
  state.currentConvId = convId;

  // Clear prompt
  promptInput.value = '';
  promptInput.style.height = 'auto';
  charCount.textContent = '0 / 8,000';
  sendBtn.disabled = true;

  renderResponses(conv);
  simulateStreaming(conv, selectedModels);
}

/* ─── Render response grid ────────────────────────────────────────────────── */
function renderResponses(conv) {
  // Hide empty state
  if (emptyState) emptyState.style.display = 'none';

  const n = conv.responses.length;
  responseGrid.innerHTML = '';
  responseGrid.className = `response-grid cols-${Math.min(n, 4)}`;

  // Response tabs header
  if (n > 1) {
    responseHeader.hidden = false;
    responseTabs.innerHTML = '';
    conv.responses.forEach((r, i) => {
      const model = MODELS.find((m) => m.id === r.modelId);
      const tab = document.createElement('button');
      tab.className = `response-tab${i === 0 ? ' active' : ''}`;
      tab.setAttribute('role', 'tab');
      tab.textContent = model ? model.name : r.modelId;
      tab.style.setProperty('--model-color', model ? model.color : '#888');
      tab.addEventListener('click', () => {
        responseTabs.querySelectorAll('.response-tab').forEach((t) => t.classList.remove('active'));
        tab.classList.add('active');
      });
      responseTabs.appendChild(tab);
    });
  } else {
    responseHeader.hidden = true;
  }

  // Cards
  conv.responses.forEach((r) => {
    const model = MODELS.find((m) => m.id === r.modelId);
    const card = document.createElement('div');
    card.className = 'response-card';
    card.id = `card-${conv.id}-${r.modelId}`;

    card.innerHTML = `
      <div class="card-header">
        <div class="card-model-logo" style="background:${model ? model.color + '20' : '#888'};color:${model ? model.color : '#888'}">
          ${model ? model.initial : '?'}
        </div>
        <span class="card-model-name">${escHtml(model ? model.name : r.modelId)}</span>
        <div class="card-status loading" id="status-${conv.id}-${r.modelId}"></div>
      </div>
      <div class="card-body thinking" id="body-${conv.id}-${r.modelId}">
        <span class="skeleton skeleton-md"></span>
        <span class="skeleton skeleton-lg"></span>
        <span class="skeleton skeleton-sm"></span>
      </div>
      <div class="card-footer">
        <span class="card-tokens" id="tokens-${conv.id}-${r.modelId}">—</span>
        <div class="card-actions">
          <button class="card-action-btn" title="Copy response" onclick="copyResponse('${conv.id}','${r.modelId}')">Copy</button>
        </div>
      </div>`;

    responseGrid.appendChild(card);
  });

  // Scroll to bottom
  responseGrid.scrollTop = responseGrid.scrollHeight;
}

/* ─── Simulate streaming responses (staggered) ───────────────────────────── */
function simulateStreaming(conv, models) {
  models.forEach((model, idx) => {
    const delay = idx * 220 + Math.random() * 400;
    const demoText = DEMO_RESPONSES[idx % DEMO_RESPONSES.length];
    const startTime = Date.now();

    setTimeout(() => {
      const bodyEl   = $(`body-${conv.id}-${model.id}`);
      const statusEl = $(`status-${conv.id}-${model.id}`);
      const tokensEl = $(`tokens-${conv.id}-${model.id}`);

      if (!bodyEl) return;

      bodyEl.classList.remove('thinking');
      bodyEl.innerHTML = '';

      let charIndex = 0;
      const totalChars = demoText.length;
      const charsPerTick = Math.ceil(totalChars / 60);

      const tick = setInterval(() => {
        charIndex = Math.min(charIndex + charsPerTick, totalChars);
        bodyEl.textContent = demoText.slice(0, charIndex);

        const r = conv.responses.find((rr) => rr.modelId === model.id);
        if (r) r.text = demoText.slice(0, charIndex);

        if (charIndex >= totalChars) {
          clearInterval(tick);
          if (statusEl) { statusEl.classList.remove('loading'); statusEl.classList.add('done'); }
          const elapsed = Date.now() - startTime;
          const tokenCount = Math.round(totalChars / 4);
          if (tokensEl) tokensEl.textContent = `~${tokenCount} tokens · ${(elapsed / 1000).toFixed(1)}s`;
          if (r) { r.status = 'done'; r.tokens = tokenCount; r.ms = elapsed; }
        }

        responseGrid.scrollTop = responseGrid.scrollHeight;
      }, 50);

    }, delay);
  });
}

/* ─── Copy response text ──────────────────────────────────────────────────── */
function copyResponse(convId, modelId) {
  const conv = state.conversations.find((c) => c.id === convId);
  if (!conv) return;
  const r = conv.responses.find((rr) => rr.modelId === modelId);
  if (!r || !r.text) return;
  navigator.clipboard.writeText(r.text).catch(() => {});
}

/* ─── Theme toggle ────────────────────────────────────────────────────────── */
function toggleTheme() {
  state.isDark = !state.isDark;
  document.body.classList.toggle('dark-theme', state.isDark);
  document.body.classList.toggle('light-theme', !state.isDark);
}

/* ─── Utility ─────────────────────────────────────────────────────────────── */
function escHtml(str) {
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

/* ─── Boot ────────────────────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', init);
