import torch
import cv2
from ultralytics import YOLO  # type: ignore
from PIL import Image
import matplotlib.pyplot as plt

model = None
DEBUG = False


def load_model():
    global model

    model = YOLO("./ai_models/best_openvino_model")
    model.predict(torch.zeros(1, 3, 704, 704), verbose=False)  # JIT Loader


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

        plt.subplot(223)
        plt.imshow(result.orig_img)
        plt.title("Original image from YOLO")
        plt.show()

    return boxes.flatten().tolist(), conf.tolist()
