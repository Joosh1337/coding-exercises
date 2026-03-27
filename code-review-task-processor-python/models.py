# /models.py
from dataclasses import dataclass, field
from typing import Protocol, Dict, Any, Optional, Set


# Copilot CR: Add input validation
@dataclass
class Email:
    id: int
    subject: str
    body: str
    # CR: Consider making this an enum to avoid misspelled types
    # Copilot CR: Add Literal Types for now, enums long-term
    status: str
    tags: Set[str] = field(default_factory=set)


# Copilot CR: Add input validation
@dataclass
class Task:
    id: str
    # CR: Consider making this an enum to avoid misspelled types
    # Copilot CR: Add Literal Types for now, enums long-term
    type: str
    payload: Dict[str, Any]


# Copilot CR: Protocols lack error documentation: This doesn't specify what exceptions
#     should be raised or under what conditions
class Database(Protocol):
    """Interface for database operations."""

    def get_email_by_id(self, email_id: int) -> Optional[Email]: ...

    def update_email(self, email: Email) -> None: ...

    def execute_raw_query(self, query: str, params: Optional[tuple] = None) -> None: ...


# Copilot CR: Protocols lack error documentation: This doesn't specify what exceptions
#     should be raised or under what conditions
class EmailAPI(Protocol):
    """Interface for an external email sending service."""

    def send_alert(self, recipient: str, subject: str, body: str) -> None: ...


# Copilot CR: Protocols lack error documentation: This doesn't specify what exceptions
#     should be raised or under what conditions
class TaskQueue(Protocol):
    """Interface for a task queue."""

    def get_task(self) -> Optional[Task]: ...

    def complete_task(self, task: Task) -> None: ...
