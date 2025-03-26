export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-4 md:p-8">
      <div className="w-full max-w-md">
        <div className="mb-8 flex flex-col items-center text-center">
          <h1 className="text-3xl font-bold tracking-tight">Kogase</h1>
          <p className="text-sm text-muted-foreground">
            Game Analytics Platform
          </p>
        </div>
        {children}
      </div>
    </div>
  );
} 