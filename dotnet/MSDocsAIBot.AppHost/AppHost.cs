var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MSDocsAIBot>("msdocsaibot");

builder.Build().Run();
