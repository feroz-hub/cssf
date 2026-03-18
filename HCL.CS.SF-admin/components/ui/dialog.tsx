"use client";

import { type ReactNode } from "react";

import { Button } from "@/components/ui/button";

type DialogProps = {
  open: boolean;
  title: string;
  description?: string;
  children: ReactNode;
  onClose: () => void;
};

export function Dialog({ open, title, description, children, onClose }: DialogProps) {
  if (!open) {
    return null;
  }

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true">
      <div className="modal-card">
        <header className="modal-header">
          <div>
            <h3>{title}</h3>
            {description ? <p>{description}</p> : null}
          </div>
          <Button type="button" variant="ghost" onClick={onClose}>
            Close
          </Button>
        </header>
        <div className="modal-body">{children}</div>
      </div>
    </div>
  );
}
