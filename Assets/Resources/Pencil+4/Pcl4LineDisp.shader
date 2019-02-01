Shader "Hidden/Pcl4LineDisp"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Blend One Zero

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _LineTex;

			fixed4 frag (v2f i) : SV_Target
			{
#if UNITY_COLORSPACE_GAMMA
				float4 from = tex2D(_MainTex, i.uv);
				float4 lineColor = tex2D(_LineTex, i.uv);
				return float4(from.rgb * (1.0 - lineColor.a) + lineColor.rgb, 1.0 - (1.0 - from.a) * (1.0 - lineColor.a));
#else
				float4 from = tex2D(_MainTex, i.uv);
				float4 lineColor = tex2D(_LineTex, i.uv);

				if (lineColor.a == 0.0)
				{
					return from;
				}

				from.rgb = LinearToGammaSpace(from.rgb);

				lineColor.rgb /= lineColor.a;
				lineColor.rgb = LinearToGammaSpace(lineColor.rgb);

				return float4(GammaToLinearSpace(lerp(from.rgb, lineColor.rgb, lineColor.a)), 1.0 - (1.0 - from.a) * (1.0 - lineColor.a));
#endif
			}
			ENDCG
		}
	}
}
