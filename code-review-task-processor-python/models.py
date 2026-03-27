# /models.py
from dataclasses import dataclass, field
from typing import Protocol, Dict, Any, Optional, Set


@dataclass
class Email:
    id: int
    subject: str
    body: str
    status: str
    tags: Set[str] = field(default_factory=set)


@dataclass
class Task:
    id: str
    type: str
    payload: Dict[str, Any]


class Database(Protocol):
    """Interface for database operations."""

    def get_email_by_id(self, email_id: int) -> Optional[Email]: ...

    def update_email(self, email: Email) -> None: ...

    def execute_raw_query(self, query: str, params: Optional[tuple] = None) -> None: ...


class EmailAPI(Protocol):
    """Interface for an external email sending service."""

    def send_alert(self, recipient: str, subject: str, body: str) -> None: ...


class TaskQueue(Protocol):
    """Interface for a task queue."""

    def get_task(self) -> Optional[Task]: ...

    def complete_task(self, task: Task) -> None: ...
