import os
import subprocess
import shutil
import glob
import argparse
import re

# Script is in /scripts/, so root is one level up
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(SCRIPT_DIR)

PROJECT_NAME = "PicoEntityStore"
PROJECT_PATH = os.path.join(ROOT_DIR, PROJECT_NAME, f"{PROJECT_NAME}.csproj")
RELEASE_DIR = os.path.join(ROOT_DIR, PROJECT_NAME, "bin", "Release")
DIST_DIR = os.path.join(ROOT_DIR, "dist")

def run_command(command, description):
    print(f"--- {description} ---")
    # Run from the root directory
    result = subprocess.run(command, shell=True, cwd=ROOT_DIR)
    if result.returncode != 0:
        print(f"Error: {description} failed.")
        exit(1)

def increment_version(current_version, part):
    parts = current_version.split('.')
    while len(parts) < 3:
        parts.append('0')
    
    major, minor, patch = map(int, parts[:3])
    
    if part == 'major':
        major += 1
        minor = 0
        patch = 0
    elif part == 'minor':
        minor += 1
        patch = 0
    elif part == 'patch':
        patch += 1
    
    return f"{major}.{minor}.{patch}"

def update_csproj_version(version_part):
    with open(PROJECT_PATH, 'r', encoding='utf-8') as f:
        content = f.read()
    
    version_match = re.search(r'<Version>(.*?)</Version>', content)
    if not version_match:
        print("Error: Could not find <Version> tag in .csproj")
        exit(1)
    
    current_version = version_match.group(1)
    new_version = increment_version(current_version, version_part)
    
    print(f"Incrementing version: {current_version} -> {new_version} ({version_part})")
    
    new_content = re.sub(r'<Version>.*?</Version>', f'<Version>{new_version}</Version>', content)
    
    with open(PROJECT_PATH, 'w', encoding='utf-8') as f:
        f.write(new_content)
    
    return new_version

def main():
    parser = argparse.ArgumentParser(description="Prepare PicoEntityStore NuGet package.")
    parser.add_argument("--version", choices=['major', 'minor', 'patch'], required=True,
                        help="The part of the version to increment (major, minor, or patch).")
    
    args = parser.parse_args()

    # 1. Update version in .csproj
    new_version = update_csproj_version(args.version)

    # 2. Clean up old builds and dist folder
    dirs_to_clean = [RELEASE_DIR, DIST_DIR]
    for d in dirs_to_clean:
        if os.path.exists(d):
            print(f"Cleaning {d}...")
            shutil.rmtree(d)
    
    os.makedirs(DIST_DIR, exist_ok=True)

    # 3. Dotnet restore and clean
    run_command("dotnet clean", "Cleaning project")

    # 4. Dotnet build in Release mode
    run_command(f"dotnet build {PROJECT_PATH} -c Release", "Building project")

    # 5. Dotnet pack in Release mode
    run_command(f"dotnet pack {PROJECT_PATH} -c Release --no-build", "Generating NuGet package")

    # 6. Copy the generated .nupkg to dist/
    nupkg_files = glob.glob(os.path.join(RELEASE_DIR, "*.nupkg"))
    if not nupkg_files:
        print("No .nupkg file found in Release directory.")
        exit(1)

    print("\nCopying package(s) to dist/ folder...")
    for nupkg in nupkg_files:
        shutil.copy(nupkg, DIST_DIR)

    # 7. Final message
    dist_nupkgs = glob.glob(os.path.join(DIST_DIR, "*.nupkg"))
    print(f"\nPackage(s) version {new_version} ready for upload in dist/:")
    for nupkg in dist_nupkgs:
        print(f"  - {os.path.abspath(nupkg)}")

if __name__ == "__main__":
    main()
