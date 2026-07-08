# SoundInTheory.Piranha.PageManagerExtensions

A Piranha CMS manager extension module (`IModule`) with a Vite + Vue 2 asset pipeline and
dev/prod asset serving. Generated from the `piranha.sit.module` template.

## What's in here

| Path | Purpose |
| --- | --- |
| `Module.cs` | The `IModule` entry point. `Init()` is where you register fields/blocks/menu/hooks. |
| `PageManagerExtensionsExtensions.cs` | `Add*`/`Use*` startup wiring + dev/prod asset serving. |
| `Permissions.cs` | Permission/claim names for the module. |
| `resources/assets/vue/` | Vue 2 source. `app.js` auto-registers every component under `manager/components`. |
| `vite.config.js` / `package.json` | Build the Vue assets to `./assets/vue` (embedded into the assembly). |

## Build the assets

The Vue assets are generated, not committed. Build them before building the project:

```bash
npm install
npm run build      # one-off build
npm run watch      # rebuild on change (use while developing against a DEBUG host)
```

In a **DEBUG** build, assets are served straight from the source `assets/` folder, so
`npm run watch` + a browser refresh picks up changes with no recompile. In **RELEASE**, the
assets embedded into the assembly are served instead.

## Wire it into a host project

Reference this project, then in the host's `Program.cs`:

```csharp
builder.Services.AddPiranha(options =>
{
    options.UseManager();
    // ...
    options.UsePageManagerExtensions();
});

app.UsePiranha(options =>
{
    options.UseManager();
    // ...
    options.UsePageManagerExtensions();
});
```

## Where to go next

`Init()` and the `Add*` extension are where you hook into Piranha. See the `piranha` Claude skills
(`building-modules`, `custom-field-types`, `custom-blocks`, `manager-menu-and-permissions`,
`custom-manager-pages`, `toolbar-and-modal-actions`, `customizing-built-in-screens`) for each
extension point.
