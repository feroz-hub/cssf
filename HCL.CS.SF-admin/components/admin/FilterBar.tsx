"use client";

import { type ReactNode } from "react";

type Props = {
  children: ReactNode;
};

export function FilterBar({ children }: Props) {
  return <div className="filter-bar">{children}</div>;
}

