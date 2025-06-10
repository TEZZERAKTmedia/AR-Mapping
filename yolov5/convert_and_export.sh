#!/bin/bash

# Paths
BEST_PT="runs/train/doors/weights/best.pt"
ONNX_SOURCE="runs/train/doors/weights/best.onnx"
DEST_DIR="../SAWYER_AR/Assets/Models/YOLO"

# Check if best.pt exists
if [ ! -f "$BEST_PT" ]; then
  echo "❌ Error: $BEST_PT not found."
  exit 1
fi

echo "📦 Exporting $BEST_PT to ONNX (opset 11)..."
python export.py \
  --weights "$BEST_PT" \
  --include onnx \
  --opset 11

# Confirm ONNX export succeeded
if [ ! -f "$ONNX_SOURCE" ]; then
  echo "❌ Export failed. $ONNX_SOURCE not found."
  exit 1
fi

echo "🧹 Cleaning up old ONNX file in Unity folder (if it exists)..."
rm -f "$DEST_DIR/best.onnx"

echo "📂 Moving ONNX model to Unity folder..."
mv "$ONNX_SOURCE" "$DEST_DIR/best.onnx"

echo "✅ Export and move complete!"
