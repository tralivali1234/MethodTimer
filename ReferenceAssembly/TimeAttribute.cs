using System;

namespace MethodTimer
{
    /// <summary>
    /// Used to flag items as requiring timing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor)]
    public class TimeAttribute : Attribute
    {
    }
}