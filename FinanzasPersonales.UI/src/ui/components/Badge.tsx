import { ReactNode } from 'react';

interface BadgeProps {
  children: ReactNode;
  variant: 'income' | 'expense';
  className?: string;
}

export function Badge({ children, variant, className = '' }: BadgeProps) {
  const variants = {
    income: 'badge-income',
    expense: 'badge-expense',
  };

  return (
    <span className={`${variants[variant]} ${className}`}>
      {children}
    </span>
  );
}
