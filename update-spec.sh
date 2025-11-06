#!/bin/bash
# Script to update the OpenAPI specification from MuleSoft Exchange
# Usage: ./update-spec.sh [version]
# Example: ./update-spec.sh 1.0.11

set -e

VERSION=${1:-"1.0.10"}
SPEC_URL="https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/organizations/9b04bfa8-6eeb-4d85-b676-91db930f8411/assets/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-unified-api-dev/${VERSION}/files/fat-oas/zip/?sha=1762453698202"
SPEC_DIR="./specs"
SPEC_FILE="${SPEC_DIR}/rosetta-api-v${VERSION}.json"

echo "🔄 Updating Rosetta API OpenAPI Specification"
echo "=============================================="
echo "Version: ${VERSION}"
echo ""

# Create specs directory if it doesn't exist
mkdir -p "${SPEC_DIR}"

# Download the specification
echo "📥 Downloading specification from MuleSoft Exchange..."
curl -L "${SPEC_URL}" -o /tmp/rosetta-spec.zip

# Extract the api.json file
echo "📦 Extracting specification..."
unzip -q -o /tmp/rosetta-spec.zip -d /tmp/rosetta-spec
cp /tmp/rosetta-spec/api.json "${SPEC_FILE}"

# Clean up
rm -rf /tmp/rosetta-spec /tmp/rosetta-spec.zip

echo "✅ Specification updated: ${SPEC_FILE}"
echo ""
echo "Next steps:"
echo "1. Update nswag.json to point to the new spec file"
echo "2. Run 'dotnet build' to regenerate the client code"
echo "3. Test the changes"
echo "4. Update version in UCD.Rosetta.Client.csproj"
echo "5. Commit and tag the release"
