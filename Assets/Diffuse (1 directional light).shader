Shader "Diffuse (1 Directional Light)" {

Properties {
	_MainTex ("Base (RGB)", 2D) = ""
}

SubShader {
	Pass {
		Tags {"LightMode" = "Vertex"}
		GLSLPROGRAM
		varying lowp vec2 uv;
		varying lowp vec3 lighting;
		
		#ifdef VERTEX
		void main() {
			gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			uv = gl_MultiTexCoord0.xy;
			lighting = 2. * (gl_LightSource[0].diffuse.rgb
				* max(dot(gl_LightSource[0].position.xyz, gl_NormalMatrix * gl_Normal), 0.)
					+ gl_LightModel.ambient.rgb);
		}
		#endif
		
		#ifdef FRAGMENT
		uniform lowp sampler2D _MainTex;
		void main() {
			gl_FragColor = vec4(texture2D(_MainTex, uv).rgb * lighting, 1);
		}
		#endif		
		ENDGLSL
	}
	
	Pass {
		Tags {"LightMode"="VertexLM" }
		GLSLPROGRAM
		uniform lowp mat4 unity_LightmapMatrix;
		varying lowp vec2 uv, uv2;
				
		#ifdef VERTEX
		void main() {
			gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			uv = gl_MultiTexCoord0.xy;
			uv2 = vec2(unity_LightmapMatrix[0].x, unity_LightmapMatrix[1].y)
				* gl_MultiTexCoord1.xy + unity_LightmapMatrix[3].xy;
		}
		#endif
		
		#ifdef FRAGMENT
		uniform lowp sampler2D _MainTex, unity_Lightmap;
		void main() {
			gl_FragColor = 
				vec4(texture2D(_MainTex, uv).rgb * texture2D(unity_Lightmap, uv2).rgb * 2., 1);
		}
		#endif		
		ENDGLSL
	}
	
	Pass {	// Editor only - Graphics Emulation uses the wrong lightmap type
		Tags {"LightMode"="VertexLMRGBM" }
		GLSLPROGRAM
		uniform lowp mat4 unity_LightmapMatrix;
		varying lowp vec2 uv, uv2;
				
		#ifdef VERTEX
		void main() {
			gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			uv = gl_MultiTexCoord0.xy;
			uv2 = vec2(unity_LightmapMatrix[0].x, unity_LightmapMatrix[1].y)
				* gl_MultiTexCoord1.xy + unity_LightmapMatrix[3].xy;
		}
		#endif
		
		#ifdef FRAGMENT
		uniform lowp sampler2D _MainTex, unity_Lightmap;
		void main() {
			gl_FragColor = 
				vec4(texture2D(_MainTex, uv).rgb * texture2D(unity_Lightmap, uv2).rgb * 2., 1);
		}
		#endif		
		ENDGLSL
	}
}

SubShader {
	Pass {
		Tags {"LightMode" = "Vertex"}
		Lighting On Material {
			Ambient (1,1,1)
			Diffuse (1,1,1)
		}
		SetTexture[_MainTex] {Combine texture * primary Double} 
	}
	
	Pass {
		Tags {"LightMode" = "VertexLM"}
		BindChannels {
			Bind "vertex", vertex
			Bind "texcoord1", texcoord0
			Bind "texcoord", texcoord1
		}
		SetTexture[unity_Lightmap] {Matrix[unity_LightmapMatrix]}
		SetTexture[_MainTex] {Combine texture * previous Double}
	}
}
 
}