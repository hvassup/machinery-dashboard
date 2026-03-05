import type { Metadata } from "next";
import "./globals.css";
import Link from "next/link";
import NotificationToggle from "@/components/NotificationToggle";

export const metadata: Metadata = {
  title: "Machinery Dashboard",
  description: "Real-time machinery status dashboard",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="bg-gray-50 min-h-screen">
        <nav className="bg-white border-b px-6 py-3 flex items-center gap-6 text-sm font-medium">
          <Link href="/" className="text-blue-600 hover:underline">Dashboard</Link>
          <Link href="/schedule" className="text-blue-600 hover:underline">Schedule Job</Link>
          <div className="ml-auto">
            <NotificationToggle />
          </div>
        </nav>
        <main className="p-6">{children}</main>
      </body>
    </html>
  );
}
