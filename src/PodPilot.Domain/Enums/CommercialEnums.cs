namespace PodPilot.Domain.Enums;

/// <summary>
/// Commercial subscription plan tiers.
/// </summary>
public enum SubscriptionPlanTier
{
    /// <summary>Free tier.</summary>
    Free = 0,

    /// <summary>Pro tier.</summary>
    Pro = 1,

    /// <summary>Team tier.</summary>
    Team = 2,

    /// <summary>Enterprise tier.</summary>
    Enterprise = 3,
}

/// <summary>
/// Billing period for a subscription.
/// </summary>
public enum BillingInterval
{
    /// <summary>Billed monthly.</summary>
    Monthly = 0,

    /// <summary>Billed yearly.</summary>
    Yearly = 1,
}

/// <summary>
/// Pricing model for a plan.
/// </summary>
public enum PricingModel
{
    /// <summary>Flat subscription fee.</summary>
    Flat = 0,

    /// <summary>Per-seat pricing.</summary>
    SeatBased = 1,

    /// <summary>Metered usage pricing.</summary>
    UsageBased = 2,

    /// <summary>Hybrid flat + usage.</summary>
    Hybrid = 3,
}

/// <summary>
/// Payment provider.
/// </summary>
public enum PaymentProviderKind
{
    /// <summary>Stripe.</summary>
    Stripe = 0,

    /// <summary>Razorpay.</summary>
    Razorpay = 1,
}

/// <summary>
/// Subscription lifecycle status.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Trialing.</summary>
    Trialing = 0,

    /// <summary>Active paid subscription.</summary>
    Active = 1,

    /// <summary>Past due.</summary>
    PastDue = 2,

    /// <summary>Canceled.</summary>
    Canceled = 3,

    /// <summary>Incomplete setup.</summary>
    Incomplete = 4,
}

/// <summary>
/// Product edition / license edition.
/// </summary>
public enum LicenseEdition
{
    /// <summary>Community Edition.</summary>
    Community = 0,

    /// <summary>Professional.</summary>
    Professional = 1,

    /// <summary>Enterprise.</summary>
    Enterprise = 2,
}

/// <summary>
/// License deployment mode.
/// </summary>
public enum LicenseDeploymentMode
{
    /// <summary>Online validated license.</summary>
    Online = 0,

    /// <summary>Offline / air-gapped license.</summary>
    Offline = 1,

    /// <summary>Self-hosted license key.</summary>
    SelfHosted = 2,
}

/// <summary>
/// Usage metric kinds for metering and invoices.
/// </summary>
public enum UsageMetricKind
{
    /// <summary>GPU hours.</summary>
    GpuHours = 0,

    /// <summary>API / gateway requests.</summary>
    Requests = 1,

    /// <summary>LLM tokens.</summary>
    Tokens = 2,

    /// <summary>Bandwidth gigabytes.</summary>
    BandwidthGb = 3,

    /// <summary>Storage gigabytes.</summary>
    StorageGb = 4,

    /// <summary>Organizations count.</summary>
    Organizations = 5,

    /// <summary>Models count.</summary>
    Models = 6,

    /// <summary>Providers count.</summary>
    Providers = 7,
}

/// <summary>
/// Onboarding wizard step.
/// </summary>
public enum OnboardingStep
{
    /// <summary>Create organization.</summary>
    CreateOrganization = 0,

    /// <summary>Connect provider.</summary>
    ConnectProvider = 1,

    /// <summary>Create pod.</summary>
    CreatePod = 2,

    /// <summary>Install Ollama.</summary>
    InstallOllama = 3,

    /// <summary>Pull first model.</summary>
    PullFirstModel = 4,

    /// <summary>Connect Claude Code / IDE.</summary>
    ConnectClaudeCode = 5,

    /// <summary>Test AI gateway.</summary>
    TestAiGateway = 6,

    /// <summary>Completed.</summary>
    Completed = 7,
}
