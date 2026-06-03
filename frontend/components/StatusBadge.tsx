export function StatusBadge({ status }: { status: string }) {
  const s = status.toUpperCase();
  const cls =
    s === "UP"
      ? "badge-up"
      : s === "DOWN"
        ? "badge-down"
        : s === "PAUSED"
          ? "badge-paused"
          : "badge-unknown";
  return <span className={`badge ${cls}`}>{s}</span>;
}
