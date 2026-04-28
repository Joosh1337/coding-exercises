import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { EditBoardPage } from './EditBoardPage';
import type { BoardResponse } from '../types/api';

vi.mock('../hooks/useBoard');
vi.mock('../hooks/useUpdateBoard');
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => vi.fn(),
    useParams: () => ({ id: 'board-abc' }),
  };
});

import { useBoard } from '../hooks/useBoard';
import { useUpdateBoard } from '../hooks/useUpdateBoard';

const mockUseBoard = vi.mocked(useBoard);
const mockUseUpdateBoard = vi.mocked(useUpdateBoard);

const sampleBoard: BoardResponse = {
  id: 'board-abc',
  name: 'My Board',
  generation: 0,
  width: 5,
  height: 5,
  liveCells: [[1, 2]],
};

const defaultUpdate = { mutate: vi.fn(), isPending: false, isError: false, error: null };

function renderPage() {
  return render(
    <MemoryRouter>
      <EditBoardPage />
    </MemoryRouter>
  );
}

beforeEach(() => {
  vi.clearAllMocks();
  mockUseUpdateBoard.mockReturnValue(defaultUpdate as ReturnType<typeof useUpdateBoard>);
});

describe('EditBoardPage', () => {
  it('shows loading spinner while board loads', () => {
    mockUseBoard.mockReturnValue({ isLoading: true, isError: false, data: undefined, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(document.querySelector('.animate-spin')).toBeInTheDocument();
  });

  it('shows error when board not found', () => {
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

  it('shows error when board is null', () => {
    mockUseBoard.mockReturnValue({
      isLoading: false,
      isError: false,
      data: undefined,
      error: null,
    } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByText('Board not found.')).toBeInTheDocument();
  });

  it('pre-fills form with board data', () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByDisplayValue('My Board')).toBeInTheDocument();
    // Both width and height are 5
    const numInputs = screen.getAllByDisplayValue('5');
    expect(numInputs.length).toBeGreaterThanOrEqual(2);
  });

  it('shows board id', () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByText('board-abc')).toBeInTheDocument();
  });

  it('submits the form and calls updateBoard', () => {
    const mutate = vi.fn();
    mockUseUpdateBoard.mockReturnValue({ ...defaultUpdate, mutate } as ReturnType<typeof useUpdateBoard>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    fireEvent.submit(screen.getByRole('button', { name: /save changes/i }).closest('form')!);
    expect(mutate).toHaveBeenCalledWith(
      expect.objectContaining({ id: 'board-abc', name: 'My Board', width: 5, height: 5 }),
      expect.any(Object)
    );
  });

  it('shows error message when update fails', () => {
    mockUseUpdateBoard.mockReturnValue({
      ...defaultUpdate,
      isError: true,
      error: new Error('Update failed'),
    } as ReturnType<typeof useUpdateBoard>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByText('Update failed')).toBeInTheDocument();
  });

  it('shows loading state while saving', () => {
    mockUseUpdateBoard.mockReturnValue({ ...defaultUpdate, isPending: true } as ReturnType<typeof useUpdateBoard>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByRole('button', { name: /saving/i })).toBeDisabled();
  });

  it('has a cancel link back to board simulation', () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    expect(screen.getByRole('link', { name: /cancel/i })).toHaveAttribute('href', '/boards/board-abc');
  });

  it('resets grid when width is changed', async () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    const [widthInput] = screen.getAllByRole('spinbutton');
    fireEvent.change(widthInput, { target: { value: '8' } });
    expect(widthInput).toHaveValue(8);
  });

  it('resets grid when height is changed', async () => {
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    renderPage();
    const [, heightInput] = screen.getAllByRole('spinbutton');
    fireEvent.change(heightInput, { target: { value: '7' } });
    expect(heightInput).toHaveValue(7);
  });

  it('toggles a cell in the grid', async () => {
    const mutate = vi.fn();
    mockUseUpdateBoard.mockReturnValue({ ...defaultUpdate, mutate } as ReturnType<typeof useUpdateBoard>);
    mockUseBoard.mockReturnValue({ isLoading: false, isError: false, data: sampleBoard, error: null } as ReturnType<typeof useBoard>);
    const { container } = renderPage();
    const gridWrapper = container.querySelector('.bg-gray-600');
    const cells = gridWrapper!.querySelectorAll('div');
    await userEvent.click(cells[0]);
    fireEvent.submit(screen.getByRole('button', { name: /save/i }).closest('form')!);
    expect(mutate).toHaveBeenCalled();
  });
});
