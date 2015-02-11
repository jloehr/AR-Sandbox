Shader "Custom/TerrainShader" {
	Properties {
		_Water ("Water", 2D) = "white" {}
		_Sand ("Sand", 2D) = "white" {}
		_Grass ("Rock", 2D) = "white" {}
		_Rock ("Rock", 2D) = "white" {}
		_WaterLevel ("Water Level", Float) = 0
		_LayerSize ("Layer Size", Float) = 20
		_BlendRange ("Blend Range", Range(0, 0.5)) = 0.1
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
     
            uniform sampler2D _Water;
            uniform sampler2D _Sand;
            uniform sampler2D _Grass;
            uniform sampler2D _Rock;

            uniform float _WaterLevel;
            uniform float _LayerSize;
            uniform float _BlendRange;

			struct fragmentInput {
				float4 pos : SV_POSITION;
                float4 texcoord : TEXCOORD0;
				float4 blend: COLOR;
			};
      
			fragmentInput vert (appdata_base v)
			{
				float NumOfTextures = 4;
				fragmentInput o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                o.texcoord = v.texcoord;

				//  |-----------|--------|--------|------------------|
				//  +   Water   L   Sand    Green    Rock            0
				//     |--------|--------|--------|--------|
				//     0                                   1

				float MinValue = _WaterLevel - (NumOfTextures - 1) * _LayerSize; 
				float MaxValue = (_WaterLevel + _LayerSize); 
				float Blend = MaxValue - v.vertex.z;
				Blend = clamp(Blend / (NumOfTextures *_LayerSize), 0, 1);

				o.blend.xyz = 0;
				o.blend.w = Blend;
				return o;
			}
			

			inline float CalculateBlend(float TextureFloat)
			{
				return  1 - clamp((1 - TextureFloat) / _BlendRange, 0 , 1);
			}

			inline float4 DoBlending(float TextureID, float TextureFloat, fixed4 BaseTexture, fixed4 BlendTexture)
			{
				float Blend = CalculateBlend(clamp(TextureFloat - TextureID, 0 , 1));
				return lerp(BaseTexture, BlendTexture, Blend);
			} 

			float4 frag (fragmentInput i) : COLOR0 
			{ 	
				float NumOfTextures = 4;
				float TextureFloat = i.blend.w * NumOfTextures;

				if(TextureFloat < 1)
				{
					fixed4 WaterColor = tex2D(_Water, i.texcoord);
					fixed4 SandColor = tex2D(_Sand, i.texcoord);

					return DoBlending(0, TextureFloat, WaterColor, SandColor);
				} 
				else if(TextureFloat < 2)
				{
					fixed4 SandColor = tex2D(_Sand, i.texcoord);
					fixed4 GrassColor = tex2D(_Grass, i.texcoord);

					return DoBlending(1, TextureFloat, SandColor, GrassColor);
				} 
				else if(TextureFloat < 3)
				{
					fixed4 GrassColor = tex2D(_Grass, i.texcoord);
					fixed4 RockColor = tex2D(_Rock, i.texcoord);

					return DoBlending(2, TextureFloat, GrassColor, RockColor);
				}
				
				fixed4 RockColor = tex2D(_Rock, i.texcoord);

				return RockColor;

				fixed4 WaterColor = tex2D(_Water, i.texcoord);
				fixed4 SandColor = tex2D(_Sand, i.texcoord);

				return lerp(WaterColor, SandColor, i.blend.w);

				//return i.texcoord;	
                //return tex2D(_Water, i.texcoord);
			}

      ENDCG
    }
  } 
	FallBack "Diffuse"
}
