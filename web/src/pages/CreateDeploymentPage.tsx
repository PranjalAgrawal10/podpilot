import { useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
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
  Table,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { deploymentService } from '../services/deploymentService';
import { providerService } from '../services/providerService';
import { useOrganization } from '../contexts/OrganizationContext';
import { getApiErrorMessage } from '../utils/getApiErrorMessage';
import { PERMISSIONS, type DeploymentTemplate, type GpuCatalogEntry } from '../types';

const RUNTIMES = ['Ollama', 'Vllm', 'LlamaCpp'] as const;
const STEPS = ['Provider', 'Region', 'GPU', 'Runtime', 'Models', 'Review'] as const;
type RegionSort = 'latency' | 'price' | 'availability';

export const CreateDeploymentPage = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canManage = hasPermission(PERMISSIONS.DeploymentManage);

  const [step, setStep] = useState(0);
  const [name, setName] = useState('');
  const [providerId, setProviderId] = useState('');
  const [region, setRegion] = useState('');
  const [regionSort, setRegionSort] = useState<RegionSort>('latency');
  const [gpuCode, setGpuCode] = useState('');
  const [gpuManualOverride, setGpuManualOverride] = useState(false);
  const [runtime, setRuntime] = useState<string>('Ollama');
  const [selectedModels, setSelectedModels] = useState<string[]>([]);
  const [templateCode, setTemplateCode] = useState<string | null>(null);
  const [templateApplied, setTemplateApplied] = useState(false);

  const { data: providers = [] } = useQuery({
    queryKey: ['providers', currentOrganization?.id],
    queryFn: providerService.list,
    enabled: !!currentOrganization?.id && canManage,
  });

  const enabledProviders = providers.filter((p) => p.isEnabled && p.isValidated);

  const { data: templates = [] } = useQuery({
    queryKey: ['deployment-templates', currentOrganization?.id],
    queryFn: deploymentService.listTemplates,
    enabled: !!currentOrganization?.id && canManage,
  });

  const { data: regions = [], isLoading: regionsLoading } = useQuery({
    queryKey: ['deployment-regions', currentOrganization?.id, providerId, regionSort],
    queryFn: () => deploymentService.listRegions(providerId, regionSort),
    enabled: !!providerId && canManage,
  });

  const { data: gpus = [] } = useQuery({
    queryKey: ['gpu-catalog', currentOrganization?.id],
    queryFn: deploymentService.listGpus,
    enabled: !!currentOrganization?.id && canManage,
  });

  const { data: modelCatalog = [] } = useQuery({
    queryKey: ['model-catalog', currentOrganization?.id],
    queryFn: deploymentService.listModelCatalog,
    enabled: !!currentOrganization?.id && canManage,
  });

  const { data: recommendation, isFetching: recommending } = useQuery({
    queryKey: ['gpu-recommend', currentOrganization?.id, selectedModels],
    queryFn: () => deploymentService.recommendGpu(selectedModels),
    enabled: selectedModels.length > 0 && canManage,
  });

  useEffect(() => {
    if (recommendation?.recommendedGpuCode && !gpuManualOverride) {
      setGpuCode(recommendation.recommendedGpuCode);
    }
  }, [recommendation, gpuManualOverride]);

  useEffect(() => {
    const code = searchParams.get('template');
    if (!code || templateApplied || templates.length === 0) return;
    const template = templates.find((t) => t.code === code);
    if (!template) return;
    setTemplateCode(template.code);
    setRuntime(template.runtime || 'Ollama');
    setSelectedModels([...template.defaultModelCodes]);
    if (template.recommendedGpuCode) {
      setGpuCode(template.recommendedGpuCode);
      setGpuManualOverride(true);
    }
    setName((prev) => prev.trim() || template.name);
    setTemplateApplied(true);
  }, [searchParams, templates, templateApplied]);

  const createMutation = useMutation({
    mutationFn: () =>
      deploymentService.create({
        name: name.trim(),
        providerId,
        region,
        gpuCode,
        runtime,
        models: selectedModels,
        templateCode: templateCode || undefined,
      }),
    onSuccess: (deployment) => {
      toast.success('Deployment started');
      void queryClient.invalidateQueries({ queryKey: ['deployments'] });
      void queryClient.invalidateQueries({ queryKey: ['deployment-dashboard'] });
      navigate(`/deployments/${deployment.id}`);
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Failed to create deployment')),
  });

  const applyTemplate = (template: DeploymentTemplate) => {
    setTemplateCode(template.code);
    setRuntime(template.runtime || 'Ollama');
    setSelectedModels([...template.defaultModelCodes]);
    if (template.recommendedGpuCode) {
      setGpuCode(template.recommendedGpuCode);
      setGpuManualOverride(true);
    }
    if (!name.trim()) {
      setName(template.name);
    }
    toast.info(`Template "${template.name}" applied`);
  };

  const selectedGpu: GpuCatalogEntry | undefined = gpus.find((g) => g.code === gpuCode);
  const requiredVram = recommendation?.requiredVramGb
    ?? Math.max(0, ...selectedModels.map((code) => {
      const m = modelCatalog.find((x) => x.code === code || x.modelReference === code);
      return m?.requiredVramGb ?? 0;
    }));
  const vramWarn = selectedGpu && requiredVram > 0 && selectedGpu.vramGb < requiredVram;

  const canNext = (): boolean => {
    switch (step) {
      case 0:
        return !!providerId;
      case 1:
        return !!region;
      case 2:
        return !!gpuCode;
      case 3:
        return !!runtime;
      case 4:
        return selectedModels.length > 0;
      case 5:
        return !!name.trim() && !!providerId && !!region && !!gpuCode && selectedModels.length > 0;
      default:
        return false;
    }
  };

  const toggleModel = (code: string) => {
    setSelectedModels((prev) =>
      prev.includes(code) ? prev.filter((c) => c !== code) : [...prev, code]);
  };

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to create a deployment.</Alert>;
  }

  if (!canManage) {
    return <Alert color="warning">You don&apos;t have permission to create deployments.</Alert>;
  }

  if (enabledProviders.length === 0) {
    return (
      <Alert color="info">
        Add and validate a compute provider before deploying.{' '}
        <Link to="/providers/add">Add Provider</Link>
      </Alert>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <h1 className="page-title mb-1">Deploy AI Pod</h1>
        <p className="text-muted mb-0">
          <Link to="/deployments">Deployments</Link> / Create
        </p>
      </div>

      <Progress value={((step + 1) / STEPS.length) * 100} className="mb-3" />
      <div className="d-flex flex-wrap gap-2 mb-4">
        {STEPS.map((label, index) => (
          <Badge key={label} color={index === step ? 'primary' : index < step ? 'success' : 'secondary'}>
            {index + 1}. {label}
          </Badge>
        ))}
      </div>

      {step === 0 && (
        <Card className="mb-3">
          <CardBody>
            <h5 className="mb-3">Optional template</h5>
            <Row className="mb-4">
              {templates.map((t) => (
                <Col key={t.id} md={6} lg={4} className="mb-3">
                  <Card
                    className={`h-100 ${templateCode === t.code ? 'border-primary' : ''}`}
                    style={{ cursor: 'pointer' }}
                    onClick={() => applyTemplate(t)}
                  >
                    <CardBody>
                      <div className="d-flex justify-content-between mb-2">
                        <strong>{t.name}</strong>
                        <Badge color="secondary">{t.kind}</Badge>
                      </div>
                      <p className="text-muted small mb-0">{t.description || t.runtime}</p>
                    </CardBody>
                  </Card>
                </Col>
              ))}
            </Row>

            <FormGroup>
              <Label for="providerId">Compute provider</Label>
              <Input
                id="providerId"
                type="select"
                value={providerId}
                onChange={(e) => {
                  setProviderId(e.target.value);
                  setRegion('');
                }}
              >
                <option value="">Select provider</option>
                {enabledProviders.map((p) => (
                  <option key={p.id} value={p.id}>{p.displayName}</option>
                ))}
              </Input>
            </FormGroup>
          </CardBody>
        </Card>
      )}

      {step === 1 && (
        <Card className="mb-3">
          <CardBody>
            <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
              <h5 className="mb-0">Region</h5>
              <Input
                type="select"
                value={regionSort}
                onChange={(e) => setRegionSort(e.target.value as RegionSort)}
                style={{ maxWidth: 200 }}
              >
                <option value="latency">Sort: latency</option>
                <option value="price">Sort: price</option>
                <option value="availability">Sort: availability</option>
              </Input>
            </div>
            {regionsLoading && <Spinner size="sm" />}
            {!regionsLoading && regions.length === 0 && (
              <Alert color="warning">No regions available for this provider.</Alert>
            )}
            <Table responsive hover size="sm">
              <thead>
                <tr>
                  <th />
                  <th>Region</th>
                  <th>Area</th>
                  <th>Latency</th>
                  <th>Price</th>
                  <th>Availability</th>
                </tr>
              </thead>
              <tbody>
                {regions.map((r) => (
                  <tr
                    key={r.code}
                    style={{ cursor: 'pointer' }}
                    className={region === r.code ? 'table-primary' : undefined}
                    onClick={() => setRegion(r.code)}
                  >
                    <td>
                      <Input type="radio" name="region" checked={region === r.code} readOnly />
                    </td>
                    <td>
                      <strong>{r.name}</strong>
                      <div className="text-muted small">{r.code}</div>
                    </td>
                    <td>{r.area}</td>
                    <td>{r.estimatedLatencyMs != null ? `${r.estimatedLatencyMs} ms` : '—'}</td>
                    <td>{r.priceScore != null ? r.priceScore.toFixed(2) : '—'}</td>
                    <td>{r.availabilityScore}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          </CardBody>
        </Card>
      )}

      {step === 2 && (
        <Card className="mb-3">
          <CardBody>
            <h5 className="mb-3">GPU</h5>
            {recommending && <p className="text-muted small">Updating recommendation…</p>}
            {recommendation && (
              <Alert color="info" className="mb-3">
                Recommended: <strong>{recommendation.recommendedGpuCode}</strong>
                {' '}(min {recommendation.minimumGpuCode}, {recommendation.requiredVramGb}GB VRAM).{' '}
                {recommendation.estimatedPerformance}
                {recommendation.warnings.length > 0 && (
                  <ul className="mb-0 mt-2">
                    {recommendation.warnings.map((w) => <li key={w}>{w}</li>)}
                  </ul>
                )}
                {gpuManualOverride && gpuCode !== recommendation.recommendedGpuCode && (
                  <div className="mt-2">
                    <Button
                      color="primary"
                      size="sm"
                      onClick={() => {
                        setGpuCode(recommendation.recommendedGpuCode);
                        setGpuManualOverride(false);
                      }}
                    >
                      Apply recommendation
                    </Button>
                  </div>
                )}
              </Alert>
            )}
            <Table responsive hover size="sm">
              <thead>
                <tr>
                  <th />
                  <th>GPU</th>
                  <th>VRAM</th>
                  <th>CUDA</th>
                  <th>$/hr</th>
                </tr>
              </thead>
              <tbody>
                {gpus.map((g) => (
                  <tr
                    key={g.id}
                    style={{ cursor: 'pointer' }}
                    className={gpuCode === g.code ? 'table-primary' : undefined}
                    onClick={() => {
                      setGpuCode(g.code);
                      setGpuManualOverride(true);
                    }}
                  >
                    <td>
                      <Input
                        type="radio"
                        name="gpu"
                        checked={gpuCode === g.code}
                        onChange={() => {
                          setGpuCode(g.code);
                          setGpuManualOverride(true);
                        }}
                      />
                    </td>
                    <td>{g.name} <Badge color="secondary" className="ms-1">{g.code}</Badge></td>
                    <td>{g.vramGb} GB</td>
                    <td>{g.cudaCapability}</td>
                    <td>${g.estimatedHourlyCostUsd.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
            {vramWarn && (
              <Alert color="warning" className="mb-0">
                Selected GPU has {selectedGpu?.vramGb}GB VRAM but models require ~{requiredVram}GB.
              </Alert>
            )}
          </CardBody>
        </Card>
      )}

      {step === 3 && (
        <Card className="mb-3">
          <CardBody>
            <h5 className="mb-3">Runtime</h5>
            <FormGroup>
              <Label for="runtime">Inference runtime</Label>
              <Input
                id="runtime"
                type="select"
                value={runtime}
                onChange={(e) => setRuntime(e.target.value)}
              >
                {RUNTIMES.map((r) => (
                  <option key={r} value={r}>{r}</option>
                ))}
              </Input>
            </FormGroup>
            <p className="text-muted small mb-0">Ollama is the default for one-click coding and chat pods.</p>
          </CardBody>
        </Card>
      )}

      {step === 4 && (
        <Card className="mb-3">
          <CardBody>
            <h5 className="mb-3">Models</h5>
            <Table responsive hover size="sm">
              <thead>
                <tr>
                  <th />
                  <th>Model</th>
                  <th>VRAM</th>
                  <th>Size</th>
                  <th>Runtime</th>
                </tr>
              </thead>
              <tbody>
                {modelCatalog.map((m) => (
                  <tr key={m.id} style={{ cursor: 'pointer' }} onClick={() => toggleModel(m.code)}>
                    <td>
                      <Input
                        type="checkbox"
                        checked={selectedModels.includes(m.code)}
                        onChange={() => toggleModel(m.code)}
                      />
                    </td>
                    <td>
                      <div>{m.name}</div>
                      <div className="text-muted small">{m.modelReference}</div>
                    </td>
                    <td>{m.requiredVramGb} GB</td>
                    <td>{m.downloadSizeGb} GB</td>
                    <td>{m.preferredRuntime}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
            {selectedModels.length === 0 && (
              <Alert color="warning" className="mb-0">Select at least one model.</Alert>
            )}
          </CardBody>
        </Card>
      )}

      {step === 5 && (
        <Card className="mb-3">
          <CardBody>
            <h5 className="mb-3">Review & deploy</h5>
            <FormGroup>
              <Label for="name">Deployment name</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="my-coding-pod"
              />
            </FormGroup>
            <Row>
              <Col md={6}>
                <dl className="mb-0">
                  <dt>Provider</dt>
                  <dd>{enabledProviders.find((p) => p.id === providerId)?.displayName}</dd>
                  <dt>Region</dt>
                  <dd>{region}</dd>
                  <dt>GPU</dt>
                  <dd>{gpuCode}{selectedGpu ? ` · $${selectedGpu.estimatedHourlyCostUsd.toFixed(2)}/hr` : ''}</dd>
                </dl>
              </Col>
              <Col md={6}>
                <dl className="mb-0">
                  <dt>Runtime</dt>
                  <dd>{runtime}</dd>
                  <dt>Models</dt>
                  <dd>{selectedModels.join(', ')}</dd>
                  <dt>Template</dt>
                  <dd>{templateCode || '—'}</dd>
                </dl>
              </Col>
            </Row>
            {selectedGpu && (
              <Alert color="secondary" className="mt-3 mb-0">
                Estimated hourly cost: <strong>${selectedGpu.estimatedHourlyCostUsd.toFixed(2)}</strong>
                {' '}(~${(selectedGpu.estimatedHourlyCostUsd * 730).toFixed(0)}/mo if always on)
              </Alert>
            )}
            {vramWarn && (
              <Alert color="warning" className="mt-3 mb-0">
                VRAM warning: {selectedGpu?.vramGb}GB selected vs ~{requiredVram}GB required.
              </Alert>
            )}
          </CardBody>
        </Card>
      )}

      <div className="d-flex gap-2">
        {step > 0 && (
          <Button color="secondary" outline onClick={() => setStep((s) => s - 1)}>
            Back
          </Button>
        )}
        {step < STEPS.length - 1 && (
          <Button color="primary" disabled={!canNext()} onClick={() => setStep((s) => s + 1)}>
            Next
          </Button>
        )}
        {step === STEPS.length - 1 && (
          <Button
            color="primary"
            disabled={!canNext() || createMutation.isPending}
            onClick={() => createMutation.mutate()}
          >
            {createMutation.isPending ? <Spinner size="sm" /> : 'Deploy'}
          </Button>
        )}
        <Button color="link" tag={Link} to="/deployments">Cancel</Button>
      </div>
    </div>
  );
};
