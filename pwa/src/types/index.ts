export interface Machine {
  machineId: string;
  name: string;
  status: "Idle" | "WarmingUp" | "Processing" | "CoolingDown" | string;
  lastSeen: string;
  currentOrderId: string | null;
}

export interface ScheduledOrder {
  orderId: string;
  productId: string;
  quantity: number;
  scheduledAt: string;
}

export interface HistoricalOrder {
  orderId: string;
  productId: string;
  quantity: number;
  startedAt: string | null;
  finishedAt: string | null;
}

export interface MachineDetail extends Machine {
  scheduledOrders: ScheduledOrder[];
  historicalOrders: HistoricalOrder[];
  history: MachineEventRecord[];
}

export interface MachineEventRecord {
  eventType: string;
  orderId: string | null;
  timestamp: string;
}
