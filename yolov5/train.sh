#!/bin/bash

# Enable strict error handling
set -euo pipefail

# === 🧹 CLEANUP ===
echo "🧹 Cleaning previous training artifacts..."

# Cleanup runs/doors only
if [ -d "runs/doors" ]; then
  echo "🔻 Removing previous runs/doors directory..."
  rm -rf "runs/doors"
fi

# Remove previous ONNX exports if they exist
if [ -f "runs/doors/weights/best.onnx" ]; then
  rm -f "runs/doors/weights/best.onnx"
fi

if [ -f "../SAWYER_AR/3D_UI/Assets/Models/Yolo/best.onnx" ]; then
  echo "🔻 Removing previously exported ONNX model..."
  rm -f "../SAWYER_AR/3D_UI/Assets/Models/Yolo/best.onnx"
fi

# === 🚀 TRAIN ===
echo "🚀 Starting training..."
python train.py \
  --img 320 \
  --batch 16 \
  --epochs 20 \
  --data data/doors.yaml \
  --weights yolov5n.pt \
  --name doors &

TRAIN_PID=$!
echo "⏳ Waiting for training (PID $TRAIN_PID) to finish..."
wait $TRAIN_PID

# === ✅ POST-TRAINING CHECK ===
if [ ! -f runs/doors/weights/best.pt ]; then
  echo "❌ Training completed but best.pt not found!"
  exit 1
fi

# === 📦 EXPORT ===
echo "📦 Exporting model to ONNX format..."
python export.py \
  --weights runs/doors/weights/best.pt \
  --include onnx \
  --opset 11

# === 📁 COPY TO UNITY ===
echo "📁 Moving ONNX model to Unity Assets..."
mv runs/doors/weights/best.onnx ../SAWYER_AR/3D_UI/Assets/Models/Yolo/best.onnx

echo "✅ All steps complete!"
