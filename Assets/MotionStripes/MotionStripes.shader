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

Shader "Hidden/MotionStripes"
{
    Properties
    {
		_MainTex("", 2D) = ""{}
		_VectorLut("Vector LUT", 2D) = "white" {}
    }
	SubShader
	{
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always Cull Off ZWrite Off
			
			CGPROGRAM
			#pragma multi_compile _ UNITY_COLORSPACE_GAMMA
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include			"UnityCG.cginc"

			half				_Blend;
			half				_ColorBlend;
			half				_Amplitude;
			float2				_Scale;
			float2				_CamRotOffset;
			float2				_CamTransOffset;
			sampler2D_half		_CameraMotionVectorsTexture;
			sampler2D			_CameraDepthTexture;
			sampler2D			_VectorLut;
			sampler2D			_MainTex;

			struct				appdata
			{
				float4 vertex   : POSITION;  // The vertex position in model space.
				float3 normal   : NORMAL;    // The vertex normal in model space.
				float4 texcoord : TEXCOORD0; // The first UV coordinate.
			};

			struct				v2f
			{
				float4			vertex : SV_POSITION;
				float2			scoord : TEXCOORD0;
				float4			uv : TEXCOORD1;
				half4			color : COLOR;
			};

			v2f					vert(appdata v)
			{
				// output
				v2f				o;
				//Retrieve the motion vector.
				float4			uv = float4(v.texcoord.xy, 0, 0);
				half2			mv = tex2Dlod(_CameraMotionVectorsTexture, uv).rg;
				mv -= _CamRotOffset;
				mv -= lerp(half2(0, 0), _CamTransOffset, (tex2Dlod(_CameraDepthTexture, uv)));
				half4			color = tex2Dlod(_VectorLut, length(mv));
				mv *= _Amplitude;
	
				// Make a rotation matrix based on the motion vector.
				float2x2		rot = float2x2(-mv.y, -mv.x, +mv.x, -mv.y);
				// Rotate and scale the body of the arrow.
				float2			pos = mul(rot, v.vertex.zy) * _Scale;
				// Normalized variant of the motion vector and the rotation matrix.
				float2			mv_n = normalize(mv);
				float2x2		rot_n = float2x2(mv_n.y, mv_n.x, -mv_n.x, mv_n.y);
				// Rotate and scale the head of the arrow.
				float2			head = float2(v.vertex.x, -abs(v.vertex.x)) * 0.3;

				head *= saturate(color.a);
				pos += mul(rot_n, head) * _Scale;
				// Offset the arrow position.
				pos += v.texcoord.xy * 2 - 1 - (mv / 100);
				// Convert to the screen coordinates.
				float2 scoord = (pos + 1) * 0.5 * _ScreenParams.xy;
				// Snap to a pixel-perfect position.
				scoord = round(scoord);

				// Bring back to the normalized screen space.
				pos = (scoord + 0.5) * (_ScreenParams.zw - 1) * 2 - 1;
				pos.y *= _ProjectionParams.x;


				// Color tweaks
				color.a *= _Blend;

				// Output

				o.vertex = float4(pos, 0, 1);
				o.scoord = scoord;
				o.color = saturate(color);
				o.uv = uv;
				return o;
			}

			half4			frag(v2f i) : SV_Target
			{
				return (lerp(half4(tex2D(_MainTex, i.uv).rgb, i.color.a), i.color, _ColorBlend));
			}
			ENDCG
		}
	}
}
