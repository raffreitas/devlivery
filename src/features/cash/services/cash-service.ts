import { authService } from "@/features/auth/services/auth-service";
import { type ApiResponse, api } from "@/shared/services/api";
import type {
  CashDeposit,
  CashSession,
  CloseCashSessionFormData,
  CreateCashDepositFormData,
  CreateCashSessionFormData,
  PaymentMethodTotal,
} from "../types";

// DTOs from backend
interface CashSessionDto {
  id: string;
  attendantId: string;
  attendantName: string;
  openingAmount: number;
  closingAmount: number | null;
  expectedCashAmount: number;
  totalRevenue: number;
  totalOrders: number;
  paymentBreakdown: Array<{
    method: string;
    amount: number;
    count: number;
  }>;
  startAt: string;
  endAt: string | null;
  status: string;
  notes: string | null;
}

interface CreateCashSessionPayload {
  attendantId: string;
  attendantName: string;
  openingAmount: number;
  notes?: string;
}

interface CloseCashSessionPayload {
  closingAmount: number;
  notes?: string;
}

interface CashDepositDto {
  id: string;
  cashSessionId: string;
  attendantId: string;
  attendantName: string;
  amount: number;
  depositedAt: string;
  notes: string | null;
}

interface CreateCashDepositPayload {
  attendantId: string;
  attendantName: string;
  amount: number;
  notes?: string;
}

function mapDtoToDomain(dto: CashSessionDto): CashSession {
  return {
    id: dto.id,
    attendant: dto.attendantName,
    openingAmount: dto.openingAmount,
    closingAmount: dto.closingAmount ?? undefined,
    expectedCashAmount: dto.expectedCashAmount,
    startAt: dto.startAt,
    endAt: dto.endAt ?? undefined,
    notes: dto.notes ?? undefined,
    status: dto.status as "open" | "closed",
    salesTotals: {
      totalRevenue: dto.totalRevenue,
      totalOrders: dto.totalOrders,
    },
    paymentBreakdown:
      dto.paymentBreakdown?.map(
        (pb): PaymentMethodTotal => ({
          method: pb.method,
          amount: pb.amount,
          count: pb.count,
        }),
      ) ?? [],
  };
}

function mapDepositDtoToDomain(dto: CashDepositDto): CashDeposit {
  return {
    id: dto.id,
    cashSessionId: dto.cashSessionId,
    amount: dto.amount,
    depositedAt: dto.depositedAt,
    attendant: dto.attendantName,
    notes: dto.notes ?? undefined,
  };
}

export const cashService = {
  async getAll(): Promise<CashSession[]> {
    const response =
      await api.get<ApiResponse<CashSessionDto[]>>("/api/cash-register/sessions");
    return response.data?.map(mapDtoToDomain) ?? [];
  },

  async getById(id: string): Promise<CashSession> {
    const response = await api.get<ApiResponse<CashSessionDto>>(
      `/api/cash-register/sessions/${id}`,
    );
    return mapDtoToDomain(response.data ?? ({} as CashSessionDto));
  },

  async getActive(): Promise<CashSession | null> {
    try {
      const response = await api.get<ApiResponse<CashSessionDto>>(
        "/api/cash-register/sessions/active",
      );
      return mapDtoToDomain(response.data ?? ({} as CashSessionDto));
    } catch (error) {
      // 404 means no active session
      if (error && typeof error === "object" && "status" in error) {
        if (error.status === 404) {
          return null;
        }
      }
      throw error;
    }
  },

  async create(dto: CreateCashSessionFormData): Promise<CashSession> {
    const authData = authService.getAuth();
    if (!authData.user || !authData.token) {
      throw new Error("Usuário não autenticado");
    }

    // TODO: In future, get user info in backend from token and remove from payload
    const { id, name } = authData.user;

    const payload: CreateCashSessionPayload = {
      attendantId: id,
      attendantName: name,
      openingAmount: dto.openingAmount,
      notes: dto.notes,
    };

    const response = await api.post<ApiResponse<CashSessionDto>>(
      "/api/cash-register/sessions",
      payload,
    );
    if (!response.success || !response.data) {
      throw new Error(response.message || "Falha ao criar sessão de caixa");
    }

    return mapDtoToDomain(response.data);
  },

  async close(id: string, dto: CloseCashSessionFormData): Promise<CashSession> {
    const payload: CloseCashSessionPayload = {
      closingAmount: dto.closingAmount,
      notes: dto.notes,
    };

    const response = await api.put<ApiResponse<CashSessionDto>>(
      `/api/cash-register/sessions/${id}/close`,
      payload,
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Falha ao fechar sessão de caixa");
    }
    return mapDtoToDomain(response.data ?? ({} as CashSessionDto));
  },

  async createDeposit(
    sessionId: string,
    dto: CreateCashDepositFormData,
  ): Promise<CashDeposit> {
    const authData = authService.getAuth();
    if (!authData.user || !authData.token) {
      throw new Error("Usuário não autenticado");
    }

    const { id, name } = authData.user;

    const payload: CreateCashDepositPayload = {
      attendantId: id,
      attendantName: name,
      amount: dto.amount,
      notes: dto.notes,
    };

    const response = await api.post<ApiResponse<CashDepositDto>>(
      `/api/cash-register/sessions/${sessionId}/deposits`,
      payload,
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Falha ao criar aporte de caixa");
    }

    return mapDepositDtoToDomain(response.data);
  },

  async getDeposits(sessionId: string): Promise<CashDeposit[]> {
    const response = await api.get<ApiResponse<CashDepositDto[]>>(
      `/api/cash-register/sessions/${sessionId}/deposits`,
    );
    return response.data?.map(mapDepositDtoToDomain) ?? [];
  },
};
