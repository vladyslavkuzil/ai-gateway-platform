from fastapi import APIRouter, Header, HTTPException
from src.models import ChatCompletionRequest, HealthResponse
from src.modes import handle_chat
from src.state import state

router = APIRouter()

@router.get("/health", response_model=HealthResponse)
def health() -> HealthResponse:
    return HealthResponse(status="ok", mode=state.mode)

@router.post("/v1/chat/completions")
def chat_completions(
    body: ChatCompletionRequest, 
   x_mock_mode: str | None = Header(default=None, alias="X-Mock-Mode"),
):
    if body.stream:
        raise HTTPException(status_code=400, detail="Streaming is not supported")
    
    mode = x_mock_mode or state.mode
    return handle_chat(body, mode, state)

