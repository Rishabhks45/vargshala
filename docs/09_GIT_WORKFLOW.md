# 09. Git Workflow & Version Control Conventions

## 1. Branching Strategy

We follow a **Trunk-Based / GitHub Flow** model:
- `main`: Production-ready, always deployable branch.
- `develop` (Optional if using staging): Pre-release integration branch.
- `feature/<feature-name>`: Feature branches created from `main`.
- `fix/<bug-name>`: Bugfix branches created from `main`.
- `hotfix/<critical-fix>`: Urgent production fixes.

---

## 2. Conventional Commits Standard

All commit messages must follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <short summary in imperative mood>

[optional body explaining motivation and architectural impact]

[optional footer, e.g. Closes #123]
```

### Allowed Types
- `feat`: A new feature (e.g. `feat(fees): add student fee receipt generation`).
- `fix`: A bug fix (e.g. `fix(auth): fix tenant context extraction in signalr hub`).
- `refactor`: Code change that neither fixes a bug nor adds a feature.
- `test`: Adding or correcting tests.
- `docs`: Documentation updates.
- `perf`: Performance improvements.
- `chore`: Build tasks, dependencies, tooling updates.

### Allowed Scopes
- `auth`, `tenant`, `org`, `teacher`, `student`, `batch`, `fees`, `notes`, `tasks`, `quizzes`, `chat`, `events`, `api`, `web`, `infra`.

### Examples
- `feat(quizzes): implement anti-cheating tab switch counter`
- `fix(tenant): enforce organization query filter on fee structures`
- `docs(api): document fee payment recording endpoint`

---

## 3. Pull Request Guidelines
1. **Self-Review**: Review your own diff before submitting PR.
2. **Automated Verification**: Ensure all tests pass (`dotnet test`) and no build warnings exist (`dotnet build /warnaserror`).
3. **No Secrets**: Confirm no connection strings, JWT secret keys, or passwords exist in the diff.
