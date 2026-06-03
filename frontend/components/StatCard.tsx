export function StatCard({
  label,
  value,
  suffix,
}: {
  label: string;
  value: string | number;
  suffix?: string;
}) {
  return (
    <div className="card">
      <div className="stat-value">
        {value}
        {suffix && (
          <span style={{ fontSize: "1rem", color: "var(--text-muted)" }}>
            {" "}
            {suffix}
          </span>
        )}
      </div>
      <div className="stat-label">{label}</div>
    </div>
  );
}
