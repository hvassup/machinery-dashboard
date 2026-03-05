"use client";

import { useEffect, useState } from "react";
import { registerAndSubscribe, unsubscribe, isSubscribed } from "@/lib/push";

export default function NotificationToggle() {
  const [mounted, setMounted] = useState(false);
  const [subscribed, setSubscribed] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setMounted(true);
    isSubscribed().then(setSubscribed).catch(() => {});
  }, []);

  const toggle = async () => {
    setLoading(true);
    setError(null);
    try {
      if (subscribed) {
        await unsubscribe();
        setSubscribed(false);
      } else {
        await registerAndSubscribe();
        setSubscribed(true);
      }
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : "Failed");
    } finally {
      setLoading(false);
    }
  };

  if (!mounted || !("Notification" in window)) return null;

  return (
    <div className="flex items-center gap-2">
      {error && <span className="text-red-500 text-xs">{error}</span>}
      <button
        onClick={toggle}
        disabled={loading}
        className={`text-xs px-3 py-1 rounded border font-medium transition-colors ${
          subscribed
            ? "bg-green-100 border-green-400 text-green-800 hover:bg-green-200"
            : "bg-gray-100 border-gray-300 text-gray-700 hover:bg-gray-200"
        } disabled:opacity-50`}
      >
        {loading ? "..." : subscribed ? "Notifications on" : "Enable notifications"}
      </button>
    </div>
  );
}
