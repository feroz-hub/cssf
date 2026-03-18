"use client";

import { type ReactNode } from "react";

import { Button } from "@/components/ui/button";

type AlertDialogProps = {
  open: boolean;
  title: string;
  description: string;
  confirmLabel?: string;
  cancelLabel?: string;
  confirmVariant?: "primary" | "secondary" | "danger" | "ghost";
  disabled?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
  children?: ReactNode;
};

export function AlertDialog({
  open,
  title,
  description,
  confirmLabel = "Confirm",
  cancelLabel = "Cancel",
  confirmVariant = "danger",
  disabled = false,
  onConfirm,
  onCancel,
  children
}: AlertDialogProps) {
  if (!open) {
    return null;
  }

  return (
    <div className="modal-overlay" role="alertdialog" aria-modal="true">
      <div className="modal-card modal-alert">
        <h3>{title}</h3>
        <p>{description}</p>
        {children}
        <div className="dialog-actions">
          <Button type="button" variant="ghost" onClick={onCancel} disabled={disabled}>
            {cancelLabel}
          </Button>
          <Button type="button" variant={confirmVariant} onClick={onConfirm} disabled={disabled}>
            {confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}
