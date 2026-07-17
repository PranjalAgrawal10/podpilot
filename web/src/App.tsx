import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ToastContainer } from 'react-toastify';
import { AuthProvider } from './contexts/AuthContext';
import { OrganizationProvider } from './contexts/OrganizationContext';
import { ThemeProvider } from './contexts/ThemeContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { MainLayout } from './layouts/MainLayout';
import { AuthLayout } from './layouts/AuthLayout';
import { MarketingLayout } from './layouts/MarketingLayout';
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
import { PodPoolsPage } from './pages/PodPoolsPage';
import { AutoScalingPage } from './pages/AutoScalingPage';
import { CapacityPage } from './pages/CapacityPage';
import { HealthPage } from './pages/HealthPage';
import { LoadBalancerPage } from './pages/LoadBalancerPage';
import { ObservabilityDashboardPage } from './pages/ObservabilityDashboardPage';
import { MetricsPage } from './pages/MetricsPage';
import { AnalyticsPage } from './pages/AnalyticsPage';
import { ObservabilityHealthPage } from './pages/ObservabilityHealthPage';
import { AlertsPage } from './pages/AlertsPage';
import { CostsPage } from './pages/CostsPage';
import { AiProvidersPage } from './pages/AiProvidersPage';
import { AddAiProviderPage } from './pages/AddAiProviderPage';
import { AiProviderDetailsPage } from './pages/AiProviderDetailsPage';
import { ModelRegistryPage } from './pages/ModelRegistryPage';
import { RoutingPoliciesPage } from './pages/RoutingPoliciesPage';
import { AiProviderHealthPage } from './pages/AiProviderHealthPage';
import { RoutingDashboardPage } from './pages/RoutingDashboardPage';
import { RoutingPolicySettingsPage } from './pages/RoutingPolicySettingsPage';
import { ModelRankingPage } from './pages/ModelRankingPage';
import { RoutingSimulatePage } from './pages/RoutingSimulatePage';
import { PluginsPage } from './pages/PluginsPage';
import { PluginMarketplacePage } from './pages/PluginMarketplacePage';
import { PluginDetailsPage } from './pages/PluginDetailsPage';
import { PluginSettingsPage } from './pages/PluginSettingsPage';
import { McpServersPage } from './pages/McpServersPage';
import { McpToolsPage } from './pages/McpToolsPage';
import { SecurityPage } from './pages/SecurityPage';
import { AuditLogsPage } from './pages/AuditLogsPage';
import { SecretsPage } from './pages/SecretsPage';
import { IdentityProvidersPage } from './pages/IdentityProvidersPage';
import { OrganizationPoliciesPage } from './pages/OrganizationPoliciesPage';
import { CompliancePage } from './pages/CompliancePage';
import { SessionsPage } from './pages/SessionsPage';
import { TrustedDevicesPage } from './pages/TrustedDevicesPage';
import { BillingPage } from './pages/BillingPage';
import { UsagePage } from './pages/UsagePage';
import { SubscriptionsPage } from './pages/SubscriptionsPage';
import { DownloadsPage } from './pages/DownloadsPage';
import { DocumentationPage } from './pages/DocumentationPage';
import { SystemStatusPage } from './pages/SystemStatusPage';
import { OnboardingWizardPage } from './pages/OnboardingWizardPage';
import { LandingPage } from './pages/marketing/LandingPage';
import { FeaturesPage } from './pages/marketing/FeaturesPage';
import { PricingPage } from './pages/marketing/PricingPage';
import { DocsMarketingPage } from './pages/marketing/DocsMarketingPage';
import { BlogPage } from './pages/marketing/BlogPage';
import { RoadmapPage } from './pages/marketing/RoadmapPage';
import { CommunityPage } from './pages/marketing/CommunityPage';
import { ContactPage } from './pages/marketing/ContactPage';
import { DeploymentsPage } from './pages/DeploymentsPage';
import { CreateDeploymentPage } from './pages/CreateDeploymentPage';
import { DeploymentDetailsPage } from './pages/DeploymentDetailsPage';
import { DeploymentLogsPage } from './pages/DeploymentLogsPage';
import { DeploymentTemplatesPage } from './pages/DeploymentTemplatesPage';
import { DeploymentModelCatalogPage } from './pages/DeploymentModelCatalogPage';
import { GpuCatalogPage } from './pages/GpuCatalogPage';
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
                <Route element={<MarketingLayout />}>
                  <Route path="/" element={<LandingPage />} />
                  <Route path="/features" element={<FeaturesPage />} />
                  <Route path="/pricing" element={<PricingPage />} />
                  <Route path="/documentation" element={<DocsMarketingPage />} />
                  <Route path="/blog" element={<BlogPage />} />
                  <Route path="/roadmap" element={<RoadmapPage />} />
                  <Route path="/community" element={<CommunityPage />} />
                  <Route path="/contact" element={<ContactPage />} />
                  <Route path="/status" element={<SystemStatusPage />} />
                </Route>

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
                    <Route path="/deployments" element={<DeploymentsPage />} />
                    <Route path="/deployments/create" element={<CreateDeploymentPage />} />
                    <Route path="/deployments/templates" element={<DeploymentTemplatesPage />} />
                    <Route path="/deployments/models" element={<DeploymentModelCatalogPage />} />
                    <Route path="/deployments/gpus" element={<GpuCatalogPage />} />
                    <Route path="/deployments/:id/logs" element={<DeploymentLogsPage />} />
                    <Route path="/deployments/:id" element={<DeploymentDetailsPage />} />
                    <Route path="/gateway" element={<GatewayPage />} />
                    <Route path="/scheduler" element={<SchedulerPage />} />
                    <Route path="/scheduler/queue" element={<QueuePage />} />
                    <Route path="/scheduler/requests" element={<RequestsPage />} />
                    <Route path="/scheduler/requests/:id" element={<RequestDetailsPage />} />
                    <Route path="/orchestration/pools" element={<PodPoolsPage />} />
                    <Route path="/orchestration/scaling" element={<AutoScalingPage />} />
                    <Route path="/orchestration/capacity" element={<CapacityPage />} />
                    <Route path="/orchestration/health" element={<HealthPage />} />
                    <Route path="/orchestration/load-balancer" element={<LoadBalancerPage />} />
                    <Route path="/observability" element={<ObservabilityDashboardPage />} />
                    <Route path="/observability/metrics" element={<MetricsPage />} />
                    <Route path="/observability/analytics" element={<AnalyticsPage />} />
                    <Route path="/observability/health" element={<ObservabilityHealthPage />} />
                    <Route path="/observability/alerts" element={<AlertsPage />} />
                    <Route path="/observability/costs" element={<CostsPage />} />
                    <Route path="/ai/providers" element={<AiProvidersPage />} />
                    <Route path="/ai/providers/add" element={<AddAiProviderPage />} />
                    <Route path="/ai/providers/:id" element={<AiProviderDetailsPage />} />
                    <Route path="/ai/models" element={<ModelRegistryPage />} />
                    <Route path="/ai/routing" element={<RoutingPoliciesPage />} />
                    <Route path="/ai/health" element={<AiProviderHealthPage />} />
                    <Route path="/routing" element={<RoutingDashboardPage />} />
                    <Route path="/routing/policies" element={<RoutingPolicySettingsPage />} />
                    <Route path="/routing/models" element={<ModelRankingPage />} />
                    <Route path="/routing/simulate" element={<RoutingSimulatePage />} />
                    <Route path="/plugins" element={<PluginsPage />} />
                    <Route path="/plugins/marketplace" element={<PluginMarketplacePage />} />
                    <Route path="/plugins/:id/settings" element={<PluginSettingsPage />} />
                    <Route path="/plugins/:id" element={<PluginDetailsPage />} />
                    <Route path="/mcp/servers" element={<McpServersPage />} />
                    <Route path="/mcp/tools" element={<McpToolsPage />} />
                    <Route path="/security" element={<SecurityPage />} />
                    <Route path="/security/audit" element={<AuditLogsPage />} />
                    <Route path="/security/secrets" element={<SecretsPage />} />
                    <Route path="/security/identity-providers" element={<IdentityProvidersPage />} />
                    <Route path="/security/policies" element={<OrganizationPoliciesPage />} />
                    <Route path="/security/compliance" element={<CompliancePage />} />
                    <Route path="/security/sessions" element={<SessionsPage />} />
                    <Route path="/security/devices" element={<TrustedDevicesPage />} />
                    <Route path="/billing" element={<BillingPage />} />
                    <Route path="/billing/usage" element={<UsagePage />} />
                    <Route path="/billing/subscriptions" element={<SubscriptionsPage />} />
                    <Route path="/downloads" element={<DownloadsPage />} />
                    <Route path="/docs" element={<DocumentationPage />} />
                    <Route path="/system-status" element={<SystemStatusPage />} />
                    <Route path="/onboarding" element={<OnboardingWizardPage />} />
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
