import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BoardGrid } from '../../src/components/BoardGrid';

const simple3x3 = [
  [1, 0, 1],
  [0, 1, 0],
  [1, 0, 1],
];

function getCells(container: HTMLElement) {
  return container.querySelector('.bg-gray-600')!.querySelectorAll('div');
}

describe('BoardGrid', () => {
  it('renders width × height cells', () => {
    const { container } = render(<BoardGrid display={simple3x3} />);
    expect(getCells(container)).toHaveLength(9);
  });

  it('renders an empty grid without errors', () => {
    const { container } = render(<BoardGrid display={[]} />);
    expect(container.firstChild).toBeInTheDocument();
  });

  it('applies alive class to live cells', () => {
    const { container } = render(<BoardGrid display={[[1, 0]]} />);
    const cells = getCells(container);
    expect(cells[0]).toHaveClass('bg-green-400');
    expect(cells[1]).toHaveClass('bg-gray-900');
  });

  it('does not apply cursor-pointer when not editable', () => {
    const { container } = render(<BoardGrid display={[[1, 0]]} />);
    getCells(container).forEach((cell) => expect(cell).not.toHaveClass('cursor-pointer'));
  });

  it('applies cursor-pointer when editable', () => {
    const { container } = render(<BoardGrid display={[[1, 0]]} editable onToggle={vi.fn()} />);
    getCells(container).forEach((cell) => expect(cell).toHaveClass('cursor-pointer'));
  });

  it('calls onToggle with correct coordinates when editable cell is clicked', async () => {
    const onToggle = vi.fn();
    const { container } = render(
      <BoardGrid display={simple3x3} editable onToggle={onToggle} />
    );
    const cells = getCells(container);
    await userEvent.click(cells[4]); // row 1, col 1 in a 3-col grid
    expect(onToggle).toHaveBeenCalledWith(1, 1);
  });

  it('does not call onToggle when not editable', async () => {
    const onToggle = vi.fn();
    const { container } = render(<BoardGrid display={simple3x3} onToggle={onToggle} />);
    await userEvent.click(getCells(container)[0]);
    expect(onToggle).not.toHaveBeenCalled();
  });
});
