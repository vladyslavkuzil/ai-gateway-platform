from fastapi import FastAPI
from src.routes import chat, admin
app = FastAPI(title="Mock Provider", version="1.0.0")
app.include_router(chat.router)
app.include_router(admin.router)
