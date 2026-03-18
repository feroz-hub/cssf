import { Skeleton } from "@/components/ui/skeleton";

export default function ExternalAuthLoading() {
  return (
    <section className="card">
      <header className="card-head">
        <Skeleton style={{ height: 24, width: 240 }} />
        <Skeleton style={{ height: 36, width: 150 }} />
      </header>
      <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
        {Array.from({ length: 4 }).map((_, index) => (
          <Skeleton key={index} style={{ height: 60 }} />
        ))}
      </div>
    </section>
  );
}
