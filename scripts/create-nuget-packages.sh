#!/usr/bin/env bash

# exit immediately if a command exits with a non-zero status
set -e

# construct directory paths
SCRIPT_DIRECTORY="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIRECTORY="$(dirname "$SCRIPT_DIRECTORY")"
SOURCE_DIRECTORY="$ROOT_DIRECTORY/source"
OUTPUT_DIRECTORY="$ROOT_DIRECTORY/nupkgs"

echo -e "\033[36mPacking Retry.Reqnroll Packages ...\033[0m"
echo ""

# purge output directory
rm -rf "$OUTPUT_DIRECTORY"

# create output directory
mkdir -p "$OUTPUT_DIRECTORY"

# pack all generator projects
projects=(
    "Reqnroll.Retry.MSTest"
    "Reqnroll.Retry.NUnit"
    "Reqnroll.Retry.xUnit"
    "Reqnroll.Retry.TUnit"
)

for project in "${projects[@]}"; do
    echo -e "\033[33mPacking $project ...\033[0m"

    if ! dotnet pack "$SOURCE_DIRECTORY/$project/$project.csproj" \
        --configuration Release \
        --output "$OUTPUT_DIRECTORY"; then
        echo -e "\033[31mFailed To Pack $project\033[0m"

        exit 1
    fi
done

echo -e "\n\033[32mPackages Created In $OUTPUT_DIRECTORY:\033[0m"

ls -1 "$OUTPUT_DIRECTORY"/*.nupkg
