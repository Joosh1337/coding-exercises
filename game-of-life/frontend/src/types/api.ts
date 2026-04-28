export interface ApiResponse<T> {
  data: T;
  message: string;
}

export interface ErrorResponse {
  errorCode: number;
  message: string;
  details?: string[];
}

export interface BoardResponse {
  id: string;
  name: string;
  generation: number;
  width: number;
  height: number;
  liveCells: number[][];
}

export interface BoardRepresentationResponse {
  id: string;
  generation: number;
  width: number;
  height: number;
  boardDisplay: number[][];
}

export interface CreateBoardRequest {
  name?: string;
  width: number;
  height: number;
  initialCells: number[][];
}

export interface CreateBoardResult {
  id: string;
}
