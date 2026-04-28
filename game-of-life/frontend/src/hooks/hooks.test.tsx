import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { useBoard } from './useBoard';
import { useBoards } from './useBoards';
import { useStatesAhead, useFinalState } from './useBoardStates';
import { useCreateBoard } from './useCreateBoard';
import { useDeleteBoard } from './useDeleteBoard';
import { useUpdateBoard } from './useUpdateBoard';

vi.mock('../api/client');
import * as client from '../api/client';

function makeWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
      mutations: { retry: false },
    },
  });
  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

const sampleBoard = {
  id: 'b1',
  name: 'Test',
  generation: 0,
  width: 3,
  height: 3,
  liveCells: [] as number[][],
};

const sampleState = {
  id: 'b1',
  generation: 1,
  width: 3,
  height: 3,
  boardDisplay: [[0, 0, 0], [0, 0, 0], [0, 0, 0]],
};

beforeEach(() => vi.clearAllMocks());

describe('useBoard', () => {
  it('fetches a single board by id', async () => {
    vi.mocked(client.fetchBoard).mockResolvedValue(sampleBoard);
    const { result } = renderHook(() => useBoard('b1'), { wrapper: makeWrapper() });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual(sampleBoard);
    expect(client.fetchBoard).toHaveBeenCalledWith('b1');
  });

  it('is disabled when id is empty', () => {
    const { result } = renderHook(() => useBoard(''), { wrapper: makeWrapper() });
    expect(result.current.fetchStatus).toBe('idle');
  });
});

describe('useBoards', () => {
  it('fetches boards with pagination params', async () => {
    vi.mocked(client.fetchBoards).mockResolvedValue([sampleBoard]);
    const { result } = renderHook(() => useBoards(1, 10), { wrapper: makeWrapper() });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual([sampleBoard]);
    expect(client.fetchBoards).toHaveBeenCalledWith(1, 10);
  });
});

describe('useStatesAhead', () => {
  it('calls fetchStatesAhead with id and steps', async () => {
    vi.mocked(client.fetchStatesAhead).mockResolvedValue(sampleState);
    const { result } = renderHook(() => useStatesAhead(), { wrapper: makeWrapper() });
    await act(async () => {
      result.current.mutate({ id: 'b1', steps: 5 });
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(client.fetchStatesAhead).toHaveBeenCalledWith('b1', 5);
  });
});

describe('useFinalState', () => {
  it('calls fetchFinalState when mutated', async () => {
    vi.mocked(client.fetchFinalState).mockResolvedValue(sampleState);
    const { result } = renderHook(() => useFinalState(), { wrapper: makeWrapper() });
    await act(async () => {
      result.current.mutate('b1');
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(client.fetchFinalState).toHaveBeenCalledWith('b1', expect.anything());
  });
});

describe('useCreateBoard', () => {
  it('calls createBoard and invalidates boards query on success', async () => {
    vi.mocked(client.createBoard).mockResolvedValue({ id: 'new-id' });
    const { result } = renderHook(() => useCreateBoard(), { wrapper: makeWrapper() });
    await act(async () => {
      result.current.mutate({ width: 5, height: 5, initialCells: [] });
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(client.createBoard).toHaveBeenCalled();
  });
});

describe('useDeleteBoard', () => {
  it('calls deleteBoard and invalidates boards query on success', async () => {
    vi.mocked(client.deleteBoard).mockResolvedValue(undefined);
    const { result } = renderHook(() => useDeleteBoard(), { wrapper: makeWrapper() });
    await act(async () => {
      result.current.mutate('b1');
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(client.deleteBoard).toHaveBeenCalledWith('b1', expect.anything());
  });
});

describe('useUpdateBoard', () => {
  it('calls updateBoard with the correct args and invalidates queries on success', async () => {
    vi.mocked(client.updateBoard).mockResolvedValue(undefined);
    const { result } = renderHook(() => useUpdateBoard(), { wrapper: makeWrapper() });
    await act(async () => {
      result.current.mutate({ id: 'b1', name: 'New', width: 3, height: 3, initialCells: [] });
    });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(client.updateBoard).toHaveBeenCalledWith('b1', {
      name: 'New',
      width: 3,
      height: 3,
      initialCells: [],
    });
  });
});
