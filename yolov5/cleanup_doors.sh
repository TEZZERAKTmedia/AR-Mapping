#!/bin/bash

# Expected relative path from this script
RELATIVE_TARGET_PATH="runs/train/doors2"

# Resolve to absolute path safely
TARGET_DIR="$(cd "$(dirname "$0")" && cd "$RELATIVE_TARGET_PATH" 2>/dev/null && pwd)"

# Confirm and delete
if [ -n "$TARGET_DIR" ] && [ -d "$TARGET_DIR" ]; then
  echo "🧹 Cleaning up: $TARGET_DIR"
  rm -rf "$TARGET_DIR"
  echo "✅ Removed: $TARGET_DIR"
else
  echo "⚠️  Relative path 'runs/train/doors' not found from script location. No action taken."
fi
