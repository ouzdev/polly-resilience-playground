# Polly Resilience Playground

[![GitHub Repo](https://img.shields.io/badge/GitHub-ouzdev/polly--resilience--playground-blue?logo=github)](https://github.com/ouzdev/polly-resilience-playground.git)
[![Build Status](https://github.com/ouzdev/polly-resilience-playground/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ouzdev/polly-resilience-playground/actions)

## Overview

**Polly Resilience Playground** is a demonstration project showcasing the use of **Polly**, a .NET resilience and transient-fault-handling library. This project implements various resilience patterns like:

- Retry
- Circuit Breaker
- Timeout
- Bulkhead Isolation
- Fallback

The goal is to provide practical examples and insights into building resilient .NET applications.

---

## Features

- **Retry Strategy**: Automatically retries failed operations with exponential backoff.
- **Circuit Breaker**: Prevents further calls to a failing resource until it recovers.
- **Timeout**: Ensures operations don't hang indefinitely.
- **Bulkhead Isolation**: Limits the number of concurrent operations to prevent resource exhaustion.
- **Fallback**: Provides a default value or alternate strategy when all else fails.

---

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/ouzdev/polly-resilience-playground.git
   ```

2. Navigate to the project directory:
   ```bash
   cd polly-resilience-playground
   ```

3. Open the solution in your favorite IDE (e.g., Visual Studio, Rider).

4. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

---

## Usage

### Option 1: Run Locally

1. Build and run the project:
   ```bash
   dotnet run
   ```

2. Access the available endpoints to test resilience strategies. Example:
   - `/retry`: Demonstrates the Retry pattern in action.

3. Explore the logs to see how Polly handles transient faults and implements resilience.

### Option 2: Run with Docker Compose

This project includes a `docker-compose.yml` file to set up the entire testing environment, which includes:
- **Guest API**: Simulates external API calls.
- **External API**: Represents services with transient faults for testing.
- **Grafana**: For visualizing metrics.
- **Prometheus**: For collecting metrics.

#### Steps to Run

1. Ensure Docker and Docker Compose are installed on your machine.

2. Navigate to the project directory:
   ```bash
   cd polly-resilience-playground
   ```

3. Start the Docker containers:
   ```bash
   docker-compose up
   ```

4. Verify the services are running:
   - **Grafana**: Access at [http://localhost:3000](http://localhost:3000).
   - **Prometheus**: Access at [http://localhost:9090](http://localhost:9090).
   - **APIs**: Use the provided endpoints for testing resilience strategies.

5. Stop the services:
   ```bash
   docker-compose down
   ```

---

## Project Structure

- **Controllers**: Defines API endpoints for demonstrating Polly strategies.
- **Services**: Implements the core logic and integrates Polly strategies.
- **Strategies**: Contains custom Polly strategies (e.g., Retry, Circuit Breaker).

---

## Articles

This project is accompanied by a three-part article series:

📌 **#1 - [Polly ile Resilient .NET Uygulamaları Geliştirme Rehberi](#)**
📌 **#2 - [Polly'nin Dayanıklılık (Resiliency) Patternleri](#)**
📌 **#3 - [Polly Metriklerini Analiz Etme ve Görselleştirme](#)**

---

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a feature branch.
3. Submit a pull request.

---

## License

This project is licensed under the [MIT License](LICENSE).

---

## Resources

- [Polly Documentation](https://github.com/App-vNext/Polly)
- [.NET Resilience Best Practices](https://learn.microsoft.com/en-us/dotnet/architecture/cloud-native/resilient-cloud-apps)

---

Feel free to explore and adapt the project to your needs. Happy coding! 🎉
