import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateBoard } from "../api/client";

export function useUpdateBoard() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      id,
      ...rest
    }: {
      id: string;
      name: string;
      width: number;
      height: number;
      initialCells: number[][];
    }) => updateBoard(id, rest),
    onSuccess: (_data, { id }) => {
      queryClient.invalidateQueries({ queryKey: ["boards"] });
      queryClient.invalidateQueries({ queryKey: ["board", id] });
    },
  });
}
