using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalContext : MonoBehaviour
{
    private static GlobalContext instance;

    [SerializeField] private MeshCollider asteriodCollider;

    private bool pause = false;
    private bool end = false;

    public static bool Pause
    {
        get => instance.pause;
        set => instance.pause = value;
    }
    public static bool End
    {
        get => instance.end;
        set => instance.end = value;
    }

    public static MeshCollider AsteriodCollider
    {
        get => instance.asteriodCollider;
        set => instance.asteriodCollider = value;
    }

    void Start()
    {
        instance = this;
        pause = false;
    }


}
