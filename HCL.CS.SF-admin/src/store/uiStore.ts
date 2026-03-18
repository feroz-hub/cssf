import { create } from "zustand";

const STORAGE_KEY = "HCL.CS.SF-sidebar-collapsed";

function getInitialCollapsed(): boolean {
  if (typeof window === "undefined") return false;
  try {
    return window.localStorage.getItem(STORAGE_KEY) === "true";
  } catch {
    return false;
  }
}

type UiState = {
  navCollapsed: boolean;
  activeRoute: string;
  setNavCollapsed: (value: boolean) => void;
  toggleNavCollapsed: () => void;
  setActiveRoute: (route: string) => void;
};

export const useUiStore = create<UiState>((set, get) => ({
  navCollapsed: getInitialCollapsed(),
  activeRoute: "/future-dashboard",
  setNavCollapsed: (value) => {
    set({ navCollapsed: value });
    try {
      window.localStorage.setItem(STORAGE_KEY, String(value));
    } catch {}
  },
  toggleNavCollapsed: () => {
    const next = !get().navCollapsed;
    set({ navCollapsed: next });
    try {
      window.localStorage.setItem(STORAGE_KEY, String(next));
    } catch {}
  },
  setActiveRoute: (route) => set({ activeRoute: route }),
}));
