

Shader "Unlit/PhongTerrain"
{
	Properties
	{
       
        //Colouring Properties
        _COLOR_0("Color 0", Color) = (1,1,1,1)
		_COLOR_1("Color 1",Color) = (0.509,0.509,0.509,1)
		_COLOR_2("Color 2",Color) = (0.153,0.349,0.179,1)
		_COLOR_3("Color 3",Color) = (0.890,0.831,0.659,1)
		_HEIGHT_0("Height 0",float) = 100
		_HEIGHT_1("Height 1",float) = 50
		_HEIGHT_2("Height 2",float) = 0
		_BLEND("Blending",float) = 10

         //Illumination Properties
		_PointLightColor("Point Light Color", Color) = (0, 0, 0)
		_PointLightPosition("Point Light Position", Vector) = (0.0, 0.0, 0.0)

	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

            //Colouring
            uniform fixed4 _COLOR_0;
			uniform fixed4 _COLOR_1;
			uniform fixed4 _COLOR_2;
			uniform fixed4 _COLOR_3;
			uniform float _HEIGHT_0;
			uniform float _HEIGHT_1;
			uniform float _HEIGHT_2;
			uniform float _BLEND;

            //Illumination
			uniform float3 _PointLightColor;
			uniform float3 _PointLightPosition;


			struct vertIn
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 color : COLOR;
			};

			struct vertOut
			{
        
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 worldVertex : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
			};

			// Implementation of the vertex shader
			vertOut vert(vertIn v)
			{
				vertOut o;

				// Convert Vertex position and corresponding normal into world coords.
				float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
				float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), v.normal.xyz));

				// Transform vertex in world coordinates to camera coordinates, and pass colour
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;

                //Colouring
                o.worldVertex = mul(UNITY_MATRIX_M, v.vertex);

				// Pass out the world vertex position and world normal to be interpolated
				// in the fragment shader (and utilised)
				o.worldVertex = worldVertex;
				o.worldNormal = worldNormal;

				return o;
			}

			// Implementation of the fragment shader
			fixed4 frag(vertOut v) : SV_Target
			{

                //Colouring terrain

                float4 color=_COLOR_0;
				//Greater than first height then white
				if (v.worldVertex.y>=_HEIGHT_0+_BLEND){
					v.color = _COLOR_0;
				}
				//Blend first and second
				if (v.worldVertex.y>_HEIGHT_0-_BLEND && v.worldVertex.y<_HEIGHT_0+_BLEND){
					float weight=(_HEIGHT_0+_BLEND-v.worldVertex.y)/(2*_BLEND);
					v.color=weight*_COLOR_1+(1-weight)*_COLOR_0;
				}
				//Less than first height but greater than second grey
				if (v.worldVertex.y<_HEIGHT_0-_BLEND && v.worldVertex.y>_HEIGHT_1+_BLEND) {
					v.color =_COLOR_1;
				}
				//Blend second and third
				if (v.worldVertex.y>_HEIGHT_1-_BLEND && v.worldVertex.y<_HEIGHT_1+_BLEND){
					float weight=(_HEIGHT_1+_BLEND-v.worldVertex.y)/(2*_BLEND);
					v.color=weight*_COLOR_2+(1-weight)*_COLOR_1;
				}
				//Less than second but greater than third
				if (v.worldVertex.y<_HEIGHT_1-_BLEND && v.worldVertex.y>_HEIGHT_2+_BLEND){
					v.color=_COLOR_2;
				}
				//Blend third and sand
				if (v.worldVertex.y>_HEIGHT_2-_BLEND && v.worldVertex.y<_HEIGHT_2+_BLEND){
					float weight=(_HEIGHT_2+_BLEND-v.worldVertex.y)/(2*_BLEND);
					v.color=weight*_COLOR_3+(1-weight)*_COLOR_2;
				}
				//Otherwise sand
				if (v.worldVertex.y<_HEIGHT_2-_BLEND){
					v.color=_COLOR_3;
				}

				// Illumination

				// Our interpolated normal might not be of length 1
				float3 interpNormal = normalize(v.worldNormal);

				// Calculate ambient RGB intensities
				float Ka = 1;
				float3 amb = v.color.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * Ka;

				// Calculate diffuse RBG reflections
				float fAtt = 1;
				float Kd = 1;
				float3 L = normalize(_PointLightPosition - v.worldVertex.xyz);
				float LdotN = dot(L, interpNormal);
				float3 dif = fAtt * _PointLightColor.rgb * Kd * v.color.rgb * saturate(LdotN);

				// Calculate specular reflections 
				// For a matte surface
				float3 spe = 0;

				// Combine Phong illumination model components
				float4 returnColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
				returnColor.rgb = amb.rgb + dif.rgb + spe.rgb;
				returnColor.a = v.color.a;

				return returnColor;
			}
			ENDCG
		}
	}
}