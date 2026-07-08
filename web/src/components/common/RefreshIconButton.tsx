interface RefreshIconButtonProps {
  onClick: () => void;
  disabled?: boolean;
  loading?: boolean;
  label?: string;
}

export const RefreshIconButton = ({
  onClick,
  disabled = false,
  loading = false,
  label = 'Refresh status',
}: RefreshIconButtonProps) => (
  <button
    type="button"
    className="btn btn-sm btn-outline-secondary pod-refresh-btn"
    onClick={onClick}
    disabled={disabled || loading}
    title={label}
    aria-label={label}
  >
    {loading ? (
      <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true" />
    ) : (
      <svg
        xmlns="http://www.w3.org/2000/svg"
        width="16"
        height="16"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true"
      >
        <path d="M21 12a9 9 0 1 1-2.64-6.36" />
        <path d="M21 3v6h-6" />
      </svg>
    )}
  </button>
);
