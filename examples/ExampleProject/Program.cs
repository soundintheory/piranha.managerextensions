using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AttributeBuilder;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using SoundInTheory.Piranha.PageManagerExtensions;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;
using SoundInTheory.Piranha.ManagerScopes;
using ExampleProject;
using ExampleProject.Models;
using ExampleProject.Web.Hosting;
using ExampleProject.Services;
using ExampleProject.Extensions;
using FluentEmail.MailKitSmtp;

var builder = WebApplication.CreateBuilder(args);

// Forwarded headers
builder.WebHost.AddForwardedHeaders();

// Sentry logging
builder.WebHost.UseAndConfigureSentry();

// Output caching
builder.AddAndConfigureOutputCache();

// Turnstile captcha
builder.AddAndConfigureTurnstile();

// Demo: register a page-tree filter that hides pages titled "Hidden..." (consumes the module's seam).
builder.Services.AddScoped<IPageTreeFilter, HideHiddenFilter>();

builder.AddPiranha(options =>
{
    /**
     * This will enable automatic reload of .cshtml
     * without restarting the application. However since
     * this adds a slight overhead it should not be
     * enabled in production.
     */
        options.AddRazorRuntimeCompilation = false;

    options.UseCms();
    options.UseManager();

    options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
    options.UseImageSharp();
    options.UseTinyMCE();
    options.UseMemoryCache();
    options.UseLinks();
    options.UseMenus();
    options.UseCroppedImageField();
    options.UseGalleryField();
    options.UseManagerLists();
    options.UseContentAreas();
    options.UseSingletons();

    // Module under development: configurable page-tree replacement.
    options.UsePageManagerExtensions();

    // Module under development: scope the manager to a Location sub-tree, with per-scope permissions.
    options.UseManagerScopes(o => o.ScopeTypes.Add(nameof(LocationPage)));

    var connectionString = builder.Configuration.GetConnectionString("piranha");

    options.UseEF<SQLiteDb>(db => db.UseSqlite(connectionString));
    options.UseIdentityWithSeed<IdentitySQLiteDb>(db => db.UseSqlite(connectionString));

    /**
     * Here you can configure the different permissions
     * that you want to use for securing content in the
     * application.
    options.UseSecurity(o =>
    {
        o.UsePermission("WebUser", "Web User");
    });
     */

    /**
     * Here you can specify the login url for the front end
     * application. This does not affect the login url of
     * the manager interface.
    options.LoginUrl = "login";
     */
});

builder.Services.AddScoped<ScopedCache>();
builder.Services.AddScoped<OptimisedContentService>();
//builder.Services.AddAndConfigureOutputCache();

/** Use the memory session store when hitting cookie size limits (eg. with Cloudflare proxy) */
//builder.Services.AddMemoryCacheSessionStore();

builder.Services
    .AddRazorPages()
    .AddViewLocalization();

builder.Services
    .AddFluentEmail(builder.Configuration.GetSection("MailSettings:FromAddress").Get<string>())
    .AddRazorRenderer()
    .AddMailKitSender(builder.Configuration.GetSection("MailSettings:SmtpClient").Get<SmtpClientOptions>());

//builder.Services.AddTransient<IStartupFilter, OutputCacheStartupFilter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/manager"),
        appBuilder => appBuilder.UseStatusCodePagesWithReExecute("/Error/{0}")
    );
}
else
{
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/manager"),
        appBuilder => appBuilder.UseStatusCodePagesWithReExecute("/Error/{0}")
    );
}

app.UsePiranha(options =>
{
    // Initialize Piranha
    App.Init(options.Api);

    // Register menus
    App.Modules.Navigation().Menus.Register("primary", "Primary Nav", maxDepth: 1);
    App.Modules.Navigation().Menus.Register("footer", "Footer Links", maxDepth: 1);

    // Image types
    App.MediaTypes.Images.Add(".svg", "image/svg+xml", false);

    // Document types
    App.MediaTypes.Documents.Add(".doc", "application/octet-stream", false);
    App.MediaTypes.Documents.Add(".docx", "application/octet-stream", false);

    // Manager CSS
    App.Modules.Manager().Styles.Add("~/assets/manager/css/app.css");

    // Manager JS
    App.Modules.Manager().Scripts.Add("~/assets/manager/js/app.js");

    // Build content types
    new ContentTypeBuilder(options.Api)
        .AddAssembly(typeof(Program).Assembly)
        .Build()
        .DeleteOrphans();

    // Configure Tiny MCE
    EditorConfig.FromFile("editorconfig.json");

    options.UseManager();
    options.UseTinyMCE();
    options.UseIdentity();
    options.UseLinks();
    options.UseMenus();
    options.UseCroppedImageField();
    options.UseGalleryField();
    options.UseManagerLists();
    options.UseContentAreas();
    options.UseSingletons();
    options.UsePageManagerExtensions();
    options.UseManagerScopes();
    //options.UseOutputCacheInvalidation();
});

// Demo seed: create a small page tree on first run so the page-tree module has data to show.
using (var seedScope = app.Services.CreateScope())
{
    var seedApi = seedScope.ServiceProvider.GetRequiredService<IApi>();
    var seedSite = await seedApi.Sites.GetDefaultAsync();
    if (seedSite != null && !(await seedApi.Pages.GetAllAsync(seedSite.Id)).Any())
    {
        async Task<Guid> CreateLocation(string title)
        {
            // A Location page is a scope root (configured via UseManagerScopes above).
            var page = await seedApi.Pages.CreateAsync<LocationPage>();
            page.SiteId = seedSite.Id;
            page.Title = title;
            page.Published = DateTime.Now;
            await seedApi.Pages.SaveAsync(page);
            return page.Id;
        }

        async Task<Guid> CreatePage(string title, Guid? parentId)
        {
            var page = await seedApi.Pages.CreateAsync<GenericPage>();
            page.SiteId = seedSite.Id;
            page.Title = title;
            page.ParentId = parentId;
            page.Published = DateTime.Now;
            await seedApi.Pages.SaveAsync(page);
            return page.Id;
        }

        // Location A has a "Hidden Page" child, so its child group is incomplete → not reorderable.
        var locationA = await CreateLocation("Location A");
        await CreatePage("About A", locationA);
        await CreatePage("Hidden Page", locationA);
        await CreatePage("Services A", locationA);

        // Location B's children are all visible → complete group → reorderable when scoped into B.
        var locationB = await CreateLocation("Location B");
        await CreatePage("About B", locationB);
        await CreatePage("Services B", locationB);
        await CreatePage("Contact B", locationB);

        // A second site, to exercise PageManagerExtensions' multi-site view + top-level reorder.
        var secondSite = new Piranha.Models.Site
        {
            Id = Guid.NewGuid(),
            Title = "Second Site",
            InternalId = "SecondSite",
            IsDefault = false
        };
        await seedApi.Sites.SaveAsync(secondSite);

        async Task CreateSecondSitePage(string title)
        {
            var page = await seedApi.Pages.CreateAsync<GenericPage>();
            page.SiteId = secondSite.Id;
            page.Title = title;
            page.Published = DateTime.Now;
            await seedApi.Pages.SaveAsync(page);
        }

        await CreateSecondSitePage("Second Site Home");
        await CreateSecondSitePage("Second Page");
        await CreateSecondSitePage("Third Page");
    }
}

app.Run();
