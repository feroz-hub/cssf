import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState
} from "react";

type Density = "compact" | "default" | "comfortable";

type DensityContextValue = {
  density: Density;
  setDensity: (value: Density) => void;
};

const DensityContext = createContext<DensityContextValue | null>(null);

const STORAGE_KEY = "HCL.CS.SF-density";

function applyDensity(value: Density) {
  if (typeof document === "undefined") return;
  document.documentElement.setAttribute("data-density", value);
}

export function DensityProvider({ children }: { children: ReactNode }) {
  const [density, setDensityState] = useState<Density>("default");

  useEffect(() => {
    if (typeof window === "undefined") return;
    const stored = window.localStorage.getItem(STORAGE_KEY) as Density | null;
    if (stored === "compact" || stored === "default" || stored === "comfortable") {
      setDensityState(stored);
      applyDensity(stored);
    } else {
      applyDensity("default");
    }
  }, []);

  const setDensity = useCallback((value: Density) => {
    setDensityState(value);
    applyDensity(value);
    try {
      window.localStorage.setItem(STORAGE_KEY, value);
    } catch {
      // ignore
    }
  }, []);

  const value = useMemo(() => ({ density, setDensity }), [density, setDensity]);

  return <DensityContext.Provider value={value}>{children}</DensityContext.Provider>;
}

export function useDensity(): DensityContextValue {
  const ctx = useContext(DensityContext);
  if (!ctx) {
    throw new Error("useDensity must be used within DensityProvider");
  }

  return ctx;
}

