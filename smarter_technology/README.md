### Objective

Imagine you work in Smarter Technology’s robotic automation factory, and your objective is to write a function for one of its robotic arms that will dispatch the packages to the correct stack according to their volume and mass.

### Rules

Sort the packages using the following criteria:

- A package is **bulky** if its volume (Width x Height x Length) is greater than or equal to 1,000,000 cm³ or when one of its dimensions is greater or equal to 150 cm.
- A package is **heavy** when its mass is greater or equal to 20 kg.

You must dispatch the packages in the following stacks:

- **STANDARD**: standard packages (those that are not bulky or heavy) can be handled normally.
- **SPECIAL**: packages that are either heavy or bulky can't be handled automatically.
- **REJECTED**: packages that are **both** heavy and bulky are rejected.

### Implementation

Implement the function **`sort(width, height, length, mass)`** (units are centimeters for the dimensions and kilogram for the mass). This function must return a string: the name of the stack where the package should go.

### Instructions
To run the test file:
1. Open the project in VS Code.
2. Ensure Python is installed on your system. You can download it from [python.org](https://www.python.org/) or use a package manager like `brew` on macOS.
3. Install `pytest` by running `pip install pytest` in your terminal.
4. Configure VS Code to use `pytest`:
   - Open the Command Palette (`Cmd+Shift+P`) and select `Python: Configure Tests`.
   - Choose `pytest` as the testing framework.
   - Select `smarter_technology` as the directory containing the requests
5. Right-click the `test_sort.py` file in VS Code and select "Run Tests".