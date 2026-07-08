import { Alert } from 'reactstrap';
import { HealthBadge } from './HealthBadge';
import type { ProviderAccountInfo, ProviderConnectionStatus } from '../../types';

interface ConnectionStatusProps {
  status: ProviderConnectionStatus;
  message?: string | null;
  account?: ProviderAccountInfo | null;
  isValid?: boolean;
}

export const ConnectionStatus = ({
  status,
  message,
  account,
  isValid,
}: ConnectionStatusProps) => {
  const alertColor = isValid === false ? 'danger' : status === 'Connected' ? 'success' : 'warning';

  return (
    <div className="connection-status">
      <div className="d-flex align-items-center gap-2 mb-2">
        <span className="text-muted small">Connection Status</span>
        <HealthBadge status={status} />
      </div>

      <Alert color={alertColor} className="connection-status-alert mb-0">
        {message ?? (status === 'Connected' ? 'Connection successful.' : 'Connection status unknown.')}
        {account && (
          <div className="mt-2 connection-account-info">
            {account.accountName && (
              <div>
                <strong>Account:</strong> {account.accountName}
              </div>
            )}
            {account.email && (
              <div>
                <strong>Email:</strong> {account.email}
              </div>
            )}
            {account.balance != null && (
              <div>
                <strong>Balance:</strong>{' '}
                {account.currency ? `${account.currency} ` : ''}
                {account.balance.toFixed(2)}
              </div>
            )}
          </div>
        )}
      </Alert>
    </div>
  );
};
