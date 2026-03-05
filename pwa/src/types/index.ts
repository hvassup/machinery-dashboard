export interface Machine {
  machineId: string;
  name: string;
  status: "Idle" | "WarmingUp" | "Processing" | "CoolingDown" | string;
  lastSeen: string;
  currentOrderId: string | null;
}

export interface MachineDetail extends Machine {
  history: MachineEventRecord[];
}

export interface MachineEventRecord {
  eventType: string;
  orderId: string | null;
  timestamp: string;
}
