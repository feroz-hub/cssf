"use client";

import { type ReactNode } from "react";

type Column = {
  key: string;
  label: string;
  sortable?: boolean;
  sortDirection?: "asc" | "desc" | null;
  onSort?: () => void;
};

type Props =
  | {
      columns: string[];
      children: ReactNode;
      empty?: ReactNode;
    }
  | {
      columns: Column[];
      children: ReactNode;
      empty?: ReactNode;
    };

function isColumnObjectArray(columns: Props["columns"]): columns is Column[] {
  return typeof (columns as Column[])[0] === "object";
}

export function DataTable({ columns, children, empty }: Props) {
  return (
    <div className="table-wrap">
      <table className="table">
        <thead>
          <tr>
            {isColumnObjectArray(columns)
              ? columns.map((col) => (
                  <th
                    key={col.key}
                    onClick={col.sortable && col.onSort ? col.onSort : undefined}
                    style={col.sortable ? { cursor: "pointer", userSelect: "none" } : undefined}
                  >
                    <span>
                      {col.label}
                      {col.sortable && col.sortDirection
                        ? col.sortDirection === "asc"
                          ? " ↑"
                          : " ↓"
                        : null}
                    </span>
                  </th>
                ))
              : columns.map((col) => <th key={col}>{col}</th>)}
          </tr>
        </thead>
        <tbody>{children || empty}</tbody>
      </table>
    </div>
  );
}

