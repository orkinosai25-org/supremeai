/**
 * SupremeAI — Multi-Model AI SaaS Platform
 *
 * Features:
 *  - Azure AI Foundry model catalogue with tier badges (Diamond / Emerald / Gold / Silver)
 *  - Checkbox model selection with Select All / Clear All / preset modes
 *  - Content-type pane switcher: Chat · Images · Video · Creative Writing · Code
 *  - Multi-column response grid with per-model tab navigation
 *  - Simulated streaming responses
 *  - Dark / Light theme toggle
 */

/* ─── Model Catalogue — Azure AI Foundry ───────────────────────────────────── */
const MODELS = [
  // ── Diamond tier (frontier / flagship) ──────────────────────────────────
  {
    id: 'gpt-4o',
    name: 'GPT-4o',
    provider: 'Azure OpenAI',
    tier: 'diamond',
    color: '#10A37F',
    initial: 'G4',
    defaultSelected: true,
  },
  {
    id: 'o1-preview',
    name: 'o1 Preview',
    provider: 'Azure OpenAI',
    tier: 'diamond',
    color: '#10A37F',
    initial: 'o1',
    defaultSelected: false,
  },
  {
    id: 'claude-3-5-sonnet',
    name: 'Claude 3.5 Sonnet',
    provider: 'Anthropic',
    tier: 'diamond',
    color: '#CC785C',
    initial: 'C3',
    defaultSelected: true,
  },
  {
    id: 'gemini-1-5-pro',
    name: 'Gemini 1.5 Pro',
    provider: 'Google',
    tier: 'diamond',
    color: '#4285F4',
    initial: 'Gm',
    defaultSelected: true,
  },
  {
    id: 'mistral-large-2407',
    name: 'Mistral Large',
    provider: 'Mistral AI',
    tier: 'diamond',
    color: '#FF7000',
    initial: 'ML',
    defaultSelected: false,
  },
  // ── Emerald tier (high-performance) ─────────────────────────────────────
  {
    id: 'gpt-4o-mini',
    name: 'GPT-4o Mini',
    provider: 'Azure OpenAI',
    tier: 'emerald',
    color: '#10A37F',
    initial: 'Gm',
    defaultSelected: false,
  },
  {
    id: 'phi-3-5-mini',
    name: 'Phi-3.5 Mini',
    provider: 'Microsoft',
    tier: 'emerald',
    color: '#0078D4',
    initial: 'Φ',
    defaultSelected: false,
  },
  {
    id: 'phi-3-medium',
    name: 'Phi-3 Medium 128k',
    provider: 'Microsoft',
    tier: 'emerald',
    color: '#0078D4',
    initial: 'Φ3',
    defaultSelected: false,
  },
  {
    id: 'llama-3-1-70b',
    name: 'Llama 3.1 70B',
    provider: 'Meta',
    tier: 'emerald',
    color: '#0668E1',
    initial: 'L3',
    defaultSelected: false,
  },
  {
    id: 'cohere-command-r-plus',
    name: 'Command R+',
    provider: 'Cohere',
    tier: 'emerald',
    color: '#D700D7',
    initial: 'Co',
    defaultSelected: false,
  },
  // ── Gold tier ────────────────────────────────────────────────────────────
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
    id: 'jais-30b',
    name: 'Jais 30B',
    provider: 'Core42',
    tier: 'gold',
    color: '#8B5CF6',
    initial: 'Ja',
    defaultSelected: false,
  },
  // ── Coming soon ──────────────────────────────────────────────────────────
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

/* Image-generation models */
const IMAGE_MODELS = [
  { id: 'dalle-3',  name: 'DALL-E 3',        provider: 'Azure OpenAI',  tier: 'diamond', color: '#10A37F', initial: 'D3' },
  { id: 'siu',      name: 'Stable Image Ultra', provider: 'Stability AI', tier: 'emerald', color: '#FF4D00', initial: 'SI' },
  { id: 'sdxl',     name: 'Stable Diffusion XL', provider: 'Stability AI', tier: 'gold',  color: '#FF4D00', initial: 'SD' },
];

/* Video-generation models */
const VIDEO_MODELS = [
  { id: 'sora',      name: 'Sora',              provider: 'OpenAI',  tier: 'diamond', color: '#10A37F', initial: 'So', comingSoon: true },
  { id: 'runway-g3', name: 'Runway Gen-3 Alpha', provider: 'Runway',  tier: 'emerald', color: '#00C9A7', initial: 'Rw' },
  { id: 'kling',     name: 'Kling 1.5',          provider: 'Kuaishou', tier: 'gold',   color: '#FF6B35', initial: 'Kl' },
];

/* Tier label display names */
const TIER_LABELS = { diamond: 'Diamond', emerald: 'Emerald', gold: 'Gold', silver: 'Silver' };

/* ─── Demo response fragments ───────────────────────────────────────────────── */
const DEMO_RESPONSES = [
  `Here's a comprehensive answer to your question. I've carefully analysed the request and identified the key concepts involved.\n\nThe primary considerations are:\n\n1. **Context Understanding** — Breaking down what you're asking into its core components.\n2. **Knowledge Synthesis** — Combining relevant information to form a coherent response.\n3. **Structured Output** — Presenting the answer in a clear, actionable format.\n\nIn summary, the answer depends on several factors, but the most important thing is to approach this systematically and validate each step before proceeding.`,

  `Great question! Let me walk you through this step by step.\n\nFirst, it's important to understand the underlying principles at play here. The core mechanism involves a multi-layered approach where each component interacts with the others.\n\nFrom my analysis:\n- The primary factor accounts for roughly 60% of the outcome\n- Secondary considerations provide important nuance\n- Edge cases should always be handled explicitly\n\nI'd recommend starting with the fundamentals and building from there. Would you like me to go deeper on any specific aspect?`,

  `Excellent prompt! Here's my take:\n\nThe question touches on an interesting intersection of technology and human experience. At its core, we're looking at how intelligent systems can augment human capability without replacing human judgement.\n\n**Key insight**: The best AI tools remain transparent about their reasoning and limitations.\n\nPractical next steps:\n1. Define your success criteria clearly\n2. Identify measurable outcomes\n3. Iterate based on real feedback\n\nLet me know if you'd like code examples or a deeper conceptual breakdown.`,

  `I've processed your request. Here's a direct, concise answer:\n\nThe solution involves three main components working in concert. Implementing them correctly will give you the results you're looking for.\n\n\`\`\`\ncomponent_a -> transforms input\ncomponent_b -> validates output\ncomponent_c -> handles edge cases\n\`\`\`\n\nThis approach is battle-tested and scales well. The main trade-off is initial setup complexity vs long-term maintainability — usually worth it for non-trivial systems.`,
];

/* ─── Application State ─────────────────────────────────────────────────────── */
const state = {
  mode: 'all',           // 'all' | 'default' | 'custom'
  selectedIds: new Set(MODELS.map((m) => m.id)),
  conversations: [],
  currentConvId: null,
  activePane: 'chat',    // 'chat' | 'images' | 'video' | 'creative' | 'code'
  selectedImageModel: IMAGE_MODELS[0].id,
  selectedVideoModel: VIDEO_MODELS[1].id,
  creativeStyle: 'story',
  codeLang: 'any',
  codeTask: 'write',
  layoutSplit: true,     // true = side-by-side, false = stacked single tab
  isDark: true,
};

/* ─── DOM References ─────────────────────────────────────────────────────────── */
const $ = (id) => document.getElementById(id);

const modelList      = $('modelList');
const selectedCount  = $('selectedCount');
const btnAll         = $('btnAll');
const btnDefault     = $('btnDefault');
const btnCustom      = $('btnCustom');
const btnSelectAll   = $('btnSelectAll');
const btnClearAll    = $('btnClearAll');
const promptForm     = $('promptForm');
const promptInput    = $('promptInput');
const sendBtn        = $('sendBtn');
const charCount      = $('charCount');
const responseGrid   = $('responseGrid');
const emptyState     = $('emptyState');
const responseHeader = $('responseHeader');
const responseTabs   = $('responseTabs');
const themeToggle    = $('themeToggle');
const contextLabel   = $('contextLabel');
const contextModels  = $('contextModels');

/* ─── Initialise ─────────────────────────────────────────────────────────────── */
function init() {
  renderModelList();
  updateSelectionUI();
  renderImageModelCards();
  renderVideoModelCards();
  bindEvents();
  bindPaneEvents();
  bindChipEvents();
}

/* ─── Render model list (with checkboxes) ────────────────────────────────────── */
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
      <div class="model-checkbox" aria-hidden="true">
        <svg width="9" height="9" viewBox="0 0 12 12" fill="none" stroke="currentColor" stroke-width="2.8" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="1 6 5 10 11 2"/>
        </svg>
      </div>
      <div class="model-logo" style="background:${model.color}20;color:${model.color}">
        ${escHtml(model.initial)}
      </div>
      <div class="model-info">
        <div class="model-name">${escHtml(model.name)}${model.comingSoon ? ' <span style="font-size:0.65em;opacity:0.55">(soon)</span>' : ''}</div>
        <div class="model-provider">${escHtml(model.provider)}</div>
      </div>
      <span class="gem-badge gem-${model.tier}">${TIER_LABELS[model.tier]}</span>`;

    li.addEventListener('click', () => toggleModel(model.id));
    li.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); toggleModel(model.id); }
    });

    modelList.appendChild(li);
  });
}

/* ─── Render image/video model cards ────────────────────────────────────────── */
function renderImageModelCards() {
  const grid = $('imageModelCards');
  if (!grid) return;
  grid.innerHTML = '';
  IMAGE_MODELS.forEach((model) => {
    const card = buildModelCard(model, state.selectedImageModel === model.id);
    card.addEventListener('click', () => {
      state.selectedImageModel = model.id;
      grid.querySelectorAll('.model-card').forEach((c) => c.classList.remove('selected'));
      card.classList.add('selected');
    });
    grid.appendChild(card);
  });
}

function renderVideoModelCards() {
  const grid = $('videoModelCards');
  if (!grid) return;
  grid.innerHTML = '';
  VIDEO_MODELS.forEach((model) => {
    const card = buildModelCard(model, state.selectedVideoModel === model.id);
    card.addEventListener('click', () => {
      state.selectedVideoModel = model.id;
      grid.querySelectorAll('.model-card').forEach((c) => c.classList.remove('selected'));
      card.classList.add('selected');
    });
    grid.appendChild(card);
  });
}

function buildModelCard(model, isSelected) {
  const div = document.createElement('div');
  div.className = `model-card${isSelected ? ' selected' : ''}`;
  div.innerHTML = `
    <div class="model-card-logo" style="background:${model.color}20;color:${model.color}">${escHtml(model.initial)}</div>
    <div class="model-card-name">${escHtml(model.name)}${model.comingSoon ? ' <span style="font-size:0.7em;opacity:0.55">(soon)</span>' : ''}</div>
    <div class="model-card-provider">${escHtml(model.provider)}</div>
    <span class="gem-badge gem-${model.tier}">${TIER_LABELS[model.tier]}</span>`;
  return div;
}

/* ─── Toggle a single model ──────────────────────────────────────────────────── */
function toggleModel(id) {
  const model = MODELS.find((m) => m.id === id);
  if (!model) return;

  if (state.selectedIds.has(id)) {
    if (state.selectedIds.size === 1) return; // keep at least one selected
    state.selectedIds.delete(id);
  } else {
    state.selectedIds.add(id);
  }
  state.mode = 'custom';
  setQuickBtn('custom');
  refreshModelItems();
  updateSelectionUI();
}

/* ─── Quick-select preset modes ─────────────────────────────────────────────── */
function applyMode(mode) {
  state.mode = mode;
  setQuickBtn(mode);

  if (mode === 'all') {
    MODELS.forEach((m) => state.selectedIds.add(m.id));
  } else if (mode === 'default') {
    state.selectedIds.clear();
    MODELS.filter((m) => m.defaultSelected).forEach((m) => state.selectedIds.add(m.id));
  }

  refreshModelItems();
  updateSelectionUI();
}

function setQuickBtn(mode) {
  [btnAll, btnDefault, btnCustom].forEach((b) => b.classList.remove('active'));
  if (mode === 'all')     btnAll.classList.add('active');
  if (mode === 'default') btnDefault.classList.add('active');
  if (mode === 'custom')  btnCustom.classList.add('active');
}

/* ─── Select All / Clear All ─────────────────────────────────────────────────── */
function selectAllModels() {
  MODELS.forEach((m) => state.selectedIds.add(m.id));
  state.mode = 'all';
  setQuickBtn('all');
  refreshModelItems();
  updateSelectionUI();
}

function clearAllModels() {
  // Keep just the first non-comingSoon model selected
  const fallback = MODELS.find((m) => !m.comingSoon);
  state.selectedIds.clear();
  if (fallback) state.selectedIds.add(fallback.id);
  state.mode = 'custom';
  setQuickBtn('custom');
  refreshModelItems();
  updateSelectionUI();
}

/* ─── Refresh model item checkbox visual state ───────────────────────────────── */
function refreshModelItems() {
  modelList.querySelectorAll('.model-item').forEach((li) => {
    const sel = state.selectedIds.has(li.dataset.id);
    li.classList.toggle('selected', sel);
    li.setAttribute('aria-selected', String(sel));
  });
}

/* ─── Update badge + context bar + send button ───────────────────────────────── */
function updateSelectionUI() {
  const n = state.selectedIds.size;
  selectedCount.textContent = state.mode === 'all' ? 'All' : String(n);

  // Context bar
  if (contextModels) {
    contextModels.textContent =
      state.mode === 'all' ? 'All models selected' :
      n === 1              ? '1 model selected' :
                             `${n} models selected`;
  }

  updateSendState();
}

function updateSendState() {
  if (sendBtn) sendBtn.disabled = !promptInput.value.trim() || state.selectedIds.size === 0;
}

/* ─── Bind core events ───────────────────────────────────────────────────────── */
function bindEvents() {
  btnAll.addEventListener('click',     () => applyMode('all'));
  btnDefault.addEventListener('click', () => applyMode('default'));
  btnCustom.addEventListener('click',  () => applyMode('custom'));
  btnSelectAll.addEventListener('click', selectAllModels);
  btnClearAll.addEventListener('click',  clearAllModels);

  promptInput.addEventListener('input', () => {
    autoResize(promptInput);
    const len = promptInput.value.length;
    charCount.textContent = `${len.toLocaleString()} / 8,000`;
    updateSendState();
  });

  promptInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      if (sendBtn && !sendBtn.disabled) submitChatPrompt();
    }
  });

  promptForm.addEventListener('submit', (e) => {
    e.preventDefault();
    if (sendBtn && !sendBtn.disabled) submitChatPrompt();
  });

  themeToggle.addEventListener('click', toggleTheme);

  const layoutToggle = $('btnLayoutToggle');
  if (layoutToggle) {
    layoutToggle.addEventListener('click', () => {
      state.layoutSplit = !state.layoutSplit;
      const currentConv = state.conversations.find((c) => c.id === state.currentConvId);
      if (currentConv) renderResponses(currentConv);
    });
  }
}

/* ─── Bind pane-switching events ─────────────────────────────────────────────── */
function bindPaneEvents() {
  document.querySelectorAll('.content-tab').forEach((tab) => {
    tab.addEventListener('click', () => {
      const pane = tab.dataset.pane;
      switchPane(pane);
    });
  });

  // Image pane form
  const imageForm = $('imagePromptForm');
  if (imageForm) imageForm.addEventListener('submit', (e) => { e.preventDefault(); submitImagePrompt(); });

  // Video pane form
  const videoForm = $('videoPromptForm');
  if (videoForm) videoForm.addEventListener('submit', (e) => { e.preventDefault(); submitVideoPrompt(); });

  // Creative pane form
  const creativeForm = $('creativePromptForm');
  if (creativeForm) creativeForm.addEventListener('submit', (e) => { e.preventDefault(); submitCreativePrompt(); });

  // Code pane form
  const codeForm = $('codePromptForm');
  if (codeForm) codeForm.addEventListener('submit', (e) => { e.preventDefault(); submitCodePrompt(); });

  // Auto-resize for all textarea inputs
  document.querySelectorAll('.prompt-input').forEach((ta) => {
    ta.addEventListener('input', () => autoResize(ta));
  });
}

/* ─── Bind style/task chip events ────────────────────────────────────────────── */
function bindChipEvents() {
  $('creativeStyleChips').querySelectorAll('.style-chip').forEach((chip) => {
    chip.addEventListener('click', () => {
      $('creativeStyleChips').querySelectorAll('.style-chip').forEach((c) => c.classList.remove('active'));
      chip.classList.add('active');
      state.creativeStyle = chip.dataset.style;
    });
  });

  $('langChips').querySelectorAll('.style-chip').forEach((chip) => {
    chip.addEventListener('click', () => {
      $('langChips').querySelectorAll('.style-chip').forEach((c) => c.classList.remove('active'));
      chip.classList.add('active');
      state.codeLang = chip.dataset.lang;
    });
  });

  $('taskChips').querySelectorAll('.task-chip').forEach((chip) => {
    chip.addEventListener('click', () => {
      $('taskChips').querySelectorAll('.task-chip').forEach((c) => c.classList.remove('active'));
      chip.classList.add('active');
      state.codeTask = chip.dataset.task;
    });
  });
}

/* ─── Switch content pane ────────────────────────────────────────────────────── */
function switchPane(pane) {
  state.activePane = pane;

  // Update tabs
  document.querySelectorAll('.content-tab').forEach((tab) => {
    tab.classList.toggle('active', tab.dataset.pane === pane);
    tab.setAttribute('aria-selected', String(tab.dataset.pane === pane));
  });

  // Update panes
  document.querySelectorAll('.app-pane').forEach((p) => {
    p.classList.remove('active');
    p.hidden = true;
  });

  const targetPane = $(`${pane}Pane`);
  if (targetPane) { targetPane.classList.add('active'); targetPane.hidden = false; }

  // Update context label
  if (contextLabel) {
    const icons = {
      chat:     '<svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>',
      images:   '<svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="3" width="18" height="18" rx="2"/><polyline points="21 15 16 10 5 21"/></svg>',
      video:    '<svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polygon points="23 7 16 12 23 17 23 7"/><rect x="1" y="5" width="15" height="14" rx="2"/></svg>',
      creative: '<svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>',
      code:     '<svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="16 18 22 12 16 6"/><polyline points="8 6 2 12 8 18"/></svg>',
    };
    const labels = { chat: 'Chat', images: 'Images', video: 'Video', creative: 'Creative', code: 'Code' };
    contextLabel.innerHTML = `${icons[pane] || ''} ${labels[pane] || pane}`;
  }
}

/* ─── Auto-resize textarea ───────────────────────────────────────────────────── */
function autoResize(el) {
  el.style.height = 'auto';
  el.style.height = Math.min(el.scrollHeight, 180) + 'px';
}

/* ─── Submit chat prompt ─────────────────────────────────────────────────────── */
function submitChatPrompt() {
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
      status: 'loading',
      tokens: 0,
      ms: 0,
    })),
  };

  state.conversations.push(conv);
  state.currentConvId = convId;

  promptInput.value = '';
  promptInput.style.height = 'auto';
  charCount.textContent = '0 / 8,000';
  sendBtn.disabled = true;

  renderResponses(conv);
  simulateStreaming(conv, selectedModels);
}

/* ─── Render response grid ───────────────────────────────────────────────────── */
function renderResponses(conv) {
  if (emptyState) emptyState.style.display = 'none';

  const n = conv.responses.length;
  responseGrid.innerHTML = '';

  const maxCols = state.layoutSplit ? Math.min(n, 4) : 1;
  responseGrid.className = `response-grid cols-${maxCols}`;

  // Response tabs header
  if (n > 1) {
    responseHeader.hidden = false;
    responseTabs.innerHTML = '';
    conv.responses.forEach((r, i) => {
      const model = MODELS.find((m) => m.id === r.modelId);
      const tab = document.createElement('button');
      tab.className = `response-tab${i === 0 ? ' active' : ''}`;
      tab.setAttribute('role', 'tab');
      tab.innerHTML = `
        <span class="tab-dot" style="background:${model ? model.color : '#888'}"></span>
        ${escHtml(model ? model.name : r.modelId)}`;
      tab.addEventListener('click', () => {
        responseTabs.querySelectorAll('.response-tab').forEach((t) => t.classList.remove('active'));
        tab.classList.add('active');
      });
      responseTabs.appendChild(tab);
    });
  } else {
    responseHeader.hidden = true;
  }

  // Response cards
  conv.responses.forEach((r) => {
    const model = MODELS.find((m) => m.id === r.modelId);
    const card = document.createElement('div');
    card.className = 'response-card';
    card.id = `card-${conv.id}-${r.modelId}`;

    card.innerHTML = `
      <div class="card-header">
        <div class="card-model-logo" style="background:${model ? model.color + '20' : '#888'};color:${model ? model.color : '#888'}">
          ${model ? escHtml(model.initial) : '?'}
        </div>
        <span class="card-model-name">${escHtml(model ? model.name : r.modelId)}</span>
        <span class="gem-badge gem-${model ? model.tier : 'silver'}" style="margin-right:0.25rem">${model ? TIER_LABELS[model.tier] : ''}</span>
        <div class="card-status loading" id="status-${conv.id}-${r.modelId}"></div>
      </div>
      <div class="card-body thinking" id="body-${conv.id}-${r.modelId}">
        <span class="skeleton skeleton-md"></span>
        <span class="skeleton skeleton-lg"></span>
        <span class="skeleton skeleton-sm"></span>
        <span class="skeleton skeleton-md"></span>
      </div>
      <div class="card-footer">
        <span class="card-tokens" id="tokens-${conv.id}-${r.modelId}">—</span>
        <div class="card-actions">
          <button class="card-action-btn" title="Copy response" onclick="copyResponse('${conv.id}','${r.modelId}')">Copy</button>
        </div>
      </div>`;

    responseGrid.appendChild(card);
  });

  responseGrid.scrollTop = responseGrid.scrollHeight;
}

/* ─── Simulate streaming responses ──────────────────────────────────────────── */
function simulateStreaming(conv, models) {
  models.forEach((model, idx) => {
    const delay = idx * 240 + Math.random() * 350;
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

/* ─── Submit image / video / creative / code prompts (UI demo) ──────────────── */
function submitImagePrompt() {
  const prompt = $('imagePromptInput').value.trim();
  if (!prompt) return;
  const results = $('imageResults');
  const model = IMAGE_MODELS.find((m) => m.id === state.selectedImageModel) || IMAGE_MODELS[0];
  results.innerHTML = `
    <div class="response-card" style="max-width:520px;margin:0 auto">
      <div class="card-header">
        <div class="card-model-logo" style="background:${model.color}20;color:${model.color}">${escHtml(model.initial)}</div>
        <span class="card-model-name">${escHtml(model.name)}</span>
        <div class="card-status loading" id="img-status"></div>
      </div>
      <div class="card-body" style="display:flex;align-items:center;justify-content:center;min-height:220px;">
        <div style="text-align:center;color:var(--text-muted)">
          <span class="skeleton" style="width:280px;height:200px;display:block;border-radius:var(--radius-md)"></span>
          <p style="margin-top:0.75rem;font-size:0.85rem">Generating image…</p>
        </div>
      </div>
    </div>`;
  setTimeout(() => {
    const st = document.getElementById('img-status');
    if (st) { st.classList.remove('loading'); st.classList.add('done'); }
    const body = results.querySelector('.card-body');
    if (body) body.innerHTML = `<div style="text-align:center;padding:1rem">
      <div style="width:100%;height:200px;background:linear-gradient(135deg,${model.color}22,var(--bg-mid));border-radius:var(--radius-md);display:flex;align-items:center;justify-content:center;color:var(--text-muted);font-size:0.875rem">
        🖼️ Image generated for: "${escHtml(prompt)}"
      </div>
      <p style="margin-top:0.75rem;font-size:0.8rem;color:var(--text-muted)">
        Connect Azure AI Foundry to render real DALL-E 3 images.
      </p>
    </div>`;
  }, 2000);
  $('imagePromptInput').value = '';
}

function submitVideoPrompt() {
  const prompt = $('videoPromptInput').value.trim();
  if (!prompt) return;
  const results = $('videoResults');
  const model = VIDEO_MODELS.find((m) => m.id === state.selectedVideoModel) || VIDEO_MODELS[0];
  results.innerHTML = `
    <div class="response-card" style="max-width:520px;margin:0 auto">
      <div class="card-header">
        <div class="card-model-logo" style="background:${model.color}20;color:${model.color}">${escHtml(model.initial)}</div>
        <span class="card-model-name">${escHtml(model.name)}</span>
        <div class="card-status loading" id="vid-status"></div>
      </div>
      <div class="card-body" style="display:flex;align-items:center;justify-content:center;min-height:220px">
        <div style="text-align:center;color:var(--text-muted)">
          <span class="skeleton" style="width:320px;height:180px;display:block;border-radius:var(--radius-md)"></span>
          <p style="margin-top:0.75rem;font-size:0.85rem">Generating video…</p>
        </div>
      </div>
    </div>`;
  setTimeout(() => {
    const st = document.getElementById('vid-status');
    if (st) { st.classList.remove('loading'); st.classList.add('done'); }
    const body = results.querySelector('.card-body');
    if (body) body.innerHTML = `<div style="text-align:center;padding:1rem">
      <div style="width:100%;height:180px;background:linear-gradient(135deg,#E0115F22,var(--bg-mid));border-radius:var(--radius-md);display:flex;align-items:center;justify-content:center;color:var(--text-muted);font-size:0.875rem">
        🎬 Video generated for: "${escHtml(prompt)}"
      </div>
      <p style="margin-top:0.75rem;font-size:0.8rem;color:var(--text-muted)">Connect Azure AI Foundry to render real video clips.</p>
    </div>`;
  }, 3000);
  $('videoPromptInput').value = '';
}

function submitCreativePrompt() {
  const prompt = $('creativePromptInput').value.trim();
  if (!prompt) return;
  const grid = $('creativeResponseGrid');
  const selectedModels = MODELS.filter((m) => state.selectedIds.has(m.id) && !m.comingSoon).slice(0, 2);
  if (!selectedModels.length) return;

  grid.innerHTML = '';
  grid.className = `response-grid cols-${Math.min(selectedModels.length, 2)}`;

  const fakeConv = { id: 'cr-' + Date.now().toString(36), responses: selectedModels.map((m) => ({ modelId: m.id, text: '', status: 'loading' })) };

  selectedModels.forEach((model) => {
    const card = document.createElement('div');
    card.className = 'response-card';
    card.innerHTML = `
      <div class="card-header">
        <div class="card-model-logo" style="background:${model.color}20;color:${model.color}">${escHtml(model.initial)}</div>
        <span class="card-model-name">${escHtml(model.name)}</span>
        <div class="card-status loading" id="cr-status-${model.id}"></div>
      </div>
      <div class="card-body thinking" id="cr-body-${model.id}">
        <span class="skeleton skeleton-lg"></span>
        <span class="skeleton skeleton-md"></span>
        <span class="skeleton skeleton-lg"></span>
      </div>
      <div class="card-footer">
        <span style="font-size:0.725rem;color:var(--text-muted)">✍️ ${escHtml(state.creativeStyle)} · ${escHtml(model.name)}</span>
        <button class="card-action-btn" onclick="copyText(document.getElementById('cr-body-${model.id}').textContent)">Copy</button>
      </div>`;
    grid.appendChild(card);
  });

  selectedModels.forEach((model, idx) => {
    const demoText = `**${prompt}**\n\n${DEMO_RESPONSES[idx % DEMO_RESPONSES.length]}`;
    setTimeout(() => {
      const body = $(`cr-body-${model.id}`);
      const st   = $(`cr-status-${model.id}`);
      if (!body) return;
      body.classList.remove('thinking');
      body.textContent = demoText;
      if (st) { st.classList.remove('loading'); st.classList.add('done'); }
    }, 1200 + idx * 600);
  });

  $('creativePromptInput').value = '';
}

function submitCodePrompt() {
  const prompt = $('codePromptInput').value.trim();
  if (!prompt) return;
  const grid = $('codeResponseGrid');
  const selectedModels = MODELS.filter((m) => state.selectedIds.has(m.id) && !m.comingSoon).slice(0, 2);
  if (!selectedModels.length) return;

  grid.innerHTML = '';
  grid.className = `response-grid cols-${Math.min(selectedModels.length, 2)}`;

  selectedModels.forEach((model, idx) => {
    const card = document.createElement('div');
    card.className = 'response-card';
    const codeSnippet = `# ${escHtml(prompt)} — ${escHtml(state.codeLang !== 'any' ? state.codeLang : 'code')}\n\ndef solution():\n    # ${escHtml(state.codeTask)}: ${escHtml(prompt)}\n    pass  # Connect Azure AI Foundry for real output`;
    card.innerHTML = `
      <div class="card-header">
        <div class="card-model-logo" style="background:${model.color}20;color:${model.color}">${escHtml(model.initial)}</div>
        <span class="card-model-name">${escHtml(model.name)}</span>
        <div class="card-status loading" id="cd-status-${model.id}"></div>
      </div>
      <div class="card-body thinking" id="cd-body-${model.id}">
        <span class="skeleton skeleton-lg"></span>
        <span class="skeleton skeleton-md"></span>
        <span class="skeleton skeleton-sm"></span>
      </div>
      <div class="card-footer">
        <span style="font-size:0.725rem;color:var(--text-muted)">💻 ${escHtml(state.codeLang !== 'any' ? state.codeLang : 'code')} · ${escHtml(state.codeTask)}</span>
        <button class="card-action-btn" onclick="copyText(document.getElementById('cd-body-${model.id}').textContent)">Copy</button>
      </div>`;
    grid.appendChild(card);

    setTimeout(() => {
      const body = $(`cd-body-${model.id}`);
      const st   = $(`cd-status-${model.id}`);
      if (!body) return;
      body.classList.remove('thinking');
      body.textContent = codeSnippet + '\n\n' + DEMO_RESPONSES[(idx + 3) % DEMO_RESPONSES.length];
      if (st) { st.classList.remove('loading'); st.classList.add('done'); }
    }, 1000 + idx * 500);
  });

  $('codePromptInput').value = '';
}

/* ─── Copy helpers ──────────────────────────────────────────────────────────── */
function copyResponse(convId, modelId) {
  const conv = state.conversations.find((c) => c.id === convId);
  if (!conv) return;
  const r = conv.responses.find((rr) => rr.modelId === modelId);
  if (!r || !r.text) return;
  navigator.clipboard.writeText(r.text).catch(() => {});
}

function copyText(text) {
  if (text) navigator.clipboard.writeText(text).catch(() => {});
}

/* ─── Prompt suggestion (from empty state chips) ─────────────────────────────── */
function setPromptSuggestion(text) {
  if (promptInput) {
    promptInput.value = text;
    autoResize(promptInput);
    charCount.textContent = `${text.length.toLocaleString()} / 8,000`;
    updateSendState();
    promptInput.focus();
  }
}

/* ─── Theme toggle ───────────────────────────────────────────────────────────── */
function toggleTheme() {
  state.isDark = !state.isDark;
  document.body.classList.toggle('dark-theme',  state.isDark);
  document.body.classList.toggle('light-theme', !state.isDark);
}

/* ─── Escape HTML ─────────────────────────────────────────────────────────────── */
function escHtml(str) {
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

/* ─── Boot ────────────────────────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', init);
