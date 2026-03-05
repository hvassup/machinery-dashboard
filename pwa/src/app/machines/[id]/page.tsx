"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { getMachine } from "@/lib/api";
import type { MachineDetail, ScheduledOrder } from "@/types";
import StatusBadge from "@/components/StatusBadge";
import Link from "next/link";

function eventColor(eventType: string): string {
  if (eventType === "Idle")                                return "bg-red-500";
  if (eventType === "WarmingUp" || eventType === "CoolingDown") return "bg-yellow-400";
  if (eventType === "BeginningOrder" || eventType === "FinishedOrder") return "bg-green-500";
  return "bg-gray-400";
}

export default function MachineDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [machine, setMachine] = useState<MachineDetail | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = () =>
      getMachine(id)
        .then(setMachine)
        .catch((e) => setError(e.message));

    load();
    const interval = setInterval(load, 3000);
    return () => clearInterval(interval);
  }, [id]);

  if (error) return <p className="text-red-600">Error: {error}</p>;
  if (!machine) return <p className="text-gray-500">Loading...</p>;

  return (
    <div className="max-w-2xl">
      <div className="flex items-center gap-3 mb-6">
        <Link href="/" className="text-blue-600 hover:underline text-sm">← Back</Link>
        <h1 className="text-2xl font-bold">{machine.name}</h1>
        <StatusBadge status={machine.status} />
      </div>

      <div className="bg-white rounded-lg border p-4 mb-6">
        <p className="text-sm text-gray-600">Machine ID: <span className="font-mono">{machine.machineId}</span></p>
        {machine.currentOrderId && (
          <p className="text-sm text-gray-600 mt-1">Current Order: <span className="font-mono">{machine.currentOrderId}</span></p>
        )}
        <p className="text-sm text-gray-400 mt-1">Last seen: {new Date(machine.lastSeen).toLocaleString()}</p>
      </div>

      {machine.scheduledOrders.length > 0 && (
        <div className="mb-6">
          <h2 className="text-lg font-semibold mb-3">Scheduled Orders</h2>
          <div className="bg-white rounded-lg border divide-y">
            {machine.scheduledOrders.map((o: ScheduledOrder) => (
              <div key={o.orderId} className="flex justify-between items-center px-4 py-2 text-sm">
                <div>
                  <span className="font-medium">{o.productId}</span>
                  <span className="text-gray-500 ml-2">×{o.quantity}</span>
                </div>
                <span className="text-gray-400">{new Date(o.scheduledAt).toLocaleTimeString()}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="flex justify-between items-center mb-3">
        <h2 className="text-lg font-semibold">Event History</h2>
        <Link href={`/schedule?machineId=${id}`} className="text-sm bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700">
          Schedule Job
        </Link>
      </div>

      <div className="bg-white rounded-lg border divide-y">
        {machine.history.length === 0 && (
          <p className="text-sm text-gray-500 p-4">No events yet.</p>
        )}
        {machine.history.map((evt, i) => (
          <div key={i} className="flex justify-between items-center px-4 py-2 text-sm">
            <div className="flex items-center gap-2">
              <span className={`w-2.5 h-2.5 rounded-full flex-shrink-0 ${eventColor(evt.eventType)}`} />
              <span className="font-medium">{evt.eventType}</span>
              {evt.orderId && <span className="text-gray-500 font-mono text-xs">{evt.orderId.slice(0, 8)}…</span>}
            </div>
            <span className="text-gray-400">{new Date(evt.timestamp).toLocaleTimeString()}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
