# Docker spec-update sandbox

This standalone tooling container supplies Bash, `curl`, `unzip`, Mike Farah `yq` v4, and the .NET 8 SDK. The repository is bind-mounted at `/workspace`, so commands run in the container update the normal host checkout immediately. You never need to open an interactive shell in the container.

The image build does not copy the checkout or `.env` into the image. At runtime the trusted tooling container can see the bind-mounted checkout, so do not run untrusted commands in it.

## Check status without starting anything

Choose a unique Compose project name for this checkout. Use a different value for every Git worktree and set it again in each new terminal. From the repository root:

```powershell
$env:ROSETTA_SANDBOX_PROJECT = "rosetta-api-client-primary"
docker info
docker compose -f .devcontainer/docker-compose.sandbox.yml ps --status running --services
```

On macOS, use `export ROSETTA_SANDBOX_PROJECT=rosetta-api-client-primary` instead of the PowerShell assignment. The remaining `docker` commands are the same. Set the variable again in each new terminal.

`docker info` fails when Docker Desktop's Linux engine is not available. An access-denied or permission error means its status could not be inspected; it does not prove Docker is stopped. The second command prints `tools` only when this checkout's sandbox service is running. Agents must ask before starting Docker Desktop or the sandbox.

## Start the sandbox

After the user confirms, create the container the first time (or rebuild after changing the Dockerfile):

```powershell
docker compose -f .devcontainer/docker-compose.sandbox.yml up --build --detach --wait
```

After a normal `stop`, start the saved container without building or reinstalling tools:

```powershell
docker compose -f .devcontainer/docker-compose.sandbox.yml start --wait
```

The first image build needs internet access to pull the base images and install packages. The first .NET build also needs internet access to restore NuGet packages and tools. Stopping preserves those packages inside the container. The image remains available locally if the container is removed, but a replacement container would need to restore NuGet packages again because this setup has no separate NuGet cache volume.

## Apply and validate an API update

First determine the new version from the MuleSoft Exchange page or the scheduled GitHub version check. The check itself does not modify the repository. Then run:

```powershell
docker compose -f .devcontainer/docker-compose.sandbox.yml exec -T tools bash ./update-spec.sh <version>
docker compose -f .devcontainer/docker-compose.sandbox.yml exec -T tools dotnet build UCD.Rosetta.sln
```

The updater reads the archive's `exchange.json`, accepts a YAML or JSON main document, and normalizes it to the existing checked-in `specs/rosetta-api.json`. It also refreshes `specs/rosetta-api.graphql` and the README version badge. The Debug build regenerates the tracked REST client through NSwag. Verify that the README badge matches the applied spec version, review all resulting host-side changes, and report the README version change to the user before committing them.

The `IntegrationTests` project calls the real Rosetta service and requires credentials. Do not run it as part of a routine spec refresh unless the user explicitly requests it and has provided configuration safely.

## Stop or remove the sandbox

Stop it while keeping the container for a quick restart:

```powershell
docker compose -f .devcontainer/docker-compose.sandbox.yml stop
```

Remove the container and Compose network without affecting repository files:

```powershell
docker compose -f .devcontainer/docker-compose.sandbox.yml down
```

The sandbox has no named data volumes. Neither command reverts files written to the bind-mounted working tree.

This configuration has been built and checked on Docker Desktop for Windows. The same Compose setup is designed for Docker Desktop on macOS, but has not yet been run on a Mac. Docker Desktop may require the checkout directory to be shared before the bind mount can start. On native Linux, configure the service to run with the host user's UID and GID before writing through the bind mount, or generated files can become root-owned.
