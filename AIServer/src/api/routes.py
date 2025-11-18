from fastapi import APIRouter, UploadFile, Query
from PIL import Image
import io
from schemas.models import DummyModel
from services.ai_service import detect_obj

router = APIRouter()


@router.get("/")
def root():
    return {
        "status": "ok",
        "service": "flag detection service",
    }

@router.post("/detect_flags")
async def detect_flags(img_file: UploadFile, min_conf: float = Query(0.2, ge=0, le=1)):
    print(f"called with conf={min_conf}")
    img = await img_file.read()
    img = Image.open(io.BytesIO(img)).convert("RGB")

    boxes, conf = detect_obj(img, min_conf)
    print(f"Returning prediction results, Conf: {conf}, Boxes: {boxes}")

    return {
        "boxes": boxes,
        "conf": conf,
    }
