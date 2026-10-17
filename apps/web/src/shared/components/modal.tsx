import { cn } from "cn";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
} from "@/shared/components/ui/drawer";
import { useMediaQuery } from "@/shared/hooks/use-media-query";

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
  dismissible?: boolean;
}

/**
 * Modal responsivo: Dialog a partir do breakpoint lg e Drawer abaixo dele.
 */
export function Modal({
  isOpen,
  onClose,
  title,
  description,
  children,
  footer,
  dismissible = true,
}: ModalProps) {
  const isDesktop = useMediaQuery("(min-width: 1024px)");
  const handleOpenChange = (open: boolean) => {
    if (!open && dismissible) onClose();
  };

  if (isDesktop) {
    return (
      <Dialog open={isOpen} onOpenChange={handleOpenChange}>
        <DialogContent
          className={cn(
            "max-h-[calc(100dvh-2rem)] gap-4 sm:max-w-3xl",
            footer
              ? "grid-rows-[auto_minmax(0,1fr)_auto]"
              : "grid-rows-[auto_minmax(0,1fr)]",
          )}
        >
          <DialogHeader>
            <DialogTitle className="text-base">{title}</DialogTitle>
            {description && (
              <DialogDescription>{description}</DialogDescription>
            )}
          </DialogHeader>
          <div className="-mx-6 overflow-y-auto px-6">{children}</div>
          {footer && <DialogFooter>{footer}</DialogFooter>}
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Drawer open={isOpen} onOpenChange={handleOpenChange} showSwipeHandle>
      <DrawerContent>
        <DrawerHeader>
          <DrawerTitle className="text-base">{title}</DrawerTitle>
          {description && <DrawerDescription>{description}</DrawerDescription>}
        </DrawerHeader>
        <div className="min-h-0 flex-1 overflow-y-auto p-4">{children}</div>
        {footer && <DrawerFooter>{footer}</DrawerFooter>}
      </DrawerContent>
    </Drawer>
  );
}
