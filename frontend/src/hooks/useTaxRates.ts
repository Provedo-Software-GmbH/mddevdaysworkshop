import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import type { TaxRate } from "@/types/event";

export function useTaxRates() {
  return useQuery<TaxRate[]>({
    queryKey: ["tax-rates"],
    queryFn: () => api.get<TaxRate[]>("/tax-rates"),
  });
}

export function useTaxRate(countryCode: string, id: string) {
  return useQuery<TaxRate>({
    queryKey: ["tax-rates", countryCode, id],
    queryFn: () => api.get<TaxRate>(`/tax-rates/${countryCode}/${id}`),
    enabled: !!countryCode && !!id,
  });
}

export function useCreateTaxRate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Omit<TaxRate, "id">) =>
      api.post<TaxRate>("/tax-rates", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tax-rates"] });
    },
  });
}

export function useUpdateTaxRate() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      countryCode,
      id,
      data,
    }: {
      countryCode: string;
      id: string;
      data: Omit<TaxRate, "id" | "countryCode">;
    }) => api.put<TaxRate>(`/tax-rates/${countryCode}/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tax-rates"] });
    },
  });
}
