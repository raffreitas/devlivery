import { useEffect } from "react";
import { type UseFormReturn, useFieldArray, useWatch } from "react-hook-form";
import { moneyDifference, sumMoney } from "@/shared/utils/money";
import type { OrderFormData, PaymentMethod } from "../types";

export function useOrderPayments(
  form: UseFormReturn<OrderFormData>,
  subtotal: number,
) {
  const {
    fields: paymentFields,
    append,
    remove,
  } = useFieldArray({
    control: form.control,
    name: "payments",
  });

  const deliveryFee =
    useWatch({ control: form.control, name: "deliveryFee" }) ?? 0;
  const payments = useWatch({ control: form.control, name: "payments" });

  const total = sumMoney([subtotal, deliveryFee]);
  const totalPaid = sumMoney(
    (payments ?? []).map((payment) => payment.amount || 0),
  );
  const electronicTotal = sumMoney(
    (payments ?? [])
      .filter((payment) => payment.method !== "Cash")
      .map((payment) => payment.amount || 0),
  );
  const hasElectronicExcess = moneyDifference(electronicTotal, total) > 0;
  const remainingAmount = moneyDifference(total, totalPaid);

  useEffect(() => {
    if (
      paymentFields.length === 1 &&
      payments?.[0]?.amount !== total &&
      payments?.[0]?.method !== "Cash"
    ) {
      form.setValue(`payments.0.amount`, total, { shouldValidate: true });
    }
  }, [total, paymentFields.length, form, payments]);

  const changePaymentMethods = (values: string[]) => {
    const methods = values as PaymentMethod[];
    // Pelo menos uma forma de pagamento deve permanecer selecionada
    if (methods.length === 0) return;

    const removedIndex = paymentFields.findIndex(
      (p) => !methods.includes(p.method),
    );
    if (removedIndex >= 0) {
      remove(removedIndex);
      return;
    }

    const added = methods.find(
      (method) => !paymentFields.some((p) => p.method === method),
    );
    if (added) {
      append({ method: added, amount: Math.max(0, remainingAmount) });
    }
  };

  return {
    paymentFields,
    deliveryFee,
    total,
    remainingAmount,
    hasElectronicExcess,
    changePaymentMethods,
  };
}
