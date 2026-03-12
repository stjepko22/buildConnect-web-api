## Backend (.NET)

### Architecture
- Follow existing layering (Controller → Service → Repository).
- Keep business logic out of controllers.
- Do not bypass the service layer.
- Do not access DbContext directly from controllers.
- Reuse existing patterns before introducing new ones.
- Do not introduce new architectural patterns unless they are already used in the project.

### Naming Conventions
- Follow existing naming conventions consistently.
- Interfaces must use the `I` prefix. Example: `IOrderService`, `IOrderRepository`.
- Async methods must end with `Async`. Example: `GetOrdersAsync`, `CreateOrderAsync`.
- Use clear and descriptive names for classes, methods, variables, and DTOs.

### API Design
- Maintain existing route patterns and controller conventions.
- Preserve DTO usage and validation patterns.
- Do not expose EF entities directly in API responses.
- Use request/response DTOs where the existing codebase does so.
- Do not break backward compatibility.
- New fields must be optional unless explicitly required.
- Keep response shapes consistent with existing endpoints.

### Services
- Put business rules and orchestration logic in services.
- Keep services focused and aligned with existing service responsibilities.
- Do not move repository logic into controllers.
- Do not place persistence-specific logic in controllers.

### Repositories & Data Access
- Use existing repositories and EF patterns.
- Keep database access inside repositories unless the project already uses a different pattern.
- Avoid unnecessary database calls.
- Ensure queries are efficient.
- Use `AsNoTracking()` for read-only queries where appropriate and consistent with the project.
- Avoid loading unnecessary related data.
- Preserve existing transaction and unit-of-work patterns if present.

### Validation & Error Handling
- Follow existing validation patterns before adding new ones.
- Handle edge cases and null checks consistently with the codebase.
- Do not swallow exceptions.
- Use existing exception handling and error response patterns.
- Return appropriate HTTP status codes based on existing API conventions.

### Performance & Reliability
- Prevent redundant API calls and repeated database queries.
- Ensure async operations are properly awaited.
- Avoid blocking async code.
- Consider performance impact when adding loops, joins, projections, and mappings.
- Keep implementations simple, predictable, and production-safe.

### When Implementing Changes
1. Understand existing patterns before writing new code.
2. Modify only necessary files.
3. Keep changes minimal and consistent with the project.
4. Explain important decisions when they are not obvious.
5. Maintain naming consistency.
6. Prefer extending existing code over rewriting working code.
7. Check whether similar functionality already exists before adding new files or services.

### Avoid
- Bypassing service or repository layers.
- Large architectural changes.
- Renaming or restructuring files without need.
- Breaking existing API contracts.
- Returning database entities directly from controllers.
- Adding unnecessary abstractions, helpers, or base classes.
- Using unclear variable or method names.

### Output Expectations
When making changes:
- Provide clean and readable code.
- Maintain consistency with the codebase.
- Keep code minimal, safe, and production-ready.
- Suggest verification steps when appropriate.
- Use clear naming.

### DTO & Mapping Rules
- Prefer explicit mapping over hidden or unclear mapping logic unless a mapping library is already consistently used in the project.
- Keep DTOs separated by responsibility (request/response) when the project already follows that pattern.
- Do not add fields to DTOs unless they are required by the use case.

### Change Safety Rules
- Do not make speculative changes.
- Do not modify unrelated code.
- Do not create new folders, base classes, or abstractions unless necessary and consistent with the existing structure.
- If multiple implementation options exist, prefer the one that matches the current codebase.

ODGOVARAJ NA HRVATSKOM JEZIKU.