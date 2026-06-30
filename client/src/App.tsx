import { useEffect, useMemo, useState } from "react";
import "./App.css";
import {
  fetchCatalog,
  runDemo,
  type CapturedSql,
  type DemoInfo,
  type DemoResult,
} from "./api";

const SQL_KEYWORDS =
  /\b(SELECT|FROM|WHERE|INNER|LEFT|RIGHT|JOIN|ON|AND|OR|NOT|ORDER BY|GROUP BY|LIMIT|INSERT INTO|VALUES|UPDATE|SET|DELETE|RETURNING|AS|IN|EXISTS|COUNT|LIKE|DISTINCT|CASE|WHEN|THEN|ELSE|END)\b/gi;

function highlightSql(sql: string): string {
  const escaped = sql
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
  return escaped
    .replace(SQL_KEYWORDS, (m) => `<span class="kw">${m}</span>`)
    .replace(/'[^']*'/g, (m) => `<span class="str">${m}</span>`)
    .replace(/@\w+/g, (m) => `<span class="num">${m}</span>`);
}

function SqlCard({ item, index }: { item: CapturedSql; index: number }) {
  const params = Object.entries(item.parameters);
  return (
    <div className="sql-card">
      <pre dangerouslySetInnerHTML={{ __html: highlightSql(item.sql) }} />
      {params.length > 0 && (
        <div className="params">
          {params.map(([k, v]) => (
            <span key={k} style={{ marginRight: 14 }}>
              <code>{k}</code> = {JSON.stringify(v)}
            </span>
          ))}
        </div>
      )}
      <div className="sql-meta">
        <span>Consulta #{index + 1}</span>
        <span>
          Tempo: <b>{item.elapsedMs.toFixed(3)} ms</b>
        </span>
      </div>
    </div>
  );
}

export default function App() {
  const [catalog, setCatalog] = useState<DemoInfo[]>([]);
  const [selected, setSelected] = useState<DemoInfo | null>(null);
  const [result, setResult] = useState<DemoResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchCatalog()
      .then(setCatalog)
      .catch((e) => setError(String(e)));
  }, []);

  const grouped = useMemo(() => {
    const map = new Map<string, DemoInfo[]>();
    for (const d of catalog) {
      if (!map.has(d.group)) map.set(d.group, []);
      map.get(d.group)!.push(d);
    }
    return [...map.entries()];
  }, [catalog]);

  async function select(demo: DemoInfo) {
    setSelected(demo);
    setResult(null);
    setError(null);
    await execute(demo);
  }

  async function execute(demo: DemoInfo) {
    setLoading(true);
    setError(null);
    try {
      setResult(await runDemo(demo));
    } catch (e) {
      setError(String(e));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="layout">
      <aside className="sidebar">
        <div className="brand">
          <h1>
            EF Core <span>10</span> · Demo
          </h1>
          <p>.NET 10 · SQLite · React — cada demo mostra o SQL gerado</p>
        </div>

        {grouped.map(([group, demos]) => (
          <div key={group}>
            <div className="group-title">{group}</div>
            {demos.map((d) => (
              <button
                key={d.path}
                className={`demo-btn ${selected?.path === d.path ? "active" : ""}`}
                onClick={() => select(d)}
              >
                <span>
                  <span className="method">{d.method}</span>
                  {d.title}
                </span>
                <span className="tech">{d.technique}</span>
              </button>
            ))}
          </div>
        ))}
      </aside>

      <main className="main">
        {!selected && !error && (
          <div className="empty">
            <h2>👈 Selecione uma demonstração</h2>
            <p>
              Cada item executa uma técnica do EF Core e exibe o SQL realmente
              enviado ao SQLite, com parâmetros e tempo.
            </p>
          </div>
        )}

        {error && <div className="error">{error}</div>}

        {selected && result && (
          <>
            <div className="head">
              <span className="badge">{result.technique}</span>
              <h2>{result.title}</h2>
            </div>
            <div className="explanation">{result.explanation}</div>

            <div className="run-row">
              <button
                className="run-btn"
                disabled={loading}
                onClick={() => execute(selected)}
              >
                {loading ? "Executando…" : "▶ Executar de novo"}
              </button>
              <span className="sql-count">
                <b>{result.sqlCount}</b> consulta(s) enviada(s) ao banco
              </span>
            </div>

            <div className="section-title">SQL gerado</div>
            {result.sql.length === 0 && (
              <div className="result">Nenhum SQL capturado.</div>
            )}
            {result.sql.map((s, i) => (
              <SqlCard key={i} item={s} index={i} />
            ))}

            <div className="section-title">Resultado</div>
            <div className="result">
              <pre>{JSON.stringify(result.data, null, 2)}</pre>
            </div>
          </>
        )}
      </main>
    </div>
  );
}
