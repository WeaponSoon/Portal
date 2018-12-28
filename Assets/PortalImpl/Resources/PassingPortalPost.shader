Shader "Unlit/PassingPortalPost"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PortalImg ("Portal Image", 2D) = "white"{}

		_PortalPos ("Portal Position", Vector) = (0,0,1,0)
		_PortalNor ("Portal Normal", Vector) = (0,0,-1,0)		

		_NearPlane ("Near Plane", Float) = 0.3
		_HalfWidth ("Half Width", Float) = 0.3
		_HalfHeight ("Half Height", Float) = 0.3
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
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

			sampler2D _MainTex;
			sampler2D _PortalImg;
			float4 _MainTex_ST;
			float _HalfHeight;
			float _HalfWidth;
			float _NearPlane;
			float4 _PortalNor;
			float4 _PortalPos;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				float posX = -_HalfWidth + 2 * _HalfWidth * i.uv.x;
				float posY = -_HalfHeight + 2 * _HalfHeight * i.uv.y;
				float ratio = (posX-_PortalPos.x)*_PortalNor.x +
					(posY-_PortalPos.y)*_PortalNor.y +
					(_NearPlane - _PortalPos.z)*_PortalNor.z;
				float mask = (ratio > 0 ? 1 : 0); 
				
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 colOther = tex2D(_PortalImg, i.uv);

				return col*mask + colOther * (1-mask);
			}
			ENDCG
		}
	}
}
