"""Drop the gmeinder "My Docs" add-on into a RackPeek checkout.

usage: python gmeinder-addon/apply.py [repo-root]

1. Copies everything under gmeinder-addon/files/ into the repo (same relative paths).
2. Swaps the Docs page's sidebar <aside> for the DocsSidebar component.
3. Registers the user docs store after the built-in docs provider in Program.cs.

Safe to re-run. Exits non-zero, changing nothing further, if upstream moved an anchor;
re-check the two patches below against the new upstream code in that case.
"""
import pathlib
import re
import shutil
import sys

ADDON = pathlib.Path(__file__).resolve().parent
ROOT = pathlib.Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else ADDON.parent

SIDEBAR = """        <!-- Sidebar -->
        <aside class="md:col-span-4 lg:col-span-3">
            <DocsSidebar Items="_docsIndex"
                         IndexLoading="_indexLoading"
                         IndexNotFound="_indexNotFound"
                         @bind-Filter="_filter"/>
        </aside>"""

REGISTRATION_ANCHOR = "builder.Services.AddScoped<IDocsContentProvider, StaticWebAssetDocsContentProvider>();"
REGISTRATION = "builder.Services.AddUserDocs(yamlPath);"


def fail(msg):
    sys.exit(f"apply.py: {msg}")


def copy_files():
    src = ADDON / "files"
    for f in sorted(p for p in src.rglob("*") if p.is_file()):
        dest = ROOT / f.relative_to(src)
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(f, dest)
        print(f"  copied {dest.relative_to(ROOT)}")


def patch_docs_page():
    page = ROOT / "Shared.Rcl/Docs/DocsHomePage.razor"
    text = page.read_text(encoding="utf-8")
    new, n = re.subn(r"        <!-- Sidebar -->\n        <aside .*?\n        </aside>", lambda _: SIDEBAR, text,
                     count=1, flags=re.S)
    if n != 1:
        fail("sidebar <aside> block not found in DocsHomePage.razor")
    if new == text:
        print("  DocsHomePage.razor already patched")
        return
    page.write_text(new, encoding="utf-8", newline="\n")
    print("  patched DocsHomePage.razor (sidebar -> DocsSidebar)")


def patch_program():
    prog = ROOT / "RackPeek.Web/Program.cs"
    text = prog.read_text(encoding="utf-8")
    if REGISTRATION in text:
        print("  Program.cs already patched")
        return
    lines = text.split("\n")
    hits = [i for i, line in enumerate(lines) if line.strip() == REGISTRATION_ANCHOR]
    if len(hits) != 1:
        fail("built-in docs provider registration not found in Program.cs")
    indent = lines[hits[0]][: len(lines[hits[0]]) - len(lines[hits[0]].lstrip())]
    lines.insert(hits[0] + 1, indent + REGISTRATION)
    prog.write_text("\n".join(lines), encoding="utf-8", newline="\n")
    print("  patched Program.cs (AddUserDocs)")


if __name__ == "__main__":
    print(f"applying gmeinder add-on to {ROOT}")
    copy_files()
    patch_docs_page()
    patch_program()
    print("done")
