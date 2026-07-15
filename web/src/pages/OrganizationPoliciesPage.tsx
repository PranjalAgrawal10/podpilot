import { useEffect, useState, type FormEvent } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import {
  Alert,
  Button,
  Col,
  Form,
  FormGroup,
  Input,
  Label,
  Row,
  Spinner,
} from 'reactstrap';
import { policyService } from '../services/policyService';
import { useOrganization } from '../contexts/OrganizationContext';
import {
  PERMISSIONS,
  type GovernancePolicy,
  type SecurityPolicy,
  type UpdatePoliciesRequest,
} from '../types';

const defaultSecurity = (): SecurityPolicy => ({
  minPasswordLength: 12,
  requireUppercase: true,
  requireDigit: true,
  requireNonAlphanumeric: true,
  requireMfa: false,
  sessionTimeoutMinutes: 480,
  maxConcurrentSessions: 5,
  ipAllowList: [],
  geoAllowList: [],
  apiKeyExpirationDays: 90,
  enforceApiKeyRotation: false,
  failedLoginAlertThreshold: 5,
});

const defaultGovernance = (): GovernancePolicy => ({
  allowedProviders: [],
  allowedModels: [],
  maximumGpuCostPerHour: null,
  maximumRunningPods: null,
  maximumQueueSize: null,
  maximumDailySpendUsd: null,
  allowedPlugins: [],
  allowedMcpServers: [],
  emptyAllowListMeansAllowAll: true,
});

const listToCsv = (items: string[]) => items.join(', ');
const csvToList = (value: string) =>
  value
    .split(',')
    .map((part) => part.trim())
    .filter(Boolean);

export const OrganizationPoliciesPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.PolicyRead);
  const canManage = hasPermission(PERMISSIONS.PolicyManage);

  const [security, setSecurity] = useState<SecurityPolicy>(defaultSecurity());
  const [governance, setGovernance] = useState<GovernancePolicy>(defaultGovernance());
  const [ipAllowList, setIpAllowList] = useState('');
  const [geoAllowList, setGeoAllowList] = useState('');
  const [allowedProviders, setAllowedProviders] = useState('');
  const [allowedModels, setAllowedModels] = useState('');
  const [allowedPlugins, setAllowedPlugins] = useState('');
  const [allowedMcpServers, setAllowedMcpServers] = useState('');

  const { data, isLoading, error } = useQuery({
    queryKey: ['policies', currentOrganization?.id],
    queryFn: policyService.get,
    enabled: !!currentOrganization?.id && canRead,
  });

  useEffect(() => {
    if (!data) return;
    setSecurity(data.security);
    setGovernance(data.governance);
    setIpAllowList(listToCsv(data.security.ipAllowList));
    setGeoAllowList(listToCsv(data.security.geoAllowList));
    setAllowedProviders(listToCsv(data.governance.allowedProviders));
    setAllowedModels(listToCsv(data.governance.allowedModels));
    setAllowedPlugins(listToCsv(data.governance.allowedPlugins));
    setAllowedMcpServers(listToCsv(data.governance.allowedMcpServers));
  }, [data]);

  const updateMutation = useMutation({
    mutationFn: (payload: UpdatePoliciesRequest) => policyService.update(payload),
    onSuccess: () => {
      toast.success('Policies updated');
      queryClient.invalidateQueries({ queryKey: ['policies', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to manage policies.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view policies.</Alert>;
  }

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (!canManage) {
      toast.error('You need Policy.Manage to update policies.');
      return;
    }
    updateMutation.mutate({
      security: {
        ...security,
        ipAllowList: csvToList(ipAllowList),
        geoAllowList: csvToList(geoAllowList),
      },
      governance: {
        ...governance,
        allowedProviders: csvToList(allowedProviders),
        allowedModels: csvToList(allowedModels),
        allowedPlugins: csvToList(allowedPlugins),
        allowedMcpServers: csvToList(allowedMcpServers),
      },
    });
  };

  return (
    <div>
      <h1 className="page-title mb-1">Organization policies</h1>
      <p className="text-muted mb-4">Security and governance settings for {currentOrganization.name}.</p>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load policies'}</Alert>
      )}

      {!isLoading && !error && (
        <Form onSubmit={onSubmit} style={{ maxWidth: 900 }}>
          <fieldset disabled={!canManage}>
            <h2 className="h5 mb-3">Security policy</h2>
            <Row>
              <Col md={4}>
                <FormGroup>
                  <Label>Min password length</Label>
                  <Input
                    type="number"
                    value={security.minPasswordLength}
                    onChange={(e) =>
                      setSecurity({ ...security, minPasswordLength: Number(e.target.value) })
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup>
                  <Label>Session timeout (minutes)</Label>
                  <Input
                    type="number"
                    value={security.sessionTimeoutMinutes}
                    onChange={(e) =>
                      setSecurity({ ...security, sessionTimeoutMinutes: Number(e.target.value) })
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup>
                  <Label>Max concurrent sessions</Label>
                  <Input
                    type="number"
                    value={security.maxConcurrentSessions}
                    onChange={(e) =>
                      setSecurity({ ...security, maxConcurrentSessions: Number(e.target.value) })
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={4}>
                <FormGroup check className="mb-3">
                  <Input
                    type="checkbox"
                    checked={security.requireUppercase}
                    onChange={(e) => setSecurity({ ...security, requireUppercase: e.target.checked })}
                  />
                  <Label check>Require uppercase</Label>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup check className="mb-3">
                  <Input
                    type="checkbox"
                    checked={security.requireDigit}
                    onChange={(e) => setSecurity({ ...security, requireDigit: e.target.checked })}
                  />
                  <Label check>Require digit</Label>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup check className="mb-3">
                  <Input
                    type="checkbox"
                    checked={security.requireNonAlphanumeric}
                    onChange={(e) =>
                      setSecurity({ ...security, requireNonAlphanumeric: e.target.checked })
                    }
                  />
                  <Label check>Require symbol</Label>
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={4}>
                <FormGroup check className="mb-3">
                  <Input
                    type="checkbox"
                    checked={security.requireMfa}
                    onChange={(e) => setSecurity({ ...security, requireMfa: e.target.checked })}
                  />
                  <Label check>Require MFA</Label>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup check className="mb-3">
                  <Input
                    type="checkbox"
                    checked={security.enforceApiKeyRotation}
                    onChange={(e) =>
                      setSecurity({ ...security, enforceApiKeyRotation: e.target.checked })
                    }
                  />
                  <Label check>Enforce API key rotation</Label>
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup>
                  <Label>API key expiration (days)</Label>
                  <Input
                    type="number"
                    value={security.apiKeyExpirationDays}
                    onChange={(e) =>
                      setSecurity({ ...security, apiKeyExpirationDays: Number(e.target.value) })
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={4}>
                <FormGroup>
                  <Label>Failed login alert threshold</Label>
                  <Input
                    type="number"
                    value={security.failedLoginAlertThreshold}
                    onChange={(e) =>
                      setSecurity({
                        ...security,
                        failedLoginAlertThreshold: Number(e.target.value),
                      })
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup>
                  <Label>IP allow list (comma-separated)</Label>
                  <Input value={ipAllowList} onChange={(e) => setIpAllowList(e.target.value)} />
                </FormGroup>
              </Col>
              <Col md={4}>
                <FormGroup>
                  <Label>Geo allow list (comma-separated)</Label>
                  <Input value={geoAllowList} onChange={(e) => setGeoAllowList(e.target.value)} />
                </FormGroup>
              </Col>
            </Row>

            <h2 className="h5 mb-3 mt-4">Governance policy</h2>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label>Allowed providers</Label>
                  <Input
                    value={allowedProviders}
                    onChange={(e) => setAllowedProviders(e.target.value)}
                    placeholder="Comma-separated"
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label>Allowed models</Label>
                  <Input
                    value={allowedModels}
                    onChange={(e) => setAllowedModels(e.target.value)}
                    placeholder="Comma-separated"
                  />
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label>Allowed plugins</Label>
                  <Input
                    value={allowedPlugins}
                    onChange={(e) => setAllowedPlugins(e.target.value)}
                    placeholder="Comma-separated"
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label>Allowed MCP servers</Label>
                  <Input
                    value={allowedMcpServers}
                    onChange={(e) => setAllowedMcpServers(e.target.value)}
                    placeholder="Comma-separated"
                  />
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={3}>
                <FormGroup>
                  <Label>Max GPU cost/hour</Label>
                  <Input
                    type="number"
                    step="0.01"
                    value={governance.maximumGpuCostPerHour ?? ''}
                    onChange={(e) =>
                      setGovernance({
                        ...governance,
                        maximumGpuCostPerHour: e.target.value === '' ? null : Number(e.target.value),
                      })
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup>
                  <Label>Max running pods</Label>
                  <Input
                    type="number"
                    value={governance.maximumRunningPods ?? ''}
                    onChange={(e) =>
                      setGovernance({
                        ...governance,
                        maximumRunningPods: e.target.value === '' ? null : Number(e.target.value),
                      })
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup>
                  <Label>Max queue size</Label>
                  <Input
                    type="number"
                    value={governance.maximumQueueSize ?? ''}
                    onChange={(e) =>
                      setGovernance({
                        ...governance,
                        maximumQueueSize: e.target.value === '' ? null : Number(e.target.value),
                      })
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup>
                  <Label>Max daily spend (USD)</Label>
                  <Input
                    type="number"
                    step="0.01"
                    value={governance.maximumDailySpendUsd ?? ''}
                    onChange={(e) =>
                      setGovernance({
                        ...governance,
                        maximumDailySpendUsd: e.target.value === '' ? null : Number(e.target.value),
                      })
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
            <FormGroup check className="mb-4">
              <Input
                type="checkbox"
                checked={governance.emptyAllowListMeansAllowAll}
                onChange={(e) =>
                  setGovernance({
                    ...governance,
                    emptyAllowListMeansAllowAll: e.target.checked,
                  })
                }
              />
              <Label check>Empty allow list means allow all</Label>
            </FormGroup>
          </fieldset>

          {canManage && (
            <Button color="primary" type="submit" disabled={updateMutation.isPending}>
              {updateMutation.isPending ? <Spinner size="sm" /> : 'Save policies'}
            </Button>
          )}
        </Form>
      )}
    </div>
  );
};
