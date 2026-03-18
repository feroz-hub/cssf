import { Skeleton } from "@/components/ui/skeleton";

export default function UsersLoading() {
  return (
    <section className="card">
      <header className="card-head">
        <Skeleton style={{ height: 24, width: 220 }} />
        <Skeleton style={{ height: 36, width: 220 }} />
      </header>
      <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
        {Array.from({ length: 8 }).map((_, index) => (
          <Skeleton key={index} style={{ height: 42 }} />
        ))}
      </div>
    </section>
  );
}
