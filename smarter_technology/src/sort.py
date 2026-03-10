def sort(width, height, length, mass) -> str:
    oversize_dim = width >= 150 or height >= 150 or length >= 150
    oversize_volume = width * height * length >= 1_000_000
    is_bulky = oversize_dim or oversize_volume
    is_heavy = mass >= 20

    if is_heavy and is_bulky:
        return "REJECTED"
    if is_heavy or is_bulky:
        return "SPECIAL"
    return "STANDARD"