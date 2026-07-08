import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Button, Card, CardBody, Form, FormGroup, Input, Label } from 'reactstrap';
import { toast } from 'react-toastify';
import { useAuth } from '../contexts/AuthContext';
import type { LoginRequest } from '../types';

export const LoginPage = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const returnUrl = searchParams.get('returnUrl');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { register, handleSubmit, formState: { errors } } = useForm<LoginRequest>();

  const emailField = register('email', { required: 'Email is required' });
  const passwordField = register('password', { required: 'Password is required' });

  const onSubmit = async (data: LoginRequest) => {
    setIsSubmitting(true);
    try {
      await login(data);
      toast.success('Welcome back!');
      navigate(returnUrl && returnUrl.startsWith('/') ? returnUrl : '/dashboard');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Login failed';
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Card className="auth-card">
      <CardBody>
        <h2 className="text-center mb-4">Sign In</h2>
        <Form onSubmit={handleSubmit(onSubmit)}>
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
          <Button color="primary" block className="mt-3" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in...' : 'Sign In'}
          </Button>
        </Form>
        <p className="text-center mt-3 mb-0">
          Don&apos;t have an account? <Link to="/register">Register</Link>
        </p>
      </CardBody>
    </Card>
  );
};
