# Autonomous-Drone-Unity
**A fully autonomous drone simulation (Unity)** that searches a city for a very small flag, filters noisy detections, and autonomously drops a payload.

[![Watch the demo video](https://img.youtube.com/vi/kd2UeGSbFoc/hqdefault.jpg)](https://www.youtube.com/watch?v=kd2UeGSbFoc)

---

## TL;DR
A Unity drone streams images to a server running a YOLO model (OpenVINO optimized). False positives are reduced with **Evidence Accumulation** and positional noise is smoothed with an **Exponential Moving Average (EMA)**. A small **Finite State Machine (FSM)** controls safe decision-making and the drop behavior.

---

## Features
- Real-time small-object detection (YOLO + OpenVINO)  
- Evidence accumulation to reduce false positives  
- EMA for stable position estimates from noisy detections  
- FSM-based decision logic for robust autonomous actions  
- Unity client + server inference pipeline (easy to extend)

---

## How it works (short)
1. Unity drone streams camera frames to a server.  
2. Server runs a YOLO model (OpenVINO-optimized) and returns detections.  
3. Evidence accumulation integrates multiple detection frames to confirm targets.  
4. EMA smooths the detected position for safe approach.  
5. FSM decides behaviors: search → approach → drop → return.

---
