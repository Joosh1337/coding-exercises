import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { fetchBoards } from "../api/client";

export function useBoards(page: number, pageSize: number) {
  return useQuery({
    queryKey: ["boards", page, pageSize],
    queryFn: () => fetchBoards(page, pageSize),
    placeholderData: keepPreviousData,
  });
}
