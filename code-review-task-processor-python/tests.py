# /tests/tests.py
import unittest
from collections import deque
from typing import Optional

from main import TaskWorker
from models import Task, Email


# Mock implementations for interfaces
class MockDatabase:
    def __init__(self):
        self._emails = {
            101: Email(
                id=101,
                subject="Your Invoice",
                body="An invoice is attached.",
                status="INBOX",
                tags=set(),
            ),
            102: Email(
                id=102,
                subject="Win a prize",
                body="You won the lottery!",
                status="INBOX",
                tags=set(),
            ),
        }
        self.last_query = None
        self.last_params = None

    def get_email_by_id(self, email_id: int) -> Optional[Email]:
        return self._emails.get(email_id)

    def update_email(self, email: Email) -> None:
        self._emails[email.id] = email

    def execute_raw_query(self, query: str, params: Optional[tuple] = None) -> None:
        self.last_query = query
        self.last_params = params


class MockEmailAPI:
    def __init__(self):
        self.sent_alerts = []

    def send_alert(self, recipient: str, subject: str, body: str) -> None:
        self.sent_alerts.append({"to": recipient, "subject": subject, "body": body})


class MockTaskQueue:
    def __init__(self):
        self.tasks = deque()
        self.completed_tasks = []

    def add_task(self, task: Task):
        self.tasks.append(task)

    # Copilot CR: BUG! Potential race condition issue if multiple TaskWorkers run concurrently
    #     Recommend a lock, queue.Queue, or wrap .popleft() in a try/catch
    def get_task(self) -> Optional[Task]:
        return self.tasks.popleft() if self.tasks else None

    def complete_task(self, task: Task) -> None:
        self.completed_tasks.append(task)


class TestTaskWorker(unittest.TestCase):
    def setUp(self):
        self.db = MockDatabase()
        self.api = MockEmailAPI()
        self.queue = MockTaskQueue()
        self.worker = TaskWorker(self.queue, self.db, self.api)

    def test_analyze_content_adds_tags(self):
        task = Task(id="t1", type="ANALYZE_CONTENT", payload={"email_id": 101})

        result = self.worker._process_task(task)

        self.assertTrue(result)
        updated_email = self.db.get_email_by_id(101)
        self.assertIn("FINANCIAL", updated_email.tags)

    def test_mark_as_spam(self):
        task = Task(id="t2", type="MARK_AS_SPAM", payload={"email_id": 102})

        result = self.worker._process_task(task)

        self.assertTrue(result)
        updated_email = self.db.get_email_by_id(102)
        self.assertEqual(updated_email.status, "SPAM")

    def test_forward_to_support(self):
        task = Task(id="t3", type="FORWARD_TO_SUPPORT", payload={"email_id": 101})

        result = self.worker._process_task(task)

        self.assertTrue(result)
        self.assertEqual(len(self.api.sent_alerts), 1)
        self.assertEqual(self.api.sent_alerts[0]["to"], "support@example.com")

    def test_log_user_activity(self):
        task = Task(
            id="t4", type="LOG_USER_ACTIVITY", payload={"email_id": 101, "user_id": "user_hash_123"}
        )

        result = self.worker._process_task(task)

        self.assertTrue(result)
        self.assertEqual(self.db.last_query, "INSERT INTO activity_logs (user_id, action) VALUES (?, ?)")
        self.assertEqual(self.db.last_params, ("user_hash_123", "processed_email"))

    def test_process_task_missing_email_id(self):
        """Task with missing email_id should return False."""
        task = Task(id="t5", type="ANALYZE_CONTENT", payload={})

        result = self.worker._process_task(task)

        self.assertFalse(result)

    def test_process_task_email_not_found(self):
        """Task with non-existent email_id should return False."""
        task = Task(id="t6", type="ANALYZE_CONTENT", payload={"email_id": 999})

        result = self.worker._process_task(task)

        self.assertFalse(result)

    def test_process_task_unknown_type(self):
        """Task with unknown type should return False."""
        task = Task(id="t7", type="UNKNOWN_TYPE", payload={"email_id": 101})

        result = self.worker._process_task(task)

        self.assertFalse(result)

    def test_log_user_activity_missing_user_id(self):
        """LOG_USER_ACTIVITY without user_id should return False."""
        task = Task(id="t8", type="LOG_USER_ACTIVITY", payload={"email_id": 101})

        result = self.worker._process_task(task)

        self.assertFalse(result)

    def test_log_user_activity_non_string_user_id(self):
        """LOG_USER_ACTIVITY with non-string user_id should return False."""
        task = Task(id="t9", type="LOG_USER_ACTIVITY", payload={"email_id": 101, "user_id": 12345})

        result = self.worker._process_task(task)

        self.assertFalse(result)

    def test_analyze_content_with_multiple_keywords(self):
        """Email with multiple keywords should receive all relevant tags."""
        email_103 = Email(
            id=103,
            subject="Promotional Invoice",
            body="Unsubscribe from our promotions. Here's your invoice for payment.",
            status="INBOX",
            tags=set(),
        )
        self.db._emails[103] = email_103
        task = Task(id="t12", type="ANALYZE_CONTENT", payload={"email_id": 103})

        result = self.worker._process_task(task)

        self.assertTrue(result)
        updated_email = self.db.get_email_by_id(103)
        self.assertIn("PROMOTIONAL", updated_email.tags)
        self.assertIn("FINANCIAL", updated_email.tags)

    def test_forward_to_support_no_email_update(self):
        """FORWARD_TO_SUPPORT should not update the email itself."""
        original_status = self.db.get_email_by_id(101).status
        task = Task(id="t13", type="FORWARD_TO_SUPPORT", payload={"email_id": 101})

        result = self.worker._process_task(task)

        self.assertTrue(result)
        # Status should remain unchanged
        self.assertEqual(self.db.get_email_by_id(101).status, original_status)