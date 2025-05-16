using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace AuAvengers.Animations;

[RegisterInIl2Cpp]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Unity")]
public sealed class UE_DeleteAfter : MonoBehaviour
{
    public float endTime = -1f;
    public float currentPosition;
    public Action<UE_DeleteAfter, int> after;
    public int inst;

    public void Update()
    {
        currentPosition += Time.deltaTime;
        if (currentPosition > endTime)
        {
            if (after != null) after(this, inst);
            gameObject.DestroyImmediate();
        }
    }
}
