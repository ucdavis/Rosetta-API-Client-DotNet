# Repository agent instructions

## Docker spec-update sandbox

Use the [Docker spec-update sandbox quick start](README.md#docker-spec-update-sandbox) and the detailed [sandbox guide](docs/SANDBOX.md) when checking or applying a Rosetta API specification update.

Choose a unique, lowercase `ROSETTA_SANDBOX_PROJECT` value for the current checkout and set it in every shell that runs Compose; never reuse another worktree's project name. Before starting anything, check the Docker engine with `docker info` and inspect the sandbox with `docker compose -f .devcontainer/docker-compose.sandbox.yml ps --status running --services`.

Do not start Docker Desktop or the sandbox service implicitly. If a check proves either is stopped, tell the user which one is stopped and ask for confirmation before starting it. Treat access-denied or permission errors as an inability to inspect Docker, not proof that Docker is stopped; request permission to retry the read-only check with the required access.

After confirmation, use `docker compose -f .devcontainer/docker-compose.sandbox.yml start --wait` for an existing stopped container. Use `up --build --detach --wait` for first setup or after changing the tooling image.

Run updater and build commands non-interactively with `docker compose ... exec -T tools ...`. The repository is bind-mounted at `/workspace`, so changes made by the container are changes to the normal host working tree; the user does not need to open a shell in the container. Review the working-tree diff after an update and never commit or discard changes automatically.

The version-check workflow and public MuleSoft page only report whether a new version exists. They do not edit files. Applying an update requires an explicit version passed to `update-spec.sh`.

When applying a specification update, update the README specification version badge to the same version as part of the work; the updater normally does this. Verify the badge and report the old and new README versions to the user without waiting for a separate request. If the badge cannot be updated, report the failure and do not call the update complete.

Do not run the integration tests unless the user asks and the required credentials are available. They call the real Rosetta service.
