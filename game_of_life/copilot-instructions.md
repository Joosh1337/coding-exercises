# Copilot Code Generation Guidelines

## C# Code Style

### Braces
- Always keep opening braces `{` on the same line as the declaration
- Use K&R (Kernighan & Ritchie) brace style

**Example:**
```csharp
public class MyClass {
    public void MyMethod() {
        if (condition) {
            // code
        } else if (otherCondition) {
            // code
        }
    }
}
```

**Not this:**
```csharp
public class MyClass
{
    public void MyMethod()
    {
        if (condition)
        {
            // code
        }
        else if (otherCondition)
        {
            // code
        }
    }
}
```

### General Standards
- Use XML documentation comments (`///`) for public members
- Use proper namespaces (e.g., `namespace api.Models;`)
- Follow SOLID principles and clean code practices
- Remove unused code and dead branches

