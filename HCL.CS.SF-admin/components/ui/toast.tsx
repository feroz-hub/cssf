import { cn } from "@/lib/utils";

export type ToastKind = "success" | "error" | "info";

export type ToastItem = {
  id: string;
  title: string;
  kind: ToastKind;
};

type ToastProps = {
  item: ToastItem;
  onDismiss: (id: string) => void;
};

export function Toast({ item, onDismiss }: ToastProps) {
  return (
    <div className={cn("toast", `toast-${item.kind}`)}>
      <span>{item.title}</span>
      <button type="button" onClick={() => onDismiss(item.id)} aria-label="Dismiss notification">
        ×
      </button>
    </div>
  );
}
