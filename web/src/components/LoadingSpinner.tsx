import { Spinner } from 'reactstrap';

interface LoadingSpinnerProps {
  message?: string;
  fullPage?: boolean;
}

export const LoadingSpinner = ({ message = 'Loading...', fullPage = false }: LoadingSpinnerProps) => {
  const content = (
    <div className="loading-spinner d-flex flex-column align-items-center justify-content-center gap-3">
      <Spinner color="primary" />
      <span className="text-muted">{message}</span>
    </div>
  );

  if (fullPage) {
    return <div className="loading-spinner-fullpage">{content}</div>;
  }

  return content;
};
