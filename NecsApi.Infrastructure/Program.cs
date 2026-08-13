// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using NecsApi.Infrastructure.Services;

namespace NecsApi.Infrastructure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var scriptGenerationService = new ScriptGenerationService();

            scriptGenerationService.GenerateBuildScript(
                branchName: "main",
                dotNetVersion: "10.x");

            scriptGenerationService.GeneratePrLintScript(branchName: "main");
        }
    }
}
