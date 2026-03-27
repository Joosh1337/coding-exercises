# /main.py
import time
from models import Task, Database, EmailAPI, TaskQueue


class TaskWorker:
    def __init__(self, queue: TaskQueue, database: Database, email_api: EmailAPI):
        self.queue = queue
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
                print(f"An unexpected error occurred: {e}. Worker will continue.")

    def _process_task(self, t: Task) -> bool:
        """Processes a single task. Returns True on success, False on failure."""
        dat = t.payload
        email_id = dat.get("email_id")

        if not email_id:
            return False  # Invalid payload

        email = self.d.get_email_by_id(email_id)
        if not email:
            return False  # Email not found

        # CR: Should switch to match-case statement
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
            # CR: :nit: Consider making a different print statement if no tags were found for clarity
            print(f"Analyzed email {email.id}, added tags: {email.tags}")
            return True

        elif t.type == "MARK_AS_SPAM":
            email.status = "SPAM"
            # CR: BUG! The email never gets updated after marking it as spam
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
            # CR: Potential security concern: user_id can be "any" type, so we should parse it to avoid SQL injection
            user_id = dat["user_id"]
            query = f"INSERT INTO activity_logs (user_id, action) VALUES ({user_id}, 'processed_email')"
            self.d.execute_raw_query(query)
            # CR: :nit: Should we add a print statement here to be consistent?
            return True

        else:
            print(f"Unknown task type: {t.type}")
            return False
