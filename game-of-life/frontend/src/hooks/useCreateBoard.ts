import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createBoard } from "../api/client";

export function useCreateBoard() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createBoard,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["boards"] });
    },
  });
}
