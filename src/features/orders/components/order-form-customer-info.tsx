import { AutocompleteSelect } from "@/shared/components/autocomplete-select";
import { Input } from "@/shared/components/input";
import { getPaymentOptions } from "../constants/payment-methods";
import type { PaymentMethod } from "../types";

interface CustomerInfoSectionProps {
  customerName: string;
  deliveryAddress: string;
  deliveryFee: number;
  paymentMethod: PaymentMethod | null;
  onCustomerNameChange: (value: string) => void;
  onDeliveryAddressChange: (value: string) => void;
  onDeliveryFeeChange: (value: number) => void;
  onPaymentMethodChange: (value: PaymentMethod | null) => void;
}

export function CustomerInfoSection({
  customerName,
  deliveryAddress,
  deliveryFee,
  paymentMethod,
  onCustomerNameChange,
  onDeliveryAddressChange,
  onDeliveryFeeChange,
  onPaymentMethodChange,
}: CustomerInfoSectionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold">Dados do Cliente</h3>

      <Input
        label="Nome do Cliente"
        type="text"
        value={customerName}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
          onCustomerNameChange(e.target.value)
        }
        required
      />

      <Input
        label="Endereço de Entrega"
        type="text"
        value={deliveryAddress}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
          onDeliveryAddressChange(e.target.value)
        }
        required
      />

      <Input
        label="Taxa de Entrega"
        type="number"
        min="0"
        value={deliveryFee}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
          onDeliveryFeeChange(Number.parseFloat(e.target.value || "") || 0)
        }
        required
      />

      <AutocompleteSelect<PaymentMethod>
        id="payment-select"
        label="Método de Pagamento"
        placeholder="Selecione um método de pagamento"
        value={paymentMethod}
        autocomplete={false}
        onChange={onPaymentMethodChange}
        options={getPaymentOptions()}
      />
    </div>
  );
}
