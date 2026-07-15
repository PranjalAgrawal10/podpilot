import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Badge, Button, Spinner, Table } from 'reactstrap';
import { securityService } from '../services/securityService';
import { useOrganization } from '../contexts/OrganizationContext';
import { useSecurityHub } from '../hooks/useSecurityHub';
import { PERMISSIONS } from '../types';

export const TrustedDevicesPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.SecurityRead);
  const canManage = hasPermission(PERMISSIONS.SecurityManage);
  useSecurityHub(currentOrganization?.id);

  const { data: devices = [], isLoading, error } = useQuery({
    queryKey: ['security-devices', currentOrganization?.id],
    queryFn: securityService.listDevices,
    enabled: !!currentOrganization?.id && canRead,
  });

  const revokeMutation = useMutation({
    mutationFn: (id: string) => securityService.revokeDevice(id),
    onSuccess: () => {
      toast.success('Device revoked');
      queryClient.invalidateQueries({ queryKey: ['security-devices', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['security-dashboard', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view trusted devices.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view trusted devices.</Alert>;
  }

  return (
    <div>
      <h1 className="page-title mb-1">Trusted devices</h1>
      <p className="text-muted mb-4">Registered devices for {currentOrganization.name}.</p>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load trusted devices'}
        </Alert>
      )}
      {!isLoading && !error && devices.length === 0 && (
        <Alert color="info">No trusted devices found.</Alert>
      )}

      {devices.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Device</th>
              <th>Last IP</th>
              <th>Trusted at</th>
              <th>Last seen</th>
              <th>Status</th>
              {canManage && <th />}
            </tr>
          </thead>
          <tbody>
            {devices.map((device) => (
              <tr key={device.id}>
                <td className="fw-semibold">{device.deviceName}</td>
                <td>{device.lastIpAddress || '—'}</td>
                <td className="text-nowrap">{new Date(device.trustedAt).toLocaleString()}</td>
                <td className="text-nowrap">{new Date(device.lastSeenAt).toLocaleString()}</td>
                <td>
                  <Badge color={device.isRevoked ? 'secondary' : 'success'}>
                    {device.isRevoked ? 'Revoked' : 'Trusted'}
                  </Badge>
                </td>
                {canManage && (
                  <td className="text-end">
                    {!device.isRevoked && (
                      <Button
                        color="danger"
                        size="sm"
                        outline
                        disabled={revokeMutation.isPending}
                        onClick={() => {
                          if (window.confirm(`Revoke device "${device.deviceName}"?`)) {
                            revokeMutation.mutate(device.id);
                          }
                        }}
                      >
                        Revoke
                      </Button>
                    )}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
