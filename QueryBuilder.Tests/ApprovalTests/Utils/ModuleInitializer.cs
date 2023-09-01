using System.Runtime.CompilerServices;
using DiffEngine;

namespace SqlKata.Tests.ApprovalTests.Utils
{
    public static class ModuleInitializer
    {

        [ModuleInitializer]
        public static void Initialize() =>
            VerifyDiffPlex.Initialize();


        [ModuleInitializer]
        public static void OtherInitialize()
        {
            DiffTools.AddToolBasedOn(DiffTool.AraxisMerge, "araxis");
            VerifierSettings.InitializePlugins();
            VerifierSettings.ScrubLinesContaining("DiffEngineTray");
            VerifierSettings.IgnoreStackTrace();
            VerifierSettings.AddScrubber(x => x
                .Replace("SELECT", "\nSELECT")
                .Replace("INNER", "\nINNER")
                .Replace("FROM", "\nFROM")
            );
        }
    }
}
