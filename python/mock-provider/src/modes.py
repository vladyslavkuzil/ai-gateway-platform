import uuid
from fastapi import HTTPException
from src.models import ChatCompletionRequest
from src.state import MockState

def estimate_tokens(body: ChatCompletionRequest) -> tuple[int, int]:
    prompt_chars = sum(len(m.content) for m in body.messages)
    prompt_tokens = max(1, prompt_chars // 4)
    completion_tokens = max(body.max_tokens, 16)
    return prompt_tokens, completion_tokens

def build_completion(body: ChatCompletionRequest) -> dict:
    prompt_tokens, completion_tokens = estimate_tokens(body)
    return {
        "id": f"mock-{uuid.uuid4()}",
        "object": "chat.completion",
        "model": body.model,
        "choices": [
            {
                "index": 0,
                "finish_reason": "stop",
                "message": {
                    "role": "assistant",
                    "content": f"mock reply for model={body.model}",
                },
            }
        ],
        "usage": {
            "prompt_tokens": prompt_tokens,
            "completion_tokens": completion_tokens,
            "total_tokens": prompt_tokens + completion_tokens,
        },
    }

def handle_chat(body: ChatCompletionRequest, mode: str, state: MockState) -> dict:
    state.call_count += 1
    if mode == "server_error":
        raise HTTPException(
            status_code=503,
            detail={
                "error": {
                    "code": "server_error",
                    "message": "mock definite non-start failure",
                    "generation_started": False,
                }
            },
        )
    if mode == "healthy":
        return build_completion(body)
    raise HTTPException(
        status_code=400,
        detail={"error": {"code": "unknown_mode", "message": f"unsupported mode: {mode}"}},
    )
