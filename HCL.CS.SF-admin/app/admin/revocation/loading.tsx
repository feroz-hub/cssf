import { Skeleton } from "@/components/ui/skeleton";

export default function RevocationLoading() {
  return (
    <section className="card">
      <header className="card-head">
        <Skeleton style={{ height: 24, width: 280 }} />
      </header>
      <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
        {Array.from({ length: 7 }).map((_, index) => (
          <Skeleton key={index} style={{ height: 42 }} />
        ))}
      </div>
    </section>
  );
}
