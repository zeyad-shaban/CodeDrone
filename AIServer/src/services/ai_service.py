import torch
import cv2
from ultralytics import YOLO  # type: ignore
from PIL import Image
import matplotlib.pyplot as plt
import time
import os, shutil

model = None
DEBUG = True

folder = "./detected"
for filename in os.listdir(folder):
    file_path = os.path.join(folder, filename)
    try:
        if os.path.isfile(file_path) or os.path.islink(file_path):
            os.unlink(file_path)
        elif os.path.isdir(file_path):
            shutil.rmtree(file_path)
    except Exception as e:
        print("Failed to delete %s. Reason: %s" % (file_path, e))


def load_model():
    global model

    model = YOLO("./ai_models/best_openvino_model")
    model.predict(torch.zeros(1, 3, 608, 608), verbose=False)  # JIT Loader


def detect_obj(img: Image.Image, min_conf: float):
    if model is None:
        print("Model not loaded, can't run predictions")
        return [], []

    result = model.predict(img, verbose=False, conf=min_conf)[0]

    if result is None or result.boxes is None:
        return {
            "boxes": [],
            "conf": [],
        }

    boxes = result.boxes.xyxy
    conf: torch.Tensor = result.boxes.conf  # type: ignore

    if DEBUG:
        annotated_img = result.plot()

        print("boxes: ")
        print(boxes)
        print("conf: ")
        print(conf)

        plt.subplot(221)
        plt.imshow(img)
        plt.title("Original Image")

        plt.subplot(222)
        plt.imshow(annotated_img)
        plt.title("Annotated image")

        # plt.show()
        print("saved")
        cv2.imwrite(f"./detected/{time.time()}.png", annotated_img)

    return boxes.flatten().tolist(), conf.tolist()
