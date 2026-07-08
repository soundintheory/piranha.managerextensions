# SoundInTheory.Piranha.ManagerScopes

Scopes the Piranha manager to a page **sub-tree** — e.g. a franchise location — so an editor can
"switch into" a location and work as if its page were the site root. Adds a header switcher, hides a
scope's sub-pages from the main tree, and gates each scope behind its own permission.

Built on top of **SoundInTheory.Piranha.PageManagerExtensions**, whose page-tree seams it drives
(`IPageTreeRootResolver`, `IPageTreeFilter`). That module must be enabled too.

## What it does

- **Scopes** are pages whose page-type id is configured (`ScopeTypes`). Any page of that type becomes
  a switchable root.
- A **header switcher** (top-right, typeahead) lets the user pick a scope or "All content" (unscoped).
  The selection is stored in **session**; switching reloads so every screen re-roots.
- **Unscoped view:** scope roots the user can access are shown; their sub-pages are hidden (they only
  appear once you switch in). Other content is shown only to users allowed the unscoped interface.
- **Scoped view:** the page tree is re-rooted at the scope; you see and manage its sub-tree.
- **Per-scope permissions** are registered dynamically (one `Scope_{id}` per scope, plus
  `ManagerScopes_Unscoped`). They appear in Piranha's **role editor** — that is the whole
  access-management UI. Assign them to roles there.
- **Enforcement:** page save/delete is checked against the owning scope in a hook, so the boundary
  holds even for Piranha's own core save/delete API (which the claim alone can't gate).
- **Scoped navigation:** while scoped, the left-hand manager nav is replaced with a scope-specific one
  (the core nav is hidden via CSS). Its items come from `IScopedMenuItemProvider`s aggregated by
  `ScopeMenuService` — a built-in "Pages" item plus provider contributions. The default
  `RegionScopedMenuItemProvider` adds one item per region on the scope page's type. Register your own
  `IScopedMenuItemProvider` to add more.
- **Single-region edit view:** each region nav item opens `manager/scoperegion/{pageId}/{regionId}`, a
  focused editor that reuses Piranha's `region` component. Saving posts only that region; the server
  loads a fresh copy of the page, merges the region in, and saves — so edits to other regions (by anyone)
  are never clobbered.

## Setup

```csharp
// AddPiranha(...)
options.UsePageManagerExtensions();               // required dependency
options.UseManagerScopes(o =>
{
    o.ScopeTypes.Add(nameof(LocationPage));        // page types that act as scopes
    // o.RequireUnscopedPermission = true;         // default: restrict the unscoped view
});

// app.UsePiranha(...)  — after App.Init(...)
options.UsePageManagerExtensions();
options.UseManagerScopes();                        // adds session middleware, seeds permissions + hooks
```

Then, in the manager **role editor**, grant roles the scopes they may enter (and, for full-site roles,
the "Use unscoped interface" permission). Newly granted permissions take effect on the user's next
sign-in.

## Options

| Option | Default | Meaning |
|---|---|---|
| `ScopeTypes` | *(empty)* | Page-type ids whose pages are scope roots. |
| `RequireUnscopedPermission` | `true` | When true, only holders of `ManagerScopes_Unscoped` (and admins) see the unscoped tree; others see only the scopes they can access. |

## Notes / limits

- Single default site (matches PageManagerExtensions). Multi-site is a future enhancement.
- The header switcher is positioned over the manager header with fixed CSS — the manager has no header
  extension slot, so re-verify after a Piranha upgrade.
- Reorder (drag) is disabled whenever the tree is scoped or filtered (see PageManagerExtensions'
  `CanReorder`) to avoid corrupting the sort order of out-of-scope siblings.
