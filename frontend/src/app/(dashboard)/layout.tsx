"use client";

import { useEffect } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { LucideHome, LucideBarChart2, LucideList, LucideSettings, LucideLogOut } from "lucide-react";

import { useAuth } from "@/lib/hooks/use-auth";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const { user, loading, logout } = useAuth();

  useEffect(() => {
    if (!loading && !user) {
      router.push("/login");
    }
  }, [user, loading, router]);

  const handleLogout = async () => {
    await logout();
    router.push("/login");
  };

  if (loading) {
    return (
      <div className="flex h-screen flex-col">
        <header className="border-b">
          <div className="container flex h-16 items-center justify-between">
            <div className="flex items-center gap-4">
              <Skeleton className="h-8 w-24" />
            </div>
            <div className="flex items-center gap-4">
              <Skeleton className="h-8 w-8 rounded-full" />
              <Skeleton className="h-4 w-24" />
            </div>
          </div>
        </header>
        <div className="container flex flex-1">
          <aside className="w-64 border-r pr-4 pt-8">
            <nav className="flex flex-col gap-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </nav>
          </aside>
          <main className="flex-1 p-8">
            <Skeleton className="h-8 w-64 mb-6" />
            <Skeleton className="h-64 w-full rounded-lg" />
          </main>
        </div>
      </div>
    );
  }

  const navItems = [
    {
      label: "Dashboard",
      href: "/dashboard",
      icon: LucideHome,
    },
    {
      label: "Analytics",
      href: "/dashboard/analytics",
      icon: LucideBarChart2,
    },
    {
      label: "Events",
      href: "/dashboard/events",
      icon: LucideList,
    },
    {
      label: "Settings",
      href: "/dashboard/settings",
      icon: LucideSettings,
    },
  ];

  return (
    <div className="flex h-screen flex-col">
      <header className="border-b">
        <div className="container flex h-16 items-center justify-between">
          <div className="flex items-center gap-4">
            <h1 className="text-xl font-bold">Kogase</h1>
          </div>
          <div className="flex items-center gap-4">
            <span className="text-sm text-muted-foreground">
              {user?.name || user?.email}
            </span>
            <Button variant="outline" size="sm" onClick={handleLogout}>
              <LucideLogOut className="mr-2 h-4 w-4" />
              Log out
            </Button>
          </div>
        </div>
      </header>
      <div className="container flex flex-1">
        <aside className="w-64 border-r pr-4 pt-8">
          <nav className="flex flex-col gap-2">
            {navItems.map((item) => {
              const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-2 rounded-lg px-3 py-2 transition-colors ${
                    isActive
                      ? "bg-primary text-primary-foreground"
                      : "hover:bg-muted"
                  }`}
                >
                  <item.icon className="h-5 w-5" />
                  {item.label}
                </Link>
              );
            })}
          </nav>
        </aside>
        <main className="flex-1 p-8">
          {children}
        </main>
      </div>
    </div>
  );
} 