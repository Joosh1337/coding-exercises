from src.sort import sort

def test_sort_standard():
    assert sort(1,1,1,1) == "STANDARD"

def test_sort_bulky_at_least_150():
    assert sort(1,150,1,1) == "SPECIAL"

def test_sort_bulky_at_least_1M_cm_cubed():
    assert sort(100,100,100,1) == "SPECIAL"

def test_sort_heavy():
    assert sort(1,1,1,25) == "SPECIAL"

def test_sort_heavy_and_bulky_at_least_150():
    assert sort(1,150,1,25) == "REJECTED"

def test_sort_heavy_and_bulky_at_least_1M_cm_cubed():
    assert sort(100,100,100,25) == "REJECTED"

def test_sort_heavy_and_bulky_at_least_150_and_1M_cm_cubed():
    assert sort(100,150,100,25) == "REJECTED"