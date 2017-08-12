using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MotionStripes))]
public class                                MotionStripesEditor : Editor
{
    SerializedProperty                      _blendRatio;
    SerializedProperty                      _colorBlendRatio;

    SerializedProperty                      _motionVectorsAmplitude;
    SerializedProperty                      _motionVectorsResolution;
    static GUIContent                       _textArrowsAmplitude = new GUIContent("Arrows Amplitude");
    static GUIContent                       _textArrowsResolution = new GUIContent("Arrows Resolution");


    void                                    OnEnable()
    {
        _blendRatio = serializedObject.FindProperty("_blendRatio");
        _colorBlendRatio = serializedObject.FindProperty("_colorBlendRatio");

        _motionVectorsAmplitude = serializedObject.FindProperty("_motionVectorsAmplitude");
        _motionVectorsResolution = serializedObject.FindProperty("_motionVectorsResolution");
    }

    public override void                    OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_motionVectorsAmplitude, _textArrowsAmplitude);
        EditorGUILayout.PropertyField(_motionVectorsResolution, _textArrowsResolution);
        EditorGUILayout.PropertyField(_blendRatio);
        EditorGUILayout.PropertyField(_colorBlendRatio);
        serializedObject.ApplyModifiedProperties();
    }
}
