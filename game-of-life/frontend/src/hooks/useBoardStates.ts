import { useMutation } from "@tanstack/react-query";
import { fetchFinalState, fetchNextState, fetchStatesAhead } from "../api/client";

export function useNextState() {
  return useMutation({ mutationFn: fetchNextState });
}

export function useStatesAhead() {
  return useMutation({
    mutationFn: ({ id, steps }: { id: string; steps: number }) =>
      fetchStatesAhead(id, steps),
  });
}

export function useFinalState() {
  return useMutation({ mutationFn: fetchFinalState });
}
