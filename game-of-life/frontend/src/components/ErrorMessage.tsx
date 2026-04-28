interface ErrorMessageProps {
  message: string;
}

export function ErrorMessage({ message }: ErrorMessageProps) {
  return (
    <div className="px-4 py-3 bg-red-950 border border-red-700 rounded text-red-300 text-sm">
      {message}
    </div>
  );
}
