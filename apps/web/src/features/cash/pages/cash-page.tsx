import { History, Minus, Plus, TriangleAlert } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { LoadingOverlay } from "@/shared/components/loading";
import { Modal } from "@/shared/components/modal";
import { PageHeader } from "@/shared/components/page-header";
import { QueryState } from "@/shared/components/query-state";
import { StatTile } from "@/shared/components/stat-tile";
import { Alert, AlertDescription } from "@/shared/components/ui/alert";
import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { formatMoney } from "@/shared/utils/formatters";
import { CashDepositForm } from "../components/cash-deposit-form";
import { CashSessionEntries } from "../components/cash-session-entries";
import { CloseCashForm } from "../components/close-cash-form";
import { LastCashSession } from "../components/last-cash-session";
import { OpenCashForm } from "../components/open-cash-form";
import { useCashOverview } from "../hooks/use-cash-overview";
import type {
  CloseCashSessionFormData,
  CreateCashDepositFormData,
} from "../types";

export function CashPage() {
  const [depositModalOpen, setDepositModalOpen] = useState(false);
  const {
    overview,
    openCashSession,
    closeCashSession,
    createDeposit,
    isOpening,
    isClosing,
    isCreatingDeposit,
  } = useCashOverview();
  const currentSession = overview.data?.activeSession;
  const disabled =
    !!overview.error ||
    overview.isPending ||
    isClosing ||
    isOpening ||
    isCreatingDeposit;
  const handleClose = async (dto: CloseCashSessionFormData) => {
    if (currentSession) await closeCashSession({ id: currentSession.id, dto });
  };
  const handleDeposit = async (dto: CreateCashDepositFormData) => {
    if (!currentSession) return;
    await createDeposit({ sessionId: currentSession.id, dto });
    setDepositModalOpen(false);
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
            <Button
              size="sm"
              variant="ghost"
              render={<Link to="/cash/history" />}
            >
              <History data-icon="inline-start" />
              Histórico
            </Button>
            {currentSession && (
              <>
                <Button
                  size="sm"
                  variant="secondary"
                  disabled={disabled}
                  onClick={() => setDepositModalOpen(true)}
                >
                  <Plus data-icon="inline-start" />
                  Aporte
                </Button>
                <Button
                  size="sm"
                  variant="secondary"
                  disabled
                  title="Sangria disponível futuramente"
                >
                  <Minus data-icon="inline-start" />
                  Sangria
                </Button>
              </>
            )}
          </>
        }
      />
      <div className="p-4 md:p-6">
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
                  <StatTile label="Sangrias" value="—" foot="Em breve" />
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
                      key={`${currentSession.id}:${overview.dataUpdatedAt}`}
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
      <Modal
        dismissible={!isCreatingDeposit}
        isOpen={depositModalOpen}
        onClose={() => setDepositModalOpen(false)}
        title="Adicionar aporte"
      >
        <CashDepositForm
          onSubmit={handleDeposit}
          isLoading={isCreatingDeposit}
        />
      </Modal>
    </div>
  );
}
