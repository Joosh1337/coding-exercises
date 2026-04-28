import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, act, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { BoardSimulationPage } from './BoardSimulationPage';
import type { BoardResponse, BoardRepresentationResponse } from '../types/api';

vi.mock('../hooks/useBoard');
vi.mock('../hooks/useBoardStates');
vi.mock('../api/client');
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useParams: () => ({ id: 'board-xyz' }),
  };
});

import { useBoard } from '../hooks/useBoard';
import { useFinalState, useStatesAhead } from '../hooks/useBoardStates';
import * as clientModule from '../api/client';

const mockUseBoard = vi.mocked(useBoard);
const mockUseFinalState = vi.mocked(useFinalState);
const mockUseStatesAhead = vi.mocked(useStatesAhead);
const mockFetchStatesAhead = vi.mocked(clientModule.fetchStatesAhead);

const sampleBoard: BoardResponse = {
  id: 'board-xyz',
  name: 'Test Board',
  generation: 0,
  width: 3,
  height: 3,
  liveCells: [[0, 0], [1, 1]],
};

const nextStateResponse: BoardRepresentationResponse = {
  id: 'board-xyz',
  generation: 1,
  width: 3,
  height: 3,
  boardDisplay: [[0, 0, 0], [0, 1, 0], [0, 0, 0]],
};

const statesAheadMutation = {
  mutate: vi.fn(),
  isPending: false,
  isError: false,
  error: null,
};

const finalStateMutation = {
  mutate: vi.fn(),
  isPending: false,
  isError: false,
  error: null,
};

function renderPage() {
  return render(
    <MemoryRouter>
      <BoardSimulationPage />
    </MemoryRouter>
  );
}

beforeEach(() => {
  vi.clearAllMocks();
  mockUseStatesAhead.mockReturnValue(statesAheadMutation as ReturnType<typeof useStatesAhead>);
  mockUseFinalState.mockReturnValue(finalStateMutation as ReturnType<typeof useFinalState>);
});

describe('BoardSimulationPage', () => {
  it('shows loading spinner while board loads', () => {
    mockUseBoard.mockReturnValue({ isLoading: true, isError: false, data: undefined, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(document.querySelector('.animate-spin')).toBeInTheDocument();
  });

  it('shows error when board fetch fails', () => {
    mockUseBoard.mockReturnValue({
      isLoading: false,
      isError: true,
      data: undefined,
      error: new Error('Board not found'),
    } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByText('Board not found')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /back to boards/i })).toBeInTheDocument();
  });

  it('shows board name and generation 0', () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByText('Test Board')).toBeInTheDocument();
    expect(screen.getByText('0')).toBeInTheDocument(); // generation counter
  });

  it('shows "Unnamed board" for boards with no name', () => {
    mockUseBoard.mockReturnValue({
      isLoading: false,
      isError: false,
      data: { ...sampleBoard, name: '' },
      error: null,
    } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByText(/unnamed board/i)).toBeInTheDocument();
  });

  it('calls statesAhead mutation when Next is clicked', async () => {
    const mutate = vi.fn();
    mockUseStatesAhead.mockReturnValue({ ...statesAheadMutation, mutate } as ReturnType<typeof useStatesAhead>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /next/i }));
    expect(mutate).toHaveBeenCalledWith(
      { id: 'board-xyz', steps: 1 },
      expect.any(Object)
    );
  });

  it('calls finalState mutation when End is clicked', async () => {
    const mutate = vi.fn();
    mockUseFinalState.mockReturnValue({ ...finalStateMutation, mutate } as ReturnType<typeof useFinalState>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /end/i }));
    expect(mutate).toHaveBeenCalledWith('board-xyz', expect.any(Object));
  });

  it('Previous and Start buttons are disabled at generation 0', () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByRole('button', { name: /previous/i })).toBeDisabled();
    expect(screen.getByRole('button', { name: /start/i })).toBeDisabled();
  });

  it('shows error message from statesAhead mutation', () => {
    mockUseStatesAhead.mockReturnValue({
      ...statesAheadMutation,
      isError: true,
      error: new Error('Steps error'),
    } as ReturnType<typeof useStatesAhead>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByText('Steps error')).toBeInTheDocument();
  });

  it('shows error message from finalState mutation', () => {
    mockUseFinalState.mockReturnValue({
      ...finalStateMutation,
      isError: true,
      error: new Error('No final state'),
    } as ReturnType<typeof useFinalState>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByText('No final state')).toBeInTheDocument();
  });

  it('shows Play button when not playing', () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByRole('button', { name: /play/i })).toBeInTheDocument();
  });

  it('switches to Stop button when Play is clicked', async () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    mockFetchStatesAhead.mockResolvedValue(nextStateResponse);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /play/i }));
    expect(screen.getByRole('button', { name: /stop/i })).toBeInTheDocument();
  });

  it('stops playing when Stop is clicked', async () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    mockFetchStatesAhead.mockResolvedValue(nextStateResponse);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /play/i }));
    await userEvent.click(screen.getByRole('button', { name: /stop/i }));
    expect(screen.getByRole('button', { name: /play/i })).toBeInTheDocument();
  });

  it('shows Restart button after reaching final state via End', async () => {
    const mutate = vi.fn().mockImplementation((_id, { onSuccess }) => {
      onSuccess({ generation: 10, width: 3, height: 3, boardDisplay: [[0, 0, 0], [0, 0, 0], [0, 0, 0]] });
    });
    mockUseFinalState.mockReturnValue({ ...finalStateMutation, mutate } as ReturnType<typeof useFinalState>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /end/i }));
    expect(screen.getByRole('button', { name: /restart/i })).toBeInTheDocument();
  });

  it('advances history when Next onSuccess is called', async () => {
    const mutate = vi.fn().mockImplementation((_args, { onSuccess }) => {
      onSuccess(nextStateResponse);
    });
    mockUseStatesAhead.mockReturnValue({ ...statesAheadMutation, mutate } as ReturnType<typeof useStatesAhead>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /next/i }));
    expect(screen.getByText('1')).toBeInTheDocument(); // generation 1
  });

  it('Jump button calls statesAhead with custom steps', async () => {
    const mutate = vi.fn();
    mockUseStatesAhead.mockReturnValue({ ...statesAheadMutation, mutate } as ReturnType<typeof useStatesAhead>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /jump/i }));
    expect(mutate).toHaveBeenCalledWith(
      { id: 'board-xyz', steps: 1 }, // default jumpSteps is 1, generation is 0, so steps = 0+1=1
      expect.any(Object)
    );
  });

  it('clicking Restart restarts play from generation 0', async () => {
    const finalMutate = vi.fn().mockImplementation((_id, { onSuccess }) => {
      onSuccess({ generation: 5, width: 3, height: 3, boardDisplay: [[0, 0, 0], [0, 0, 0], [0, 0, 0]] });
    });
    mockUseFinalState.mockReturnValue({ ...finalStateMutation, mutate: finalMutate } as ReturnType<typeof useFinalState>);
    mockFetchStatesAhead.mockResolvedValue(nextStateResponse);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    // Reach final state
    await userEvent.click(screen.getByRole('button', { name: /end/i }));
    // Now Restart should be visible
    const restart = screen.getByRole('button', { name: /restart/i });
    await userEvent.click(restart);
    // After restart, Stop should appear (because it starts playing)
    expect(screen.getByRole('button', { name: /stop/i })).toBeInTheDocument();
  });

  it('updates play speed when slider is changed', async () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    const slider = screen.getByRole('slider');
    fireEvent.change(slider, { target: { value: '200' } });
    expect(screen.getByText('200ms')).toBeInTheDocument();
  });

  it('updates jump steps when input is changed', async () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    const jumpInput = screen.getByRole('spinbutton');
    fireEvent.change(jumpInput, { target: { value: '5' } });
    expect(jumpInput).toHaveValue(5);
  });

  it('navigates back when Prev is clicked after going Next', async () => {
    const mutate = vi.fn().mockImplementation((_args, { onSuccess }) => {
      onSuccess(nextStateResponse);
    });
    mockUseStatesAhead.mockReturnValue({ ...statesAheadMutation, mutate } as ReturnType<typeof useStatesAhead>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /next/i }));
    expect(screen.getByText('1')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: /previous/i }));
    expect(screen.getByText('0')).toBeInTheDocument();
  });

  it('revert to initial resets to generation 0', async () => {
    const mutate = vi.fn().mockImplementation((_args, { onSuccess }) => {
      onSuccess(nextStateResponse);
    });
    mockUseStatesAhead.mockReturnValue({ ...statesAheadMutation, mutate } as ReturnType<typeof useStatesAhead>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    await userEvent.click(screen.getByRole('button', { name: /next/i }));
    await userEvent.click(screen.getByRole('button', { name: /start/i }));
    expect(screen.getByText('0')).toBeInTheDocument();
  });

  it('play loop calls fetchStatesAhead after the interval elapses', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: false });
    mockFetchStatesAhead.mockResolvedValue(nextStateResponse);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();

    // Start playing
    await act(async () => {
      screen.getByRole('button', { name: /play/i }).click();
    });

    // Advance the timer so the setTimeout fires
    await act(async () => {
      vi.advanceTimersByTime(600);
    });

    vi.useRealTimers();
    // fetchStatesAhead should have been called by the play loop
    expect(mockFetchStatesAhead).toHaveBeenCalledWith('board-xyz', 1);
  });
});
