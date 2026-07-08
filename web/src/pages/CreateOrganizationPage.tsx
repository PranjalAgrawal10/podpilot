import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import {
  Card,
  CardBody,
  Form,
  FormGroup,
  Label,
  Input,
  Button,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { organizationService } from '../services/organizationService';
import { createSlug } from '../utils/slug';
import type { CreateOrganizationRequest } from '../types';

export const CreateOrganizationPage = () => {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [slugPreview, setSlugPreview] = useState('');
  const { register, handleSubmit, watch, formState: { errors } } = useForm<CreateOrganizationRequest>();

  const nameField = register('name', { required: 'Organization name is required' });
  const descriptionField = register('description');
  const nameValue = watch('name');

  useEffect(() => {
    setSlugPreview(nameValue ? createSlug(nameValue) : '');
  }, [nameValue]);

  const onSubmit = async (data: CreateOrganizationRequest) => {
    setIsSubmitting(true);
    try {
      const org = await organizationService.create(data);
      toast.success('Organization created successfully!');
      navigate(`/organizations/${org.id}/settings`);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to create organization';
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div>
      <h1 className="page-title">Create Organization</h1>
      <p className="text-muted mb-4">Set up a new organization for your team.</p>

      <Card className="auth-card" style={{ maxWidth: 560 }}>
        <CardBody>
          <Form onSubmit={handleSubmit(onSubmit)}>
            <FormGroup>
              <Label for="orgName">Organization Name</Label>
              <Input
                id="orgName"
                placeholder="Acme Corp"
                innerRef={nameField.ref}
                name={nameField.name}
                onBlur={nameField.onBlur}
                onChange={nameField.onChange}
                invalid={!!errors.name}
              />
              {errors.name && (
                <div className="invalid-feedback d-block">{errors.name.message}</div>
              )}
            </FormGroup>

            <FormGroup>
              <Label for="orgSlug">Slug (auto-generated)</Label>
              <Input id="orgSlug" value={slugPreview || 'your-organization'} disabled readOnly />
              <small className="text-muted">Used in URLs. Generated from the organization name.</small>
            </FormGroup>

            <FormGroup>
              <Label for="orgDescription">Description</Label>
              <Input
                id="orgDescription"
                type="textarea"
                rows={3}
                placeholder="Optional description"
                innerRef={descriptionField.ref}
                name={descriptionField.name}
                onBlur={descriptionField.onBlur}
                onChange={descriptionField.onChange}
              />
            </FormGroup>

            <div className="d-flex gap-2">
              <Button color="primary" type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Creating...' : 'Create Organization'}
              </Button>
              <Button color="secondary" outline onClick={() => navigate('/organizations')}>
                Cancel
              </Button>
            </div>
          </Form>
        </CardBody>
      </Card>
    </div>
  );
};
