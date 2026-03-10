def sort(width, height, length, mass) -> str:
    is_at_least_150 = width >= 150 or height >= 150 or length >= 150
    is_at_least_1M_cm_cubed = width * height * length >= 1000000
    is_bulky = is_at_least_150 or is_at_least_1M_cm_cubed
    is_heavy = mass >= 20

    if is_heavy and is_bulky:
        return "REJECTED"
    
    if is_heavy or is_bulky:
        return "SPECIAL"

    return "STANDARD"