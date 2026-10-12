# TB Optimizer Git history migration

Standalone repository extracted from
[tharbad08/TimberbornMods, TB-Optimizer branch](https://github.com/tharbad08/TimberbornMods/tree/TB-Optimizer)
at source commit `03ff4883b2688de4e11f25d27b8e2930803a3aa4`.

`git subtree split --prefix=BenchmarkAndOptimizerV11Port` reconstructs
the **entire optimizer-directory commit history** as the repository root.
Original commit authors, authored timestamps, messages, and file-by-file
progression are retained. Commit IDs change because the monorepo path and
unrelated files have been removed. Other mod histories are not included.

The final standalone setup commit adds the original root LICENSE, a
legacy installation docs. GitHub Actions workflow provisioning is
handled separately because the migration token lacks workflow-write permission.
This bootstrap import workflow is intentionally not copied into the
resulting history. The separate experimental SWC compatibility PR in
the old monorepo is not part of the source `TB-Optimizer` branch.
