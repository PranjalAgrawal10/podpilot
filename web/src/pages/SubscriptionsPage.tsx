import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Alert, Badge, Button, Spinner, Table } from 'reactstrap';
import { toast } from 'react-toastify';
import { billingService } from '../services/billingService';
import { useCommercialHub } from '../hooks/useCommercialHub';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const SubscriptionsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const queryClient = useQueryClient();
  const canRead =
    hasPermission(PERMISSIONS.BillingRead) || hasPermission(PERMISSIONS.BillingView);
  const canManage = hasPermission(PERMISSIONS.BillingManage);
  useCommercialHub(currentOrganization?.id);

  const { data: invoices = [], isLoading, error } = useQuery({
    queryKey: ['billing-invoices', currentOrganization?.id],
    queryFn: billingService.listInvoices,
    enabled: !!currentOrganization?.id && canRead,
  });

  const generateMutation = useMutation({
    mutationFn: billingService.generateInvoice,
    onSuccess: () => {
      toast.success('Invoice generated');
      void queryClient.invalidateQueries({ queryKey: ['billing-invoices'] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view invoices.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view invoices.</Alert>;
  }

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">Invoices</h1>
          <p className="text-muted mb-0">
            Billing history for {currentOrganization.name}.{' '}
            <Link to="/billing">Manage subscription</Link>
          </p>
        </div>
        {canManage && (
          <Button
            color="primary"
            size="sm"
            disabled={generateMutation.isPending}
            onClick={() => generateMutation.mutate()}
          >
            Generate invoice
          </Button>
        )}
      </div>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load invoices'}
        </Alert>
      )}

      {!isLoading && invoices.length === 0 && (
        <Alert color="info">No invoices yet.</Alert>
      )}

      {invoices.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Number</th>
              <th>Status</th>
              <th>Period</th>
              <th>Total</th>
            </tr>
          </thead>
          <tbody>
            {invoices.map((invoice) => (
              <tr key={invoice.id}>
                <td>{invoice.invoiceNumber}</td>
                <td>
                  <Badge color="secondary">{invoice.status}</Badge>
                </td>
                <td className="small">
                  {new Date(invoice.periodStart).toLocaleDateString()} –{' '}
                  {new Date(invoice.periodEnd).toLocaleDateString()}
                </td>
                <td>${invoice.totalUsd.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
