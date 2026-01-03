import { WalletIcon } from "lucide-react";
import { useState } from "react";
import { CashDepositForm } from "@/features/cash/components/cash-deposit-form";
import { CashPaymentBreakdown } from "@/features/cash/components/cash-payment-breakdown";
import { CashSummaryCard } from "@/features/cash/components/cash-summary-card";
import { CloseCashForm } from "@/features/cash/components/close-cash-form";
import { OpenCashForm } from "@/features/cash/components/open-cash-form";
import { useCashSessions } from "@/features/cash/hooks/use-cash-sessions";
import type { CreateCashDepositFormData } from "@/features/cash/types";
import { Modal } from "@/shared/components/modal";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent } from "@/shared/components/ui/card";

export function CashPage() {
  const [isOpenModalOpen, setIsOpenModalOpen] = useState(false);
  const [isCloseModalOpen, setIsCloseModalOpen] = useState(false);
  const [isDepositModalOpen, setIsDepositModalOpen] = useState(false);

  const {
    currentSession,
    deposits,
    openCashSession,
    closeCashSession,
    createDeposit,
    isOpening,
    isClosing,
    isCreatingDeposit,
  } = useCashSessions();

  const handleOpenCash = async (dto: {
    openingAmount: number;
    notes?: string;
  }) => {
    await openCashSession(dto);
    setIsOpenModalOpen(false);
  };

  const handleCloseCash = async (dto: {
    closingAmount: number;
    notes?: string;
  }) => {
    if (!currentSession) return;
    // Backend now calculates sales totals and payment breakdown automatically
    await closeCashSession({
      id: currentSession.id,
      dto,
    });
    setIsCloseModalOpen(false);
  };

  const handleCreateDeposit = async (dto: CreateCashDepositFormData) => {
    if (!currentSession) return;
    await createDeposit({
      sessionId: currentSession.id,
      dto,
    });
    setIsDepositModalOpen(false);
  };

  const expectedCashAmount = currentSession?.expectedCashAmount ?? 0;
  const depositsTotal = deposits?.reduce((sum, d) => sum + d.amount, 0) ?? 0;
  const cashSales =
    currentSession?.paymentBreakdown.find((p) => p.method === "Cash")?.amount ??
    0;

  return (
    <>
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl sm:text-3xl font-bold text-foreground">
            Controle de Caixa
          </h1>
          <p className="text-muted-foreground mt-2">
            Gerencie abertura, fechamento e resumo de caixa
          </p>
        </div>

        {/* Cash Session Section */}
        <div className="space-y-4">
          {currentSession ? (
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
              <CashSummaryCard
                session={currentSession}
                deposits={deposits}
                onOpenClose={() => setIsCloseModalOpen(true)}
                onAddDeposit={() => setIsDepositModalOpen(true)}
              />

              <CashPaymentBreakdown
                paymentBreakdown={currentSession.paymentBreakdown}
                totalRevenue={currentSession.salesTotals.totalRevenue}
              />
            </div>
          ) : (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-16 px-6">
                <div className="flex items-center justify-center w-16 h-16 sm:w-20 sm:h-20 rounded-full bg-orange-100 dark:bg-orange-950 text-orange-600 dark:text-orange-400 mb-4">
                  <WalletIcon className="w-8 h-8 sm:w-10 sm:h-10" />
                </div>
                <h3 className="text-xl font-semibold text-foreground mb-2">
                  Nenhum caixa aberto
                </h3>
                <p className="text-sm text-muted-foreground text-center max-w-md mb-6">
                  Abra um novo caixa para começar a registrar vendas e controlar
                  o fluxo de caixa do seu estabelecimento
                </p>
                <Button onClick={() => setIsOpenModalOpen(true)} size="lg">
                  <WalletIcon className="w-4 h-4 mr-2" />
                  Abrir Caixa
                </Button>
              </CardContent>
            </Card>
          )}
        </div>
      </div>

      {/* Open Cash Modal */}
      <Modal
        isOpen={isOpenModalOpen}
        onClose={() => setIsOpenModalOpen(false)}
        title="Abrir Caixa"
      >
        <OpenCashForm
          onSubmit={handleOpenCash}
          isSubmitting={isOpening}
          onCancel={() => setIsOpenModalOpen(false)}
        />
      </Modal>

      {/* Close Cash Modal */}
      <Modal
        isOpen={isCloseModalOpen}
        onClose={() => setIsCloseModalOpen(false)}
        title="Fechar Caixa"
      >
        {currentSession && (
          <CloseCashForm
            expectedCashAmount={expectedCashAmount}
            openingAmount={currentSession.openingAmount}
            depositsTotal={depositsTotal}
            cashSales={cashSales}
            onSubmit={handleCloseCash}
            isSubmitting={isClosing}
            onCancel={() => setIsCloseModalOpen(false)}
          />
        )}
      </Modal>

      {/* Deposit Cash Modal */}
      <Modal
        isOpen={isDepositModalOpen}
        onClose={() => setIsDepositModalOpen(false)}
        title="Adicionar Aporte de Caixa"
      >
        <CashDepositForm
          onSubmit={handleCreateDeposit}
          isLoading={isCreatingDeposit}
        />
      </Modal>
    </>
  );
}
