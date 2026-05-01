import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { BoardCard } from '../../src/components/BoardCard';
import type { BoardResponse } from '../../src/types/api';

const board: BoardResponse = {
  id: 'board-123',
  name: 'My Board',
  generation: 0,
  width: 10,
  height: 5,
  liveCells: [[1, 2], [3, 4]],
};

function renderCard(overrides: Partial<BoardResponse> = {}, isDeleting = false, onDelete = vi.fn()) {
  return render(
    <MemoryRouter>
      <BoardCard board={{ ...board, ...overrides }} onDelete={onDelete} isDeleting={isDeleting} />
    </MemoryRouter>
  );
}

describe('BoardCard', () => {
  it('shows the board name', () => {
    renderCard();
    expect(screen.getByText('My Board')).toBeInTheDocument();
  });

  it('shows "Unnamed board" when name is empty', () => {
    renderCard({ name: '' });
    expect(screen.getByText('Unnamed board')).toBeInTheDocument();
  });

  it('shows dimensions and live cell count', () => {
    renderCard();
    expect(screen.getByText(/10 × 5/)).toBeInTheDocument();
    expect(screen.getByText(/2 live cells/)).toBeInTheDocument();
  });

  it('has a Simulate link pointing to /boards/:id', () => {
    renderCard();
    const link = screen.getByRole('link', { name: /simulate/i });
    expect(link).toHaveAttribute('href', '/boards/board-123');
  });

  it('has an Edit link pointing to /boards/:id/edit', () => {
    renderCard();
    const link = screen.getByRole('link', { name: /edit/i });
    expect(link).toHaveAttribute('href', '/boards/board-123/edit');
  });

  it('calls onDelete when Delete button is clicked', async () => {
    const onDelete = vi.fn();
    renderCard({}, false, onDelete);
    await userEvent.click(screen.getByRole('button', { name: /delete/i }));
    expect(onDelete).toHaveBeenCalledOnce();
  });

  it('shows "Deleting…" and disables button when isDeleting is true', () => {
    renderCard({}, true);
    const btn = screen.getByRole('button', { name: /deleting/i });
    expect(btn).toBeDisabled();
  });
});
