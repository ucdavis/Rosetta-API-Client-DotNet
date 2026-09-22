#!/bin/bash
# Script to update the OpenAPI specification from MuleSoft Exchange
# macOS, Linux, WSL, or Git Bash usage:
#   ./update-spec.sh <version>
#   Example: ./update-spec.sh 1.0.33
# VS Code PowerShell usage with Git for Windows in its default location:
#   & "C:\Program Files\Git\bin\bash.exe" ./update-spec.sh <version>
# PowerShell cannot execute this .sh file directly; the command above launches Git Bash.
# Requires curl, unzip, awk, sed, grep, mktemp, and Mike Farah yq v4.
# The Docker spec-update sandbox provides all required tools; see docs/SANDBOX.md.
#
# To find the latest version:
#   1. Open https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-rosetta-api/
#   2. Open the Download dropdown menu
#   3. Hover over any download link — the version number appears in the URL shown in the browser status bar

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

if [[ "$#" -ne 1 || ! "$1" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    echo "Usage: $0 <major.minor.patch>" >&2
    exit 2
fi

VERSION="$1"
SPEC_URL="https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/organizations/9b04bfa8-6eeb-4d85-b676-91db930f8411/assets/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-rosetta-api/${VERSION}/files/fat-oas/zip/"
SPEC_DIR="${SCRIPT_DIR}/specs"
SPEC_FILE="${SPEC_DIR}/rosetta-api.json"
GRAPHQL_FILE="${SPEC_DIR}/rosetta-api.graphql"
README_FILE="${SCRIPT_DIR}/README.md"

for required_command in curl unzip awk sed grep mktemp yq; do
    if ! command -v "${required_command}" >/dev/null 2>&1; then
        echo "Required command not found: ${required_command}" >&2
        exit 1
    fi
done

YQ_VERSION_OUTPUT="$(yq --version 2>&1)"
if [[ ! "${YQ_VERSION_OUTPUT}" =~ mikefarah/yq.*version[[:space:]]v?4\. ]]; then
    echo "❌ Mike Farah yq v4 is required; found: ${YQ_VERSION_OUTPUT}" >&2
    exit 1
fi

if [[ ! -f "${README_FILE}" ]]; then
    echo "❌ README.md was not found; the specification version badge cannot be updated." >&2
    exit 1
fi

README_BADGE="$(grep -oE 'Rosetta%20API%20Spec-v[0-9]+\.[0-9]+\.[0-9]+-blue' "${README_FILE}" || true)"
if [[ -z "${README_BADGE}" || "${README_BADGE}" == *$'\n'* ]]; then
    echo "❌ Expected exactly one Rosetta API specification version badge in README.md." >&2
    exit 1
fi
README_OLD_VERSION="${README_BADGE#Rosetta%20API%20Spec-v}"
README_OLD_VERSION="${README_OLD_VERSION%-blue}"

echo "🔄 Updating Rosetta API OpenAPI Specification"
echo "=============================================="
echo "Version: ${VERSION}"
echo ""

# Create specs directory if it doesn't exist
mkdir -p "${SPEC_DIR}"

TEMP_DIR="$(mktemp -d "${TMPDIR:-/tmp}/rosetta-spec.XXXXXX")"
SPEC_ZIP="${TEMP_DIR}/rosetta-spec.zip"
EXTRACT_DIR="${TEMP_DIR}/extracted"
SPEC_TMP="${TEMP_DIR}/rosetta-api.json"
GRAPHQL_TMP="${TEMP_DIR}/rosetta-api.graphql"
README_TMP="${TEMP_DIR}/README.md"

cleanup() {
    rm -rf "${TEMP_DIR}"
}
trap cleanup EXIT

# Download the specification
echo "📥 Downloading specification from MuleSoft Exchange..."
curl --fail --location --show-error "${SPEC_URL}" -o "${SPEC_ZIP}"

# Extract and normalize the OpenAPI document. MuleSoft declares the archive's
# main document in exchange.json; recent releases use api.yaml instead of JSON.
echo "📦 Extracting specification..."
mkdir -p "${EXTRACT_DIR}"

ARCHIVE_LIST="$(unzip -Z1 "${SPEC_ZIP}")"
if [[ -z "${ARCHIVE_LIST}" ]]; then
    echo "❌ Downloaded specification archive is empty." >&2
    exit 1
fi

while IFS= read -r archive_entry; do
    archive_entry="${archive_entry%$'\r'}"
    case "${archive_entry}" in
        ""|/*|../*|*/../*|*/..|*\\*)
            echo "❌ Unsafe path in specification archive: ${archive_entry:-<empty>}" >&2
            exit 1
            ;;
    esac
done <<< "${ARCHIVE_LIST}"

if unzip -Z -l "${SPEC_ZIP}" | awk '$1 ~ /^l/ { found=1 } END { exit(found ? 0 : 1) }'; then
    echo "❌ Symbolic links are not allowed in the specification archive." >&2
    exit 1
fi

unzip -q -o "${SPEC_ZIP}" -d "${EXTRACT_DIR}"

MANIFEST_FILE="${EXTRACT_DIR}/exchange.json"
if [[ -f "${MANIFEST_FILE}" ]]; then
    SPEC_ENTRY="$(yq eval -r '.main // ""' "${MANIFEST_FILE}")"
else
    spec_candidates=()
    for candidate in api.yaml api.yml api.json; do
        if [[ -f "${EXTRACT_DIR}/${candidate}" ]]; then
            spec_candidates+=("${candidate}")
        fi
    done

    if [[ "${#spec_candidates[@]}" -ne 1 ]]; then
        echo "❌ Archive has no exchange.json and expected exactly one api.yaml, api.yml, or api.json file." >&2
        exit 1
    fi

    SPEC_ENTRY="${spec_candidates[0]}"
fi

case "${SPEC_ENTRY}" in
    ""|/*|../*|*/../*|*/..|*\\*)
        echo "❌ Invalid main specification path in archive: ${SPEC_ENTRY:-<empty>}" >&2
        exit 1
        ;;
esac

SOURCE_SPEC="${EXTRACT_DIR}/${SPEC_ENTRY}"
if [[ ! -f "${SOURCE_SPEC}" ]]; then
    echo "❌ Main specification file was not found in archive: ${SPEC_ENTRY}" >&2
    exit 1
fi

OPENAPI_VERSION="$(yq eval -r '.openapi // .swagger // ""' "${SOURCE_SPEC}")"
if [[ -z "${OPENAPI_VERSION}" || "${OPENAPI_VERSION}" == "null" ]]; then
    echo "❌ Main document does not look like an OpenAPI or Swagger specification: ${SPEC_ENTRY}" >&2
    exit 1
fi

# Keep the checked-in artifact as JSON so the existing NSwag configuration does
# not depend on YAML support. yq accepts both YAML and JSON inputs.
yq eval -o=json '.' "${SOURCE_SPEC}" > "${SPEC_TMP}"
echo "✅ Loaded ${SPEC_ENTRY} (OpenAPI/Swagger ${OPENAPI_VERSION})"

# Extract embedded GraphQL schema from externalDocs.description
echo "📐 Extracting GraphQL schema..."

extract_description() {
    yq eval -r '.externalDocs.description // ""' "${SOURCE_SPEC}"
}

extract_description \
    | awk '{
        sub(/\r$/, "")
        sub(/[[:blank:]]+$/, "")
        if (!found && $0 ~ /^[[:blank:]]*```[[:space:]]*(graphql)?[[:space:]]*$/) { found=1; next }
        if (found && $0 ~ /^[[:blank:]]*```[[:space:]]*$/) { exit }
        if (found) { print }
    }' \
    | sed 's/^    //' \
    > "${GRAPHQL_TMP}"

# Append schema root declaration required by ZeroQL codegen
printf '\nschema {\n  query: Query\n}\n' >> "${GRAPHQL_TMP}"

# Validate: must contain at least one type definition
if ! grep -q '^type ' "${GRAPHQL_TMP}"; then
    echo "❌ Extracted content does not look like a GraphQL schema (no 'type' definitions found)"
    exit 1
fi

# Prepare and verify the README badge before publishing the downloaded specs.
echo "📝 Updating README badge..."
sed -E "s/Rosetta%20API%20Spec-v[0-9]+\.[0-9]+\.[0-9]+-blue/Rosetta%20API%20Spec-v${VERSION}-blue/" "${README_FILE}" > "${README_TMP}"

UPDATED_BADGE="$(grep -oE 'Rosetta%20API%20Spec-v[0-9]+\.[0-9]+\.[0-9]+-blue' "${README_TMP}" || true)"
if [[ "${UPDATED_BADGE}" != "Rosetta%20API%20Spec-v${VERSION}-blue" ]]; then
    echo "❌ README.md did not update to specification version ${VERSION}." >&2
    exit 1
fi

mv "${SPEC_TMP}" "${GRAPHQL_TMP}" "${SPEC_DIR}/"
mv "${README_TMP}" "${README_FILE}"
echo "✅ GraphQL schema extracted: ${GRAPHQL_FILE}"
echo "✅ README badge updated from v${README_OLD_VERSION} to v${VERSION}"

echo "✅ Specification updated: ${SPEC_FILE}"
echo "📝 Version: ${VERSION}"
echo ""
echo "Next steps:"
echo "1. Run 'dotnet build UCD.Rosetta.sln' to regenerate and validate the client code"
echo "2. Review the updated spec, generated client, GraphQL schema, and README"
echo "3. Run IntegrationTests only when real-service credentials are configured and testing was requested"
