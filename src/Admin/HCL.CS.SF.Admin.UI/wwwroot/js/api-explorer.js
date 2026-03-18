/* global HCL, ApiExplorerData */

(function () {
  if (typeof ApiExplorerData === 'undefined') {
    // Data registry is expected to be loaded before this script.
    console.error('ApiExplorerData not found. Did you include api-explorer-data.js?');
    return;
  }

  // ---------------------------------------------------------------------------
  // Next.js data port (types removed)
  // ---------------------------------------------------------------------------
  const endpointSchemas = ApiExplorerData.endpointSchemas;
  const ApiRoutes = ApiExplorerData.ApiRoutes;

  const TYPE_BADGE_COLORS = {
    string: '#3b82f6',
    guid: '#8b5cf6',
    number: '#f59e0b',
    boolean: '#10b981',
    date: '#ec4899',
    enum: '#6366f1',
    object: '#64748b',
    array: '#0ea5e9'
  };

  const state = {
    endpoints: [],
    query: '',
    base: 'api',
    method: 'POST',
    selected: null, // { group, name, path }
    bodyMode: 'form', // 'form' | 'raw'
    formPairs: [], // {id, key, value}
    rawBody: '{}',
    result: null,
    status: null,
    contentType: null,
    collapsedGroups: new Set(),
    flow: 'none',
    // Used to avoid auto-populating editors when flow presets set everything.
    suppressAutoPopulateOnce: false
  };

  function uid() {
    if (typeof crypto !== 'undefined' && crypto.randomUUID) return crypto.randomUUID();
    return 'id_' + Math.random().toString(36).slice(2);
  }

  function flattenRoutes() {
    const out = [];
    const top = ApiRoutes || {};

    for (const [group, value] of Object.entries(top)) {
      if (!value || typeof value !== 'object') continue;

      for (const [name, path] of Object.entries(value)) {
        if (typeof path === 'string' && path.startsWith('/')) {
          out.push({ group, name, path });
        }
      }
    }

    return out.sort((a, b) => `${a.group}.${a.name}`.localeCompare(`${b.group}.${b.name}`));
  }

  function groupByCategory(endpoints) {
    const map = new Map();
    for (const e of endpoints) {
      const key = e.group;
      if (!map.has(key)) map.set(key, []);
      map.get(key).push(e);
    }
    return map;
  }

  function formatGroupLabel(group) {
    return group
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, (s) => s.toUpperCase())
      .trim();
  }

  function parseValue(value) {
    const t = (value || '').trim();
    if (t === '') return '';
    if (t === 'true') return true;
    if (t === 'false') return false;
    if (t === 'null') return null;
    if (/^-?\d+$/.test(t)) return parseInt(t, 10);
    if (/^-?\d*\.\d+$/.test(t)) return parseFloat(t);
    if ((t.startsWith('{') && t.endsWith('}')) || (t.startsWith('[') && t.endsWith(']'))) {
      try {
        return JSON.parse(t);
      } catch {
        return value;
      }
    }
    return value;
  }

  function formPairsToJson(pairs) {
    const obj = {};
    for (const { key, value } of pairs) {
      const k = (key || '').trim();
      if (k === '') continue;
      obj[k] = parseValue(value);
    }
    return JSON.stringify(Object.keys(obj).length ? obj : {});
  }

  function jsonToFormPairs(jsonBody) {
    const raw = (jsonBody || '').trim();
    if (!raw || raw === '""') return [];

    try {
      const parsed = JSON.parse(raw);
      if (parsed === null || typeof parsed !== 'object' || Array.isArray(parsed)) return [];

      return Object.entries(parsed).map(([key, value]) => ({
        id: uid(),
        key,
        value: typeof value === 'string' ? value : JSON.stringify(value)
      }));
    } catch {
      return [];
    }
  }

  function schemaToFormPairs(schema) {
    return (schema?.fields || []).map((f) => ({
      id: uid(),
      key: f.name,
      value: f.defaultValue ?? ''
    }));
  }

  function schemaToJsonTemplate(schema) {
    const fields = schema?.fields || [];
    if (fields.length === 0) return '{}';

    const obj = {};
    for (const f of fields) {
      if (f.defaultValue !== undefined) {
        obj[f.name] = parseValue(f.defaultValue);
      } else if (f.type === 'boolean') {
        obj[f.name] = false;
      } else if (f.type === 'number') {
        obj[f.name] = 0;
      } else {
        obj[f.name] = '';
      }
    }

    return JSON.stringify(obj, null, 2);
  }

  function getFieldSchema(key) {
    const schema = state.selected ? endpointSchemas[state.selected.path] : null;
    if (!schema) return undefined;
    return (schema.fields || []).find((f) => f.name === key);
  }

  function getEndpointByPath(path) {
    return state.endpoints.find((e) => e.path === path) || null;
  }

  // ---------------------------------------------------------------------------
  // DOM helpers
  // ---------------------------------------------------------------------------
  function el(id) {
    return document.getElementById(id);
  }

  function setText(id, text) {
    const node = el(id);
    if (!node) return;
    node.textContent = text ?? '';
  }

  function escapeHtml(s) {
    return (s ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  }

  // ---------------------------------------------------------------------------
  // Render: Endpoint list (grouped, collapsible)
  // ---------------------------------------------------------------------------
  function filteredEndpoints() {
    const q = (state.query || '').trim().toLowerCase();
    if (!q) return state.endpoints;
    return state.endpoints.filter(
      (e) => e.group.toLowerCase().includes(q) || e.name.toLowerCase().includes(q) || e.path.toLowerCase().includes(q)
    );
  }

  function renderEndpointList() {
    const container = el('apiExplorerEndpointList');
    if (!container) return;

    container.innerHTML = '';

    const filtered = filteredEndpoints();
    const grouped = groupByCategory(filtered);

    for (const [group, items] of grouped.entries()) {
      const groupId = `group_${group}`;
      const collapsed = state.collapsedGroups.has(group);

      const header = document.createElement('div');
      header.style.marginBottom = '0.25rem';
      header.innerHTML = `
        <button type="button" class="hcl-btn hcl-btn-ghost" style="width:100%; justify-content:space-between; text-align:left; display:flex; align-items:center; gap:0.5rem;"
                onclick="window.__apiExplorerToggleGroup('${escapeHtml(group)}')">
          <span style="font-weight:600;">${escapeHtml(formatGroupLabel(group))}</span>
          <span style="opacity:0.7;">${collapsed ? '▶' : '▼'}</span>
        </button>
      `;
      container.appendChild(header);

      if (collapsed) continue;

      const list = document.createElement('div');
      list.style.display = 'flex';
      list.style.flexDirection = 'column';
      list.style.gap = '0.25rem';

      for (const e of items) {
        const active = state.selected && state.selected.path === e.path;
        const hasSchema = !!endpointSchemas[e.path];
        const itemBtn = document.createElement('button');
        itemBtn.type = 'button';
        itemBtn.className = active ? 'hcl-btn hcl-btn-primary' : 'hcl-btn hcl-btn-ghost';
        itemBtn.style.width = '100%';
        itemBtn.style.textAlign = 'left';
        itemBtn.style.justifyContent = 'flex-start';
        itemBtn.style.display = 'flex';
        itemBtn.style.flexDirection = 'column';
        itemBtn.style.alignItems = 'flex-start';
        itemBtn.style.padding = '0.5rem 0.75rem';
        itemBtn.style.gap = '0.15rem';
        itemBtn.onclick = () => selectEndpoint(e.path);

        itemBtn.innerHTML = `
          <span style="display:flex; align-items:center; gap:0.35rem;">
            <span style="font-weight:700;">${escapeHtml(e.name)}</span>
            ${hasSchema ? '<span style="width:6px;height:6px;border-radius:50%;background:var(--accent); flex-shrink:0;"></span>' : ''}
          </span>
          <span class="hcl-text-secondary" style="font-family:var(--font-mono); font-size:0.75rem; word-break:break-all;">${escapeHtml(e.path)}</span>
        `;

        list.appendChild(itemBtn);
      }

      const groupWrapper = document.createElement('div');
      groupWrapper.style.marginBottom = '0.25rem';
      groupWrapper.appendChild(list);
      container.appendChild(groupWrapper);
    }
  }

  window.__apiExplorerToggleGroup = function toggle(group) {
    if (state.collapsedGroups.has(group)) state.collapsedGroups.delete(group);
    else state.collapsedGroups.add(group);
    renderEndpointList();
  };

  // ---------------------------------------------------------------------------
  // Request editor logic (schema-aware Form / Raw JSON)
  // ---------------------------------------------------------------------------
  function currentSchema() {
    if (!state.selected) return null;
    return endpointSchemas[state.selected.path] || null;
  }

  function setBodyMode(mode) {
    if (state.bodyMode === mode) return;

    if (mode === 'form') switchToForm();
    if (mode === 'raw') switchToRaw();
  }

  function switchToForm() {
    const schema = currentSchema();
    if (schema && schema.fields && schema.fields.length > 0) {
      // Merge current raw JSON values into schema fields.
      let currentValues = {};
      try {
        const str = state.bodyMode === 'raw' ? state.rawBody : formPairsToJson(state.formPairs);
        const parsed = JSON.parse(str);
        if (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) {
          for (const [k, v] of Object.entries(parsed)) {
            currentValues[k] = typeof v === 'string' ? v : JSON.stringify(v);
          }
        }
      } catch {
        currentValues = {};
      }

      const pairs = schema.fields.map((f) => ({
        id: uid(),
        key: f.name,
        value: currentValues[f.name] ?? f.defaultValue ?? ''
      }));

      for (const [k, v] of Object.entries(currentValues)) {
        if (!schema.fields.some((f) => f.name === k)) {
          pairs.push({ id: uid(), key: k, value: v });
        }
      }

      state.formPairs = pairs;
    } else {
      state.formPairs = jsonToFormPairs(state.rawBody);
    }

    state.bodyMode = 'form';
    renderRequestEditor();
  }

  function switchToRaw() {
    const str = state.bodyMode === 'form' ? formPairsToJson(state.formPairs) : state.rawBody;
    try {
      const parsed = JSON.parse(str);
      state.rawBody = JSON.stringify(parsed, null, 2);
    } catch {
      state.rawBody = str === '""' ? '{}' : str;
    }
    state.bodyMode = 'raw';
    renderRequestEditor();
  }

  function addCustomField() {
    state.formPairs.push({ id: uid(), key: '', value: '' });
    renderRequestEditor();
  }

  function removeFormPair(id) {
    state.formPairs = state.formPairs.filter((p) => p.id !== id);
    renderRequestEditor();
  }

  function updateFormPair(id, field, value) {
    const pair = state.formPairs.find((p) => p.id === id);
    if (!pair) return;
    pair[field] = value;
  }

  function TypeBadge({ type }) {
    const color = TYPE_BADGE_COLORS[type] || '#6b7280';
    return `
      <span style="display:inline-block; padding:0.1rem 0.4rem; border-radius:4px; font-size:0.6875rem; font-weight:800; font-family:var(--font-mono); color:#fff; background:${color}; line-height:1.4; text-transform:uppercase; letter-spacing:0.02em;">
        ${escapeHtml(type)}
      </span>
    `;
  }

  function renderSchemaFieldInput(field, pair) {
    const val = pair.value ?? '';

    if (field.type === 'boolean') {
      const checked = val === true || val === 'true';
      return `
        <label style="display:flex; align-items:center; gap:0.5rem; cursor:pointer;">
          <input type="checkbox" ${checked ? 'checked' : ''} style="width:16px;height:16px; accent-color: var(--accent);"
                 onchange="window.__apiExplorerOnBooleanChange('${pair.id}', this.checked)"/>
          <span style="font-size:0.8125rem; color:var(--text-secondary)">${checked ? 'true' : 'false'}</span>
        </label>
      `;
    }

    if (field.type === 'enum' && field.enumValues) {
      const options = field.enumValues.map((ev) => {
        const numericVal = (ev.split(' - ')[0] || ev).trim();
        const selected = numericVal === val ? 'selected' : '';
        return `<option value="${escapeHtml(numericVal)}" ${selected}>${escapeHtml(ev)}</option>`;
      }).join('');

      return `
        <select class="hcl-input" style="padding:0.45rem 0.65rem; border-radius:8px; font-size:0.8125rem; font-family:var(--font-mono);"
                onchange="window.__apiExplorerOnEnumChange('${pair.id}', this.value)">
          <option value="">— select —</option>
          ${options}
        </select>
      `;
    }

    if (field.type === 'date') {
      return `
        <input type="datetime-local" class="hcl-input" value="${escapeHtml(val)}"
               style="font-family:var(--font-mono); font-size:0.8125rem;"
               onchange="window.__apiExplorerOnRawInput('${pair.id}', this.value)"/>
      `;
    }

    // Default to text input (supports numbers/guids/object/array via parseValue()).
    return `
      <input class="hcl-input" value="${escapeHtml(val)}"
             placeholder="${escapeHtml(field.placeholder || '')}"
             style="font-family:var(--font-mono); font-size:0.8125rem;"
             onchange="window.__apiExplorerOnRawInput('${pair.id}', this.value)"/>
    `;
  }

  window.__apiExplorerOnRawInput = function onRawInput(id, value) {
    updateFormPair(id, 'value', value);
  };

  window.__apiExplorerOnBooleanChange = function onBooleanChange(id, checked) {
    updateFormPair(id, 'value', checked ? 'true' : 'false');
  };

  window.__apiExplorerOnEnumChange = function onEnumChange(id, value) {
    updateFormPair(id, 'value', value);
  };

  function renderFormRow(pair) {
    const fieldDef = state.selected ? currentSchema()?.fields?.find((f) => f.name === pair.key) : null;
    const isSchemaField = !!fieldDef;

    if (isSchemaField && fieldDef) {
      const typeBadge = TypeBadge({ type: fieldDef.type });
      const requiredStar = fieldDef.required ? '<span style="color:#ef4444; font-weight:900;">*</span>' : '';
      const desc = fieldDef.description ? `<span class="hcl-text-secondary" style="font-size:0.6875rem; line-height:1.3; margin-top:0.1rem; display:block;">${escapeHtml(fieldDef.description)}</span>` : '';

      const inputHtml = renderSchemaFieldInput(fieldDef, pair);

      return `
        <div style="display:grid; grid-template-columns:minmax(120px, 0.4fr) 1fr auto; gap:0.5rem; align-items:start; padding:0.5rem 0; border-bottom:1px solid var(--border-default);">
          <div style="display:flex; flex-direction:column; gap:0.2rem; padding-top:0.45rem;">
            <div style="display:flex; align-items:center; gap:0.35rem; flex-wrap:wrap;">
              <span style="font-family:var(--font-mono); font-size:0.8125rem; font-weight:800; color:var(--text-primary)">${escapeHtml(fieldDef.name)}</span>
              ${requiredStar}
            </div>
            ${typeBadge}
            ${desc}
          </div>
          <div>${inputHtml}</div>
          <div style="padding-top:0.25rem;">
            <button type="button" class="hcl-btn hcl-btn-ghost hcl-btn-sm" onclick="window.__apiExplorerRemovePair('${pair.id}')" style="min-width:36px;">×</button>
          </div>
        </div>
      `;
    }

    // Custom key/value pair
    return `
      <div style="display:grid; grid-template-columns:minmax(120px, 0.4fr) 1fr auto; gap:0.5rem; align-items:center; padding:0.5rem 0; border-bottom:1px solid var(--border-default);">
        <input class="hcl-input" value="${escapeHtml(pair.key)}"
               placeholder="Field name"
               style="font-family:var(--font-mono); font-size:0.8125rem;"
               onchange="window.__apiExplorerOnKeyChange('${pair.id}', this.value)"/>
        <input class="hcl-input" value="${escapeHtml(pair.value)}"
               placeholder="Value"
               style="font-family:var(--font-mono); font-size:0.8125rem;"
               onchange="window.__apiExplorerOnValueChange('${pair.id}', this.value)"/>
        <button type="button" class="hcl-btn hcl-btn-ghost hcl-btn-sm" onclick="window.__apiExplorerRemovePair('${pair.id}')" style="min-width:36px;">×</button>
      </div>
    `;
  }

  window.__apiExplorerOnKeyChange = function onKeyChange(id, value) {
    updateFormPair(id, 'key', value);
  };

  window.__apiExplorerOnValueChange = function onValueChange(id, value) {
    updateFormPair(id, 'value', value);
  };

  window.__apiExplorerRemovePair = function removePair(id) {
    removeFormPair(id);
  };

  function renderRequestEditor() {
    const schema = currentSchema();

    const method = state.method;
    const isPost = method === 'POST';

    // Request body mode controls
    const bodyModeControls = el('apiExplorerBodyModeControls');
    if (bodyModeControls) {
      bodyModeControls.style.display = isPost ? '' : 'none';
    }

    // Selected path summary
    setText('apiExplorerSelectedPath', state.selected?.path ?? '—');

    if (!isPost) {
      const formContainer = el('apiExplorerFormContainer');
      const rawContainer = el('apiExplorerRawContainer');
      if (formContainer) formContainer.style.display = 'none';
      if (rawContainer) rawContainer.style.display = 'none';
      return;
    }

    // Summary banner (schema method/description)
    const schemaBanner = el('apiExplorerSchemaBanner');
    if (schemaBanner) {
      if (schema) {
        const requiredCount = (schema.fields || []).filter((f) => f.required).length;
        schemaBanner.innerHTML = `
          <div style="display:flex; align-items:center; gap:0.75rem; padding:0.75rem 1rem; border-radius:10px; background:var(--bg-overlay); border:1px solid var(--border-default); flex-wrap:wrap;">
            <span style="padding:0.15rem 0.5rem; border-radius:4px; font-size:0.6875rem; font-weight:900; font-family:var(--font-mono); color:#fff; background:${schema.method === 'POST' ? '#f59e0b' : '#3b82f6'};">
              ${escapeHtml(schema.method)}
            </span>
            <span style="font-size:0.8125rem; font-weight:600;">${escapeHtml(schema.description || '')}</span>
            <span style="margin-left:auto; display:flex; gap:0.5rem; align-items:center;">
              ${(schema.fields || []).length > 0 ? `<span class="hcl-badge" style="font-size:0.6875rem;">${schema.fields.length} field${schema.fields.length !== 1 ? 's' : ''}</span>` : '<span class="hcl-badge" style="font-size:0.6875rem;">No body</span>'}
              ${requiredCount > 0 ? `<span class="hcl-badge hcl-badge-danger" style="font-size:0.6875rem;">${requiredCount} required</span>` : ''}
            </span>
          </div>
        `;
      } else {
        schemaBanner.innerHTML = '';
      }
    }

    if (state.bodyMode === 'raw') {
      const formContainer = el('apiExplorerFormContainer');
      const rawContainer = el('apiExplorerRawContainer');
      if (formContainer) formContainer.style.display = 'none';
      if (rawContainer) rawContainer.style.display = '';

      const textarea = el('apiExplorerRawTextarea');
      if (textarea) textarea.value = state.rawBody ?? '{}';
      bindRawTextarea();
      return;
    }

    // Form mode
    const formContainer = el('apiExplorerFormContainer');
    const rawContainer = el('apiExplorerRawContainer');
    if (rawContainer) rawContainer.style.display = 'none';
    if (formContainer) formContainer.style.display = '';

    if (!formContainer) return;

    formContainer.innerHTML = '';

    if (schema && (schema.fields || []).length > 0 && state.formPairs.length === 0) {
      // Defensive: ensure there are pairs to render.
      state.formPairs = schemaToFormPairs(schema);
    }

    if (schema && schema.fields && schema.fields.length === 0 && state.formPairs.length === 0) {
      // no-body endpoints
      formContainer.innerHTML = `
        <div style="padding:1rem; border-radius:10px; background:var(--bg-overlay); border:1px solid var(--border-default); text-align:center;">
          <p class="hcl-text-secondary" style="margin:0; font-size:0.8125rem;">This endpoint does not require a request body.</p>
        </div>
      `;
      return;
    }

    for (const pair of state.formPairs) {
      formContainer.insertAdjacentHTML('beforeend', renderFormRow(pair));
    }

    formContainer.insertAdjacentHTML(
      'beforeend',
      `<div style="padding-top:0.75rem; display:flex; flex-direction:column; gap:0.5rem;">
         <button type="button" class="hcl-btn hcl-btn-secondary hcl-btn-sm" onclick="window.__apiExplorerAddCustomField()">+ Add custom field</button>
         <p class="hcl-text-secondary" style="margin:0; font-size:0.75rem;">Numbers and true/false are parsed automatically. Use Raw JSON for nested objects or arrays.</p>
       </div>`
    );
  }

  window.__apiExplorerAddCustomField = function () {
    addCustomField();
  };

  // Raw textarea wiring (update state on input)
  function bindRawTextarea() {
    const textarea = el('apiExplorerRawTextarea');
    if (!textarea) return;
    textarea.oninput = function () {
      state.rawBody = textarea.value;
    };
  }

  // ---------------------------------------------------------------------------
  // Endpoint selection
  // ---------------------------------------------------------------------------
  function selectEndpoint(path) {
    const ep = getEndpointByPath(path);
    if (!ep) return;
    state.selected = ep;

    if (!state.suppressAutoPopulateOnce) {
      const schema = currentSchema();
      if (state.bodyMode === 'form') {
        if (schema && schema.fields && schema.fields.length > 0) state.formPairs = schemaToFormPairs(schema);
        else state.formPairs = jsonToFormPairs(state.rawBody);
      } else if (state.bodyMode === 'raw') {
        state.rawBody = schema ? schemaToJsonTemplate(schema) : (state.rawBody ?? '{}');
      }
    } else {
      state.suppressAutoPopulateOnce = false;
    }

    renderEndpointList();
    renderRequestEditor();
    bindRawTextarea();
  }

  // ---------------------------------------------------------------------------
  // Flow presets
  // ---------------------------------------------------------------------------
  function applyFlow(key) {
    state.flow = key;

    if (key === 'none') return;

    // Match Next.js presets
    if (key === 'clientCredentials') {
      state.base = 'api';
      state.method = 'POST';
      state.bodyMode = 'raw';
      state.rawBody = JSON.stringify(
        {
          grant_type: 'client_credentials',
          client_id: '<client-id>',
          client_secret: '<client-secret>',
          scope: 'HCL.CS.SF.apiresource HCL.CS.SF.client'
        },
        null,
        2
      );
      selectFlowEndpoint(ApiRoutes.endpoint.token);
      return;
    }

    if (key === 'authorizationCode') {
      state.base = 'api';
      state.method = 'POST';
      state.bodyMode = 'raw';
      state.rawBody = JSON.stringify(
        {
          grant_type: 'authorization_code',
          code: '<auth-code>',
          redirect_uri: '<redirect-uri>',
          client_id: '<client-id>',
          client_secret: '<client-secret>'
        },
        null,
        2
      );
      selectFlowEndpoint(ApiRoutes.endpoint.token);
      return;
    }

    if (key === 'password') {
      state.base = 'api';
      state.method = 'POST';
      state.bodyMode = 'raw';
      state.rawBody = JSON.stringify(
        {
          username: '<user-name>',
          password: '<password>',
          client_id: '<client-id>',
          client_secret: '<client-secret>',
          scope: 'HCL.CS.SF.apiresource HCL.CS.SF.client'
        },
        null,
        2
      );
      selectFlowEndpoint(ApiRoutes.authentication.ropValidateCredentials);
      return;
    }

    if (key === 'introspect') {
      state.base = 'api';
      state.method = 'POST';
      state.bodyMode = 'raw';
      state.rawBody = JSON.stringify(
        {
          token: '<access-or-refresh-token>',
          token_type_hint: 'access_token',
          client_id: '<client-id>',
          client_secret: '<client-secret>'
        },
        null,
        2
      );
      selectFlowEndpoint(ApiRoutes.endpoint.introspect);
      return;
    }

    if (key === 'revocation') {
      state.base = 'api';
      state.method = 'POST';
      state.bodyMode = 'raw';
      state.rawBody = JSON.stringify(
        {
          token: '<access-or-refresh-token>',
          token_type_hint: 'access_token',
          client_id: '<client-id>',
          client_secret: '<client-secret>'
        },
        null,
        2
      );
      selectFlowEndpoint(ApiRoutes.endpoint.revocation);
      return;
    }
  }

  function selectFlowEndpoint(path) {
    const ep = getEndpointByPath(path);
    if (!ep) return;
    state.selected = ep;
    // Ensure we don't override rawBody when switching endpoints.
    state.suppressAutoPopulateOnce = true;
    renderEndpointList();
    renderRequestEditor();
    bindRawTextarea();
  }

  // ---------------------------------------------------------------------------
  // Execute
  // ---------------------------------------------------------------------------
  async function execute() {
    if (!state.selected) {
      if (HCL && HCL.toastError) HCL.toastError('Select an endpoint first.');
      return;
    }

    const payload = {
      base: state.base,
      method: state.method,
      path: state.selected.path,
      jsonBody: state.method === 'POST' ? '' : ''
    };

    if (state.method === 'POST') {
      const bodyToSend = state.bodyMode === 'form' ? formPairsToJson(state.formPairs) : (state.rawBody ?? '{}').trim() || '{}';

      // Validate JSON to match Next.js behavior.
      try {
        JSON.parse(bodyToSend);
      } catch {
        HCL.toastError('Request body must be valid JSON.');
        return;
      }

      payload.jsonBody = bodyToSend;
    } else {
      payload.jsonBody = '';
    }

    const res = await HCL.apiRequest('/Admin/Operations/ApiExplorer/Call', 'POST', payload);
    if (!res.ok) {
      HCL.toastError(res.message || 'Request failed.');
      state.status = res.data?.status ?? null;
      state.contentType = res.data?.contentType ?? null;
      state.result = res.data?.body ?? null;
    } else {
      HCL.toastSuccess('Request completed.');
      state.status = res.data?.status ?? null;
      state.contentType = res.data?.contentType ?? null;
      state.result = res.data?.body ?? null;
    }

    renderResponse();
  }

  function renderResponse() {
    const statusNode = el('apiExplorerStatus');
    const ctNode = el('apiExplorerContentType');
    const accessTokenNode = el('apiExplorerAccessTokenActions');
    const copyResponseBtn = el('apiExplorerCopyResponseBtn');
    const pre = el('apiExplorerResponsePre');

    if (statusNode) {
      if (state.status === null || state.status === undefined) statusNode.textContent = '';
      else statusNode.textContent = String(state.status);
    }

    if (ctNode) {
      ctNode.textContent = state.contentType ? String(state.contentType) : '';
    }

    if (accessTokenNode) accessTokenNode.innerHTML = '';

    const bodyText = (state.result === null || state.result === undefined)
      ? ''
      : (typeof state.result === 'string' ? state.result : JSON.stringify(state.result, null, 2));

    if (pre) pre.textContent = bodyText || 'No response yet. Run a request.';

    if (copyResponseBtn) {
      copyResponseBtn.onclick = function () {
        if (!bodyText) return;
        HCL.copyToClipboard(bodyText);
      };
    }

    if (accessTokenNode && state.result && typeof state.result === 'object' && !Array.isArray(state.result)) {
      const maybe = state.result.access_token;
      if (typeof maybe === 'string' && maybe) {
        accessTokenNode.innerHTML = `
          <button class="hcl-btn hcl-btn-ghost hcl-btn-sm" type="button" onclick="window.__apiExplorerCopyAccessToken()">
            Copy Access token
          </button>
        `;
        window.__apiExplorerCopyAccessToken = function () {
          HCL.copyToClipboard(maybe);
        };
      }
    }
  }

  // ---------------------------------------------------------------------------
  // Bind UI + init
  // ---------------------------------------------------------------------------
  function bindControls() {
    const query = el('apiExplorerQuery');
    if (query) {
      query.oninput = function () {
        state.query = query.value;
        renderEndpointList();
      };
    }

    const baseSelect = el('apiExplorerBaseSelect');
    if (baseSelect) {
      baseSelect.onchange = function () {
        state.base = baseSelect.value;
      };
    }

    const getBtn = el('apiExplorerMethodGet');
    const postBtn = el('apiExplorerMethodPost');
    if (getBtn && postBtn) {
      getBtn.onclick = function () {
        state.method = 'GET';
        if (HCL && HCL.toastInfo) {} // keep existing patterns
        renderRequestEditor();
      };
      postBtn.onclick = function () {
        state.method = 'POST';
        renderRequestEditor();
      };
    }

    const formBtn = el('apiExplorerBodyModeForm');
    const rawBtn = el('apiExplorerBodyModeRaw');
    if (formBtn && rawBtn) {
      formBtn.onclick = function () {
        setBodyMode('form');
      };
      rawBtn.onclick = function () {
        setBodyMode('raw');
      };
    }

    const executeBtn = el('apiExplorerExecuteBtn');
    if (executeBtn) {
      executeBtn.onclick = function () {
        execute().catch((e) => {
          HCL.toastError(e?.message || 'Request failed.');
        });
      };
    }

    const flowSelect = el('apiExplorerFlowSelect');
    if (flowSelect) {
      flowSelect.onchange = function () {
        applyFlow(flowSelect.value);
      };
    }
  }

  function init() {
    state.endpoints = flattenRoutes();
    state.selected = state.endpoints[0] || null;

    // If we have selected endpoint and current body mode, seed editor.
    const schema = currentSchema();
    if (state.bodyMode === 'form') state.formPairs = schema ? schemaToFormPairs(schema) : [];
    if (state.bodyMode === 'raw') state.rawBody = schema ? schemaToJsonTemplate(schema) : '{}';

    renderEndpointList();
    renderRequestEditor();
    renderResponse();
    bindControls();
    bindRawTextarea();

    // Ensure the initial flow select matches.
    const flowSelect = el('apiExplorerFlowSelect');
    if (flowSelect) flowSelect.value = 'none';
  }

  document.addEventListener('DOMContentLoaded', init);
})();

