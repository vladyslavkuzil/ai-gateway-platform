from fastapi import APIRouter
from src.models import SetModeRequest
from src.state import state

router = APIRouter(prefix="/admin")
@router.put("/mode")
def set_mode(body: SetModeRequest):
    state.mode = body.mode
    state.intermittent_fail_remaining = body.intermittent_failures
    return {"mode": state.mode}
    
@router.post("/reset")
def reset():
    state.reset()
    return {"status": "reset", "call_count": state.call_count}