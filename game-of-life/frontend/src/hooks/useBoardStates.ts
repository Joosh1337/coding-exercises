import { useMutation } from "@tanstack/react-query";
import { fetchFinalState, fetchStatesAhead } from "../api/client";

export function useStatesAhead() {
  return useMutation({
    mutationFn: ({ id, steps }: { id: string; steps: number }) =>
      fetchStatesAhead(id, steps),
  });
}

export function useFinalState() {
  return useMutation({ mutationFn: fetchFinalState });
}
