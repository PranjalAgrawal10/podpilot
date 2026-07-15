package podpilot

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"os"
	"strings"
	"time"
)

// Client is a minimal PodPilot HTTP client.
type Client struct {
	BaseURL     string
	AccessToken string
	HTTP        *http.Client
}

// NewClient creates a client. Base URL defaults to PODPILOT_API_URL or http://localhost:5000.
func NewClient(baseURL, accessToken string) *Client {
	if strings.TrimSpace(baseURL) == "" {
		baseURL = os.Getenv("PODPILOT_API_URL")
	}
	if strings.TrimSpace(baseURL) == "" {
		baseURL = "http://localhost:5000"
	}
	return &Client{
		BaseURL:     strings.TrimRight(baseURL, "/"),
		AccessToken: accessToken,
		HTTP:        &http.Client{Timeout: 60 * time.Second},
	}
}

type apiEnvelope struct {
	Success bool            `json:"success"`
	Data    json.RawMessage `json:"data"`
	Message string          `json:"message"`
}

// AuthResult is returned by Login.
type AuthResult struct {
	AccessToken  string `json:"accessToken"`
	RefreshToken string `json:"refreshToken"`
	ExpiresIn    int    `json:"expiresIn"`
	TokenType    string `json:"tokenType"`
}

// PodSummary is a pod list item.
type PodSummary struct {
	ID     string `json:"id"`
	Name   string `json:"name"`
	Status string `json:"status"`
}

// HealthStatus is API health.
type HealthStatus struct {
	Status string `json:"status"`
}

// GatewayHealth is gateway stats used as a health signal.
type GatewayHealth struct {
	ActiveRequests   int     `json:"activeRequests"`
	RecentErrors     int     `json:"recentErrors"`
	AverageLatencyMs float64 `json:"averageLatencyMs"`
}

// Login authenticates and stores the access token on the client.
func (c *Client) Login(email, password string) (*AuthResult, error) {
	body := map[string]string{"email": email, "password": password}
	var auth AuthResult
	if err := c.request("POST", "/api/v1/auth/login", body, false, &auth); err != nil {
		return nil, err
	}
	c.AccessToken = auth.AccessToken
	return &auth, nil
}

// ListPods returns organization pods.
func (c *Client) ListPods() ([]PodSummary, error) {
	var pods []PodSummary
	if err := c.request("GET", "/api/v1/pods", nil, true, &pods); err != nil {
		return nil, err
	}
	return pods, nil
}

// GetHealth returns API health (anonymous).
func (c *Client) GetHealth() (*HealthStatus, error) {
	var health HealthStatus
	if err := c.request("GET", "/api/v1/health", nil, false, &health); err != nil {
		return nil, err
	}
	return &health, nil
}

// GetGatewayStats returns gateway dashboard stats (authenticated).
func (c *Client) GetGatewayStats() (*GatewayHealth, error) {
	var stats GatewayHealth
	if err := c.request("GET", "/api/v1/gateway/stats", nil, true, &stats); err != nil {
		return nil, err
	}
	return &stats, nil
}

func (c *Client) request(method, path string, body any, auth bool, out any) error {
	var reader io.Reader
	if body != nil {
		encoded, err := json.Marshal(body)
		if err != nil {
			return err
		}
		reader = bytes.NewReader(encoded)
	}

	req, err := http.NewRequest(method, c.BaseURL+path, reader)
	if err != nil {
		return err
	}
	req.Header.Set("Accept", "application/json")
	if body != nil {
		req.Header.Set("Content-Type", "application/json")
	}
	if auth {
		if c.AccessToken == "" {
			return fmt.Errorf("not authenticated: call Login or set AccessToken")
		}
		req.Header.Set("Authorization", "Bearer "+c.AccessToken)
	}

	res, err := c.HTTP.Do(req)
	if err != nil {
		return fmt.Errorf("network error: %w", err)
	}
	defer res.Body.Close()

	raw, err := io.ReadAll(res.Body)
	if err != nil {
		return err
	}

	var envelope apiEnvelope
	if err := json.Unmarshal(raw, &envelope); err != nil {
		return fmt.Errorf("invalid JSON (HTTP %d): %s", res.StatusCode, truncate(string(raw), 200))
	}
	if res.StatusCode < 200 || res.StatusCode >= 300 || !envelope.Success || len(envelope.Data) == 0 || string(envelope.Data) == "null" {
		msg := envelope.Message
		if msg == "" {
			msg = fmt.Sprintf("HTTP %d: %s", res.StatusCode, truncate(string(raw), 200))
		}
		return fmt.Errorf("%s", msg)
	}
	return json.Unmarshal(envelope.Data, out)
}

func truncate(s string, n int) string {
	if len(s) <= n {
		return s
	}
	return s[:n] + "…"
}
