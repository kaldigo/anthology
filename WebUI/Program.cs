using System.Reflection;
using Anthology.Data;
using Anthology.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterType<DatabaseContext>();
    builder.RegisterType<BookService>().As<IBookService>().InstancePerLifetimeScope();
    builder.RegisterType<ClassificationService>().As<IClassificationService>().InstancePerLifetimeScope();
    builder.RegisterType<MetadataService>().As<IMetadataService>().InstancePerLifetimeScope();
    builder.RegisterType<PluginsService>().As<IPluginsService>().SingleInstance();
    builder.RegisterType<SeriesService>().As<ISeriesService>().InstancePerLifetimeScope();
    builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
});

// Add services to the container.
builder.Services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddMudServices();

var app = builder.Build();

// Configure EF Database.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseMvcWithDefaultRoute();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
