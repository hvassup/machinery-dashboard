"use client";

import { useEffect, useState, Suspense } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { getMachines, scheduleJob } from "@/lib/api";
import type { Machine } from "@/types";

function ScheduleForm() {
  const searchParams = useSearchParams();
  const router = useRouter();

  const [machines, setMachines] = useState<Machine[]>([]);
  const [machineId, setMachineId] = useState(searchParams.get("machineId") ?? "");
  const [productId, setProductId] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getMachines().then(setMachines).catch(console.error);
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    setResult(null);
    try {
      const data = await scheduleJob(machineId, productId, quantity);
      setResult(`Order scheduled: ${data.orderId}`);
      setTimeout(() => router.push(`/machines/${machineId}`), 1500);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Failed to schedule job");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-md">
      <h1 className="text-2xl font-bold mb-6">Schedule Job</h1>
      <form onSubmit={handleSubmit} className="bg-white rounded-lg border p-6 space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Machine</label>
          <select
            value={machineId}
            onChange={(e) => setMachineId(e.target.value)}
            required
            className="w-full border rounded px-3 py-2 text-sm"
          >
            <option value="">Select a machine…</option>
            {machines.map((m) => (
              <option key={m.machineId} value={m.machineId}>
                {m.name} ({m.status})
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Product ID</label>
          <input
            type="text"
            value={productId}
            onChange={(e) => setProductId(e.target.value)}
            required
            placeholder="e.g. product-123"
            className="w-full border rounded px-3 py-2 text-sm"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Quantity</label>
          <input
            type="number"
            value={quantity}
            onChange={(e) => setQuantity(Number(e.target.value))}
            required
            min={1}
            className="w-full border rounded px-3 py-2 text-sm"
          />
        </div>

        {error && <p className="text-red-600 text-sm">{error}</p>}
        {result && <p className="text-green-600 text-sm">{result}</p>}

        <button
          type="submit"
          disabled={submitting}
          className="w-full bg-blue-600 text-white rounded px-4 py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
        >
          {submitting ? "Scheduling…" : "Schedule Job"}
        </button>
      </form>
    </div>
  );
}

export default function SchedulePage() {
  return (
    <Suspense>
      <ScheduleForm />
    </Suspense>
  );
}
