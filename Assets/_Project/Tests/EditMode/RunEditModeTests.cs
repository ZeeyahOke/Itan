using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Itan.Tests
{
    /// <summary>
    /// Menu: Itan > Tests > Run EditMode Tests. Runs the same tests as the
    /// Test Runner window and prints a one-line summary to the Console.
    /// </summary>
    public static class RunEditModeTests
    {
        [MenuItem("Itan/Tests/Run EditMode Tests")]
        static void Run()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Reporter());
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, assemblyNames = new[] { "Itan.Tests.EditMode" } }));
        }

        class Reporter : ICallbacks
        {
            public void RunStarted(ITestAdaptor tests) { }
            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (!result.HasChildren && result.TestStatus == TestStatus.Failed)
                    Debug.LogError("[Tests] FAILED " + result.Test.Name + ": " + result.Message);
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                string line = "[Tests] " + result.PassCount + " passed, " + result.FailCount + " failed, " + result.SkipCount + " skipped";
                System.IO.File.WriteAllText(System.IO.Path.Combine("Logs", "itan_tests.log"), line + "\n");
                Debug.Log(line);
            }
        }
    }
}
