import { Skeleton } from "@/components/ui/skeleton";

export default function AuthToolsLoading() {
  return (
    <section className="card">
      <header className="card-head">
        <Skeleton style={{ height: 24, width: 240 }} />
      </header>
      <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
        {Array.from({ length: 6 }).map((_, index) => (
          <Skeleton key={index} style={{ height: 54 }} />
        ))}
      </div>
    </section>
  );
}

