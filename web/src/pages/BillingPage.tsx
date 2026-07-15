import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Badge,
  Button,
  Card,
  CardBody,
  Col,
  Row,
  Spinner,
  Table,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { billingService } from '../services/billingService';
import { licenseService } from '../services/licenseService';
import { CommercialDashboardWidgets } from '../components/commercial/CommercialDashboardWidgets';
import { useCommercialHub } from '../hooks/useCommercialHub';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS, type Plan } from '../types';

export const BillingPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const queryClient = useQueryClient();
  const canRead =
    hasPermission(PERMISSIONS.BillingRead) || hasPermission(PERMISSIONS.BillingView);
  const canManage = hasPermission(PERMISSIONS.BillingManage);
  const canLicenseRead = hasPermission(PERMISSIONS.LicenseRead);
  useCommercialHub(currentOrganization?.id);

  const [checkoutProvider, setCheckoutProvider] = useState<'Stripe' | 'Razorpay'>('Stripe');

  const { data: subscription, isLoading: subLoading, error: subError } = useQuery({
    queryKey: ['billing-subscription', currentOrganization?.id],
    queryFn: billingService.getSubscription,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: plans = [], isLoading: plansLoading } = useQuery({
    queryKey: ['billing-plans', currentOrganization?.id],
    queryFn: billingService.listPlans,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: license } = useQuery({
    queryKey: ['license', currentOrganization?.id],
    queryFn: licenseService.get,
    enabled: !!currentOrganization?.id && canLicenseRead,
  });

  const checkoutMutation = useMutation({
    mutationFn: (plan: Plan) =>
      billingService.startCheckout({
        planCode: plan.code,
        interval: 'Monthly',
        seatCount: 1,
        provider: checkoutProvider,
        successUrl: `${window.location.origin}/billing?checkout=success`,
        cancelUrl: `${window.location.origin}/billing?checkout=cancel`,
      }),
    onSuccess: (session) => {
      if (session.checkoutUrl) {
        window.location.href = session.checkoutUrl;
      } else {
        toast.success(`Checkout session ${session.sessionId} created`);
        void queryClient.invalidateQueries({ queryKey: ['billing-subscription'] });
      }
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const cancelMutation = useMutation({
    mutationFn: () => billingService.cancelSubscription({ atPeriodEnd: true }),
    onSuccess: () => {
      toast.success('Subscription will cancel at period end');
      void queryClient.invalidateQueries({ queryKey: ['billing-subscription'] });
      void queryClient.invalidateQueries({ queryKey: ['commercial-dashboard'] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to manage billing.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view billing.</Alert>;
  }

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">Billing</h1>
          <p className="text-muted mb-0">
            Subscription, plans, and payment for {currentOrganization.name}.
          </p>
        </div>
        <div className="d-flex flex-wrap gap-2">
          <Button tag={Link} to="/gateway" color="secondary" outline size="sm">
            API Keys
          </Button>
          <Button tag={Link} to="/billing/usage" color="secondary" outline size="sm">
            Usage
          </Button>
          <Button tag={Link} to="/billing/subscriptions" color="secondary" outline size="sm">
            Invoices
          </Button>
        </div>
      </div>

      <CommercialDashboardWidgets showTitle={false} />

      {(subLoading || plansLoading) && (
        <div className="text-center py-4">
          <Spinner />
        </div>
      )}
      {subError && (
        <Alert color="danger">
          {subError instanceof Error ? subError.message : 'Failed to load subscription'}
        </Alert>
      )}

      {subscription && (
        <Card className="mb-4">
          <CardBody>
            <div className="d-flex flex-wrap justify-content-between gap-3">
              <div>
                <h5 className="mb-2">Current subscription</h5>
                <p className="mb-1">
                  <strong>{subscription.planName}</strong>{' '}
                  <Badge color="primary" className="ms-1">
                    {subscription.status}
                  </Badge>
                </p>
                <p className="text-muted small mb-0">
                  {subscription.billingInterval} · {subscription.seatCount} seat(s) · period ends{' '}
                  {new Date(subscription.currentPeriodEnd).toLocaleDateString()}
                </p>
                {subscription.cancelAtPeriodEnd && (
                  <Alert color="warning" className="mt-3 mb-0 py-2">
                    Cancels at period end.
                  </Alert>
                )}
                {license && (
                  <p className="small text-muted mt-2 mb-0">
                    License: {license.edition} ({license.licenseKeyPrefix}…)
                  </p>
                )}
              </div>
              {canManage && !subscription.cancelAtPeriodEnd && subscription.planCode !== 'free' && (
                <Button
                  color="danger"
                  outline
                  size="sm"
                  disabled={cancelMutation.isPending}
                  onClick={() => {
                    if (window.confirm('Cancel subscription at the end of the current period?')) {
                      cancelMutation.mutate();
                    }
                  }}
                >
                  Cancel subscription
                </Button>
              )}
            </div>
          </CardBody>
        </Card>
      )}

      <div className="d-flex flex-wrap justify-content-between align-items-center mb-3">
        <h5 className="mb-0">Plans</h5>
        {canManage && (
          <div className="btn-group btn-group-sm" role="group">
            <Button
              color={checkoutProvider === 'Stripe' ? 'primary' : 'secondary'}
              outline={checkoutProvider !== 'Stripe'}
              onClick={() => setCheckoutProvider('Stripe')}
            >
              Stripe
            </Button>
            <Button
              color={checkoutProvider === 'Razorpay' ? 'primary' : 'secondary'}
              outline={checkoutProvider !== 'Razorpay'}
              onClick={() => setCheckoutProvider('Razorpay')}
            >
              Razorpay
            </Button>
          </div>
        )}
      </div>

      <Row className="g-3">
        {plans.map((plan) => (
          <Col key={plan.code} md={3} sm={6}>
            <Card className="h-100">
              <CardBody className="d-flex flex-column">
                <h5>{plan.name}</h5>
                <p className="text-muted small flex-grow-1">{plan.description || plan.tier}</p>
                <p className="mb-3">
                  <span className="h3">${plan.monthlyPriceUsd}</span>
                  <span className="text-muted">/mo</span>
                </p>
                {canManage && plan.code !== 'free' && (
                  <Button
                    color="primary"
                    size="sm"
                    disabled={checkoutMutation.isPending || subscription?.planCode === plan.code}
                    onClick={() => checkoutMutation.mutate(plan)}
                  >
                    {subscription?.planCode === plan.code
                      ? 'Current plan'
                      : `Checkout (${checkoutProvider})`}
                  </Button>
                )}
              </CardBody>
            </Card>
          </Col>
        ))}
      </Row>

      {plans.length === 0 && !plansLoading && (
        <Table responsive className="mt-3">
          <tbody>
            <tr>
              <td className="text-muted">No plans returned from the API.</td>
            </tr>
          </tbody>
        </Table>
      )}
    </div>
  );
};
