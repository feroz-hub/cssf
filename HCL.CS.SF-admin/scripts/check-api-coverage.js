/* eslint-disable no-console */

const fs = require("fs");
const path = require("path");

function readUtf8(filePath) {
  return fs.readFileSync(filePath, "utf8");
}

function uniqSorted(values) {
  return Array.from(new Set(values)).sort((a, b) => a.localeCompare(b));
}

function extractBacktickedPathsFromMarkdown(markdown) {
  const matches = markdown.match(/`\/[^`\s]+`/g) ?? [];
  return matches.map((m) => m.slice(1, -1));
}

function extractQuotedPathsFromRoutesTs(source) {
  const paths = [];

  // Double quoted strings containing absolute paths like "/Security/Api/..."
  for (const match of source.matchAll(/"\/[^"\n]*"/g)) {
    paths.push(match[0].slice(1, -1));
  }

  // Single quoted strings containing absolute paths like '/security/revocation'
  for (const match of source.matchAll(/'\/[^'\n]*'/g)) {
    paths.push(match[0].slice(1, -1));
  }

  return paths;
}

function relativeToHCL.CS.SFAdmin(...segments) {
  return path.resolve(__dirname, "..", ...segments);
}

function loadOpenApiPaths(openApiJsonPath) {
  const raw = readUtf8(openApiJsonPath);
  const doc = JSON.parse(raw);
  if (!doc || typeof doc !== "object" || !doc.paths || typeof doc.paths !== "object") {
    throw new Error(`Invalid OpenAPI JSON (missing 'paths'): ${openApiJsonPath}`);
  }

  return Object.keys(doc.paths);
}

function printList(title, items) {
  if (items.length === 0) {
    return;
  }

  console.error(`\n${title} (${items.length})`);
  for (const item of items) {
    console.error(`- ${item}`);
  }
}

function main() {
  const repoRootFromHCL.CS.SFAdmin = relativeToHCL.CS.SFAdmin("..");

  const openApiPath = path.resolve(repoRootFromHCL.CS.SFAdmin, "docs", "api", "all-apis.openapi.json");
  const mdPath = path.resolve(repoRootFromHCL.CS.SFAdmin, "docs", "api", "all-apis.md");
  const routesPath = relativeToHCL.CS.SFAdmin("lib", "api", "routes.ts");

  const docPaths = loadOpenApiPaths(openApiPath);
  const mdPaths = extractBacktickedPathsFromMarkdown(readUtf8(mdPath));
  const expectedPaths = uniqSorted([...docPaths, ...mdPaths]);

  const routesSource = readUtf8(routesPath);
  const routePaths = uniqSorted(extractQuotedPathsFromRoutesTs(routesSource));

  const expectedSet = new Set(expectedPaths);
  const routeSet = new Set(routePaths);

  const missing = expectedPaths.filter((p) => !routeSet.has(p));
  const extra = routePaths.filter((p) => !expectedSet.has(p));

  console.log(`[api-coverage] expected_paths=${expectedPaths.length} route_paths=${routePaths.length}`);

  if (missing.length === 0 && extra.length === 0) {
    console.log("[api-coverage] OK: routes cover docs inventory.");
    return;
  }

  printList("[api-coverage] MISSING in HCL.CS.SF-admin route map", missing);
  printList("[api-coverage] EXTRA in HCL.CS.SF-admin route map (not in docs)", extra);

  process.exitCode = 1;
}

main();

