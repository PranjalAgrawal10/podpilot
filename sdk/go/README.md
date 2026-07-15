# PodPilot Go SDK

## Install

```bash
go get github.com/podpilot/podpilot-go@latest
# or locally:
cd sdk/go && go build ./...
```

## Example

```go
package main

import (
	"fmt"
	"log"
	"os"

	podpilot "github.com/podpilot/podpilot-go"
)

func main() {
	client := podpilot.NewClient(os.Getenv("PODPILOT_API_URL"), "")
	auth, err := client.Login("you@example.com", "your-password")
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println("expires_in", auth.ExpiresIn)

	health, err := client.GetHealth()
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println("health", health.Status)

	pods, err := client.ListPods()
	if err != nil {
		log.Fatal(err)
	}
	for _, p := range pods {
		fmt.Println(p.ID, p.Name, p.Status)
	}

	gw, err := client.GetGatewayStats()
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println("gateway errors", gw.RecentErrors, "avg ms", gw.AverageLatencyMs)
}
```
