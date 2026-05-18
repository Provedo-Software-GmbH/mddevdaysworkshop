# Testing Agent

## Description

Creates and runs unit tests for each new feature in both backend (.NET) and frontend (React/TypeScript).

## Instructions

### General Principles

- Write **meaningful tests**, not tests for coverage metrics
- Each public method/endpoint should have tests for:
  - **Happy path** — expected input produces expected output
  - **Invalid input / edge cases** — boundary values, null, empty strings, negative numbers
  - **Error handling** — exceptions, not-found scenarios, conflict states
- **Run tests after creating them** and fix any failures before marking as done
- Aim for tests that catch real bugs and document expected behavior

### Test Naming Convention

Use the pattern: `MethodName_Scenario_ExpectedBehavior`

Examples:
- `CreateEvent_WithValidInput_ReturnsCreatedEvent`
- `CreateEvent_WithEmptyTitle_ThrowsArgumentException`
- `GetById_WithNonExistentId_ReturnsNull`
- `PublishEvent_WhenAlreadyPublished_ThrowsInvalidOperationException`

### Backend Testing (.NET)

#### Frameworks & Libraries

- **xUnit** — test framework
- **FluentAssertions** — assertion library (use `.Should()` syntax)
- **NSubstitute** — mocking library (use `Substitute.For<T>()`)

#### Project Structure

- Tests live in `src/DevConfTicketing.Tests/`
- Mirror the source project structure:
  - `src/DevConfTicketing.Application/Events/CreateEventHandler.cs` → `src/DevConfTicketing.Tests/Events/CreateEventHandlerTests.cs`
  - `src/DevConfTicketing.Application/Orders/CreateOrderHandler.cs` → `src/DevConfTicketing.Tests/Orders/CreateOrderHandlerTests.cs`

#### Writing Backend Tests

1. **Create a test class** named `{ClassUnderTest}Tests`
2. **Mock dependencies** using NSubstitute:
   ```csharp
   var repository = Substitute.For<IEventRepository>();
   var telemetry = Substitute.For<ITelemetryService>();
   ```
3. **Arrange-Act-Assert** pattern for each test method
4. **Use FluentAssertions** for all assertions:
   ```csharp
   result.Should().NotBeNull();
   result.Title.Should().Be("Expected Title");
   await act.Should().ThrowAsync<ArgumentException>();
   ```
5. **Test telemetry calls** where relevant:
   ```csharp
   telemetry.Received(1).IncrementCounter("event.created");
   ```

#### Running Backend Tests

```bash
dotnet test src/DevConfTicketing.slnx
```

### Frontend Testing (React/TypeScript)

#### Frameworks & Libraries

- **Vitest** — test runner (Jest-compatible API)
- **React Testing Library** — component testing
- **@testing-library/user-event** — simulating user interactions

#### Test File Location

- Tests are **colocated** with source files
- Component `event-card.tsx` → test file `event-card.test.tsx` in the same directory
- Hook `use-events.ts` → test file `use-events.test.ts` in the same directory
- Type-only files do not need tests

#### Writing Frontend Tests

1. **Render components** using `render()` from React Testing Library
2. **Query elements** using accessible queries: `getByRole`, `getByLabelText`, `getByText`
3. **Simulate interactions** using `userEvent` from `@testing-library/user-event`
4. **Assert on DOM state** using Vitest matchers and Testing Library queries
5. **Mock API calls** by mocking the API client or using MSW (Mock Service Worker)
6. **Test loading states**, error states, and empty states
7. **Test accessibility** — ensure key elements have proper roles and labels

#### Running Frontend Tests

```bash
cd frontend && bun test
```

### What to Test

#### Backend

- Handler business logic (validation, state transitions, calculations)
- Repository interaction verification (correct methods called with correct args)
- Error handling paths (exceptions mapped to correct responses)
- Telemetry emission (counters, spans, exception tracking)

#### Frontend

- Component rendering with different props
- User interaction flows (click, type, submit)
- Loading and error states
- Form validation
- Conditional rendering logic

### What NOT to Test

- Don't test framework internals (ASP.NET routing, Cosmos DB SDK)
- Don't test trivial getters/setters
- Don't test auto-generated code
- Don't write tests that just duplicate the implementation logic
