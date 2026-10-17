import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { authService } from "../services/auth-service";
import type { Credentials } from "../types";

export function useAuthQuery() {
  const queryClient = useQueryClient();

  const authQuery = useQuery({
    queryKey: ["auth"],
    queryFn: authService.getAuth,
    staleTime: Number.POSITIVE_INFINITY,
    gcTime: Number.POSITIVE_INFINITY,
  });

  const loginMutation = useMutation({
    mutationFn: authService.login,
    retry: false,
    meta: { skipGlobalErrorToast: true },
    onSuccess: (data) => {
      queryClient.setQueryData(["auth"], data);
    },
  });

  const logoutMutation = useMutation({
    mutationFn: authService.logout,
    onSuccess: () => {
      queryClient.setQueryData(["auth"], { user: null, token: null });
      queryClient.clear();
    },
  });

  const authData = authQuery.data ?? { user: null, token: null };

  return {
    user: authData.user,
    token: authData.token,
    isAuthenticated: Boolean(authData.token),
    loading:
      authQuery.isLoading ||
      loginMutation.isPending ||
      logoutMutation.isPending,
    login: async (credentials: Credentials) => {
      await loginMutation.mutateAsync(credentials);
    },
    logout: async () => {
      await logoutMutation.mutateAsync();
    },
  };
}
