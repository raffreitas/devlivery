import { useRef } from "react";
import { useReactToPrint } from "react-to-print";

export function usePrintOrder() {
  const contentRef = useRef<HTMLDivElement>(null);

  const handlePrint = useReactToPrint({
    contentRef,
    documentTitle: "Pedido - Devlivery",
    pageStyle: `
      @page {
        size: 55mm auto;
        margin: 1mm;
      }

      body {
        font-family: 'Courier New', monospace;
        font-size: 12px;
        line-height: 1.4;
        width: 55mm;
        margin: 0 auto;
        padding: 2mm;
        padding-right: 5mm; /* Specific for printing */
        -webkit-print-color-adjust: exact;
        print-color-adjust: exact;
        font-weight: 550;
      }

      .print-receipt {
        width: 100%;
      }

      .text-center {
        text-align: center;
      }

      .text-xl {
        font-size: 16px;
      }

      .text-lg {
        font-size: 14px;
      }

      .text-sm {
        font-size: 10px;
      }

      .text-xs {
        font-size: 9px;
      }

      .font-bold {
        font-weight: bold;
      }

      .font-semibold {
        font-weight: 600;
      }

      .mb-2 {
        margin-bottom: 4px;
      }

      .mb-3 {
        margin-bottom: 6px;
      }

      .mb-4 {
        margin-bottom: 8px;
      }

      .mt-2 {
        margin-top: 4px;
      }

      .ml-4 {
        margin-left: 8px;
      }

      .py-2 {
        padding-top: 4px;
        padding-bottom: 4px;
      }

      .pt-2 {
        padding-top: 4px;
      }

      .border-t {
        border-top: 1px solid #333;
      }

      .border-t-2 {
        border-top: 2px solid #333;
      }

      .border-b-2 {
        border-bottom: 2px solid #333;
      }

      .border-dashed {
        border-style: dashed;
      }

      .border-gray-800 {
        border-color: #333;
      }

      .text-gray-600 {
        color: #666;
      }

      .flex {
        display: flex;
      }

      .justify-between {
        justify-content: space-between;
      }
    `,
  });

  return {
    contentRef,
    handlePrint,
  };
}
