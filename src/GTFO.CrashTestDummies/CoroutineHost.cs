using System;
using UnityEngine;

namespace GTFO.CrashTestDummies;

/// <summary>
/// Minimal Il2Cpp-injected MonoBehaviour used purely as a place to run coroutines from.
/// Il2Cpp interop types must carry this exact (IntPtr) constructor.
/// </summary>
internal class CoroutineHost : MonoBehaviour
{
    public CoroutineHost(IntPtr ptr) : base(ptr) { }
}
