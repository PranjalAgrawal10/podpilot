import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import {
  Card,
  CardBody,
  CardHeader,
  Form,
  FormGroup,
  Label,
  Input,
  Button,
  Alert,
  Spinner,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { providerService } from '../services/providerService';
import { ConnectionStatus } from '../components/providers/ConnectionStatus';
import { RegionSelector } from '../components/providers/RegionSelector';
import { GpuTable } from '../components/providers/GpuTable';
import { TemplateSelector } from '../components/providers/TemplateSelector';
import { useOrganization } from '../contexts/OrganizationContext';
import {
  PERMISSIONS,
  PROVIDER_TYPES,
  PROVIDER_TYPE_LABELS,
  type CreateProviderRequest,
  type ProviderValidationResult,
} from '../types';

export const AddProviderPage = () => {
  const navigate = useNavigate();
  const { hasPermission } = useOrganization();
  const [isValidating, setIsValidating] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [validationResult, setValidationResult] = useState<ProviderValidationResult | null>(null);

  const canCreate = hasPermission(PERMISSIONS.ProviderCreate);

  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<CreateProviderRequest>({
    defaultValues: {
      providerType: 'RunPod',
    },
  });

  const nameField = register('name', { required: 'Name is required' });
  const providerTypeField = register('providerType', { required: 'Provider type is required' });
  const displayNameField = register('displayName', { required: 'Display name is required' });
  const descriptionField = register('description');
  const apiKeyField = register('apiKey', { required: 'API key is required' });
  const defaultRegionField = register('defaultRegion');

  const providerType = watch('providerType');
  const defaultRegion = watch('defaultRegion');
  const apiKey = watch('apiKey');

  const handleValidate = async () => {
    if (!apiKey) {
      toast.error('Enter an API key before validating');
      return;
    }

    setIsValidating(true);
    setValidationResult(null);
    try {
      const result = await providerService.validateNew({
        providerType,
        apiKey,
        defaultRegion: defaultRegion || null,
      });
      setValidationResult(result);
      if (result.isValid) {
        toast.success('Connection validated successfully');
        if (result.regions.length > 0 && !defaultRegion) {
          const firstRegion = result.regions.find((r) => r.isAvailable);
          if (firstRegion) {
            setValue('defaultRegion', firstRegion.regionId);
          }
        }
      } else {
        toast.error(result.message || 'Validation failed');
      }
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Validation failed';
      toast.error(message);
      setValidationResult(null);
    } finally {
      setIsValidating(false);
    }
  };

  const onSubmit = async (data: CreateProviderRequest) => {
    if (!validationResult?.isValid) {
      toast.error('Validate the connection before saving');
      return;
    }

    setIsSubmitting(true);
    try {
      const provider = await providerService.create(data);
      toast.success('Provider created successfully');
      navigate(`/providers/${provider.id}`);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to create provider';
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!canCreate) {
    return (
      <Alert color="warning">
        You don&apos;t have permission to add providers.
      </Alert>
    );
  }

  const isValidated = validationResult?.isValid === true;

  return (
    <div>
      <h1 className="page-title">Add Provider</h1>
      <p className="text-muted mb-4">
        Connect a GPU compute provider. Validate your API key before saving.
      </p>

      <Card className="auth-card provider-form-card">
        <CardBody>
          <Form onSubmit={handleSubmit(onSubmit)}>
            <div className="row">
              <div className="col-md-6">
                <FormGroup>
                  <Label for="providerName">Name</Label>
                  <Input
                    id="providerName"
                    placeholder="runpod-production"
                    innerRef={nameField.ref}
                    name={nameField.name}
                    onBlur={nameField.onBlur}
                    onChange={(e) => {
                      nameField.onChange(e);
                      setValidationResult(null);
                    }}
                    invalid={!!errors.name}
                  />
                  {errors.name && (
                    <div className="invalid-feedback d-block">{errors.name.message}</div>
                  )}
                </FormGroup>
              </div>
              <div className="col-md-6">
                <FormGroup>
                  <Label for="providerDisplayName">Display Name</Label>
                  <Input
                    id="providerDisplayName"
                    placeholder="RunPod Production"
                    innerRef={displayNameField.ref}
                    name={displayNameField.name}
                    onBlur={displayNameField.onBlur}
                    onChange={displayNameField.onChange}
                    invalid={!!errors.displayName}
                  />
                  {errors.displayName && (
                    <div className="invalid-feedback d-block">{errors.displayName.message}</div>
                  )}
                </FormGroup>
              </div>
            </div>

            <FormGroup>
              <Label for="providerType">Provider Type</Label>
              <Input
                id="providerType"
                type="select"
                innerRef={providerTypeField.ref}
                name={providerTypeField.name}
                onBlur={providerTypeField.onBlur}
                onChange={(e) => {
                  providerTypeField.onChange(e);
                  setValidationResult(null);
                }}
              >
                {PROVIDER_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {PROVIDER_TYPE_LABELS[type]}
                  </option>
                ))}
              </Input>
            </FormGroup>

            <FormGroup>
              <Label for="providerDescription">Description</Label>
              <Input
                id="providerDescription"
                type="textarea"
                rows={2}
                placeholder="Optional description"
                innerRef={descriptionField.ref}
                name={descriptionField.name}
                onBlur={descriptionField.onBlur}
                onChange={descriptionField.onChange}
              />
            </FormGroup>

            <FormGroup>
              <Label for="providerApiKey">API Key</Label>
              <Input
                id="providerApiKey"
                type="password"
                placeholder="Enter your provider API key"
                innerRef={apiKeyField.ref}
                name={apiKeyField.name}
                onBlur={apiKeyField.onBlur}
                onChange={(e) => {
                  apiKeyField.onChange(e);
                  setValidationResult(null);
                }}
                invalid={!!errors.apiKey}
              />
              {errors.apiKey && (
                <div className="invalid-feedback d-block">{errors.apiKey.message}</div>
              )}
            </FormGroup>

            {validationResult && validationResult.regions.length > 0 ? (
              <RegionSelector
                regions={validationResult.regions}
                value={defaultRegion}
                onChange={(regionId) => setValue('defaultRegion', regionId)}
                required
              />
            ) : (
              <FormGroup>
                <Label for="defaultRegion">Default Region</Label>
                <Input
                  id="defaultRegion"
                  placeholder="Validate to load regions"
                  disabled
                  innerRef={defaultRegionField.ref}
                  name={defaultRegionField.name}
                  onBlur={defaultRegionField.onBlur}
                  onChange={defaultRegionField.onChange}
                />
              </FormGroup>
            )}

            <div className="d-flex gap-2 mb-4">
              <Button
                type="button"
                color="info"
                outline
                disabled={isValidating || !apiKey}
                onClick={() => void handleValidate()}
              >
                {isValidating ? (
                  <>
                    <Spinner size="sm" className="me-2" />
                    Validating...
                  </>
                ) : (
                  'Validate Connection'
                )}
              </Button>
            </div>

            {validationResult && (
              <div className="validation-results mb-4">
                <ConnectionStatus
                  status={validationResult.connectionStatus}
                  message={validationResult.message}
                  account={validationResult.account}
                  isValid={validationResult.isValid}
                />

                {validationResult.gpus.length > 0 && (
                  <Card className="provider-section-card mt-3">
                    <CardHeader tag="h6" className="mb-0">
                      Available GPUs
                    </CardHeader>
                    <CardBody>
                      <GpuTable gpus={validationResult.gpus} compact />
                    </CardBody>
                  </Card>
                )}

                {validationResult.templates.length > 0 && (
                  <Card className="provider-section-card mt-3">
                    <CardHeader tag="h6" className="mb-0">
                      Available Templates
                    </CardHeader>
                    <CardBody>
                      <TemplateSelector templates={validationResult.templates} compact />
                    </CardBody>
                  </Card>
                )}
              </div>
            )}

            {!isValidated && (
              <Alert color="info" className="mb-3">
                Validate the connection to enable saving.
              </Alert>
            )}

            <div className="d-flex gap-2">
              <Button color="primary" type="submit" disabled={isSubmitting || !isValidated}>
                {isSubmitting ? 'Saving...' : 'Save Provider'}
              </Button>
              <Button color="secondary" outline onClick={() => navigate('/providers')}>
                Cancel
              </Button>
            </div>
          </Form>
        </CardBody>
      </Card>
    </div>
  );
};
