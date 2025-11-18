from fastapi import FastAPI, HTTPException, File, UploadFile, Query
from services.ai_service import load_model
import uvicorn
from api.routes import router


app = FastAPI()

load_model()
app.include_router(router)

if __name__ == "__main__":
    uvicorn.run("main:app", host="127.0.0.1", port=8000, reload=True)