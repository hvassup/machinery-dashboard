const statusColors: Record<string, string> = {
  Idle:        "bg-red-200 text-red-800",
  WarmingUp:   "bg-yellow-200 text-yellow-800",
  Ready:       "bg-green-100 text-green-700",
  Processing:  "bg-green-200 text-green-800",
  CoolingDown: "bg-yellow-200 text-yellow-800",
};

export default function StatusBadge({ status }: { status: string }) {
  const classes = statusColors[status] ?? "bg-red-200 text-red-800";
  return (
    <span className={`inline-block px-2 py-0.5 rounded text-sm font-medium ${classes}`}>
      {status}
    </span>
  );
}
