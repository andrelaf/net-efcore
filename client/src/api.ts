// Cliente HTTP da API de demonstração do EF Core.
export const API_BASE =
  (import.meta.env.VITE_API_BASE as string | undefined) ?? "http://localhost:5222";

export interface DemoInfo {
  group: string;
  title: string;
  technique: string;
  method: "GET" | "POST";
  path: string;
}

export interface CapturedSql {
  sql: string;
  parameters: Record<string, unknown>;
  elapsedMs: number;
}

export interface DemoResult {
  title: string;
  technique: string;
  explanation: string;
  data: unknown;
  sql: CapturedSql[];
  sqlCount: number;
}

export async function fetchCatalog(): Promise<DemoInfo[]> {
  const res = await fetch(`${API_BASE}/api/demos`);
  if (!res.ok) throw new Error(`Falha ao carregar catálogo (${res.status})`);
  return res.json();
}

export async function runDemo(demo: DemoInfo): Promise<DemoResult> {
  const res = await fetch(`${API_BASE}${demo.path}`, { method: demo.method });
  if (!res.ok) throw new Error(`Erro ${res.status} ao executar ${demo.path}`);
  return res.json();
}
