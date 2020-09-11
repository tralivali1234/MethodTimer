﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fody;
using Xunit;

public class AssemblyWithAttributeOnAssemblyTests
{
    static TestResult testResult;

    static AssemblyWithAttributeOnAssemblyTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun(
            assemblyPath: "AssemblyWithAttributeOnAssembly.dll",
            ignoreCodes: IgnoreCodes.GetIgnoreCoders()
#if NETCOREAPP2_1
            , runPeVerify: false
#endif
            );
    }

    [Fact]
    public void ClassWithNoAttribute()
    {
        var message = TraceRunner.Capture(() =>
        {
            var instance = testResult.GetInstance("ClassWithNoAttribute");
            instance.Method();
        });
        Assert.Single(message);
        Assert.StartsWith("ClassWithNoAttribute.Method ", message.First());
    }

    [Fact]
    public void ClassWithAsyncMethod()
    {
        var instance = testResult.GetInstance("ClassWithCompilerGeneratedTypes");
        var message = TraceRunner.Capture(() =>
        {
            var task = (Task) instance.AsyncMethod();
            task.Wait();
        });

        Assert.Single(message);
        Assert.StartsWith("ClassWithCompilerGeneratedTypes.AsyncMethod ", message.First());
    }

    [Fact]
    public void ClassWithYieldMethod()
    {
        var instance = testResult.GetInstance("ClassWithCompilerGeneratedTypes");
        var message = TraceRunner.Capture(() =>
        {
            var task = (IEnumerable<string>) instance.YieldMethod();
            task.ToList();
        });

        Assert.Empty(message);
        //TODO: support yield
        //Assert.True(message.First().StartsWith("ClassWithCompilerGeneratedTypes.YieldMethod "));
    }
}