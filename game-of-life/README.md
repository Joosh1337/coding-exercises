# Setup
To run anything in this repo, install the **.NET 8.0 SDK**:
1. [Download the SDK for your OS](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. Install the SDK
3. Verify the installation (look for a version starting with `8.`):
    ```shell
    dotnet --list-sdks
    ```

# Run the API with Swagger
1. Navigate to the API folder:
    ```
    cd coding-exercises/game-of-life/api
    ```
2. Start the development server (http://localhost:5141, Swagger at /swagger):
    ```
    dotnet watch
    ```
3. To kill the API process:
    ```
    ./kill.sh
    ```

# Run the tests
1. Navigate to the test project:
    ```
    cd coding-exercises/game-of-life/api.Tests
    ```
2. Run the test suite:
    ```
    dotnet test
    ```

# Run the Frontend
1. Navigate to the frontend folder:
    ```
    cd coding-exercises/game-of-life/frontend
    ```
2. Install dependencies (first time only):
    ```
    npm install
    ```
3. Start the dev server (http://localhost:5173):
    ```
    npm run dev
    ```
4. To kill the frontend process:
    ```
    npm run kill
    ```