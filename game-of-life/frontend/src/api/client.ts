import type {
  ApiResponse,
  BoardRepresentationResponse,
  BoardResponse,
  CreateBoardRequest,
  CreateBoardResult,
  ErrorResponse,
} from "../types/api";

const BASE_URL = "http://localhost:5141/api/boards";

async function handleResponse<T>(res: Response): Promise<T> {
  if (res.status === 204) return undefined as T;
  if (!res.ok) {
    const err: ErrorResponse = await res.json();
    throw new Error(err.message);
  }
  const body: ApiResponse<T> = await res.json();
  return body.data;
}

export async function fetchBoards(
  page: number,
  pageSize: number
): Promise<BoardResponse[]> {
  const res = await fetch(
    `${BASE_URL}?page=${page}&pageSize=${pageSize}`
  );
  return handleResponse<BoardResponse[]>(res);
}

export async function fetchBoard(id: string): Promise<BoardResponse> {
  const res = await fetch(`${BASE_URL}/${id}`);
  return handleResponse<BoardResponse>(res);
}

export async function createBoard(
  req: CreateBoardRequest
): Promise<CreateBoardResult> {
  const res = await fetch(BASE_URL, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(req),
  });
  return handleResponse<CreateBoardResult>(res);
}

export async function deleteBoard(id: string): Promise<void> {
  const res = await fetch(`${BASE_URL}/${id}`, { method: "DELETE" });
  return handleResponse<void>(res);
}

export async function fetchNextState(
  id: string
): Promise<BoardRepresentationResponse> {
  const res = await fetch(`${BASE_URL}/${id}/states/next`);
  return handleResponse<BoardRepresentationResponse>(res);
}

export async function fetchStatesAhead(
  id: string,
  steps: number
): Promise<BoardRepresentationResponse> {
  const res = await fetch(`${BASE_URL}/${id}/states?steps=${steps}`);
  return handleResponse<BoardRepresentationResponse>(res);
}

export async function fetchFinalState(
  id: string
): Promise<BoardRepresentationResponse> {
  const res = await fetch(`${BASE_URL}/${id}/states/final`);
  return handleResponse<BoardRepresentationResponse>(res);
}
