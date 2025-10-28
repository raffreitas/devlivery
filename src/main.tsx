import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { App } from "./app.tsx";

// biome-ignore lint/style/noNonNullAssertion: <explanation> We are sure that 'root' element exists in the HTML.</explanation>
createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
