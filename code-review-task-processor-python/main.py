# /main.py
import time
import logging
from models import Task, Database, EmailAPI, TaskQueue

logging.basicConfig(level=logging.INFO)

class TaskWorker:
    def __init__(self, queue: TaskQueue, database: Database, email_api: EmailAPI):
        self.queue = queue
        # CR: :nit: Consider using more descriptive variable names for the database and email_api attributes
        self.d = database
        self.e = email_api

    def run(self):
        """Continuously fetches and processes tasks from the queue."""
        print("Worker starting...")
        while True:
            try:
                task = self.queue.get_task()
                if task:
                    print(f"Processing task {task.id} of type {task.type}")
                    if self._process_task(task):
                        self.queue.complete_task(task)
                    else:
                        # CR: BUG! This doesn't get retried - it disappears from the queue entirely
                        #     Even if it was retried...should it be retried? It seems like anything
                        #     that fails to process will never process, as they are invalid
                        #
                        #     NOTE: Consider stripping out the invalid conditions into a new function
                        #     to determine if the task is valid before trying to process it. It would
                        #     separate validation responsibilities from the task processing and eliminate
                        #     the copy/paste of `return True` in self._process_task
                        print(f"Failed to process task {task.id}. It will be retried.")
                else:
                    time.sleep(1)
            except Exception as e:
                # Copilot CR: If something goes wrong with external services like email or database API,
                #     the task will be dropped entirely. These errors need to actually be handled
                logging.error(f"An unexpected error occurred while processing task {task.id if task else 'unknown'}: {e}", exc_info=True)

    # Copilot CR: We should rename t and dat to more descriptive names
    def _process_task(self, t: Task) -> bool:
        """Processes a single task. Returns True on success, False on failure."""
        dat = t.payload
        email_id = dat.get("email_id")

        if not email_id:
            return False  # Invalid payload

        email = self.d.get_email_by_id(email_id)
        if not email:
            return False  # Email not found

        # Copilot CR: Should switch to decorator pattern or task handler registry to make it more extensible
        if t.type == "ANALYZE_CONTENT":
            # CR: :nit: store email.body.lower() in a variable instead of repeatedly calling it
            #     AND/OR make this another match-case statement OR make a tag_keywords dict and loop through it
            if "unsubscribe" in email.body.lower():
                email.tags.add("PROMOTIONAL")
            if "invoice" in email.body.lower() or "payment" in email.body.lower():
                email.tags.add("FINANCIAL")
            if "gift card" in email.body.lower() or "lottery" in email.body.lower():
                email.tags.add("POTENTIAL_SPAM")
            # CR: We should only update if we make changes
            self.d.update_email(email)
            print(f"Analyzed email {email.id}, added tags: {email.tags}")
            return True

        elif t.type == "MARK_AS_SPAM":
            email.status = "SPAM"
            self.d.update_email(email)
            print(f"Email {email.id} marked as SPAM.")
            return True

        elif t.type == "FORWARD_TO_SUPPORT":
            # CR: Does this require a different email.status? Unclear what the different statuses are
            self.e.send_alert(
                recipient="support@example.com",
                subject=f"Forwarded Email: {email.subject}",
                body=email.body,
            )
            # CR: :nit: Should we add a print statement here to be consistent?
            return True

        elif t.type == "LOG_USER_ACTIVITY":
            user_id = dat.get("user_id")
            if not user_id or not isinstance(user_id, str):
                print(f"Invalid or empty user_id format detected")
                return False
            query = "INSERT INTO activity_logs (user_id, action) VALUES (?, ?)"
            self.d.execute_raw_query(query, (user_id, "processed_email"))
            print(f"Logged activity for user {user_id}.")
            return True

        else:
            print(f"Unknown task type: {t.type}")
            return False
