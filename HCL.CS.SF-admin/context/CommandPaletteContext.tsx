"use client";

import {
  createContext,
  type KeyboardEvent as ReactKeyboardEvent,
  type ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState
} from "react";

import { useRouter } from "next/navigation";

type Command = {
  id: string;
  label: string;
  hint?: string;
  href?: string;
  group: "navigate" | "action" | "view";
};

type CommandPaletteContextValue = {
  open: boolean;
  query: string;
  setQuery: (value: string) => void;
  openPalette: () => void;
  closePalette: () => void;
  commands: Command[];
};

const CommandPaletteContext = createContext<CommandPaletteContextValue | null>(null);

const STATIC_COMMANDS: Command[] = [
  // Navigation
  { id: "nav-dashboard", label: "Dashboard", hint: "Control center overview", href: "/admin", group: "navigate" },
  { id: "nav-users", label: "Users", hint: "User accounts and lockout", href: "/admin/users", group: "navigate" },
  { id: "nav-roles", label: "Roles & Claims", hint: "Role definitions and claims", href: "/admin/roles", group: "navigate" },
  { id: "nav-clients", label: "Clients", hint: "Manage OAuth/OIDC clients", href: "/admin/clients", group: "navigate" },
  { id: "nav-resources", label: "Resources & Scopes", hint: "API resources and scopes", href: "/admin/resources", group: "navigate" },
  { id: "nav-revocation", label: "Revocation", hint: "Revoke active tokens", href: "/admin/revocation", group: "navigate" },
  { id: "nav-audit", label: "Audit Log", hint: "System audit events", href: "/admin/audit", group: "navigate" },
  { id: "nav-operations", label: "API Explorer", hint: "Inspect and test APIs", href: "/admin/operations/api-explorer", group: "navigate" },

  // Quick actions (implemented as navigation to relevant pages)
  { id: "action-create-user", label: "Create user", hint: "Open user administration", href: "/admin/users", group: "action" },
  { id: "action-create-role", label: "Create role", hint: "Open role management", href: "/admin/roles", group: "action" },
  { id: "action-register-client", label: "Register client", hint: "Open client management", href: "/admin/clients", group: "action" }
];

export function CommandPaletteProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const [activeIndex, setActiveIndex] = useState(0);

  const commands = STATIC_COMMANDS;

  const openPalette = useCallback(() => {
    setOpen(true);
  }, []);

  const closePalette = useCallback(() => {
    setOpen(false);
    setQuery("");
    setActiveIndex(0);
  }, []);

  useEffect(() => {
    const handler = (event: KeyboardEvent) => {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setOpen((value) => !value);
      }
    };

    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);

  const value = useMemo(
    () => ({
      open,
      query,
      setQuery,
      openPalette,
      closePalette,
      commands
    }),
    [open, query, openPalette, closePalette, commands]
  );

  const filteredCommands = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) return commands;
    return commands.filter((cmd) => {
      const inLabel = cmd.label.toLowerCase().includes(term);
      const inHint = cmd.hint && cmd.hint.toLowerCase().includes(term);
      const inHref = cmd.href && cmd.href.toLowerCase().includes(term);
      const inGroup =
        (term === "go" || term === "nav") && cmd.group === "navigate"
          ? true
          : (term === "create" || term === "new") && cmd.group === "action";
      return inLabel || inHint || inHref || inGroup;
    });
  }, [commands, query]);

  const onKeyDown = (event: ReactKeyboardEvent<HTMLDivElement>) => {
    if (!open) return;
    if (event.key === "ArrowDown") {
      event.preventDefault();
      setActiveIndex((index) => Math.min(index + 1, Math.max(filteredCommands.length - 1, 0)));
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      setActiveIndex((index) => Math.max(index - 1, 0));
    } else if (event.key === "Enter") {
      event.preventDefault();
      const cmd = filteredCommands[activeIndex];
      if (cmd?.href) {
        closePalette();
        router.push(cmd.href);
      }
    } else if (event.key === "Escape") {
      event.preventDefault();
      closePalette();
    }
  };

  return (
    <CommandPaletteContext.Provider value={value}>
      {children}
      {open ? (
        <div className="command-overlay" role="dialog" aria-modal="true" onKeyDown={onKeyDown}>
          <div className="command-dialog">
            <div className="command-input-row">
              <input
                autoFocus
                className="command-input"
                placeholder="Search HCL.CS.SF Admin…"
                value={query}
                onChange={(event) => setQuery(event.target.value)}
              />
            </div>
            <div className="command-list" aria-label="Available commands">
              {filteredCommands.map((cmd, index) => (
                <button
                  key={cmd.id}
                  type="button"
                  className={
                    index === activeIndex ? "command-item command-item-active" : "command-item"
                  }
                  onClick={() => {
                    if (!cmd.href) return;
                    closePalette();
                    router.push(cmd.href);
                  }}
                >
                  <div className="command-item-main">
                    <span className="command-item-label">{cmd.label}</span>
                    {cmd.hint ? <span className="command-item-hint">{cmd.hint}</span> : null}
                  </div>
                  <span className="command-item-kbd">{cmd.group === "action" ? "action" : cmd.href}</span>
                </button>
              ))}
              {filteredCommands.length === 0 ? (
                <div className="command-empty text-caption">No matches. Try a different term.</div>
              ) : null}
            </div>
          </div>
        </div>
      ) : null}
    </CommandPaletteContext.Provider>
  );
}

export function useCommandPalette(): CommandPaletteContextValue {
  const ctx = useContext(CommandPaletteContext);
  if (!ctx) {
    throw new Error("useCommandPalette must be used within CommandPaletteProvider");
  }

  return ctx;
}

