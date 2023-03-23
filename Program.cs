﻿using System.IO;
using Microsoft.Extensions.Logging;
using ZLogger;

var app = ConsoleApp.CreateBuilder(args)
    .ConfigureLogging(x =>
    {
        x.ClearProviders();
        x.SetMinimumLevel(LogLevel.Information);
        x.AddZLoggerConsole(); 
        x.AddZLoggerFile("terraform-import-tool.log"); 
    })
    .Build();

app.AddRootCommand("new resource import", async ()=>{
    app.Logger.ZLogInformation($"start program: {DateTime.Now}");

    await MainAsync();

    app.Logger.ZLogInformation($"end program: {DateTime.Now}");
});

async ValueTask<int> MainAsync()
{
    const string NEW_RESOURCE_DIR = "new_resource";

    if (Directory.Exists(NEW_RESOURCE_DIR))
    {
        Directory.Delete(NEW_RESOURCE_DIR, true);
    }
    var newResourceDir = Directory.CreateDirectory(NEW_RESOURCE_DIR);
    var tfStateLookup = new TfstateLookup();
    var terraform = new Terraform(app.Logger);

    await foreach (string res in terraform.StateListAsync())
    {
        await terraform.ImportNewResourceAsync(tfStateLookup, res, newResourceDir);
    }

    return 0;
}


app.Run();