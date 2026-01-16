#!/bin/bash
# Script to update the OpenAPI specification from MuleSoft Exchange
# Usage: ./update-spec.sh [version]
# Example: ./update-spec.sh 1.0.11
# Check for latest version at:
# https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-unified-api-dev/

set -e

VERSION=${1:-"1.0.32"}
SPEC_URL="https://anypoint.mulesoft.com/exchange/portals/university-of-california-346/organizations/9b04bfa8-6eeb-4d85-b676-91db930f8411/assets/9b04bfa8-6eeb-4d85-b676-91db930f8411/iam-unified-api-dev/${VERSION}/files/fat-oas/zip/?sha=1762453698202"
SPEC_DIR="./specs"
SPEC_FILE="${SPEC_DIR}/rosetta-api.json"

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

# Update README badge with new version
echo "📝 Updating README badge..."
README_FILE="./README.md"
if [ -f "${README_FILE}" ]; then
    # Use sed to update the badge version (works on both macOS and Linux)
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        sed -i '' "s/Rosetta%20API%20Spec-v[0-9.]*-blue/Rosetta%20API%20Spec-v${VERSION}-blue/" "${README_FILE}"
    else
        # Linux
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
