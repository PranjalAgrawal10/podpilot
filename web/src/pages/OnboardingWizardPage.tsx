import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Badge,
  Button,
  Card,
  CardBody,
  Col,
  FormGroup,
  Input,
  Label,
  Progress,
  Row,
  Spinner,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { onboardingService } from '../services/onboardingService';
import { commercialService } from '../services/commercialService';
import { useOrganization } from '../contexts/OrganizationContext';
import type { TelemetryPreference } from '../types';

const WIZARD_STEPS = [
  {
    key: 'CreateOrganization',
    title: 'Create organization',
    description: 'Confirm your workspace name and invite team members later.',
    href: '/organizations',
  },
  {
    key: 'ConnectProvider',
    title: 'Connect a GPU provider',
    description: 'Add RunPod or another provider with an encrypted API key.',
    href: '/providers/add',
  },
  {
    key: 'CreatePod',
    title: 'Create a pod',
    description: 'Launch your first GPU workload.',
    href: '/pods/create',
  },
  {
    key: 'InstallOllama',
    title: 'Install Ollama',
    description: 'Ensure the inference runtime is available on the pod.',
    href: '/models',
  },
  {
    key: 'PullFirstModel',
    title: 'Pull first model',
    description: 'Download a starter model to the pod.',
    href: '/models/pull',
  },
  {
    key: 'ConnectClaudeCode',
    title: 'Connect Claude Code / IDE',
    description: 'Point your IDE or agent at the PodPilot gateway.',
    href: '/gateway',
  },
  {
    key: 'TestAiGateway',
    title: 'Test AI gateway',
    description: 'Send a sample request through the gateway.',
    href: '/gateway',
  },
] as const;

export const OnboardingWizardPage = () => {
  const { currentOrganization } = useOrganization();
  const queryClient = useQueryClient();

  const { data: onboarding, isLoading, error } = useQuery({
    queryKey: ['onboarding', currentOrganization?.id],
    queryFn: onboardingService.get,
    enabled: !!currentOrganization?.id,
  });

  const { data: telemetry } = useQuery({
    queryKey: ['telemetry', currentOrganization?.id],
    queryFn: commercialService.getTelemetry,
    enabled: !!currentOrganization?.id,
  });

  const completeMutation = useMutation({
    mutationFn: (step: string) => onboardingService.completeStep({ step }),
    onSuccess: () => {
      toast.success('Step marked complete');
      void queryClient.invalidateQueries({ queryKey: ['onboarding'] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const dismissMutation = useMutation({
    mutationFn: onboardingService.dismiss,
    onSuccess: () => {
      toast.info('Onboarding dismissed');
      void queryClient.invalidateQueries({ queryKey: ['onboarding'] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const telemetryMutation = useMutation({
    mutationFn: (preference: TelemetryPreference) => commercialService.updateTelemetry(preference),
    onSuccess: () => {
      toast.success('Telemetry preference saved');
      void queryClient.invalidateQueries({ queryKey: ['telemetry'] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to continue onboarding.</Alert>;
  }

  const completed = new Set(onboarding?.completedSteps ?? []);
  const progress = Math.round((completed.size / WIZARD_STEPS.length) * 100);
  const currentIndex = WIZARD_STEPS.findIndex((s) => s.key === onboarding?.currentStep);

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">Onboarding</h1>
          <p className="text-muted mb-0">Seven steps to a working GPU + AI gateway setup.</p>
        </div>
        {onboarding && !onboarding.isDismissed && !onboarding.isComplete && (
          <Button
            color="link"
            size="sm"
            disabled={dismissMutation.isPending}
            onClick={() => dismissMutation.mutate()}
          >
            Dismiss wizard
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
          {error instanceof Error ? error.message : 'Failed to load onboarding'}
        </Alert>
      )}

      {onboarding && (
        <>
          {(onboarding.isComplete || onboarding.isDismissed) && (
            <Alert color="success">
              {onboarding.isComplete
                ? 'Onboarding complete — you are ready to fly.'
                : 'Onboarding dismissed. You can still complete steps below.'}
            </Alert>
          )}

          <Progress value={progress} className="mb-4" />
          <p className="small text-muted mb-4">
            {completed.size} of {WIZARD_STEPS.length} steps · current:{' '}
            <Badge color="primary">{onboarding.currentStep}</Badge>
          </p>

          <Row className="g-3 mb-4">
            {WIZARD_STEPS.map((step, index) => {
              const isDone = completed.has(step.key);
              const isCurrent = currentIndex === index || onboarding.currentStep === step.key;
              return (
                <Col key={step.key} md={6} lg={4}>
                  <Card className={`h-100 ${isCurrent ? 'border-primary' : ''}`}>
                    <CardBody>
                      <div className="d-flex justify-content-between mb-2">
                        <span className="text-muted small">Step {index + 1}</span>
                        {isDone ? (
                          <Badge color="success">Done</Badge>
                        ) : isCurrent ? (
                          <Badge color="primary">Current</Badge>
                        ) : (
                          <Badge color="secondary">Pending</Badge>
                        )}
                      </div>
                      <h5>{step.title}</h5>
                      <p className="text-muted small">{step.description}</p>
                      <div className="d-flex gap-2 flex-wrap">
                        <Button tag={Link} to={step.href} color="secondary" outline size="sm">
                          Open
                        </Button>
                        {!isDone && (
                          <Button
                            color="primary"
                            size="sm"
                            disabled={completeMutation.isPending}
                            onClick={() => completeMutation.mutate(step.key)}
                          >
                            Mark complete
                          </Button>
                        )}
                      </div>
                    </CardBody>
                  </Card>
                </Col>
              );
            })}
          </Row>

          {telemetry && (
            <Card>
              <CardBody>
                <h5>Telemetry preference</h5>
                <p className="text-muted small">Optional product improvement data for this org.</p>
                <FormGroup check className="mb-2">
                  <Input
                    type="checkbox"
                    id="telemetry-optin"
                    checked={telemetry.optIn}
                    onChange={(e) =>
                      telemetryMutation.mutate({ ...telemetry, optIn: e.target.checked })
                    }
                  />
                  <Label check for="telemetry-optin">
                    Opt in to telemetry
                  </Label>
                </FormGroup>
                <FormGroup check className="mb-2">
                  <Input
                    type="checkbox"
                    id="telemetry-crash"
                    checked={telemetry.crashReports}
                    disabled={!telemetry.optIn}
                    onChange={(e) =>
                      telemetryMutation.mutate({ ...telemetry, crashReports: e.target.checked })
                    }
                  />
                  <Label check for="telemetry-crash">
                    Crash reports
                  </Label>
                </FormGroup>
                <FormGroup check>
                  <Input
                    type="checkbox"
                    id="telemetry-perf"
                    checked={telemetry.performanceMetrics}
                    disabled={!telemetry.optIn}
                    onChange={(e) =>
                      telemetryMutation.mutate({
                        ...telemetry,
                        performanceMetrics: e.target.checked,
                      })
                    }
                  />
                  <Label check for="telemetry-perf">
                    Performance metrics
                  </Label>
                </FormGroup>
              </CardBody>
            </Card>
          )}
        </>
      )}
    </div>
  );
};
