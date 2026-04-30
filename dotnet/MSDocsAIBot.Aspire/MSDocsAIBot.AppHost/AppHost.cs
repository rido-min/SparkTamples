var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MSDocsAIBot>("msdocsaibot")
       .WithEndpoint("http", e => e.IsProxied = false);

//builder.AddProject<Projects.conteston>("conteston");s

builder.Build().Run();
