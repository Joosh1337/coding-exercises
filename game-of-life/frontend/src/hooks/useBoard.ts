import { useQuery } from "@tanstack/react-query";
import { fetchBoard } from "../api/client";

export function useBoard(id: string) {
  return useQuery({
    queryKey: ["board", id],
    queryFn: () => fetchBoard(id),
    enabled: !!id,
  });
}
