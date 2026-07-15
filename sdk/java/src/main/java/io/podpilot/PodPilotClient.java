package io.podpilot;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Minimal Java HttpClient wrapper for PodPilot (login, list pods, gateway health).
 */
public final class PodPilotClient {

    private static final Pattern STRING_FIELD = Pattern.compile("\"%s\"\\s*:\\s*\"([^\"]*)\"");
    private static final Pattern NUMBER_FIELD = Pattern.compile("\"%s\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)");

    private final String baseUrl;
    private final HttpClient http;
    private String accessToken;

    public PodPilotClient(String baseUrl) {
        this(baseUrl, null);
    }

    public PodPilotClient(String baseUrl, String accessToken) {
        String resolved = baseUrl;
        if (resolved == null || resolved.isBlank()) {
            resolved = Optional.ofNullable(System.getenv("PODPILOT_API_URL")).orElse("http://localhost:5000");
        }
        this.baseUrl = resolved.replaceAll("/$", "");
        this.accessToken = accessToken;
        this.http = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(30)).build();
    }

    public AuthResult login(String email, String password) throws IOException, InterruptedException {
        String body = "{\"email\":\"" + escape(email) + "\",\"password\":\"" + escape(password) + "\"}";
        String data = request("POST", "/api/v1/auth/login", body, false);
        AuthResult auth = new AuthResult(
                extractString(data, "accessToken"),
                extractString(data, "refreshToken"),
                (int) extractNumber(data, "expiresIn"),
                extractString(data, "tokenType"));
        this.accessToken = auth.accessToken();
        return auth;
    }

    public List<PodSummary> listPods() throws IOException, InterruptedException {
        String data = request("GET", "/api/v1/pods", null, true);
        return parsePods(data);
    }

    public HealthStatus getHealth() throws IOException, InterruptedException {
        String data = request("GET", "/api/v1/health", null, false);
        return new HealthStatus(extractString(data, "status"));
    }

    public GatewayHealth getGatewayStats() throws IOException, InterruptedException {
        String data = request("GET", "/api/v1/gateway/stats", null, true);
        return new GatewayHealth(
                (int) extractNumber(data, "activeRequests"),
                (int) extractNumber(data, "recentErrors"),
                extractNumber(data, "averageLatencyMs"));
    }

    private String request(String method, String path, String body, boolean auth)
            throws IOException, InterruptedException {
        HttpRequest.Builder builder = HttpRequest.newBuilder(URI.create(baseUrl + path))
                .timeout(Duration.ofSeconds(60))
                .header("Accept", "application/json");
        if (body != null) {
            builder.header("Content-Type", "application/json")
                    .method(method, HttpRequest.BodyPublishers.ofString(body, StandardCharsets.UTF_8));
        } else {
            builder.method(method, HttpRequest.BodyPublishers.noBody());
        }
        if (auth) {
            if (accessToken == null || accessToken.isBlank()) {
                throw new PodPilotException("Not authenticated. Call login() or pass an access token.");
            }
            builder.header("Authorization", "Bearer " + accessToken);
        }

        HttpResponse<String> response = http.send(builder.build(), HttpResponse.BodyHandlers.ofString());
        String raw = response.body();
        if (!raw.contains("\"success\":true") && !raw.contains("\"success\": true")) {
            String message = extractString(raw, "message");
            if (message == null || message.isBlank()) {
                message = "HTTP " + response.statusCode() + ": " + truncate(raw, 200);
            }
            throw new PodPilotException(message);
        }
        int dataIdx = raw.indexOf("\"data\"");
        if (dataIdx < 0) {
            throw new PodPilotException("API response missing data.");
        }
        return raw.substring(dataIdx);
    }

    private static List<PodSummary> parsePods(String dataSection) {
        List<PodSummary> pods = new ArrayList<>();
        Matcher idMatcher = Pattern.compile("\"id\"\\s*:\\s*\"([^\"]+)\"").matcher(dataSection);
        Matcher nameMatcher = Pattern.compile("\"name\"\\s*:\\s*\"([^\"]+)\"").matcher(dataSection);
        Matcher statusMatcher = Pattern.compile("\"status\"\\s*:\\s*\"([^\"]+)\"").matcher(dataSection);
        while (idMatcher.find() && nameMatcher.find() && statusMatcher.find()) {
            pods.add(new PodSummary(idMatcher.group(1), nameMatcher.group(1), statusMatcher.group(1)));
        }
        return pods;
    }

    private static String extractString(String json, String field) {
        Matcher m = Pattern.compile(String.format(STRING_FIELD.pattern(), field)).matcher(json);
        return m.find() ? m.group(1) : "";
    }

    private static double extractNumber(String json, String field) {
        Matcher m = Pattern.compile(String.format(NUMBER_FIELD.pattern(), field)).matcher(json);
        return m.find() ? Double.parseDouble(m.group(1)) : 0;
    }

    private static String escape(String value) {
        return Objects.requireNonNullElse(value, "")
                .replace("\\", "\\\\")
                .replace("\"", "\\\"");
    }

    private static String truncate(String value, int max) {
        return value.length() <= max ? value : value.substring(0, max) + "…";
    }

    public record AuthResult(String accessToken, String refreshToken, int expiresIn, String tokenType) {
    }

    public record PodSummary(String id, String name, String status) {
    }

    public record HealthStatus(String status) {
    }

    public record GatewayHealth(int activeRequests, int recentErrors, double averageLatencyMs) {
    }

    public static final class PodPilotException extends RuntimeException {
        public PodPilotException(String message) {
            super(message);
        }
    }
}
