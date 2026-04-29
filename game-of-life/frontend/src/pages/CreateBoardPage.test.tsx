import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { CreateBoardPage } from './CreateBoardPage';

vi.mock('../hooks/useCreateBoard');
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useNavigate: () => vi.fn() };
});

import { useCreateBoard } from '../hooks/useCreateBoard';

const mockUseCreateBoard = vi.mocked(useCreateBoard);

const defaultMutation = {
  mutate: vi.fn(),
  isPending: false,
  isError: false,
  error: null,
};

function renderPage() {
  return render(
    <MemoryRouter>
      <CreateBoardPage />
    </MemoryRouter>
  );
}

beforeEach(() => {
  vi.clearAllMocks();
  mockUseCreateBoard.mockReturnValue(defaultMutation as ReturnType<typeof useCreateBoard>);
});

describe('CreateBoardPage', () => {
  it('renders the page heading', () => {
    renderPage();
    expect(screen.getByRole('heading', { name: /new board/i })).toBeInTheDocument();
  });

  it('renders name, width, height inputs', () => {
    renderPage();
    expect(screen.getByPlaceholderText(/my board/i)).toBeInTheDocument();
    expect(screen.getAllByDisplayValue('10')).toHaveLength(2); // width and height both 10
  });

  it('renders the grid (10×10 = 100 cells by default)', () => {
    const { container } = renderPage();
    const cells = container.querySelectorAll('[style*="width"]');
    expect(cells.length).toBeGreaterThanOrEqual(100);
  });

  it('submits the form with correct payload', async () => {
    const mutate = vi.fn();
    mockUseCreateBoard.mockReturnValue({ ...defaultMutation, mutate } as ReturnType<typeof useCreateBoard>);
    renderPage();
    fireEvent.submit(screen.getByRole('button', { name: /create board/i }).closest('form')!);
    expect(mutate).toHaveBeenCalledWith(
      expect.objectContaining({ width: 10, height: 10 }),
      expect.any(Object)
    );
  });

  it('shows error message when mutation fails', () => {
    mockUseCreateBoard.mockReturnValue({
      ...defaultMutation,
      isError: true,
      error: new Error('Failed to create'),
    } as ReturnType<typeof useCreateBoard>);
    renderPage();
    expect(screen.getByText('Failed to create')).toBeInTheDocument();
  });

  it('shows loading state while pending', () => {
    mockUseCreateBoard.mockReturnValue({ ...defaultMutation, isPending: true } as ReturnType<typeof useCreateBoard>);
    renderPage();
    expect(screen.getByRole('button', { name: /creating/i })).toBeDisabled();
    expect(document.querySelector('.animate-spin')).toBeInTheDocument();
  });

  it('toggles a cell when clicked', async () => {
    const mutate = vi.fn();
    mockUseCreateBoard.mockReturnValue({ ...defaultMutation, mutate } as ReturnType<typeof useCreateBoard>);
    const { container } = renderPage();
    const cells = container.querySelectorAll('[style*="width"]');
    await userEvent.click(cells[0]);
    // After toggling, submit to confirm cell state changed
    fireEvent.submit(screen.getByRole('button', { name: /create board/i }).closest('form')!);
    const callArg = mutate.mock.calls[0][0];
    // First cell of first row should now be 1
    expect(callArg.initialCells[0][0]).toBe(1);
  });

  it('resets grid when dimensions change', async () => {
    const mutate = vi.fn();
    mockUseCreateBoard.mockReturnValue({ ...defaultMutation, mutate } as ReturnType<typeof useCreateBoard>);
    renderPage();
    const widthInput = screen.getAllByRole('spinbutton')[0];
    fireEvent.change(widthInput, { target: { value: '5', valueAsNumber: 5 } });
    fireEvent.submit(screen.getByRole('button', { name: /create board/i }).closest('form')!);
    const callArg = mutate.mock.calls[0][0];
    expect(callArg.width).toBe(5);
    expect(callArg.initialCells[0]).toHaveLength(5);
  });

  it('has a back link to /', () => {
    renderPage();
    expect(screen.getByRole('link', { name: /back/i })).toHaveAttribute('href', '/');
  });
});
