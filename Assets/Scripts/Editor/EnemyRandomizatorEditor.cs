using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(EnemyRandomizator))]
public class EnemyRandomizatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnemyRandomizator myObj = (EnemyRandomizator)target;

        if (GUILayout.Button("Randomize"))
        {
            myObj.Randomize();
        }
    }
}
