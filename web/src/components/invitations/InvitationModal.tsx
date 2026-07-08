import { useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  Form,
  FormGroup,
  Label,
  Input,
} from 'reactstrap';
import type { InviteMemberRequest, OrganizationRole } from '../../types';

interface InvitationModalProps {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (data: InviteMemberRequest) => Promise<void>;
}

const inviteRoles: OrganizationRole[] = ['Admin', 'Developer', 'Viewer'];

export const InvitationModal = ({ isOpen, toggle, onSubmit }: InvitationModalProps) => {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { register, handleSubmit, reset, formState: { errors } } = useForm<InviteMemberRequest>({
    defaultValues: { role: 'Developer' },
  });

  const emailField = register('email', {
    required: 'Email is required',
    pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Invalid email address' },
  });
  const roleField = register('role', { required: 'Role is required' });

  const handleFormSubmit = async (data: InviteMemberRequest) => {
    setIsSubmitting(true);
    try {
      await onSubmit(data);
      reset({ email: '', role: 'Developer' });
      toggle();
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} toggle={toggle}>
      <ModalHeader toggle={toggle}>Invite Member</ModalHeader>
      <Form onSubmit={handleSubmit(handleFormSubmit)}>
        <ModalBody>
          <FormGroup>
            <Label for="inviteEmail">Email</Label>
            <Input
              id="inviteEmail"
              type="email"
              placeholder="colleague@example.com"
              innerRef={emailField.ref}
              name={emailField.name}
              onBlur={emailField.onBlur}
              onChange={emailField.onChange}
              invalid={!!errors.email}
            />
            {errors.email && (
              <div className="invalid-feedback d-block">{errors.email.message}</div>
            )}
          </FormGroup>
          <FormGroup>
            <Label for="inviteRole">Role</Label>
            <Input
              id="inviteRole"
              type="select"
              innerRef={roleField.ref}
              name={roleField.name}
              onBlur={roleField.onBlur}
              onChange={roleField.onChange}
              invalid={!!errors.role}
            >
              {inviteRoles.map((role) => (
                <option key={role} value={role}>
                  {role}
                </option>
              ))}
            </Input>
            {errors.role && (
              <div className="invalid-feedback d-block">{errors.role.message}</div>
            )}
          </FormGroup>
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={toggle} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button color="primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Sending...' : 'Send Invitation'}
          </Button>
        </ModalFooter>
      </Form>
    </Modal>
  );
};
