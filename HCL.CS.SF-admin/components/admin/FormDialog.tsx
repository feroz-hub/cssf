"use client";

import { type ReactNode } from "react";

import { Button } from "@/components/ui/button";
import { Dialog } from "@/components/ui/dialog";

type Props = {
  open: boolean;
  title: string;
  description?: string;
  submitLabel?: string;
  cancelLabel?: string;
  pending?: boolean;
  onClose: () => void;
  onSubmit: () => void;
  children: ReactNode;
};

export function FormDialog({
  open,
  title,
  description,
  submitLabel = "Save",
  cancelLabel = "Cancel",
  pending = false,
  onClose,
  onSubmit,
  children
}: Props) {
  return (
    <Dialog open={open} title={title} description={description} onClose={onClose}>
      <div className="form-grid">
        {children}
        <div className="dialog-actions">
          <Button type="button" variant="secondary" onClick={onClose} disabled={pending}>
            {cancelLabel}
          </Button>
          <Button type="button" onClick={onSubmit} disabled={pending}>
            {pending ? "Saving..." : submitLabel}
          </Button>
        </div>
      </div>
    </Dialog>
  );
}

