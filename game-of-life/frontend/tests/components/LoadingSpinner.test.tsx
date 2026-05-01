import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import { LoadingSpinner } from '../../src/components/LoadingSpinner';

describe('LoadingSpinner', () => {
  it('renders a spinner element', () => {
    const { container } = render(<LoadingSpinner />);
    const div = container.firstChild as HTMLElement;
    expect(div).toBeInTheDocument();
    expect(div).toHaveClass('animate-spin');
  });
});
