import { History, Minus, Plus, TriangleAlert } from "lucide-react";
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { LoadingOverlay } from "@/shared/components/loading";
import { PageHeader } from "@/shared/components/page-header";
import { QueryState } from "@/shared/components/query-state";
import { StatTile } from "@/shared/components/stat-tile";
import { Alert, AlertDescription } from "@/shared/components/ui/alert";
import { Button, buttonVariants } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { formatMoney } from "@/shared/utils/formatters";
import { CashDepositModal } from "../components/cash-deposit-modal";
import { CashSessionEntries } from "../components/cash-session-entries";
import { CashWithdrawalModal } from "../components/cash-withdrawal-modal";
import { CloseCashForm } from "../components/close-cash-form";
import { LastCashSession } from "../components/last-cash-session";
import { OpenCashForm } from "../components/open-cash-form";
import { useCashOverview } from "../hooks/use-cash-overview";
import type {
  CloseCashSessionFormData,
  CreateCashDepositFormData,
} from "../types";

export function CashPage() {
  const [dialog, setDialog] = useState<{
    type: "deposit" | "withdrawal";
    sessionId: string;
  } | null>(null);
  const {
    overview,
    openCashSession,
    closeCashSession,
    createDeposit,
    createWithdrawal,
    isOpening,
    isClosing,
    isCreatingDeposit,
    isCreatingWithdrawal,
  } = useCashOverview();
  const currentSession = overview.data?.activeSession;
  const activeDialog =
    dialog?.sessionId === currentSession?.id ? dialog?.type : undefined;
  const canMoveCash =
    currentSession &&
    !overview.error &&
    !overview.isPending &&
    !isClosing &&
    !isOpening &&
    !isCreatingDeposit &&
    !isCreatingWithdrawal;
  const availableSessionId = canMoveCash ? currentSession.id : undefined;
  useEffect(() => {
    if (!availableSessionId || activeDialog) return;
    const onKeyDown = (event: KeyboardEvent) => {
      const key = event.key.toLowerCase();
      if (
        (key !== "a" && key !== "s") ||
        event.defaultPrevented ||
        event.repeat ||
        event.isComposing ||
        event.ctrlKey ||
        event.metaKey ||
        event.altKey ||
        event.shiftKey
      )
        return;
      if (
        (event.target instanceof HTMLElement &&
          (event.target.isContentEditable ||
            event.target.closest(
              "input, textarea, select, [role=textbox], [role=combobox]",
            ))) ||
        document.querySelector('[role="dialog"]:not([data-closed])')
      )
        return;
      event.preventDefault();
      setDialog({
        type: key === "a" ? "deposit" : "withdrawal",
        sessionId: availableSessionId,
      });
    };
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [activeDialog, availableSessionId]);
  const handleClose = async (dto: CloseCashSessionFormData) => {
    if (currentSession) await closeCashSession({ id: currentSession.id, dto });
  };
  const handleDeposit = async (dto: CreateCashDepositFormData) => {
    if (!currentSession) return;
    await createDeposit({ sessionId: currentSession.id, dto });
  };

  return (
    <div className="flex min-w-0 flex-col">
      <PageHeader
        title="Caixa"
        actions={
          <>
            <LoadingOverlay
              isFetching={overview.isFetching && !overview.isPending}
              position="inline"
            />
            <Link
              to="/cash/history"
              className={buttonVariants({
                size: "sm",
                variant: "ghost",
                className: "max-md:min-h-11",
              })}
            >
              <History data-icon="inline-start" />
              Histórico
            </Link>
            {currentSession && (
              <>
                <Button
                  size="sm"
                  variant="secondary"
                  className="max-md:min-h-11"
                  disabled={!canMoveCash}
                  aria-label="Aporte"
                  aria-keyshortcuts="A"
                  onClick={() =>
                    setDialog({ type: "deposit", sessionId: currentSession.id })
                  }
                >
                  <Plus data-icon="inline-start" />
                  Aporte
                  <kbd className="hidden font-mono text-meta md:inline">A</kbd>
                </Button>
                <Button
                  size="sm"
                  variant="secondary"
                  className="max-md:min-h-11"
                  disabled={!canMoveCash}
                  aria-label="Sangria"
                  aria-keyshortcuts="S"
                  onClick={() =>
                    setDialog({
                      type: "withdrawal",
                      sessionId: currentSession.id,
                    })
                  }
                >
                  <Minus data-icon="inline-start" />
                  Sangria
                  <kbd className="hidden font-mono text-meta md:inline">S</kbd>
                </Button>
              </>
            )}
          </>
        }
      />
      <div className="cash-page-body p-4 md:p-6">
        <QueryState
          isPending={overview.isPending}
          error={overview.error}
          hasData={overview.data !== undefined}
          refetch={overview.refetch}
        >
          <fieldset
            disabled={!!overview.error || overview.isPending}
            className="min-w-0"
          >
            {currentSession ? (
              <div className="flex flex-col gap-6">
                <div className="grid gap-3 sm:grid-cols-3">
                  <StatTile
                    label="Recebido no turno"
                    emphasis
                    value={formatMoney(currentSession.totalRevenue)}
                    foot={`${currentSession.totalOrders} ${currentSession.totalOrders === 1 ? "pedido" : "pedidos"} com recebimento`}
                  />
                  <StatTile
                    label="Sangrias"
                    value={formatMoney(currentSession.withdrawalsTotal)}
                    foot={`${currentSession.withdrawalsCount} ${currentSession.withdrawalsCount === 1 ? "sangria" : "sangrias"} no turno`}
                  />
                  <StatTile
                    label="A receber"
                    value={
                      overview.data?.receivableCash
                        ? formatMoney(overview.data.receivableCash.amount)
                        : "—"
                    }
                    foot={`${overview.data?.receivableCash?.orderCount ?? 0} ${overview.data?.receivableCash?.orderCount === 1 ? "pedido" : "pedidos"} com dinheiro a receber`}
                  />
                </div>
                <div className="grid items-start gap-4 xl:grid-cols-3">
                  <div className="flex min-w-0 flex-col gap-4 xl:col-span-2">
                    <CashSessionEntries
                      key={currentSession.id}
                      sessionId={currentSession.id}
                    />
                    <p className="text-sm text-muted-foreground">
                      A receber é uma previsão de dinheiro líquido, incluindo
                      pedidos criados sem caixa. Só recebimentos confirmados
                      nesta sessão compõem o fechamento.
                    </p>
                  </div>
                  <CloseCashForm
                    key={currentSession.id}
                    {...currentSession}
                    onSubmit={handleClose}
                    isSubmitting={isClosing}
                  />
                </div>
              </div>
            ) : (
              <div className="grid items-start gap-4 lg:grid-cols-5">
                <Card size="sm" className="lg:col-span-2">
                  <CardHeader>
                    <CardTitle>
                      <h2>Abrir caixa</h2>
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="gap-4">
                    <Alert role="note">
                      <TriangleAlert
                        aria-hidden="true"
                        className="text-warning"
                      />
                      <AlertDescription>
                        Pedidos funcionam sem caixa aberto. Esses recebimentos
                        não entram retroativamente no próximo turno.
                      </AlertDescription>
                    </Alert>
                    <OpenCashForm
                      fixedOnMobile
                      onSubmit={async (dto) => {
                        await openCashSession(dto);
                      }}
                      isSubmitting={isOpening}
                    />
                    <p className="text-sm text-muted-foreground">
                      O turno será aberto em nome do operador autenticado.
                    </p>
                  </CardContent>
                </Card>
                <div className="min-w-0 lg:col-span-3">
                  <LastCashSession
                    session={overview.data?.lastClosedSession ?? null}
                  />
                </div>
              </div>
            )}
          </fieldset>
        </QueryState>
      </div>
      {currentSession && (
        <>
          <CashDepositModal
            key={`deposit:${currentSession.id}`}
            isOpen={activeDialog === "deposit"}
            onClose={() => setDialog(null)}
            onSubmit={handleDeposit}
            isLoading={isCreatingDeposit}
          />
          <CashWithdrawalModal
            key={currentSession.id}
            sessionId={currentSession.id}
            isOpen={activeDialog === "withdrawal"}
            onClose={() => setDialog(null)}
            onSubmit={(request) =>
              createWithdrawal({ sessionId: currentSession.id, request })
            }
          />
        </>
      )}
    </div>
  );
}
