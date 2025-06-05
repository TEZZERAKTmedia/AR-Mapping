#!/bin/bash

UNITY_PATH="/Applications/Unity/Hub/Editor/6000.1.4f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH=$(pwd)

echo "📁 Creating folders..."
mkdir -p "$PROJECT_PATH/Assets/TempImport"
mkdir -p "$PROJECT_PATH/Assets/Prefabs"

echo "🧹 Cleaning TempImport..."
rm -f "$PROJECT_PATH/Assets/TempImport/"*

echo "🚀 Launching Unity in batch mode..."
"$UNITY_PATH" \
  -projectPath "$PROJECT_PATH" \
  -executeMethod FBXBatchProcessor.ProcessFBX \
  -quit -batchmode -nographics

echo "✅ Done! Check Assets/Prefabs/"

