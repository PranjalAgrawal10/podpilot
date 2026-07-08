import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ToastContainer } from 'react-toastify';
import { AuthProvider } from './contexts/AuthContext';
import { OrganizationProvider } from './contexts/OrganizationContext';
import { ThemeProvider } from './contexts/ThemeContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { MainLayout } from './layouts/MainLayout';
import { AuthLayout } from './layouts/AuthLayout';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { DashboardPage } from './pages/DashboardPage';
import { ProfilePage } from './pages/ProfilePage';
import { OrganizationsPage } from './pages/OrganizationsPage';
import { CreateOrganizationPage } from './pages/CreateOrganizationPage';
import { OrganizationSettingsPage } from './pages/OrganizationSettingsPage';
import { MembersPage } from './pages/MembersPage';
import { ProvidersPage } from './pages/ProvidersPage';
import { PodsPage } from './pages/PodsPage';
import { CreatePodPage } from './pages/CreatePodPage';
import { PodDetailsPage } from './pages/PodDetailsPage';
import { AddProviderPage } from './pages/AddProviderPage';
import { EditProviderPage } from './pages/EditProviderPage';
import { ProviderDetailsPage } from './pages/ProviderDetailsPage';
import { AcceptInvitationPage } from './pages/AcceptInvitationPage';
import { GatewayPage } from './pages/GatewayPage';
import { SchedulerPage } from './pages/SchedulerPage';
import { QueuePage } from './pages/QueuePage';
import { RequestsPage } from './pages/RequestsPage';
import { RequestDetailsPage } from './pages/RequestDetailsPage';
import { ModelsPage } from './pages/ModelsPage';
import { PullModelPage } from './pages/PullModelPage';
import { ModelDetailsPage } from './pages/ModelDetailsPage';
import { ModelDownloadsPage } from './pages/ModelDownloadsPage';
import { NotFoundPage } from './pages/NotFoundPage';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'react-toastify/dist/ReactToastify.css';
import './App.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          <OrganizationProvider>
            <BrowserRouter>
              <Routes>
                <Route element={<AuthLayout />}>
                  <Route path="/login" element={<LoginPage />} />
                  <Route path="/register" element={<RegisterPage />} />
                </Route>

                <Route element={<ProtectedRoute />}>
                  <Route element={<MainLayout />}>
                    <Route path="/dashboard" element={<DashboardPage />} />
                    <Route path="/profile" element={<ProfilePage />} />
                    <Route path="/organizations" element={<OrganizationsPage />} />
                    <Route path="/organizations/create" element={<CreateOrganizationPage />} />
                    <Route path="/organizations/:id/settings" element={<OrganizationSettingsPage />} />
                    <Route path="/members" element={<MembersPage />} />
                    <Route path="/providers" element={<ProvidersPage />} />
                    <Route path="/pods" element={<PodsPage />} />
                    <Route path="/pods/create" element={<CreatePodPage />} />
                    <Route path="/pods/:id" element={<PodDetailsPage />} />
                    <Route path="/gateway" element={<GatewayPage />} />
                    <Route path="/scheduler" element={<SchedulerPage />} />
                    <Route path="/scheduler/queue" element={<QueuePage />} />
                    <Route path="/scheduler/requests" element={<RequestsPage />} />
                    <Route path="/scheduler/requests/:id" element={<RequestDetailsPage />} />
                    <Route path="/models" element={<ModelsPage />} />
                    <Route path="/models/pull" element={<PullModelPage />} />
                    <Route path="/models/downloads" element={<ModelDownloadsPage />} />
                    <Route path="/models/:id" element={<ModelDetailsPage />} />
                    <Route path="/providers/add" element={<AddProviderPage />} />
                    <Route path="/providers/:id" element={<ProviderDetailsPage />} />
                    <Route path="/providers/:id/edit" element={<EditProviderPage />} />
                    <Route path="/invitations/accept" element={<AcceptInvitationPage />} />
                  </Route>
                </Route>

                <Route path="/" element={<Navigate to="/dashboard" replace />} />
                <Route path="*" element={<NotFoundPage />} />
              </Routes>
            </BrowserRouter>
            <ToastContainer position="top-right" autoClose={4000} theme="colored" />
          </OrganizationProvider>
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
