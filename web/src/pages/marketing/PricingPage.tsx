import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, CardBody, Col, Row } from 'reactstrap';
import { billingService } from '../../services/billingService';
import { useAuth } from '../../contexts/AuthContext';
import type { Plan } from '../../types';

const STATIC_PLANS: Plan[] = [
  {
    code: 'free',
    name: 'Free',
    tier: 'Free',
    pricingModel: 'Flat',
    monthlyPriceUsd: 0,
    yearlyPriceUsd: 0,
    seatPriceUsd: 0,
    includedSeats: 1,
    description: 'Single org, starter quotas for evaluation.',
    quotas: {
      maxPods: 2,
      maxProviders: 1,
      maxModels: 5,
      maxOrganizations: 1,
      maxTeamMembers: 2,
      maxApiRequestsPerMonth: 10000,
      maxConcurrentStreams: 2,
      maxStorageGb: 20,
    },
  },
  {
    code: 'pro',
    name: 'Pro',
    tier: 'Pro',
    pricingModel: 'Flat',
    monthlyPriceUsd: 49,
    yearlyPriceUsd: 490,
    seatPriceUsd: 0,
    includedSeats: 3,
    description: 'For indie teams shipping on GPU infrastructure.',
    quotas: {
      maxPods: 10,
      maxProviders: 3,
      maxModels: 50,
      maxOrganizations: 3,
      maxTeamMembers: 10,
      maxApiRequestsPerMonth: 250000,
      maxConcurrentStreams: 20,
      maxStorageGb: 200,
    },
  },
  {
    code: 'team',
    name: 'Team',
    tier: 'Team',
    pricingModel: 'SeatBased',
    monthlyPriceUsd: 149,
    yearlyPriceUsd: 1490,
    seatPriceUsd: 25,
    includedSeats: 10,
    description: 'Shared fleets, higher quotas, priority support.',
    quotas: {
      maxPods: 50,
      maxProviders: 10,
      maxModels: 200,
      maxOrganizations: 10,
      maxTeamMembers: 50,
      maxApiRequestsPerMonth: 2000000,
      maxConcurrentStreams: 100,
      maxStorageGb: 2000,
    },
  },
  {
    code: 'enterprise',
    name: 'Enterprise',
    tier: 'Enterprise',
    pricingModel: 'Hybrid',
    monthlyPriceUsd: 0,
    yearlyPriceUsd: 0,
    seatPriceUsd: 0,
    includedSeats: 0,
    description: 'SSO, custom quotas, dedicated support, and on-prem licensing.',
    quotas: {
      maxPods: 0,
      maxProviders: 0,
      maxModels: 0,
      maxOrganizations: 0,
      maxTeamMembers: 0,
      maxApiRequestsPerMonth: 0,
      maxConcurrentStreams: 0,
      maxStorageGb: 0,
    },
  },
];

export const PricingPage = () => {
  const { isAuthenticated } = useAuth();

  const { data: apiPlans } = useQuery({
    queryKey: ['public-plans'],
    queryFn: billingService.listPlans,
    enabled: isAuthenticated,
    retry: false,
  });

  const plans = apiPlans && apiPlans.length > 0 ? apiPlans : STATIC_PLANS;

  return (
    <div className="marketing-page">
      <h1 className="marketing-page-title">Simple plans for growing fleets</h1>
      <p className="marketing-page-lead">
        Start free. Upgrade when idle savings and gateway volume justify it.
      </p>
      <Row className="g-4 mt-3">
        {plans.map((plan) => (
          <Col key={plan.code} md={3} sm={6}>
            <Card className="marketing-price-card h-100">
              <CardBody className="d-flex flex-column">
                <h3>{plan.name}</h3>
                <p className="text-muted small flex-grow-1">{plan.description}</p>
                <p className="mb-3">
                  {plan.code === 'enterprise' ? (
                    <span className="h3">Custom</span>
                  ) : (
                    <>
                      <span className="h3">${plan.monthlyPriceUsd}</span>
                      <span className="text-muted">/mo</span>
                    </>
                  )}
                </p>
                <Button
                  tag={Link}
                  to={plan.code === 'enterprise' ? '/contact' : isAuthenticated ? '/billing' : '/register'}
                  color={plan.code === 'pro' ? 'primary' : 'secondary'}
                  outline={plan.code !== 'pro'}
                >
                  {plan.code === 'enterprise' ? 'Talk to us' : 'Get started'}
                </Button>
              </CardBody>
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  );
};
