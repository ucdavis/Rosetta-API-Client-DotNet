#!/bin/bash
# Script to update the OpenAPI specification from MuleSoft Exchange
# Usage: ./update-spec.sh [version]
# Example: ./update-spec.sh 1.0.33
# Windows: run this script from Git Bash or WSL, not PowerShell or Command Prompt.
# Requires curl, unzip, awk, sed, grep, and mktemp. JSON is read with jq, macOS plutil, or PowerShell.
#
# To find the latest version:
#   1. Open https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-rosetta-api/
#   2. Open the Download dropdown menu
#   3. Hover over any download link — the version number appears in the URL shown in the browser status bar

set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

VERSION=${1:-"1.0.33"}
SPEC_URL="https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/organizations/9b04bfa8-6eeb-4d85-b676-91db930f8411/assets/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-rosetta-api/${VERSION}/files/fat-oas/zip/?sha=1786485917112"
SPEC_DIR="${SCRIPT_DIR}/specs"
SPEC_FILE="${SPEC_DIR}/rosetta-api.json"
GRAPHQL_FILE="${SPEC_DIR}/rosetta-api.graphql"

for required_command in curl unzip awk sed grep mktemp; do
    if ! command -v "${required_command}" >/dev/null 2>&1; then
        echo "Required command not found: ${required_command}" >&2
        exit 1
    fi
done

echo "🔄 Updating Rosetta API OpenAPI Specification"
echo "=============================================="
echo "Version: ${VERSION}"
echo ""

# Create specs directory if it doesn't exist
mkdir -p "${SPEC_DIR}"

TEMP_DIR="$(mktemp -d "${TMPDIR:-/tmp}/rosetta-spec.XXXXXX")"
SPEC_ZIP="${TEMP_DIR}/rosetta-spec.zip"
EXTRACT_DIR="${TEMP_DIR}/extracted"
GRAPHQL_TMP="${TEMP_DIR}/rosetta-api.graphql"

cleanup() {
    rm -rf "${TEMP_DIR}"
}
trap cleanup EXIT

# Download the specification
echo "📥 Downloading specification from MuleSoft Exchange..."
curl --fail --location --show-error "${SPEC_URL}" -o "${SPEC_ZIP}"

# Extract the api.json file
echo "📦 Extracting specification..."
mkdir -p "${EXTRACT_DIR}"
unzip -q -o "${SPEC_ZIP}" -d "${EXTRACT_DIR}"
cp "${EXTRACT_DIR}/api.json" "${SPEC_FILE}"

# Extract embedded GraphQL schema from externalDocs.description
echo "📐 Extracting GraphQL schema..."

extract_description() {
    if command -v jq >/dev/null 2>&1; then
        jq -r '.externalDocs.description' "${SPEC_FILE}"
    elif [[ "${OSTYPE:-}" == "darwin"* ]] && command -v plutil >/dev/null 2>&1; then
        plutil -extract externalDocs.description raw -o - "${SPEC_FILE}"
    elif command -v powershell.exe >/dev/null 2>&1; then
        powershell.exe -NoProfile -NonInteractive -Command \
            '[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false); (Get-Content -LiteralPath "./specs/rosetta-api.json" -Raw | ConvertFrom-Json).externalDocs.description'
    elif command -v pwsh >/dev/null 2>&1; then
        pwsh -NoProfile -NonInteractive -Command \
            '[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false); (Get-Content -LiteralPath "./specs/rosetta-api.json" -Raw | ConvertFrom-Json).externalDocs.description'
    else
        echo "❌ No supported JSON reader was found. Install jq or PowerShell; macOS can use its built-in plutil." >&2
        return 1
    fi
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

mv "${GRAPHQL_TMP}" "${GRAPHQL_FILE}"
echo "✅ GraphQL schema extracted: ${GRAPHQL_FILE}"

# Update README badge with new version
echo "📝 Updating README badge..."
README_FILE="${SCRIPT_DIR}/README.md"
if [ -f "${README_FILE}" ]; then
    # Use BSD sed syntax on macOS and GNU sed syntax elsewhere.
    if [[ "${OSTYPE:-}" == "darwin"* ]]; then
        # macOS
        sed -i '' "s/Rosetta%20API%20Spec-v[0-9.]*-blue/Rosetta%20API%20Spec-v${VERSION}-blue/" "${README_FILE}"
    else
        # Linux, WSL, or Git Bash
        sed -i "s/Rosetta%20API%20Spec-v[0-9.]*-blue/Rosetta%20API%20Spec-v${VERSION}-blue/" "${README_FILE}"
    fi
    echo "✅ README badge updated to v${VERSION}"
fi

echo "✅ Specification updated: ${SPEC_FILE}"
echo "📝 Version: ${VERSION}"
echo ""
echo "Next steps:"
echo "1. Run 'dotnet clean && dotnet build' to regenerate the client code"
echo "2. Test the changes with 'dotnet test'"
echo "3. Review and commit the updated spec and README"
