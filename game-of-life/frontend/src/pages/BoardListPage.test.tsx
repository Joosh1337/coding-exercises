import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { BoardListPage } from './BoardListPage';
import type { BoardResponse } from '../types/api';

vi.mock('../hooks/useBoards');
vi.mock('../hooks/useDeleteBoard');

import { useBoards } from '../hooks/useBoards';
import { useDeleteBoard } from '../hooks/useDeleteBoard';

const mockUseBoards = vi.mocked(useBoards);
const mockUseDeleteBoard = vi.mocked(useDeleteBoard);

const sampleBoards: BoardResponse[] = [
  { id: '1', name: 'Alpha', generation: 0, width: 5, height: 5, liveCells: [[0, 0]] },
  { id: '2', name: 'Beta', generation: 0, width: 3, height: 3, liveCells: [] },
];

const defaultDelete = {
  mutate: vi.fn(),
  isPending: false,
  variables: undefined as string | undefined,
  isError: false,
  error: null,
};

function renderPage() {
  return render(
    <MemoryRouter>
      <BoardListPage />
    </MemoryRouter>
  );
}

beforeEach(() => {
  vi.clearAllMocks();
  mockUseDeleteBoard.mockReturnValue(defaultDelete as ReturnType<typeof useDeleteBoard>);
});

describe('BoardListPage', () => {
  it('shows loading spinner while fetching', () => {
    mockUseBoards.mockReturnValue({ isLoading: true, isError: false, data: undefined, error: null } as ReturnType<typeof useBoards>);
    renderPage();
    expect(document.querySelector('.animate-spin')).toBeInTheDocument();
  });

  it('shows error message on fetch failure', () => {
    mockUseBoards.mockReturnValue({
      isLoading: false,
      isError: true,
      data: undefined,
      error: new Error('Network failure'),
    } as ReturnType<typeof useBoards>);
    renderPage();
    expect(screen.getByText('Network failure')).toBeInTheDocument();
  });

  it('shows empty state when no boards exist', () => {
    mockUseBoards.mockReturnValue({ isLoading: false, isError: false, data: [], error: null } as ReturnType<typeof useBoards>);
    renderPage();
    expect(screen.getByText(/no boards yet/i)).toBeInTheDocument();
  });

  it('renders board cards when boards exist', () => {
    mockUseBoards.mockReturnValue({ isLoading: false, isError: false, data: sampleBoards, error: null } as ReturnType<typeof useBoards>);
    renderPage();
    expect(screen.getByText('Alpha')).toBeInTheDocument();
    expect(screen.getByText('Beta')).toBeInTheDocument();
  });

  it('shows pagination when boards are present', () => {
    mockUseBoards.mockReturnValue({ isLoading: false, isError: false, data: sampleBoards, error: null } as ReturnType<typeof useBoards>);
    renderPage();
    expect(screen.getByText(/page 1/i)).toBeInTheDocument();
  });

  it('calls delete mutation when Delete is clicked', async () => {
    const mutate = vi.fn();
    mockUseDeleteBoard.mockReturnValue({ ...defaultDelete, mutate } as ReturnType<typeof useDeleteBoard>);
    mockUseBoards.mockReturnValue({ isLoading: false, isError: false, data: sampleBoards, error: null } as ReturnType<typeof useBoards>);
    renderPage();
    const deleteButtons = screen.getAllByRole('button', { name: /delete/i });
    await userEvent.click(deleteButtons[0]);
    expect(mutate).toHaveBeenCalledWith('1');
  });

  it('has a New Board link', () => {
    mockUseBoards.mockReturnValue({ isLoading: false, isError: false, data: [], error: null } as ReturnType<typeof useBoards>);
    renderPage();
    expect(screen.getByRole('link', { name: /new board/i })).toBeInTheDocument();
  });

  it('shows deleting state on the correct board card', () => {
    mockUseDeleteBoard.mockReturnValue({
      ...defaultDelete,
      isPending: true,
      variables: '1',
    } as ReturnType<typeof useDeleteBoard>);
    mockUseBoards.mockReturnValue({ isLoading: false, isError: false, data: sampleBoards, error: null } as ReturnType<typeof useBoards>);
    renderPage();
    expect(screen.getByRole('button', { name: /deleting/i })).toBeInTheDocument();
  });
});
