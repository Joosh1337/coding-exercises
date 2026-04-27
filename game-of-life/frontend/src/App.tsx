import { BrowserRouter, Route, Routes } from "react-router-dom";
import { BoardListPage } from "./pages/BoardListPage";
import { BoardSimulationPage } from "./pages/BoardSimulationPage";
import { CreateBoardPage } from "./pages/CreateBoardPage";

export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<BoardListPage />} />
        <Route path="/boards/new" element={<CreateBoardPage />} />
        <Route path="/boards/:id" element={<BoardSimulationPage />} />
      </Routes>
    </BrowserRouter>
  );
}
