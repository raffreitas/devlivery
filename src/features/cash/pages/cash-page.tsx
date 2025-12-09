import { WalletIcon } from "lucide-react";
import { useState } from "react";
import { CashDepositForm } from "@/features/cash/components/cash-deposit-form";
import { CashPaymentBreakdown } from "@/features/cash/components/cash-payment-breakdown";
import { CashSummaryCard } from "@/features/cash/components/cash-summary-card";
import { CloseCashForm } from "@/features/cash/components/close-cash-form";
import { OpenCashForm } from "@/features/cash/components/open-cash-form";
import { useCashSessions } from "@/features/cash/hooks/use-cash-sessions";
import type { CreateCashDepositFormData } from "@/features/cash/types";
import { CashModal } from "@/shared/components/cash-modal";
import { Button } from "@/shared/components/ui/button";

export function CashPage() {
  const [isOpenCashModalOpen, setIsOpenCashModalOpen] = useState(false);
  const [isCloseCashModalOpen, setIsCloseCashModalOpen] = useState(false);
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
    setIsOpenCashModalOpen(false);
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
    setIsCloseCashModalOpen(false);
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
    expectedCashAmount - (currentSession?.openingAmount ?? 0) - depositsTotal;

  return (
    <>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            Controle de Caixa
          </h1>
          <p className="text-gray-600 mt-2">
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
                onOpenClose={() => setIsCloseCashModalOpen(true)}
                onAddDeposit={() => setIsDepositModalOpen(true)}
              />

              <CashPaymentBreakdown
                paymentBreakdown={currentSession.paymentBreakdown}
                totalRevenue={currentSession.salesTotals.totalRevenue}
              />
            </div>
          ) : (
            <div className="p-6 sm:p-8 rounded-lg border-2 border-dashed border-gray-300 bg-gray-50 text-center">
              <WalletIcon className="w-9 h-9 mx-auto text-muted-foreground mb-3" />
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Nenhum caixa aberto
              </h3>
              <p className="text-sm text-text-muted-foreground mb-4">
                Abra um novo caixa para começar a registrar vendas e controlar o
                fluxo de caixa
              </p>
              <Button
                onClick={() => setIsOpenCashModalOpen(true)}
                className="bg-green-600 hover:bg-green-700"
              >
                <WalletIcon className="w-4 h-4 mr-2" />
                Abrir Caixa
              </Button>
            </div>
          )}
        </div>
      </div>

      {/* Open Cash Modal */}
      <CashModal
        isOpen={isOpenCashModalOpen}
        onClose={() => setIsOpenCashModalOpen(false)}
        title="Abrir Caixa"
      >
        <OpenCashForm
          onSubmit={handleOpenCash}
          isSubmitting={isOpening}
          onCancel={() => setIsOpenCashModalOpen(false)}
        />
      </CashModal>

      {/* Close Cash Modal */}
      <CashModal
        isOpen={isCloseCashModalOpen}
        onClose={() => setIsCloseCashModalOpen(false)}
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
            onCancel={() => setIsCloseCashModalOpen(false)}
          />
        )}
      </CashModal>

      {/* Deposit Cash Modal */}
      <CashModal
        isOpen={isDepositModalOpen}
        onClose={() => setIsDepositModalOpen(false)}
        title="Adicionar Aporte de Caixa"
      >
        <CashDepositForm
          onSubmit={handleCreateDeposit}
          isLoading={isCreatingDeposit}
        />
      </CashModal>
    </>
  );
}
