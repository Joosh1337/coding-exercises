#!/bin/sh
PIDS=$(lsof -ti :5141 -c dotnet)
if [ -z "$PIDS" ]; then
  echo "No API process on :5141"
  exit 0
fi

echo "Found PIDs: $(echo $PIDS | tr '\n' ' ')"
echo "$PIDS" | xargs kill -INT 2>/dev/null
echo "API process killed"
