import { createRoot } from "react-dom/client";
import { OrderPrint } from "../components/order-print";
import type { Order } from "../types";

export function printOrder(order: Order) {
  // Create a hidden iframe for printing
  const iframe = document.createElement("iframe");
  iframe.style.position = "absolute";
  iframe.style.width = "0";
  iframe.style.height = "0";
  iframe.style.border = "none";
  document.body.appendChild(iframe);

  const iframeDoc = iframe.contentDocument || iframe.contentWindow?.document;
  if (!iframeDoc) return;

  // Write the HTML structure
  iframeDoc.open();
  iframeDoc.write(`
    <!DOCTYPE html>
    <html>
      <head>
        <meta charset="UTF-8">
        <title>Imprimir Pedido ${order.id}</title>
        <style>
          @media print {
            @page {
              size: 50mm auto;
              margin: 2mm;
            }
            body {
              margin: 0;
              padding: 0;
            }
          }
          
          body {
            font-family: 'Courier New', monospace;
            font-size: 12px;
            line-height: 1.4;
            width: 50mm;
            margin: 0 auto;
            padding: 2mm;
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
        </style>
      </head>
      <body>
        <div id="root"></div>
      </body>
    </html>
  `);
  iframeDoc.close();

  // Wait for iframe to load
  iframe.onload = () => {
    const rootElement = iframeDoc.getElementById("root");
    if (rootElement) {
      // Render the React component
      const root = createRoot(rootElement);
      root.render(OrderPrint({ order }));

      // Wait a bit for rendering to complete, then print
      setTimeout(() => {
        iframe.contentWindow?.print();

        // Clean up after printing
        setTimeout(() => {
          document.body.removeChild(iframe);
        }, 1000);
      }, 500);
    }
  };
}
