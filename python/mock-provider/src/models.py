from typing import Literal
from pydantic import BaseModel, Field


class ChatMessage(BaseModel):
    role: Literal["system", "user", "assistant"]
    content: str

class ChatCompletionRequest(BaseModel):
    model: str
    messages: list[ChatMessage] = Field(min_length=1)
    max_tokens: int = Field(gt=0)
    temperature: float | None = None
    stream: bool | None = False

class HealthResponse(BaseModel):
    status: str
    mode: str

class SetModeRequest(BaseModel):
    mode: str
    intermittent_failures: int = 0