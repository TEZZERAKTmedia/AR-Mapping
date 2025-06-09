#!/bin/bash

# Move into the yolov5 directory where export.py lives
cd "$(dirname "$0")" || exit 1

echo "🚀 Exporting best.pt to ONNX..."

python export.py \
  --weights runs/train/doors_yolov5s/weights/best.pt \
  --imgsz 320 \
  --include onnx \
  --dynamic \
  --optimize \
  --simplify

echo "📦 Moving best.onnx to Unity project..."

# Adjust the path for your Unity project structure
mv runs/train/doors_yolov5s/weights/best.onnx "../My project/Assets/Models/YOLO/" || {
  echo "❌ Failed to move .onnx file — check path or model export."
  exit 1
}

echo "✅ Done! You can now assign it in Unity as an NNModel."
