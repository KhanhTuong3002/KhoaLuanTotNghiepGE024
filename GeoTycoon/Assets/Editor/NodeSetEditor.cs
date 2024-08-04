using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


[CustomEditor(typeof(MonopolyBoard))]
public class NodeSetEditor : Editor
{
    SerializedProperty nodeSetListProperty;

    private void OnEnable()
    {
        nodeSetListProperty = serializedObject.FindProperty("nodeSetList");
       
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MonopolyBoard monopolyBoard = (MonopolyBoard)target;
        EditorGUILayout.PropertyField(nodeSetListProperty, true);

        if(GUILayout.Button("Change Image Colors"))
        {
            Undo.RecordObject(monopolyBoard, "change image colors");
            for (int i = 0; i < monopolyBoard.nodeSetList.Count; i++)
            {
                MonopolyBoard.NodeSet nodeSet = monopolyBoard.nodeSetList[i];

                for (int j = 0; j < nodeSet.nodesInSetList.Count; j++)
                {
                    MonopolyNode node = nodeSet.nodesInSetList[j];
                    Image image = node.propertyColorField;
                    if(image != null)
                    {
                        Undo.RecordObject(image, "change image color");
                        image.color = nodeSet.setColor;
                    }
                }
            }
        }


        serializedObject.ApplyModifiedProperties();
    }
}
