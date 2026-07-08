import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import {
  Card,
  CardBody,
  Form,
  FormGroup,
  Label,
  Input,
  Button,
  Spinner,
  Alert,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { organizationService } from '../services/organizationService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS, type UpdateOrganizationRequest } from '../types';

export const OrganizationSettingsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { hasPermission } = useOrganization();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const canUpdate = hasPermission(PERMISSIONS.OrganizationUpdate);
  const canDelete = hasPermission(PERMISSIONS.OrganizationDelete);

  const { data: organization, isLoading, error } = useQuery({
    queryKey: ['organization', id],
    queryFn: () => organizationService.getById(id!),
    enabled: !!id,
  });

  const { register, handleSubmit, formState: { errors } } = useForm<UpdateOrganizationRequest>({
    values: organization
      ? { name: organization.name, description: organization.description ?? '' }
      : undefined,
  });

  const nameField = register('name', { required: 'Organization name is required' });
  const descriptionField = register('description');

  const onSubmit = async (data: UpdateOrganizationRequest) => {
    if (!id) return;
    setIsSubmitting(true);
    try {
      await organizationService.update(id, data);
      await queryClient.invalidateQueries({ queryKey: ['organizations'] });
      await queryClient.invalidateQueries({ queryKey: ['organization', id] });
      toast.success('Organization updated');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to update organization';
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!id) return;
    setIsDeleting(true);
    try {
      await organizationService.delete(id);
      await queryClient.invalidateQueries({ queryKey: ['organizations'] });
      toast.success('Organization deleted');
      navigate('/organizations');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to delete organization';
      toast.error(message);
    } finally {
      setIsDeleting(false);
      setDeleteModalOpen(false);
    }
  };

  if (isLoading) {
    return (
      <div className="text-center py-5">
        <Spinner />
      </div>
    );
  }

  if (error || !organization) {
    return (
      <Alert color="danger">
        {error instanceof Error ? error.message : 'Organization not found'}
      </Alert>
    );
  }

  return (
    <div>
      <h1 className="page-title">Organization Settings</h1>
      <p className="text-muted mb-4">{organization.slug}</p>

      <Card className="auth-card mb-4" style={{ maxWidth: 560 }}>
        <CardBody>
          {canUpdate ? (
            <Form onSubmit={handleSubmit(onSubmit)}>
              <FormGroup>
                <Label for="settingsName">Name</Label>
                <Input
                  id="settingsName"
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
                <Label for="settingsDescription">Description</Label>
                <Input
                  id="settingsDescription"
                  type="textarea"
                  rows={3}
                  innerRef={descriptionField.ref}
                  name={descriptionField.name}
                  onBlur={descriptionField.onBlur}
                  onChange={descriptionField.onChange}
                />
              </FormGroup>
              <Button color="primary" type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : 'Save Changes'}
              </Button>
            </Form>
          ) : (
            <Alert color="warning" className="mb-0">
              You don&apos;t have permission to edit this organization.
            </Alert>
          )}
        </CardBody>
      </Card>

      {canDelete && (
        <Card className="auth-card border-danger" style={{ maxWidth: 560 }}>
          <CardBody>
            <h5 className="text-danger">Danger Zone</h5>
            <p className="text-muted">
              Permanently delete this organization and all associated data.
            </p>
            <Button color="danger" outline onClick={() => setDeleteModalOpen(true)}>
              Delete Organization
            </Button>
          </CardBody>
        </Card>
      )}

      <Modal isOpen={deleteModalOpen} toggle={() => setDeleteModalOpen(false)}>
        <ModalHeader toggle={() => setDeleteModalOpen(false)}>Delete Organization</ModalHeader>
        <ModalBody>
          Are you sure you want to delete <strong>{organization.name}</strong>? This action cannot
          be undone.
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={() => setDeleteModalOpen(false)} disabled={isDeleting}>
            Cancel
          </Button>
          <Button color="danger" onClick={() => void handleDelete()} disabled={isDeleting}>
            {isDeleting ? 'Deleting...' : 'Delete'}
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};
