import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  fetchBoards,
  fetchBoard,
  createBoard,
  deleteBoard,
  fetchNextState,
  fetchStatesAhead,
  fetchFinalState,
  updateBoard,
} from './client';

const mockFetch = vi.fn();
vi.stubGlobal('fetch', mockFetch);

function makeOkResponse(data: unknown) {
  return {
    ok: true,
    status: 200,
    json: async () => ({ data, message: 'ok' }),
  } as unknown as Response;
}

function makeErrorResponse(status: number, message: string) {
  return {
    ok: false,
    status,
    json: async () => ({ errorCode: status, message }),
  } as unknown as Response;
}

function make204Response() {
  return { ok: true, status: 204, json: async () => ({}) } as unknown as Response;
}

beforeEach(() => mockFetch.mockReset());

describe('fetchBoards', () => {
  it('returns boards array on success', async () => {
    const boards = [{ id: '1', name: 'Test', width: 5, height: 5, liveCells: [], generation: 0 }];
    mockFetch.mockResolvedValue(makeOkResponse(boards));
    const result = await fetchBoards(1, 10);
    expect(result).toEqual(boards);
    expect(mockFetch).toHaveBeenCalledWith('http://localhost:5141/api/boards?page=1&pageSize=10');
  });

  it('throws on error response', async () => {
    mockFetch.mockResolvedValue(makeErrorResponse(500, 'Server error'));
    await expect(fetchBoards(1, 10)).rejects.toThrow('Server error');
  });
});

describe('fetchBoard', () => {
  it('returns board on success', async () => {
    const board = { id: 'abc', name: 'B', width: 3, height: 3, liveCells: [], generation: 0 };
    mockFetch.mockResolvedValue(makeOkResponse(board));
    const result = await fetchBoard('abc');
    expect(result).toEqual(board);
    expect(mockFetch).toHaveBeenCalledWith('http://localhost:5141/api/boards/abc');
  });

  it('throws on 404', async () => {
    mockFetch.mockResolvedValue(makeErrorResponse(404, 'Not found'));
    await expect(fetchBoard('missing')).rejects.toThrow('Not found');
  });
});

describe('createBoard', () => {
  it('posts and returns id', async () => {
    mockFetch.mockResolvedValue(makeOkResponse({ id: 'new-id' }));
    const result = await createBoard({ width: 5, height: 5, initialCells: [], name: 'New' });
    expect(result).toEqual({ id: 'new-id' });
    expect(mockFetch).toHaveBeenCalledWith(
      'http://localhost:5141/api/boards',
      expect.objectContaining({ method: 'POST' })
    );
  });

  it('throws on error', async () => {
    mockFetch.mockResolvedValue(makeErrorResponse(400, 'Bad request'));
    await expect(createBoard({ width: 5, height: 5, initialCells: [] })).rejects.toThrow('Bad request');
  });
});

describe('deleteBoard', () => {
  it('sends DELETE and returns undefined on 204', async () => {
    mockFetch.mockResolvedValue(make204Response());
    const result = await deleteBoard('abc');
    expect(result).toBeUndefined();
    expect(mockFetch).toHaveBeenCalledWith(
      'http://localhost:5141/api/boards/abc',
      { method: 'DELETE' }
    );
  });

  it('throws on error', async () => {
    mockFetch.mockResolvedValue(makeErrorResponse(404, 'Not found'));
    await expect(deleteBoard('missing')).rejects.toThrow('Not found');
  });
});

describe('fetchNextState', () => {
  it('returns board representation', async () => {
    const state = { id: 'x', generation: 1, width: 3, height: 3, boardDisplay: [[0]] };
    mockFetch.mockResolvedValue(makeOkResponse(state));
    const result = await fetchNextState('x');
    expect(result).toEqual(state);
    expect(mockFetch).toHaveBeenCalledWith('http://localhost:5141/api/boards/x/states/next');
  });
});

describe('fetchStatesAhead', () => {
  it('fetches with correct steps param', async () => {
    const state = { id: 'x', generation: 5, width: 3, height: 3, boardDisplay: [[0]] };
    mockFetch.mockResolvedValue(makeOkResponse(state));
    const result = await fetchStatesAhead('x', 5);
    expect(result).toEqual(state);
    expect(mockFetch).toHaveBeenCalledWith('http://localhost:5141/api/boards/x/states?steps=5');
  });
});

describe('fetchFinalState', () => {
  it('fetches final state', async () => {
    const state = { id: 'x', generation: 99, width: 3, height: 3, boardDisplay: [[1]] };
    mockFetch.mockResolvedValue(makeOkResponse(state));
    const result = await fetchFinalState('x');
    expect(result).toEqual(state);
    expect(mockFetch).toHaveBeenCalledWith('http://localhost:5141/api/boards/x/states/final');
  });
});

describe('updateBoard', () => {
  it('sends PUT and returns undefined on 204', async () => {
    mockFetch.mockResolvedValue(make204Response());
    const result = await updateBoard('abc', { name: 'N', width: 5, height: 5, initialCells: [] });
    expect(result).toBeUndefined();
    expect(mockFetch).toHaveBeenCalledWith(
      'http://localhost:5141/api/boards/abc',
      expect.objectContaining({ method: 'PUT' })
    );
  });

  it('throws on error', async () => {
    mockFetch.mockResolvedValue(makeErrorResponse(400, 'Invalid'));
    await expect(
      updateBoard('x', { name: '', width: 0, height: 0, initialCells: [] })
    ).rejects.toThrow('Invalid');
  });
});
