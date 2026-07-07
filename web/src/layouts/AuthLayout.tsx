import { Outlet } from 'react-router-dom';

export const AuthLayout = () => (
  <div className="auth-layout">
    <div className="auth-container">
      <div className="auth-header text-center mb-4">
        <h1 className="auth-brand">🚀 PodPilot</h1>
        <p className="text-muted">AI Infrastructure Autopilot</p>
      </div>
      <Outlet />
    </div>
  </div>
);
