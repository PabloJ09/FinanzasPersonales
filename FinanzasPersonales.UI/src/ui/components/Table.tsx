import { ReactNode } from 'react';

interface TableProps {
  children: ReactNode;
  className?: string;
}

export function Table({ children, className = '' }: TableProps) {
  return (
    <div className="overflow-x-auto rounded-lg border border-slate-700">
      <table className={`table-modern ${className}`}>
        {children}
      </table>
    </div>
  );
}

interface TableHeaderProps {
  children: ReactNode;
}

export function TableHeader({ children }: TableHeaderProps) {
  return <thead>{children}</thead>;
}

interface TableBodyProps {
  children: ReactNode;
}

export function TableBody({ children }: TableBodyProps) {
  return <tbody>{children}</tbody>;
}

interface TableRowProps {
  children: ReactNode;
  onClick?: () => void;
}

export function TableRow({ children, onClick }: TableRowProps) {
  return <tr onClick={onClick}>{children}</tr>;
}

interface TableCellProps {
  children: ReactNode;
  className?: string;
}

export function TableCell({ children, className = '' }: TableCellProps) {
  return <td className={className}>{children}</td>;
}

interface TableHeadCellProps {
  children: ReactNode;
  className?: string;
}

export function TableHeadCell({ children, className = '' }: TableHeadCellProps) {
  return <th className={className}>{children}</th>;
}
