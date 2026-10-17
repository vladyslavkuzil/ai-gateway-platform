from dataclasses import dataclass, field 
import os 

@dataclass
class MockState:
    mode: str = field(default_factory=lambda: os.getenv("MOCK_MODE", "healthy"))
    call_count: int = 0
    intermittent_fail_remaining: int = 0

    def reset(self) -> None:
        self.call_count = 0
        self.intermittent_fail_remaining = 0


state = MockState()