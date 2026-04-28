import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ErrorMessage } from './ErrorMessage';

describe('ErrorMessage', () => {
  it('renders the message text', () => {
    render(<ErrorMessage message="Something went wrong" />);
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
  });

  it('renders with error styling', () => {
    const { container } = render(<ErrorMessage message="Error" />);
    const div = container.firstChild as HTMLElement;
    expect(div).toHaveClass('bg-red-950');
  });
});
