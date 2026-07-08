import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { useQuery } from '@tanstack/react-query';
import {
  Card, CardBody, CardHeader, Form, FormGroup, Label, Input, Button, Alert, Spinner, Row, Col,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { podService } from '../services/podService';
import { providerService } from '../services/providerService';
import { useOrganization } from '../contexts/OrganizationContext';
import {
  GPU_TYPES,
  PERMISSIONS,
  type CreatePodRequest,
  type GpuType,
} from '../types';

export const CreatePodPage = () => {
  const navigate = useNavigate();
  const { hasPermission } = useOrganization();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const canCreate = hasPermission(PERMISSIONS.PodCreate);

  const { data: providers = [] } = useQuery({
    queryKey: ['providers'],
    queryFn: providerService.list,
    enabled: canCreate,
  });

  const enabledProviders = providers.filter((p) => p.isEnabled && p.isValidated);

  const { register, handleSubmit, watch, setValue, formState: { errors } } = useForm<CreatePodRequest>({
    defaultValues: {
      containerDiskGb: 50,
      volumeDiskGb: 20,
      volumeMountPath: '/workspace',
      gpuCount: 1,
      enablePublicIp: true,
      ports: ['8888/http', '11434/http'],
      environmentVariables: {},
    },
  });

  const providerId = watch('providerId');
  const selectedProvider = enabledProviders.find((p) => p.id === providerId);

  const { data: regions = [] } = useQuery({
    queryKey: ['provider-regions', providerId],
    queryFn: () => providerService.getRegions(providerId),
    enabled: !!providerId,
  });

  const { data: gpus = [] } = useQuery({
    queryKey: ['provider-gpus', providerId],
    queryFn: () => providerService.getGpus(providerId),
    enabled: !!providerId,
  });

  const { data: templates = [] } = useQuery({
    queryKey: ['provider-templates', providerId],
    queryFn: () => providerService.getTemplates(providerId),
    enabled: !!providerId,
  });

  const nameField = register('name', { required: 'Pod name is required' });
  const providerField = register('providerId', { required: 'Provider is required' });
  const gpuIdField = register('gpuId', { required: 'GPU is required' });
  const gpuTypeField = register('gpuType', { required: 'GPU type is required' });
  const regionField = register('region', { required: 'Region is required' });
  const templateIdField = register('templateId');
  const imageNameField = register('imageName', {
    required: 'Image name is required when no template is selected',
  });
  const containerDiskField = register('containerDiskGb', { valueAsNumber: true, required: true });
  const volumeDiskField = register('volumeDiskGb', { valueAsNumber: true, required: true });
  const enablePublicField = register('enablePublicIp');

  const templateId = watch('templateId');

  const onSubmit = async (data: CreatePodRequest) => {
    if (!data.templateId && !data.imageName) {
      toast.error('Select a template or provide an image name');
      return;
    }

    setIsSubmitting(true);
    try {
      const pod = await podService.create({
        ...data,
        ports: data.ports ?? ['8888/http', '11434/http'],
        environmentVariables: data.environmentVariables ?? {},
      });
      toast.success('Pod created');
      navigate(`/pods/${pod.id}`);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to create pod');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!canCreate) {
    return <Alert color="warning">You don&apos;t have permission to create pods.</Alert>;
  }

  if (enabledProviders.length === 0) {
    return (
      <Alert color="info">
        Add and validate a provider before creating pods.{' '}
        <a href="/providers/add">Add Provider</a>
      </Alert>
    );
  }

  return (
    <div>
      <h1 className="page-title mb-4">Create GPU Pod</h1>
      <Card>
        <CardHeader>Pod Configuration</CardHeader>
        <CardBody>
          <Form onSubmit={handleSubmit(onSubmit)}>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label for="name">Pod Name</Label>
                  <Input id="name" innerRef={nameField.ref} onChange={nameField.onChange} onBlur={nameField.onBlur} invalid={!!errors.name} />
                  {errors.name && <div className="invalid-feedback d-block">{errors.name.message}</div>}
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label for="providerId">Provider</Label>
                  <Input id="providerId" type="select" innerRef={providerField.ref} onChange={providerField.onChange} onBlur={providerField.onBlur} invalid={!!errors.providerId}>
                    <option value="">Select provider</option>
                    {enabledProviders.map((p) => (
                      <option key={p.id} value={p.id}>{p.displayName}</option>
                    ))}
                  </Input>
                  {errors.providerId && <div className="invalid-feedback d-block">{errors.providerId.message}</div>}
                </FormGroup>
              </Col>
            </Row>

            {selectedProvider && (
              <>
                <Row>
                  <Col md={6}>
                    <FormGroup>
                      <Label for="region">Region</Label>
                      <Input id="region" type="select" innerRef={regionField.ref} onChange={regionField.onChange} onBlur={regionField.onBlur} invalid={!!errors.region}>
                        <option value="">Select region</option>
                        {regions.map((r) => (
                          <option key={r.regionId} value={r.regionId}>{r.name}</option>
                        ))}
                      </Input>
                    </FormGroup>
                  </Col>
                  <Col md={6}>
                    <FormGroup>
                      <Label for="gpuId">GPU</Label>
                      <Input
                        id="gpuId"
                        type="select"
                        innerRef={gpuIdField.ref}
                        onChange={(e) => {
                          gpuIdField.onChange(e);
                          const gpu = gpus.find((g) => g.gpuId === e.target.value);
                          if (gpu) {
                            setValue('gpuType', gpu.gpuType as GpuType);
                          }
                        }}
                        onBlur={gpuIdField.onBlur}
                        invalid={!!errors.gpuId}
                      >
                        <option value="">Select GPU</option>
                        {gpus.map((g) => (
                          <option key={g.gpuId} value={g.gpuId}>{g.name}</option>
                        ))}
                      </Input>
                    </FormGroup>
                  </Col>
                </Row>

                <Row>
                  <Col md={6}>
                    <FormGroup>
                      <Label for="gpuType">GPU Type</Label>
                      <Input id="gpuType" type="select" innerRef={gpuTypeField.ref} onChange={gpuTypeField.onChange} onBlur={gpuTypeField.onBlur}>
                        {GPU_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                      </Input>
                    </FormGroup>
                  </Col>
                  <Col md={6}>
                    <FormGroup>
                      <Label for="templateId">Template</Label>
                      <Input
                        id="templateId"
                        type="select"
                        innerRef={templateIdField.ref}
                        onChange={(e) => {
                          templateIdField.onChange(e);
                          const template = templates.find((t) => t.templateId === e.target.value);
                          if (template?.imageName) {
                            setValue('imageName', template.imageName);
                            setValue('templateName', template.name);
                          }
                        }}
                        onBlur={templateIdField.onBlur}
                      >
                        <option value="">Custom image</option>
                        {templates.map((t) => (
                          <option key={t.templateId} value={t.templateId}>{t.name}</option>
                        ))}
                      </Input>
                    </FormGroup>
                  </Col>
                </Row>

                {!templateId && (
                  <FormGroup>
                    <Label for="imageName">Container Image</Label>
                    <Input id="imageName" innerRef={imageNameField.ref} onChange={imageNameField.onChange} onBlur={imageNameField.onBlur} placeholder="runpod/pytorch:2.1.0-py3.10-cuda11.8.0-devel-ubuntu22.04" />
                  </FormGroup>
                )}

                <Row>
                  <Col md={4}>
                    <FormGroup>
                      <Label for="containerDiskGb">Container Disk (GB)</Label>
                      <Input id="containerDiskGb" type="number" innerRef={containerDiskField.ref} onChange={containerDiskField.onChange} onBlur={containerDiskField.onBlur} />
                    </FormGroup>
                  </Col>
                  <Col md={4}>
                    <FormGroup>
                      <Label for="volumeDiskGb">Volume Disk (GB)</Label>
                      <Input id="volumeDiskGb" type="number" innerRef={volumeDiskField.ref} onChange={volumeDiskField.onChange} onBlur={volumeDiskField.onBlur} />
                    </FormGroup>
                  </Col>
                  <Col md={4} className="d-flex align-items-end">
                    <FormGroup check className="mb-3">
                      <Input type="checkbox" id="enablePublicIp" innerRef={enablePublicField.ref} onChange={enablePublicField.onChange} />
                      <Label check for="enablePublicIp">Public Endpoint</Label>
                    </FormGroup>
                  </Col>
                </Row>
              </>
            )}

            <div className="d-flex gap-2">
              <Button color="primary" type="submit" disabled={isSubmitting}>
                {isSubmitting ? <Spinner size="sm" /> : 'Create Pod'}
              </Button>
              <Button color="secondary" type="button" onClick={() => navigate('/pods')}>
                Cancel
              </Button>
            </div>
          </Form>
        </CardBody>
      </Card>
    </div>
  );
};
