import type { Machine, MachineDetail } from "@/types";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8081";

export async function getMachines(): Promise<Machine[]> {
  const res = await fetch(`${API_URL}/machines`, { cache: "no-store" });
  if (!res.ok) throw new Error("Failed to fetch machines");
  return res.json();
}

export async function getMachine(id: string): Promise<MachineDetail> {
  const res = await fetch(`${API_URL}/machine/${id}`, { cache: "no-store" });
  if (!res.ok) throw new Error("Failed to fetch machine");
  return res.json();
}

export async function scheduleJob(machineId: string, productId: string, quantity: number) {
  const res = await fetch(`${API_URL}/schedulejob`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ machineId, productId, quantity }),
  });
  if (!res.ok) throw new Error("Failed to schedule job");
  return res.json();
}
