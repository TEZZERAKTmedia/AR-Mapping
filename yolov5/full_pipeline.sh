#!/bin/bash

# Step 1: Train and clean
echo "🔧 Starting training and cleanup..."
./train.sh
if [ $? -ne 0 ]; then
  echo "❌ Training failed. Aborting pipeline."
  exit 1
fi

# Step 2: Convert to ONNX and move
echo "📦 Starting ONNX export and move..."
./convert_and_export.sh
if [ $? -ne 0 ]; then
  echo "❌ Export failed. Aborting pipeline."
  exit 1
fi

echo "✅ Full pipeline completed successfully."
