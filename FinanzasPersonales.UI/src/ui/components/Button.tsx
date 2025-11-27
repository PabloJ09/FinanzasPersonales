import { ButtonHTMLAttributes, ReactNode } from 'react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
  variant?: 'primary' | 'secondary' | 'danger';
  className?: string;
}

export function Button({ 
  children, 
  variant = 'primary', 
  className = '', 
  ...props 
}: ButtonProps) {
  const variants = {
    primary: 'btn-primary-modern',
    secondary: 'btn-secondary-modern',
    danger: 'btn-danger-modern',
  };

  return (
    <button
      className={`${variants[variant]} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}

interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
  className?: string;
}

export function IconButton({ children, className = '', ...props }: IconButtonProps) {
  return (
    <button
      className={`icon-btn ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}
