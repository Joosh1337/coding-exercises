import { BrowserRouter, Route, Routes } from "react-router-dom";
import { ErrorBoundary } from "./components/ErrorBoundary";
import { BoardListPage } from "./pages/BoardListPage";
import { BoardSimulationPage } from "./pages/BoardSimulationPage";
import { CreateBoardPage } from "./pages/CreateBoardPage";
import { EditBoardPage } from "./pages/EditBoardPage";

export function App() {
  return (
    <ErrorBoundary>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<BoardListPage />} />
          <Route path="/boards/new" element={<CreateBoardPage />} />
          <Route path="/boards/:id" element={<BoardSimulationPage />} />
          <Route path="/boards/:id/edit" element={<EditBoardPage />} />
        </Routes>
      </BrowserRouter>
    </ErrorBoundary>
  );
}
