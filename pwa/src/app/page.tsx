"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { getMachines } from "@/lib/api";
import type { Machine } from "@/types";
import StatusBadge from "@/components/StatusBadge";

export default function DashboardPage() {
  const [machines, setMachines] = useState<Machine[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = () =>
      getMachines()
        .then(setMachines)
        .catch((e) => setError(e.message));

    load();
    const id = setInterval(load, 3000);
    return () => clearInterval(id);
  }, []);

  if (error) return <p className="text-red-600">Error: {error}</p>;

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Machine Status</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {machines.map((m) => (
          <Link key={m.machineId} href={`/machines/${m.machineId}`}>
            <div className="bg-white rounded-lg border p-4 hover:shadow-md transition-shadow cursor-pointer">
              <div className="flex items-center justify-between mb-2">
                <span className="font-semibold">{m.name}</span>
                <StatusBadge status={m.status} />
              </div>
              <p className="text-xs text-gray-500">ID: {m.machineId}</p>
              {m.currentOrderId && (
                <p className="text-xs text-gray-500 mt-1">Order: {m.currentOrderId}</p>
              )}
              {m.status !== "Idle" && (
                <div className="mt-2">
                  <div className="flex justify-between text-xs text-gray-500 mb-0.5">
                    <span>{m.status === "CoolingDown" ? "Cool-down" : "Warm-up"}</span>
                    <span>{m.warmUpPercentage}%</span>
                  </div>
                  <div className="w-full bg-gray-100 rounded-full h-1.5">
                    <div
                      className={`h-1.5 rounded-full transition-all ${
                        m.status === "CoolingDown" ? "bg-yellow-400" : "bg-green-400"
                      }`}
                      style={{ width: `${m.warmUpPercentage}%` }}
                    />
                  </div>
                </div>
              )}
              <p className="text-xs text-gray-400 mt-1">
                Last seen: {new Date(m.lastSeen).toLocaleTimeString()}
              </p>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
