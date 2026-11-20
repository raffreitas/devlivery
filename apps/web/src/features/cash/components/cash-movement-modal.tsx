import { LockKeyhole } from "lucide-react";
import { type ComponentProps, type ReactNode, useMemo } from "react";
import { LoadingButton } from "@/shared/components/loading";
import { Modal } from "@/shared/components/modal";
import { Alert, AlertDescription } from "@/shared/components/ui/alert";
import { Button } from "@/shared/components/ui/button";
import { Separator } from "@/shared/components/ui/separator";
import { readAuth } from "@/shared/services/auth-storage";
import { formatCashTime } from "../utils/format-cash-date-time";

interface CashMovementModalProps {
  isOpen: boolean;
  isBusy: boolean;
  title: string;
  description: string;
  formId: string;
  submitLabel: string;
  loadingText: string;
  onClose: () => void;
  onSubmit: ComponentProps<"form">["onSubmit"];
  children: ReactNode;
}

export function CashMovementModal({
  isOpen,
  isBusy,
  title,
  description,
  formId,
  submitLabel,
  loadingText,
  onClose,
  onSubmit,
  children,
}: CashMovementModalProps) {
  const openedAt = useMemo(
    () => (isOpen ? new Date().toISOString() : null),
    [isOpen],
  );
  const operatorName = readAuth().user?.name?.trim();

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      dismissible={!isBusy}
      size="sm"
      title={title}
      description={description}
      initialFocus={() => document.getElementById(`${formId}-amount`)}
      footer={
        <div className="flex w-full flex-col gap-4">
          <Separator />
          <div className="flex flex-wrap items-center justify-end gap-2">
            {operatorName && openedAt && (
              <p className="mr-auto min-w-0 flex-1 wrap-anywhere text-meta text-muted-foreground max-sm:basis-full">
                {operatorName} ·{" "}
                <time className="whitespace-nowrap" dateTime={openedAt}>
                  {formatCashTime(openedAt)}
                </time>
              </p>
            )}
            <Button
              type="button"
              variant="outline"
              className="max-md:min-h-11"
              disabled={isBusy}
              onClick={onClose}
            >
              Cancelar
            </Button>
            <LoadingButton
              type="submit"
              form={formId}
              isLoading={isBusy}
              loadingText={loadingText}
              className="max-md:min-h-11"
              aria-label={submitLabel}
              aria-keyshortcuts="Control+Enter"
            >
              {submitLabel}
              <kbd className="ml-2 hidden rounded-sm bg-primary-foreground/10 px-1 text-meta md:inline">
                Ctrl+Enter
              </kbd>
            </LoadingButton>
          </div>
        </div>
      }
    >
      <form
        id={formId}
        className="flex flex-col gap-4"
        onSubmit={onSubmit}
        onKeyDown={(event) => {
          if (
            event.key !== "Enter" ||
            !event.ctrlKey ||
            event.repeat ||
            event.nativeEvent.isComposing ||
            event.defaultPrevented
          )
            return;
          event.preventDefault();
          if (!isBusy) event.currentTarget.requestSubmit();
        }}
      >
        <Separator />
        <fieldset
          disabled={isBusy}
          inert={isBusy}
          className="flex min-w-0 flex-col gap-4"
        >
          {children}
          <Alert role="note" className="border-dashed bg-transparent">
            <LockKeyhole aria-hidden="true" />
            <AlertDescription>
              O saldo da gaveta só aparece na conferência do fechamento, após a
              contagem.
            </AlertDescription>
          </Alert>
        </fieldset>
      </form>
    </Modal>
  );
}
