#!/bin/bash
# Script to view Unity Debug.Log messages from Quest device
# Make sure your Quest is connected via USB and USB debugging is enabled

echo "=== Unity Debug Logs from Quest ==="
echo "Press Ctrl+C to stop"
echo ""

# Filter for Unity logs (com.unity3d.*) and your app logs
adb logcat | grep -E "(Unity|Debug|GraphManager|Velocity|Acceleration)"


