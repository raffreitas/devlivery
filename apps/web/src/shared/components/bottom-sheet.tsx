import { X } from "lucide-react";
import { type ReactNode, useEffect } from "react";

interface BottomSheetProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: ReactNode;
}

export function BottomSheet({
  isOpen,
  onClose,
  title,
  children,
}: BottomSheetProps) {
  // Bloquear scroll do body quando aberto
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }

    return () => {
      document.body.style.overflow = "";
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <>
      {/* Backdrop */}
      <button
        type="button"
        className="fixed inset-0 bg-black/50 z-[9998] transition-opacity duration-300 cursor-default"
        onClick={onClose}
        aria-label="Fechar filtros"
        tabIndex={-1}
      />

      {/* Bottom Sheet */}
      <div
        role="dialog"
        aria-modal="true"
        className="fixed inset-x-0 bottom-0 z-[9999] bg-background text-foreground rounded-t-2xl shadow-2xl max-h-[90vh] flex flex-col animate-slide-up"
        style={{ paddingBottom: "env(safe-area-inset-bottom)" }}
      >
        {/* Handle bar */}
        <div className="flex justify-center pt-3 pb-2">
          <div className="w-12 h-1 bg-muted rounded-full" />
        </div>

        {/* Header */}
        {title && (
          <div className="flex items-center justify-between px-4 pb-3 border-b border-border">
            <h2 className="text-lg font-semibold text-foreground">{title}</h2>
            <button
              type="button"
              onClick={onClose}
              className="p-1 rounded-full hover:bg-accent/10 transition-colors"
              aria-label="Fechar"
            >
              <X className="w-5 h-5 text-muted-foreground" />
            </button>
          </div>
        )}

        {/* Content */}
        <div className="flex-1 overflow-y-auto px-4 py-4">{children}</div>
      </div>
    </>
  );
}
