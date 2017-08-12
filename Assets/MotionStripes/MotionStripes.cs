//
// Copyright (C) 2017 Borel Fabien
//
// Made using parts of the Kino/Vision from Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using System.Collections.Generic;

class StripeArray
{
    public Mesh mesh { get { return _mesh; } }

    Mesh _mesh;

    public int columnCount { get; private set; }
    public int rowCount { get; private set; }

    public void BuildMesh(int columns, int rows)
    {
        // base shape
        var arrow = new Vector3[2]
        {
            new Vector3( 0, 0, 0),
            new Vector3( 0, 1, 0)
        };

        // make the vertex array
        var vcount = 2 * columns * rows;
        var vertices = new List<Vector3>(vcount);
        var uvs = new List<Vector2>(vcount);

        for (var iy = 0; iy < rows; iy++)
        {
            for (var ix = 0; ix < columns; ix++)
            {
                var uv = new Vector2(
                    (0.5f + ix) / columns,
                    (0.5f + iy) / rows
                );

                for (var i = 0; i < 2; i++)
                {
                    vertices.Add(arrow[i]);
                    uvs.Add(uv);
                }
            }
        }

        // make the index array
        var indices = new int[vcount];

        for (var i = 0; i < vcount; i++)
            indices[i] = i;

        // initialize the mesh object
        _mesh = new Mesh();
        _mesh.hideFlags = HideFlags.DontSave;

        _mesh.SetVertices(vertices);
        _mesh.SetUVs(0, uvs);
        _mesh.SetIndices(indices, MeshTopology.Lines, 0);

        _mesh.UploadMeshData(true);

        // update the properties
        columnCount = columns;
        rowCount = rows;
    }

    public void DestroyMesh()
    {
        if (_mesh != null) Object.DestroyImmediate(_mesh);
        _mesh = null;
    }
}

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("MotionStripes")]
public class                MotionStripes : MonoBehaviour
{
    [SerializeField, Range(0, 130)]
    int                             _motionVectorsResolution = 65;
    [SerializeField]
    float                           _motionVectorsAmplitude = 50;
    [SerializeField, Range(0, 1)]
    float                           _blendRatio = 0.5f;
    [SerializeField, Range(0, 1)]
    float                           _colorBlendRatio = 0.5f;

    #region Private properties and methods

    Camera                          _camera;
    Vector3                         _frustrumFarCenter;     // sensible to camera-rotations
    Vector3                         _frustrumNearCenter;    // sensible to camera-rotations + player movement
    Material                        _material;
    StripeArray                     _stripes;
    
    // Target camera
    Camera                          TargetCamera {
        get { return GetComponent<Camera>(); }
    }
    
    // Check if the G-buffer is available.
    bool                            IsGBufferAvailable {
        get { return TargetCamera.actualRenderingPath == RenderingPath.DeferredShading; }
    }
    
    // Rebuild arrows if needed.
    void                            PrepareArrows()
    {
        var row = _motionVectorsResolution;
        var col = row * Screen.width / Screen.height;
    
        if (_stripes.columnCount != col || _stripes.rowCount != row)
        {
            _stripes.DestroyMesh();
            _stripes.BuildMesh(col, row);
        }
    }

    /*
    void                            OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(ViewNearFrustrumCenter(), 10);
        Gizmos.DrawSphere(ViewFarFrustrumCenter(), 10);
    }
    */

    Vector3                         ViewNearFrustrumCenter()
    {
        return this._camera.transform.position + this._camera.transform.forward * this._camera.nearClipPlane;
    }

    Vector3                         ViewFarFrustrumCenter()
    {
        return this._camera.transform.position + this._camera.transform.forward * this._camera.farClipPlane;
    }

    // Calculates a pseudo motion vector which would be attached to a pixel at the center of the image in the middle of the view frustrum
    void                            SetCameraRotationOffset()
    {
        Vector3                     oldFrustrumFarCenter = new Vector3(this._frustrumFarCenter.x, this._frustrumFarCenter.y, this._frustrumFarCenter.z);
        Vector3                     oldFrustrumNearCenter = new Vector3(this._frustrumNearCenter.x, this._frustrumNearCenter.y, this._frustrumNearCenter.z);

        this._frustrumFarCenter = ViewFarFrustrumCenter();
        this._frustrumNearCenter = ViewNearFrustrumCenter();
        Vector2 farDifference = this._camera.WorldToViewportPoint(oldFrustrumFarCenter) - this._camera.WorldToViewportPoint(this._frustrumFarCenter);
        Vector2 nearDifference = this._camera.WorldToViewportPoint(oldFrustrumNearCenter) - this._camera.WorldToViewportPoint(this._frustrumNearCenter);

        _material.SetVector("_CamRotOffset", farDifference);
        _material.SetVector("_CamTransOffset", (nearDifference - farDifference));
    }
    
    // Draw arrows in an immediate-mode fashion
    void                            DrawArrows(float aspect)
    {
        PrepareArrows();
    
        var sy = 1.0f / _motionVectorsResolution;
        var sx = sy / aspect;

        SetCameraRotationOffset();
        _material.SetVector("_Scale", new Vector2(sx, sy));
        _material.SetFloat("_Blend", _blendRatio);
        _material.SetFloat("_ColorBlend", _colorBlendRatio);
        _material.SetFloat("_Amplitude", _motionVectorsAmplitude);
        _material.SetPass(0);

        Graphics.DrawMeshNow(_stripes.mesh, Matrix4x4.identity);
    }

    #endregion

    #region MonoBehaviour functions

    void                            OnEnable()
    {
        this._camera = TargetCamera;
        this._camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        this._frustrumFarCenter = ViewFarFrustrumCenter();
        this._frustrumNearCenter = ViewNearFrustrumCenter();

        // Initialize the pairs of shaders/materials.
        _material = new Material(Shader.Find("Hidden/MotionStripes"));
        _material.hideFlags = HideFlags.DontSave;

        // Build the array of arrows.
        _stripes = new StripeArray();
        PrepareArrows();
    }


    void                            OnDisable()
    {
        DestroyImmediate(_material);
        _material = null;

        _stripes.DestroyMesh();
        _stripes = null;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetTexture("_MainTex", source);
        Graphics.Blit(source, destination);
        DrawArrows((float)source.width / source.height);
    }

    #endregion
}