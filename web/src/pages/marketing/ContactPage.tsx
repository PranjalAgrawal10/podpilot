import { useState } from 'react';
import { Alert, Button, Form, FormGroup, Input, Label } from 'reactstrap';

export const ContactPage = () => {
  const [sent, setSent] = useState(false);

  return (
    <div className="marketing-page marketing-page-narrow">
      <h1 className="marketing-page-title">Contact</h1>
      <p className="marketing-page-lead">
        Enterprise licensing, demos, or self-hosted questions — we read every note.
      </p>

      {sent ? (
        <Alert color="success">Thanks — we will reply to the email you provided.</Alert>
      ) : (
        <Form
          className="marketing-contact-form"
          onSubmit={(e) => {
            e.preventDefault();
            setSent(true);
          }}
        >
          <FormGroup>
            <Label for="contact-name">Name</Label>
            <Input id="contact-name" required />
          </FormGroup>
          <FormGroup>
            <Label for="contact-email">Email</Label>
            <Input id="contact-email" type="email" required />
          </FormGroup>
          <FormGroup>
            <Label for="contact-message">Message</Label>
            <Input id="contact-message" type="textarea" rows={5} required />
          </FormGroup>
          <Button color="primary" type="submit">
            Send message
          </Button>
        </Form>
      )}

      <p className="text-muted small mt-4 mb-0">
        Or email <a href="mailto:hello@podpilot.io">hello@podpilot.io</a>
      </p>
    </div>
  );
};
