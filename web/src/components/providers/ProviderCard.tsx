import { Link } from 'react-router-dom';
import { Card, CardBody, Button, Badge } from 'reactstrap';
import { HealthBadge } from './HealthBadge';
import { PROVIDER_TYPE_LABELS, type Provider } from '../../types';

interface ProviderCardProps {
  provider: Provider;
}

const providerIcons: Record<string, string> = {
  RunPod: '⚡',
  Vast: '🖥️',
  Lambda: 'λ',
  Azure: '☁️',
  AWS: '🌐',
  GoogleCloud: '🔷',
  Kubernetes: '⎈',
};

export const ProviderCard = ({ provider }: ProviderCardProps) => {
  const status = provider.connectionStatus ?? (provider.isValidated ? 'Connected' : 'Unknown');

  return (
    <Card className="provider-card">
      <CardBody>
        <div className="d-flex justify-content-between align-items-start mb-2">
          <div className="d-flex align-items-center gap-2">
            <span className="provider-icon" aria-hidden>
              {providerIcons[provider.providerType] ?? '🔌'}
            </span>
            <div>
              <h5 className="mb-0">{provider.displayName}</h5>
              <small className="text-muted">{provider.name}</small>
            </div>
          </div>
          <HealthBadge status={status} />
        </div>

        <div className="mb-2">
          <Badge color="info" className="provider-type-badge">
            {PROVIDER_TYPE_LABELS[provider.providerType]}
          </Badge>
          {!provider.isEnabled && (
            <Badge color="secondary" className="ms-1">
              Disabled
            </Badge>
          )}
        </div>

        {provider.description && (
          <p className="text-muted small mb-3">{provider.description}</p>
        )}

        {provider.defaultRegion && (
          <p className="text-muted small mb-3">
            <strong>Region:</strong> {provider.defaultRegion}
          </p>
        )}

        <div className="d-flex gap-2">
          <Button tag={Link} to={`/providers/${provider.id}`} color="outline-primary" size="sm">
            Details
          </Button>
          <Button tag={Link} to={`/providers/${provider.id}/edit`} color="outline-secondary" size="sm">
            Edit
          </Button>
        </div>
      </CardBody>
    </Card>
  );
};
