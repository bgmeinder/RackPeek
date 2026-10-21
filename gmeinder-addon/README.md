# gmeinder add-on: "My Docs" for the Docs page

A drop-in add-on for upstream RackPeek (tested on v2.1.0). It replaces the Docs page's left box with:

- **Add Page** — a panel matching RackPeek's "Add System" panel: an optional page name and an **add** button that
  opens a file picker for a `.md` file (the page takes the typed name, or the file's name if left empty).
- **Search** — filters as you type, matching page names *and* page text; text matches show a one-line snippet with
  the hit highlighted.
- **RackPeek Pages** — the built-in docs, collapsible.
- **My Docs** — your own Markdown pages, collapsible, each with **Rename** and **Delete** (RackPeek's own modals),
  shown on hover and on the open page.

User pages are plain `.md` files in `<config>/docs/` (`/app/config/docs` in the container — the same volume as
`config.yaml`) and are served at `/docs/my/<name>`. Names are saved lowercase and dash-separated
(`Upload Test.md` → `upload-test.md`); adding a page with an existing name replaces it.

## What it changes

| | |
|---|---|
| New files (`files/`, copied as-is) | `Shared.Rcl/Docs/DocsSidebar.razor` (the left box), `Shared.Rcl/Docs/IUserDocsStore.cs`, `RackPeek.Web/UserDocs/*` (file store, docs provider wrapper, DI extension) |
| `Shared.Rcl/Docs/DocsHomePage.razor` | The sidebar `<aside>` block is replaced by `<DocsSidebar …/>` |
| `RackPeek.Web/Program.cs` | One line after the built-in docs provider: `builder.Services.AddUserDocs(yamlPath);` |

The WebAssembly demo viewer has no user docs store, so it shows only RackPeek Pages.

## Updating to a new upstream release

```bash
git fetch upstream --tags
git checkout -B gmeinder-docs RackPeek-<new version>
git checkout <previous gmeinder-docs commit> -- gmeinder-addon   # bring the add-on folder back
python gmeinder-addon/apply.py                                  # fails loudly if upstream moved an anchor
dotnet build RackPeek.sln -c Release
docker build --network host -f RackPeek.Web/Dockerfile -t rackpeek-gmeinder:<new version>-mydocs .
```

If `apply.py` reports a missing anchor, upstream changed the Docs page or `Program.cs`: update the two patches in
`apply.py` to the new code (the new files rarely need changes).
