import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Pagination } from '../../src/components/Pagination';

describe('Pagination', () => {
  it('shows current page number', () => {
    render(<Pagination page={3} pageSize={10} itemCount={10} onPageChange={vi.fn()} />);
    expect(screen.getByText('Page 3')).toBeInTheDocument();
  });

  it('disables Previous on page 1', () => {
    render(<Pagination page={1} pageSize={10} itemCount={10} onPageChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: /previous/i })).toBeDisabled();
  });

  it('enables Previous when page > 1', () => {
    render(<Pagination page={2} pageSize={10} itemCount={10} onPageChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: /previous/i })).not.toBeDisabled();
  });

  it('disables Next when itemCount < pageSize (last page)', () => {
    render(<Pagination page={2} pageSize={10} itemCount={5} onPageChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: /next/i })).toBeDisabled();
  });

  it('enables Next when itemCount equals pageSize', () => {
    render(<Pagination page={1} pageSize={10} itemCount={10} onPageChange={vi.fn()} />);
    expect(screen.getByRole('button', { name: /next/i })).not.toBeDisabled();
  });

  it('calls onPageChange with page - 1 when Previous is clicked', async () => {
    const onPageChange = vi.fn();
    render(<Pagination page={3} pageSize={10} itemCount={10} onPageChange={onPageChange} />);
    await userEvent.click(screen.getByRole('button', { name: /previous/i }));
    expect(onPageChange).toHaveBeenCalledWith(2);
  });

  it('calls onPageChange with page + 1 when Next is clicked', async () => {
    const onPageChange = vi.fn();
    render(<Pagination page={2} pageSize={10} itemCount={10} onPageChange={onPageChange} />);
    await userEvent.click(screen.getByRole('button', { name: /next/i }));
    expect(onPageChange).toHaveBeenCalledWith(3);
  });
});
