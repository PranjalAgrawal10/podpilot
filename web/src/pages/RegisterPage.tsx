import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Button, Card, CardBody, Form, FormGroup, Input, Label } from 'reactstrap';
import { toast } from 'react-toastify';
import { useAuth } from '../contexts/AuthContext';
import type { RegisterRequest } from '../types';

export const RegisterPage = () => {
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { register, handleSubmit, formState: { errors } } = useForm<RegisterRequest>();

  const firstNameField = register('firstName', { required: 'First name is required' });
  const lastNameField = register('lastName', { required: 'Last name is required' });
  const emailField = register('email', { required: 'Email is required' });
  const passwordField = register('password', {
    required: 'Password is required',
    minLength: { value: 8, message: 'Minimum 8 characters' },
  });
  const organizationNameField = register('organizationName', { required: 'Organization name is required' });

  const onSubmit = async (data: RegisterRequest) => {
    setIsSubmitting(true);
    try {
      await registerUser(data);
      toast.success('Account created successfully!');
      navigate('/dashboard');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Registration failed';
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Card className="auth-card">
      <CardBody>
        <h2 className="text-center mb-4">Create Account</h2>
        <Form onSubmit={handleSubmit(onSubmit)}>
          <FormGroup>
            <Label for="firstName">First Name</Label>
            <Input
              id="firstName"
              innerRef={firstNameField.ref}
              name={firstNameField.name}
              onBlur={firstNameField.onBlur}
              onChange={firstNameField.onChange}
              invalid={!!errors.firstName}
            />
            {errors.firstName && <div className="invalid-feedback d-block">{errors.firstName.message}</div>}
          </FormGroup>
          <FormGroup>
            <Label for="lastName">Last Name</Label>
            <Input
              id="lastName"
              innerRef={lastNameField.ref}
              name={lastNameField.name}
              onBlur={lastNameField.onBlur}
              onChange={lastNameField.onChange}
              invalid={!!errors.lastName}
            />
            {errors.lastName && <div className="invalid-feedback d-block">{errors.lastName.message}</div>}
          </FormGroup>
          <FormGroup>
            <Label for="email">Email</Label>
            <Input
              id="email"
              type="email"
              innerRef={emailField.ref}
              name={emailField.name}
              onBlur={emailField.onBlur}
              onChange={emailField.onChange}
              invalid={!!errors.email}
            />
            {errors.email && <div className="invalid-feedback d-block">{errors.email.message}</div>}
          </FormGroup>
          <FormGroup>
            <Label for="password">Password</Label>
            <Input
              id="password"
              type="password"
              innerRef={passwordField.ref}
              name={passwordField.name}
              onBlur={passwordField.onBlur}
              onChange={passwordField.onChange}
              invalid={!!errors.password}
            />
            {errors.password && <div className="invalid-feedback d-block">{errors.password.message}</div>}
          </FormGroup>
          <FormGroup>
            <Label for="organizationName">Organization Name</Label>
            <Input
              id="organizationName"
              innerRef={organizationNameField.ref}
              name={organizationNameField.name}
              onBlur={organizationNameField.onBlur}
              onChange={organizationNameField.onChange}
              invalid={!!errors.organizationName}
            />
            {errors.organizationName && (
              <div className="invalid-feedback d-block">{errors.organizationName.message}</div>
            )}
          </FormGroup>
          <Button color="primary" block className="mt-3" disabled={isSubmitting}>
            {isSubmitting ? 'Creating account...' : 'Register'}
          </Button>
        </Form>
        <p className="text-center mt-3 mb-0">
          Already have an account? <Link to="/login">Sign In</Link>
        </p>
      </CardBody>
    </Card>
  );
};
