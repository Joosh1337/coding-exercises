from src.sort import sort

def test_sort_standard():
    assert sort(1,1,1,1) == "STANDARD"

def test_sort_bulky_oversize_dim():
    assert sort(1,150,1,1) == "SPECIAL"

def test_sort_bulky_oversize_volume():
    assert sort(100,100,100,1) == "SPECIAL"

def test_sort_heavy():
    assert sort(1,1,1,25) == "SPECIAL"

def test_sort_heavy_and_bulky_oversize_dim():
    assert sort(1,150,1,25) == "REJECTED"

def test_sort_heavy_and_bulky_oversize_volume():
    assert sort(100,100,100,25) == "REJECTED"

def test_sort_heavy_and_bulky_oversize_dim_and_oversize_volume():
    assert sort(100,150,100,25) == "REJECTED"