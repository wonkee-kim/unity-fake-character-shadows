# Fake Character Shadows
Implementing multiple lighting shadows in a pixel shader without relying on shadow maps for shadow effects.

https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/756a31b9-5d0f-416c-9708-afc6bcf9d1c2





## Demo (Available on Web, Mobile and MetaQuest - Powered by [Spatial Creator Toolkit](https://www.spatial.io/toolkit))
https://www.spatial.io/s/Fake-Character-Shadows-65ed3e7bd6afc7e93521bf59



## Problem of Shadow Mapping
[Shadow Mapping](https://en.wikipedia.org/wiki/Shadow_mapping), a common technique in realtime-graphics provides an accurate method for implementing shadows, but it comes with clear limiations.

<img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/8c7c9e71-53a0-4887-a776-6792659a9195" style="width: 48%"> <img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/7b7df436-edc4-4e95-a366-2706234e3d0d" style="width: 48%">
<img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/851d942c-711f-4d61-97f0-1e3e388db601" style="width: 48%"> <img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/27ee456a-fdcb-419a-ba43-1f7abebaf35c" style="width: 48%">

In the art concept above, if there is no strong main light and multiple light sources need to be utilized, the effectiveness of Shadow Mapping diminishes significantly. In the absence of a primary light, the Shadow Mapping techniques becomes less effective and results in unnecessary shadow map computations.

## Alternative Approach
<img src="http://wiki.polycount.com/w/images/7/7e/Blob_shadows.jpg" style="width: 50%"><br>
(Images from Polycount.wiki [http://wiki.polycount.com/wiki/Decal](http://wiki.polycount.com/wiki/Decal))

Considering past experiences with Blob Shadows in casual games for web and mobile devices where precise shadows are not always necessary and approximations are acceptable, I started exploring a simple idea applied in the pixel shader of background objects. Starting from this concept, I initiated the shader sketch.

(shader sketch 1)

https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/1029fdcd-af3a-4ce7-b396-90f7ea1e15e7

(shader sketch 2)

https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/152fc114-739a-4115-b8ed-afa8193d8456



## Implementation
### Shader
Below is the shader code that calculates the shadow approximately. As you can see it's very simple and very approximate. Even though they are applied per light, it's still very light.
```hlsl
half CalcFakeShadowPerLight(half4 light, half3 playerPos, half playerRad, half3 posToPlayer, half3 posWS)
{
    // Calc dot
    half3 playerToLight = normalize(light.xyz - playerPos);
    half d = dot(posToPlayer, playerToLight);
    float r = 1 - playerRad;
    d = saturate((d - r) / (1 - r)); // remap range: r~1 -> 0~1

    // Attenuation
    half distLightToPos = distance(posWS, light.xyz);
    half atten = 1 - saturate(distLightToPos / (light.w + epsilon)); // Apply light radius
    atten = atten * atten; // Inverse Square Law

    // Adjust attenuation and reverse
    return 1 - saturate(d * atten);
}
```
#### Breakdown
1. First dot playerToLight and PositionToPlayer.
<br><img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/54e7ee50-7ab1-4573-95c6-97ad66d65cd8" style="width: 50%"><br>

2. Adjust it with player radius.
<br><img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/cb591d35-27f8-4839-9f62-ec4794e3537d" style="width: 50%"><br>

3. Calculate attenuation using distance.
<br><img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/eeec2521-87a7-404e-a374-61cec1311a14" style="width: 50%"><br>

4. dot * attenuation
<br><img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/d4d4ccd3-3180-4ee8-b2ab-2d31b471d4d6" style="width: 50%"><br>

5. One minus the result
<br><img src="https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/6b4878a1-4d57-48ac-8764-de0466458456" style="width: 50%"><br>


### Runtime Script
Needs to fetch lighting information such as light position and radius.

Utilizes Unity [Physics.OverlapSphere](https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html) (considered more optimal than any of C# script approach). [FakeShadowsManager.cs#L61](https://github.com/wonkee-kim/unity-fake-character-shadows/blob/main/unity-fake-character-shadows-unity/Assets/FakeCharacterShadows/Scripts/FakeShadowsManager.cs#L61)
Also uses global shader variables to avoid accessing all meshes and materials of the environment.
[FakeShadowsManager.cs#L85-L87](https://github.com/wonkee-kim/unity-fake-character-shadows/blob/main/unity-fake-character-shadows-unity/Assets/FakeCharacterShadows/Scripts/FakeShadowsManager.cs#L85-L87)


## Results
https://github.com/wonkee-kim/unity-fake-character-shadows/assets/830808/756a31b9-5d0f-416c-9708-afc6bcf9d1c2

Works reasonably well.
This shadow is applied in a game (which will be shared soon), and the game can run on 10-year-old mobile devices without serious heat issues

### Advantages
Very low draw call overhead by not using shadow maps.

### Disadvantages
Increased pixel computation load. In my experience, the impact on performance from the load of a single pixel complexity is low. On contrary, more load is generated from drawing more pixels with alpha-blending as surfaces overlap.

### Improvement Ideas
- Basically it's point shadows so this can be improved by utilizing capsule shadow.
- For a simple process, the environment shader uses ShaderGraph, which is limited to modify lighting. So fake shadow is applied to Albedo and AmbientOcclusion which is not accurate way to calculate shadows.
To have more accurate lighting, it will need to be applied to lightmap sample result.
- If you have any improvement ideas, please share them in the [Issues](https://github.com/wonkee-kim/unity-fake-character-shadows/issues) section.
