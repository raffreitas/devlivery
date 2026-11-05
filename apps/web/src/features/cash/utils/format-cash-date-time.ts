const dateTime = new Intl.DateTimeFormat("pt-BR", {
  day: "2-digit",
  month: "2-digit",
  hour: "2-digit",
  minute: "2-digit",
  timeZone: "America/Sao_Paulo",
});

export function formatCashDateTime(value: string) {
  return dateTime.format(new Date(value));
}

const time = new Intl.DateTimeFormat("pt-BR", {
  hour: "2-digit",
  minute: "2-digit",
  timeZone: "America/Sao_Paulo",
});

export function formatCashTime(value: string) {
  return time.format(new Date(value));
}

const historyDateTime = new Intl.DateTimeFormat("pt-BR", {
  dateStyle: "short",
  timeStyle: "short",
  timeZone: "America/Sao_Paulo",
});

export function formatCashHistoryDateTime(value: string) {
  return historyDateTime.format(new Date(value));
}
