# Sample .NET 8 API - GitOps Deployment on GKE with Argo CD & GitHub Actions

This repository contains a complete sample .NET 8 Web API and a GitOps deployment setup for **Google Kubernetes Engine (GKE)** managed by **Argo CD** and automated with **GitHub Actions**.

---

## 🏗️ Architecture Overview

```
[ Developer ] -> git push -> [ GitHub Repo ]
                                   |
                                   v (Triggers)
                          [ GitHub Actions ]
                                   |
                 +-----------------+-----------------+
                 |                                   |
                 v                                   v
      [ Build & Push Docker ]             [ Update k8s/kustomization.yaml ]
                 |                                   |
                 v                                   v
    [ Google Artifact Registry ]          [ Git Commit & Push ]
                                                     |
                                                     v
                                              [ Argo CD ] (Watches Git Repo)
                                                     |
                                                     v (Auto-Sync)
                                            [ GKE Cluster Pods ]
```

---

## 📁 Repository Structure

```
├── .github/
│   └── workflows/
│       └── ci-cd.yml          # GitHub Actions workflow
├── argocd/
│   └── application.yaml       # Argo CD Application Custom Resource
├── k8s/
│   ├── deployment.yaml        # Kubernetes Deployment template
│   ├── service.yaml           # Kubernetes Service template
│   └── kustomization.yaml     # Kustomize manifest (updated by CI/CD)
├── src/
│   └── SampleApi/             # .NET 8 Web API Project
│       ├── Program.cs
│       ├── appsettings.json
│       └── SampleApi.csproj
├── Dockerfile                 # Multi-stage Docker build file
├── .dockerignore
└── README.md
```

---

## 🚀 Setup & Deployment Step-by-Step Guide

### Step 1: Create Google Artifact Registry (GAR) Repository

In Google Cloud Shell or local terminal with `gcloud`:

```bash
# Enable required Google Cloud APIs
gcloud services enable artifactregistry.googleapis.com container.googleapis.com

# Create Artifact Registry Docker Repository
gcloud artifacts repositories create sample-dotnet-repo \
    --repository-format=docker \
    --location=us-central1 \
    --description="Docker repository for .NET API"
```

---

### Step 2: Set Up GCP Service Account for GitHub Actions

Create a GCP Service Account with permissions to push to Artifact Registry:

```bash
# Create Service Account
gcloud iam service-accounts create github-actions-sa \
    --description="Service account for GitHub Actions CI/CD" \
    --display-name="github-actions-sa"

# Grant Artifact Registry Writer role
gcloud projects add-iam-policy-binding YOUR_GCP_PROJECT_ID \
    --member="serviceAccount:github-actions-sa@YOUR_GCP_PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/artifactregistry.writer"

# Generate Service Account Key JSON file
gcloud iam service-accounts keys create gcp-sa-key.json \
    --iam-account=github-actions-sa@YOUR_GCP_PROJECT_ID.iam.gserviceaccount.com
```

---

### Step 3: Configure GitHub Repository Secrets

In your GitHub repository, go to **Settings > Secrets and variables > Actions** and add the following repository secrets:

| Secret Name | Description / Value |
| :--- | :--- |
| `GCP_PROJECT_ID` | Your Google Cloud Project ID (e.g., `my-gcp-project-123`) |
| `GCP_SA_KEY` | Entire contents of `gcp-sa-key.json` file generated in Step 2 |

---

### Step 4: Configure Argo CD Application

1. Open `argocd/application.yaml`.
2. Replace `repoURL` with your GitHub repository URL:
   ```yaml
   repoURL: 'https://github.com/YOUR_GITHUB_USERNAME/EdgePC_Deployment.git'
   ```
3. Apply the Argo CD application manifest to your GKE cluster:
   ```bash
   kubectl apply -f argocd/application.yaml
   ```

---

### Step 5: Trigger Deployment

Push code to the `main` branch:

```bash
git add .
git commit -m "feat: setup sample dotnet app with gitops pipeline"
git push origin main
```

1. **GitHub Actions Workflow** (`.github/workflows/ci-cd.yml`):
   - Builds the Docker image.
   - Pushes it to `us-central1-docker.pkg.dev/YOUR_PROJECT_ID/sample-dotnet-repo/sample-dotnet-api:<SHA>`.
   - Updates `k8s/kustomization.yaml` with the new image tag and commits it back.
2. **Argo CD**:
   - Detects the commit in `k8s/kustomization.yaml`.
   - Auto-syncs and deploys the new pods to GKE.

---

## 🔍 Verifying the Deployment

### Check Argo CD Status
```bash
argocd app get sample-dotnet-api
```

### Check Kubernetes Pods & Service on GKE
```bash
kubectl get pods -l app=sample-dotnet-api
kubectl get svc sample-dotnet-api-service
```

### Test API Endpoints
Once the `EXTERNAL-IP` is assigned by the GKE LoadBalancer:
- Root info endpoint: `http://<EXTERNAL-IP>/`
- App info endpoint: `http://<EXTERNAL-IP>/api/info`
- Health check: `http://<EXTERNAL-IP>/health`
