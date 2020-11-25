

Shader "Unlit/PhongWater"
{
	Properties
	{
		_PointLightColor("Point Light Color", Color) = (0, 0, 0)
		_PointLightPosition("Point Light Position", Vector) = (0.0, 0.0, 0.0)
		_MainTex ("Texture", 2D) = "white" {}
		_Amplitude ("Amplitude", Range(1,10)) = 4.5
		_Speed ("Speed", Range(1,10)) = 1.5
		_Frequency ("Frequency", Range(1,10)) = 3.5
	}
	SubShader
	{
		Pass
		{
            Cull off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float3 _PointLightColor;
			uniform float3 _PointLightPosition;
			uniform sampler2D _MainTex;
			float _Amplitude, _Speed, _Frequency;

			struct vertIn
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 uv : TEXCOORD0;
			};

			struct vertOut
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 worldVertex : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
			};

			// Implementation of the vertex shader
			vertOut vert(vertIn v)
			{

				// Scaling waves by frequency to increase/decrease the amount of wave
				// speed by time to control speed of the wave
				// displacement(sin value) by amplitude to control height of the wave   
				float4 displacement = float4(0.0f, _Amplitude * sin((v.vertex.x + (_Time.y * _Speed)) * _Frequency), 0.0f, 0.0f);
				v.vertex += displacement;
                
				vertOut o;

				// Convert Vertex position and corresponding normal into world coords.
				float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
				float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), v.normal.xyz));

				// Transform vertex in world coordinates to camera coordinates, and pass UV
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				// Pass out the world vertex position and world normal to be interpolated
				// in the fragment shader (and utilised)
				o.worldVertex = worldVertex;
				o.worldNormal = worldNormal;

				return o;
			}

			// Implementation of the fragment shader
			fixed4 frag(vertOut v) : SV_Target
			{
				// Sample the texture for the "unlit" colour for this pixel
				float4 unlitColor = tex2D(_MainTex, v.uv);

				// Our interpolated normal might not be of length 1
				float3 interpNormal = normalize(v.worldNormal);

				// Calculate ambient RGB intensities
				float Ka = 1;
				float3 amb = unlitColor.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * Ka;

				// Calculate diffuse RBG reflections
				float fAtt = 1;
				float Kd = 1;
				float3 L = normalize(_PointLightPosition - v.worldVertex.xyz);
				float LdotN = dot(L, interpNormal);
				float3 dif = fAtt * _PointLightColor.rgb * Kd * unlitColor.rgb * saturate(LdotN);

				// Calculate specular reflections
				float Ks = 1;
				float specN = 5; 
				float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex.xyz);
				float3 H = normalize(V + L);
				float3 spe = fAtt * _PointLightColor.rgb * Ks * pow(saturate(dot(interpNormal, H)), specN);

				// Combine Phong illumination model components
				float4 returnColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
				returnColor.rgb = amb.rgb + dif.rgb + spe.rgb;
				returnColor.a = unlitColor.a;

				return returnColor;
			}
			ENDCG
		}
	}
}
