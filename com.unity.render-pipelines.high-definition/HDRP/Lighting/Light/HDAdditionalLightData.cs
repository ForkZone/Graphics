#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.Rendering.HDPipeline;
#endif
using UnityEngine.Serialization;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    // This enum extent the original LightType enum with new light type from HD
    public enum LightTypeExtent
    {
        Punctual, // Fallback on LightShape type
        Rectangle,
        Line,
        // Sphere,
        // Disc,
    };

    public enum SpotLightShape { Cone, Pyramid, Box };
    
    public enum LightUnit
    {
        Lumen,
        Candela,
        Lux,
        Luminance,
    }

    //@TODO: We should continuously move these values
    // into the engine when we can see them being generally useful
    [RequireComponent(typeof(Light))]
    [ExecuteInEditMode]
    public class HDAdditionalLightData : MonoBehaviour, ISerializationCallbackReceiver
    {
        public const float currentVersion = 1.1f;

        [HideInInspector]
        public float version = currentVersion;

        // To be able to have correct default values for our lights and to also control the conversion of intensity from the light editor (so it is compatible with GI)
        // we add intensity (for each type of light we want to manage).
        [System.Obsolete("directionalIntensity is deprecated, use intensity and lightUnit instead")]
        public float directionalIntensity   = Mathf.PI; // In Lux
        [System.Obsolete("punctualIntensity is deprecated, use intensity and lightUnit instead")]
        public float punctualIntensity      = 600.0f;   // Light default to 600 lumen, i.e ~48 candela
        [System.Obsolete("areaIntensity is deprecated, use intensity and lightUnit instead")]
        public float areaIntensity          = 200.0f;   // Light default to 200 lumen to better match point light

        public const float k_DefaultDirectionalLightIntensity = Mathf.PI; // In lux
        public const float k_DefaultPunctualLightIntensity = 600.0f;      // In lumens
        public const float k_DefaultAreaLightIntensity = 200.0f;          // In lumens
        
        public float intensity
        {
            get { return displayLightIntensity; }
            set { SetLightIntensity(value); }
        }

        // Only for Spotlight, should be hide for other light
        public bool enableSpotReflector = false;

        [Range(0.0f, 100.0f)]
        public float m_InnerSpotPercent = 0.0f; // To display this field in the UI this need to be public

        public float GetInnerSpotPercent01()
        {
            return Mathf.Clamp(m_InnerSpotPercent, 0.0f, 100.0f) / 100.0f;
        }

        [Range(0.0f, 1.0f)]
        public float lightDimmer = 1.0f;

        [Range(0.0f, 1.0f)]
        public float volumetricDimmer = 1.0f;

        // Used internally to convert any light unit input into light intensity
        public LightUnit lightUnit;

        // Not used for directional lights.
        public float fadeDistance = 10000.0f;

        public bool affectDiffuse = true;
        public bool affectSpecular = true;

        // This property work only with shadow mask and allow to say we don't render any lightMapped object in the shadow map
        public bool nonLightmappedOnly = false;

        public LightTypeExtent lightTypeExtent = LightTypeExtent.Punctual;

        // Only for Spotlight, should be hide for other light
        public SpotLightShape spotLightShape = SpotLightShape.Cone;

        // Only for Rectangle/Line/box projector lights
        public float shapeWidth = 0.5f;

        // Only for Rectangle/box projector lights
        public float shapeHeight = 0.5f;

        // Only for pyramid projector
        public float aspectRatio = 1.0f;

        // Only for Sphere/Disc
        public float shapeRadius = 0.0f;

        // Only for Spot/Point - use to cheaply fake specular spherical area light
        [Range(0.0f, 1.0f)]
        public float maxSmoothness = 1.0f;

        // If true, we apply the smooth attenuation factor on the range attenuation to get 0 value, else the attenuation is just inverse square and never reach 0
        public bool applyRangeAttenuation = true;

        // This is specific for the LightEditor GUI and not use at runtime
        public bool useOldInspector = false;
        public bool featuresFoldout = true;
        public bool showAdditionalSettings = false;
        public float displayLightIntensity;
        
        // Duplication of HDLightEditor.k_MinAreaWidth, maybe do something about that
        const float k_MinAreaWidth = 0.01f; // Provide a small size of 1cm for line light

        // We nee all those variables to make timeline and the animator record the intensity value and the emissive mesh changes
        [System.NonSerialized]
        float oldDisplayLightIntensity;
        [System.NonSerialized]
        float oldSpotAngle;
        [System.NonSerialized]
        bool oldEnableSpotReflector;
        [System.NonSerialized]
        Color oldLightColor;
        [System.NonSerialized]
        Vector3 oldLocalScale;
        [System.NonSerialized]
        bool oldDisplayAreaLightEmissiveMesh;
        [System.NonSerialized]
        LightTypeExtent oldLightTypeExtent;
        [System.NonSerialized]
        float oldLightColorTemperature;

        // Runtime datas used to compute light intensity
        Light       _light;
        Light       m_Light
        {
            get
            {
                if (_light == null)
                    _light = GetComponent<Light>();
                return _light;
            }
        }
        
        void SetLightIntensity(float intensity)
        {
            displayLightIntensity = intensity;

            if (lightUnit == LightUnit.Lumen)
            {
                switch (lightTypeExtent)
                {
                    case LightTypeExtent.Punctual:
                        SetLightIntensityPunctual(intensity);
                        break;
                    case LightTypeExtent.Line:
                            m_Light.intensity = LightUtils.CalculateLineLightLumenToLuminance(intensity, shapeWidth);
                        break;
                    case LightTypeExtent.Rectangle:
                        m_Light.intensity = LightUtils.ConvertRectLightLumenToLuminance(intensity, shapeWidth, shapeHeight);
                        break;
                }
            }
            else
                m_Light.intensity = intensity;
                
            m_Light.SetLightDirty(); // Should be apply only to parameter that's affect GI, but make the code cleaner
        }

        void SetLightIntensityPunctual(float intensity)
        {
            switch (m_Light.type)
            {
                case LightType.Directional:
                    m_Light.intensity = intensity; // Alwas in lux
                    break;
                case LightType.Point:
                    if (lightUnit == LightUnit.Candela)
                        m_Light.intensity = intensity;
                    else
                        m_Light.intensity = LightUtils.ConvertPointLightLumenToCandela(intensity);
                    break;
                case LightType.Spot:
                    if (lightUnit == LightUnit.Candela)
                        m_Light.intensity = intensity;
                    else if (enableSpotReflector)
                    {
                        if (spotLightShape == SpotLightShape.Cone)
                        {
                            m_Light.intensity = LightUtils.ConvertSpotLightLumenToCandela(intensity, m_Light.spotAngle * Mathf.Deg2Rad, true);
                        }
                        else if (spotLightShape == SpotLightShape.Pyramid)
                        {
                            float angleA, angleB;
                            LightUtils.CalculateAnglesForPyramid(aspectRatio, m_Light.spotAngle,
                                out angleA, out angleB);

                            m_Light.intensity = LightUtils.ConvertFrustrumLightLumenToCandela(intensity, angleA, angleB);
                        }
                        else // Box shape, fallback to punctual light.
                        {
                            m_Light.intensity = LightUtils.ConvertPointLightLumenToCandela(intensity);
                        }
                    }
                    else // Reflector disabled, fallback to punctual light.
                    {
                        m_Light.intensity = LightUtils.ConvertPointLightLumenToCandela(intensity);
                    }
                    break;
            }
        }

        // Given a correlated color temperature (in Kelvin), estimate the RGB equivalent. Curve fit error is max 0.008.
        Color CorrelatedColorTemperatureToRGB(float temperature)
        {
            float r, g, b;

            // Temperature must fall between 1000 and 40000 degrees
            // The fitting require to divide kelvin by 1000 (allow more precision)
            float kelvin = Mathf.Clamp(temperature, 1000.0f, 40000.0f) / 1000.0f;
            float kelvin2 = kelvin * kelvin;

            // Using 6570 as a pivot is an approximation, pivot point for red is around 6580 and for blue and green around 6560.
            // Calculate each color in turn (Note, clamp is not really necessary as all value belongs to [0..1] but can help for extremum).
            // Red
            r = kelvin < 6.570f ? 1.0f : Mathf.Clamp((1.35651f + 0.216422f * kelvin + 0.000633715f * kelvin2) / (-3.24223f + 0.918711f * kelvin), 0.0f, 1.0f);
            // Green
            g = kelvin < 6.570f ?
                Mathf.Clamp((-399.809f + 414.271f * kelvin + 111.543f * kelvin2) / (2779.24f + 164.143f * kelvin + 84.7356f * kelvin2), 0.0f, 1.0f) :
                Mathf.Clamp((1370.38f + 734.616f * kelvin + 0.689955f * kelvin2) / (-4625.69f + 1699.87f * kelvin), 0.0f, 1.0f);
            //Blue
            b = kelvin > 6.570f ? 1.0f : Mathf.Clamp((348.963f - 523.53f * kelvin + 183.62f * kelvin2) / (2848.82f - 214.52f * kelvin + 78.8614f * kelvin2), 0.0f, 1.0f);

            return new Color(r, g, b, 1.0f);
        }

        // When true, a mesh will be display to represent the area light (Can only be change in editor, component is added in Editor)
        public bool displayAreaLightEmissiveMesh = false;

#if UNITY_EDITOR

        private void DrawGizmos(bool selected)
        {
            var light = gameObject.GetComponent<Light>();
            var gizmoColor = light.color;
            gizmoColor.a = selected ? 1.0f : 0.3f; // Fade for the gizmo
            Gizmos.color = Handles.color = gizmoColor;

            if (lightTypeExtent == LightTypeExtent.Punctual)
            {
                switch (light.type)
                {
                    case LightType.Directional:
                        HDLightEditorUtilities.DrawDirectionalLightGizmo(light);
                        break;
                    case LightType.Point:
                        HDLightEditorUtilities.DrawPointlightGizmo(light, selected);
                        break;
                    case LightType.Spot:
                        if (spotLightShape == SpotLightShape.Cone)
                            HDLightEditorUtilities.DrawSpotlightGizmo(light, selected);
                        else if (spotLightShape == SpotLightShape.Pyramid)
                            HDLightEditorUtilities.DrawFrustumlightGizmo(light);
                        else if (spotLightShape == SpotLightShape.Box)
                            HDLightEditorUtilities.DrawFrustumlightGizmo(light);
                        break;
                }
            }
            else
            {
                switch (lightTypeExtent)
                {
                    case LightTypeExtent.Rectangle:
                        HDLightEditorUtilities.DrawArealightGizmo(light);
                        break;
                    case LightTypeExtent.Line:
                        HDLightEditorUtilities.DrawArealightGizmo(light);
                        break;
                }
            }

            if (selected)
            {
                DrawVerticalRay();
            }
        }

        // Trace a ray down to better locate the light location
        private void DrawVerticalRay()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Handles.color = Color.green;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.DrawLine(transform.position, hit.point);
                Handles.DrawWireDisc(hit.point, hit.normal, 0.5f);

                Handles.color = Color.red;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
                Handles.DrawLine(transform.position, hit.point);
                Handles.DrawWireDisc(hit.point, hit.normal, 0.5f);
            }
        }

        private void OnDrawGizmos()
        {
            // DrawGizmos(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }

        // TODO: There are a lot of old != current checks and assignation in this function, maybe think about using another system ?
        void LateUpdate()
        {
            // Check if the intensity have been changed by the inspector or an animator
            if (oldDisplayLightIntensity != displayLightIntensity
                || lightTypeExtent != oldLightTypeExtent
                || transform.localScale != oldLocalScale
                || m_Light.colorTemperature != oldLightColorTemperature)
            {
                RefreshLigthIntensity();
                UpdateAreaLightEmissiveMesh();
                oldDisplayLightIntensity = displayLightIntensity;
                oldLocalScale = transform.localScale;
            }

            // Same check for light angle to update intensity using spot angle
            if (m_Light.type == LightType.Spot && (oldSpotAngle != m_Light.spotAngle || oldEnableSpotReflector != enableSpotReflector))
            {
                RefreshLigthIntensity();
                oldSpotAngle = m_Light.spotAngle;
                oldEnableSpotReflector = enableSpotReflector;
            }

            if (m_Light.color != oldLightColor
                || transform.localScale != oldLocalScale
                || displayAreaLightEmissiveMesh != oldDisplayAreaLightEmissiveMesh
                || lightTypeExtent != oldLightTypeExtent
                || m_Light.colorTemperature != oldLightColorTemperature)
            {
                UpdateAreaLightEmissiveMesh();
                oldLightColor = m_Light.color;
                oldLocalScale = transform.localScale;
                oldDisplayAreaLightEmissiveMesh = displayAreaLightEmissiveMesh;
                oldLightTypeExtent = lightTypeExtent;
                oldLightColorTemperature = m_Light.colorTemperature;
            }
        }

        // The editor can only access displayLightIntensity (because of SerializedProperties) so we update the intensity to get the real value
        void RefreshLigthIntensity()
        {
            intensity = displayLightIntensity;
        }

        public static bool IsAreaLight(LightTypeExtent lightType)
        {
            return lightType != LightTypeExtent.Punctual;
        }

        public static bool IsAreaLight(SerializedProperty lightType)
        {
            return IsAreaLight((LightTypeExtent)lightType.enumValueIndex);
        }
        
        public void UpdateAreaLightEmissiveMesh()
        {
            MeshRenderer  emissiveMeshRenderer = GetComponent<MeshRenderer>();
            MeshFilter    emissiveMeshFilter = GetComponent<MeshFilter>();

            bool displayEmissiveMesh = IsAreaLight(lightTypeExtent) && lightTypeExtent != LightTypeExtent.Line && displayAreaLightEmissiveMesh;

            // Ensure that the emissive mesh components are here
            if (displayEmissiveMesh)
            {
                if (emissiveMeshRenderer == null)
                    emissiveMeshRenderer = gameObject.AddComponent<MeshRenderer>();
                if (emissiveMeshFilter == null)
                    emissiveMeshFilter = gameObject.AddComponent<MeshFilter>();
            }
            else // Or remove them if the option is disabled
            {
                if (emissiveMeshRenderer != null)
                    DestroyImmediate(emissiveMeshRenderer);
                if (emissiveMeshFilter != null)
                    DestroyImmediate(emissiveMeshFilter);

                // We don't have anything to do left if the dislay emissive mesh option is disabled
                return ;
            }

            // Update light area size from GameObject transform scale
            Vector3 lightSize = m_Light.transform.localScale;
            lightSize = Vector3.Max(Vector3.one * k_MinAreaWidth, lightSize);
            m_Light.transform.localScale = lightSize;

            float areaLightIntensity = intensity;

            switch (lightTypeExtent)
            {
                case LightTypeExtent.Rectangle:
                    shapeWidth = lightSize.x;
                    shapeHeight = lightSize.y;
                    
                    // If the light unit is in lumen, we need a convertion to get the good intensity value
                    if (lightUnit == LightUnit.Lumen)
                    {
                        areaLightIntensity = LightUtils.ConvertRectLightLumenToLuminance(
                            intensity,
                            shapeWidth,
                            shapeHeight);
                    }
                    break;
                default:
                    break;
            }

            if (emissiveMeshRenderer.sharedMaterial == null)
                emissiveMeshRenderer.material = new Material(Shader.Find("HDRenderPipeline/Unlit"));
            
            // Update Mesh emissive properties
            emissiveMeshRenderer.sharedMaterial.SetColor("_UnlitColor", Color.black);
            // Note that we must use the light in linear RGB
            emissiveMeshRenderer.sharedMaterial.SetColor("_EmissiveColor", m_Light.color.linear * areaLightIntensity * CorrelatedColorTemperatureToRGB(m_Light.colorTemperature));
        }

#endif

        // As we have our own default value, we need to initialize the light intensity correctly
        public static void InitDefaultHDAdditionalLightData(HDAdditionalLightData lightData)
        {
            // Special treatment for Unity built-in area light. Change it to our rectangle light
            var light = lightData.gameObject.GetComponent<Light>();

            // Set light intensity and unit using its type
            switch (light.type)
            {
                case LightType.Directional:
                    lightData.lightUnit = LightUnit.Lux;
                    lightData.intensity = k_DefaultDirectionalLightIntensity;
                    break;
                case LightType.Area: // Rectangle by default when light is created
                    lightData.lightUnit = LightUnit.Lumen;
                    lightData.intensity = k_DefaultAreaLightIntensity;
                    break;
                case LightType.Point:
                case LightType.Spot:
                    lightData.lightUnit = LightUnit.Lumen;
                    lightData.intensity = k_DefaultPunctualLightIntensity;
                    break;
            }

            // Sanity check: lightData.lightTypeExtent is init to LightTypeExtent.Punctual (in case for unknow reasons we recreate additional data on an existing line)
            if (light.type == LightType.Area && lightData.lightTypeExtent == LightTypeExtent.Punctual)
            {
                lightData.lightTypeExtent = LightTypeExtent.Rectangle;
                light.type = LightType.Point; // Same as in HDLightEditor
#if UNITY_EDITOR
                light.lightmapBakeType = LightmapBakeType.Realtime;
#endif
            }

            // We don't use the global settings of shadow mask by default
            light.lightShadowCasterMode = LightShadowCasterMode.Everything;
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            // If we are deserializing an old version, convert the light intensity to the new system
            if (version == 1.0f)
            {
                //TODO: test this

// Pragma to disable the warning got by using deprecated properties (areaIntensity, directionalIntensity, ...)
#pragma warning disable 0618
                switch (lightTypeExtent)
                {
                    case LightTypeExtent.Punctual:
                        switch (m_Light.type)
                        {
                            case LightType.Directional:
                                lightUnit = LightUnit.Lux;
                                intensity = directionalIntensity;
                                break;
                            case LightType.Spot:
                            case LightType.Point:
                                lightUnit = LightUnit.Lumen;
                                intensity = punctualIntensity;
                                break;
                        }
                        break;
                    case LightTypeExtent.Line:
                    case LightTypeExtent.Rectangle:
                        lightUnit = LightUnit.Lumen;
                        intensity = areaIntensity;
                        break;
                }
#pragma warning restore 0618

                version = currentVersion;
            }
        }
    }
}
